using System.Text.Json;
using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Cpq.Api.Models.Domain;
using Cpq.Api.Models.Requests;
using Cpq.Api.Models.Responses;
using Cpq.Api.Services.Bom;
using Cpq.Api.Services.Rules;
using Cpq.Api.Services.Specs;
using Cpq.Api.Services.Validation;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly CpqDbContext _db;
    private readonly IRulesEngineClient _rulesEngine;
    private readonly IProductSpecRepository _specRepository;
    private readonly IBomService _bomService;
    private readonly SelectionValidator _selectionValidator;
    private readonly ILogger<ConfigurationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ConfigurationService(
        CpqDbContext db,
        IRulesEngineClient rulesEngine,
        IProductSpecRepository specRepository,
        IBomService bomService,
        SelectionValidator selectionValidator,
        ILogger<ConfigurationService> logger)
    {
        _db = db;
        _rulesEngine = rulesEngine;
        _specRepository = specRepository;
        _bomService = bomService;
        _selectionValidator = selectionValidator;
        _logger = logger;
    }

    public async Task<ConfigurationResponse> CreateAsync(CreateConfigurationRequest request, CancellationToken ct = default)
    {
        var productType = await _db.ProductTypes
            .Include(pt => pt.Family)
            .FirstOrDefaultAsync(pt => pt.Code == request.ProductTypeCode && pt.IsActive, ct)
            ?? throw new EntityNotFoundException(nameof(ProductType), "Code", request.ProductTypeCode);

        var configuration = new Models.Domain.Configuration
        {
            ProductTypeId = productType.Id,
            Reference = request.Reference,
            Status = "draft",
            ConfigData = JsonDocument.Parse("{}"),
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _db.Configurations.AddAsync(configuration, ct);

        await _db.ConfigurationHistories.AddAsync(new ConfigurationHistory
        {
            ConfigurationId = configuration.Id,
            Action = "created",
            SelectionsSnapshot = JsonDocument.Parse("{}"),
            ChangedFields = new List<string>(),
            PerformedBy = request.CreatedBy,
            PerformedAt = DateTimeOffset.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created configuration {Id} for product type {Code}", configuration.Id, request.ProductTypeCode);

        configuration.ProductType = productType;
        return MapToResponse(configuration, null, null, null);
    }

    public async Task<ConfigurationResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        var configuration = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        var validationResult = ParseValidationResult(configuration.ValidationResult);
        return MapToResponse(configuration, validationResult, null, null);
    }

    public async Task<PagedResponse<ConfigurationResponse>> ListAsync(ListConfigurationsRequest filter, CancellationToken ct = default)
    {
        var query = _db.Configurations
            .Include(c => c.ProductType)
                .ThenInclude(pt => pt.Family)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(c => c.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.ProductTypeCode))
            query = query.Where(c => c.ProductType.Code == filter.ProductTypeCode);

        if (!string.IsNullOrEmpty(filter.ProductFamilyCode))
            query = query.Where(c => c.ProductType.Family.Code == filter.ProductFamilyCode);

        var totalCount = await query.CountAsync(ct);

        query = filter.SortDirection?.ToLower() == "asc"
            ? query.OrderBy(c => c.CreatedAt)
            : query.OrderByDescending(c => c.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<ConfigurationResponse>
        {
            Items = items.Select(c => MapToResponse(c, ParseValidationResult(c.ValidationResult), null, null)).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ConfigurationResponse> UpdateAsync(Guid id, UpdateConfigurationRequest request, CancellationToken ct = default)
    {
        var configuration = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        // Optimistic concurrency check
        if (configuration.Version != request.ExpectedVersion)
        {
            throw new ConcurrencyConflictException(id, request.ExpectedVersion, configuration.Version);
        }

        // Validate selections payload
        var validationCheck = await _selectionValidator.ValidateAsync(
            configuration.ProductTypeId, request, ct);

        if (!validationCheck.IsValid)
        {
            throw new ArgumentException(
                $"Invalid selections: {string.Join("; ", validationCheck.Errors)}");
        }

        // Merge selections into ConfigData
        var currentData = DeserializeConfigData(configuration.ConfigData);
        var changedFields = new List<string>();

        foreach (var (key, value) in request.Selections)
        {
            var valueObj = ConvertJsonElement(value);
            if (!currentData.ContainsKey(key) || !Equals(currentData[key]?.ToString(), valueObj?.ToString()))
            {
                changedFields.Add(key);
            }
            currentData[key] = valueObj!;
        }

        // Load spec context for rules
        var specContext = await _specRepository.GetSpecContextAsync(configuration.ProductTypeId, ct);

        // Build rule context
        var ruleContext = new Dictionary<string, object>
        {
            ["userSelections"] = currentData,
            ["specs"] = specContext,
            ["productType"] = new
            {
                code = configuration.ProductType.Code,
                variant = configuration.ProductType.Variant,
                family = configuration.ProductType.Family.Code
            }
        };

        var rulePrefix = configuration.ProductType.Family.RulePrefix;

        // Call validate rule
        ValidationResultResponse? validationResult = null;
        try
        {
            var validatePath = $"{rulePrefix}/validate";
            var validateDoc = await _rulesEngine.EvaluateRawAsync(validatePath, ruleContext, ct);
            validationResult = ParseValidationFromRulesResult(validateDoc);
        }
        catch (RulesEngineException ex)
        {
            _logger.LogWarning(ex, "Validation rule call failed for configuration {Id}, proceeding without validation", id);
        }

        // Call options rule
        Dictionary<string, List<OptionResponse>>? availableOptions = null;
        List<string>? resetFields = null;
        try
        {
            var optionsPath = $"{rulePrefix}/options";
            var optionsDoc = await _rulesEngine.EvaluateRawAsync(optionsPath, ruleContext, ct);
            (availableOptions, resetFields) = ParseOptionsResult(optionsDoc);
        }
        catch (RulesEngineException ex)
        {
            _logger.LogWarning(ex, "Options rule call failed for configuration {Id}", id);
        }

        // Apply reset fields — remove them from ConfigData
        if (resetFields is { Count: > 0 })
        {
            foreach (var field in resetFields)
            {
                currentData.Remove(field);
            }
        }

        // Persist
        var newConfigData = JsonSerializer.Serialize(currentData, JsonOptions);
        configuration.ConfigData = JsonDocument.Parse(newConfigData);
        configuration.ValidationResult = validationResult is not null
            ? JsonDocument.Parse(JsonSerializer.Serialize(validationResult, JsonOptions))
            : null;
        configuration.Version++;
        configuration.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.ConfigurationHistories.AddAsync(new ConfigurationHistory
        {
            ConfigurationId = configuration.Id,
            Action = "updated",
            SelectionsSnapshot = JsonDocument.Parse(newConfigData),
            ValidationSnapshot = configuration.ValidationResult,
            ChangedFields = changedFields,
            PerformedBy = request.PerformedBy,
            PerformedAt = DateTimeOffset.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated configuration {Id} to version {Version}", id, configuration.Version);

        return MapToResponse(configuration, validationResult, availableOptions, resetFields);
    }

    public async Task<ValidationResultResponse> ValidateAsync(Guid id, CancellationToken ct = default)
    {
        var configuration = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        var specContext = await _specRepository.GetSpecContextAsync(configuration.ProductTypeId, ct);

        var ruleContext = new Dictionary<string, object>
        {
            ["userSelections"] = DeserializeConfigData(configuration.ConfigData),
            ["specs"] = specContext,
            ["productType"] = new
            {
                code = configuration.ProductType.Code,
                variant = configuration.ProductType.Variant,
                family = configuration.ProductType.Family.Code
            }
        };

        var rulePrefix = configuration.ProductType.Family.RulePrefix;
        var validatePath = $"{rulePrefix}/validate";

        var validateDoc = await _rulesEngine.EvaluateRawAsync(validatePath, ruleContext, ct);
        var validationResult = ParseValidationFromRulesResult(validateDoc);

        // Persist validation result
        configuration.ValidationResult = JsonDocument.Parse(JsonSerializer.Serialize(validationResult, JsonOptions));
        configuration.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return validationResult;
    }

    public async Task<ConfigurationResponse> FinalizeAsync(Guid id, CancellationToken ct = default)
    {
        var configuration = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        if (configuration.Status == "finalized")
        {
            throw new InvalidOperationException($"Configuration {id} is already finalized.");
        }

        // Run final validation
        var validationResult = await ValidateAsync(id, ct);

        if (!validationResult.Valid)
        {
            throw new InvalidOperationException(
                $"Cannot finalize configuration {id}: validation failed with {validationResult.Errors.Count} error(s).");
        }

        // Reload after save from ValidateAsync
        configuration = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        configuration.Status = "finalized";
        configuration.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.ConfigurationHistories.AddAsync(new ConfigurationHistory
        {
            ConfigurationId = configuration.Id,
            Action = "finalized",
            SelectionsSnapshot = configuration.ConfigData,
            ValidationSnapshot = configuration.ValidationResult,
            PerformedAt = DateTimeOffset.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Finalized configuration {Id}", id);

        return MapToResponse(configuration, validationResult, null, null);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var configuration = await _db.Configurations.FindAsync(new object[] { id }, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        if (configuration.Status == "finalized")
        {
            throw new InvalidOperationException($"Cannot delete finalized configuration {id}.");
        }

        _db.Configurations.Remove(configuration);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted configuration {Id}", id);
    }

    public async Task<ConfigurationResponse> CloneAsync(Guid id, CancellationToken ct = default)
    {
        var source = await LoadConfigurationWithProductType(id, ct)
            ?? throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);

        var clone = new Models.Domain.Configuration
        {
            ProductTypeId = source.ProductTypeId,
            Reference = source.Reference is not null ? $"{source.Reference} (copy)" : null,
            Status = "draft",
            ConfigData = JsonDocument.Parse(source.ConfigData.RootElement.GetRawText()),
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = source.CreatedBy
        };

        await _db.Configurations.AddAsync(clone, ct);

        await _db.ConfigurationHistories.AddAsync(new ConfigurationHistory
        {
            ConfigurationId = clone.Id,
            Action = "cloned",
            SelectionsSnapshot = clone.ConfigData,
            ChangedFields = new List<string> { "clonedFrom" },
            PerformedAt = DateTimeOffset.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);

        clone.ProductType = source.ProductType;

        _logger.LogInformation("Cloned configuration {SourceId} to {CloneId}", id, clone.Id);

        return MapToResponse(clone, null, null, null);
    }

    public Task<BomResponse> GenerateBomAsync(Guid id, CancellationToken ct = default)
        => _bomService.GenerateBomAsync(id, ct);

    // ---- Private helpers ----

    private async Task<Models.Domain.Configuration?> LoadConfigurationWithProductType(Guid id, CancellationToken ct)
    {
        return await _db.Configurations
            .Include(c => c.ProductType)
                .ThenInclude(pt => pt.Family)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    private static ConfigurationResponse MapToResponse(
        Models.Domain.Configuration config,
        ValidationResultResponse? validation,
        Dictionary<string, List<OptionResponse>>? availableOptions,
        List<string>? resetFields)
    {
        var isComplete = validation?.Valid == true && validation.Errors.Count == 0;

        return new ConfigurationResponse
        {
            Id = config.Id,
            ProductType = new ProductTypeResponse
            {
                Code = config.ProductType.Code,
                Name = config.ProductType.Name,
                Variant = config.ProductType.Variant,
                Description = config.ProductType.Description,
                DisplayOrder = config.ProductType.DisplayOrder,
                Family = new ProductFamilyResponse
                {
                    Code = config.ProductType.Family.Code,
                    Name = config.ProductType.Family.Name,
                    Description = config.ProductType.Family.Description,
                    IsActive = config.ProductType.Family.IsActive
                }
            },
            Status = config.Status,
            Reference = config.Reference,
            Config = config.ConfigData,
            Validation = validation,
            AvailableOptions = availableOptions,
            ResetFields = resetFields,
            IsComplete = isComplete,
            CanFinalize = isComplete && config.Status != "finalized",
            Version = config.Version,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt,
            CreatedBy = config.CreatedBy
        };
    }

    private static Dictionary<string, object?> DeserializeConfigData(JsonDocument configData)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in configData.RootElement.EnumerateObject())
        {
            result[prop.Name] = ConvertJsonElement(prop.Value);
        }
        return result;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt64(out var longVal) ? (object)longVal : element.GetDouble(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.Clone()
        };
    }

    private static ValidationResultResponse? ParseValidationResult(JsonDocument? doc)
    {
        if (doc is null) return null;
        try
        {
            return JsonSerializer.Deserialize<ValidationResultResponse>(
                doc.RootElement.GetRawText(), JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static ValidationResultResponse ParseValidationFromRulesResult(JsonDocument doc)
    {
        try
        {
            JsonElement resultEl;
            if (doc.RootElement.TryGetProperty("result", out resultEl))
            {
                // GoRules result structure
            }
            else
            {
                resultEl = doc.RootElement;
            }

            var valid = true;
            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            if (resultEl.TryGetProperty("valid", out var validEl))
                valid = validEl.GetBoolean();

            if (resultEl.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var err in errorsEl.EnumerateArray())
                {
                    errors.Add(new ValidationError
                    {
                        Parameter = err.TryGetProperty("parameter", out var p) ? p.GetString() ?? string.Empty : string.Empty,
                        Rule = err.TryGetProperty("rule", out var r) ? r.GetString() ?? string.Empty : string.Empty,
                        Message = err.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty
                    });
                }
            }

            if (resultEl.TryGetProperty("warnings", out var warningsEl) && warningsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var warn in warningsEl.EnumerateArray())
                {
                    warnings.Add(new ValidationWarning
                    {
                        Parameter = warn.TryGetProperty("parameter", out var p) ? p.GetString() ?? string.Empty : string.Empty,
                        Rule = warn.TryGetProperty("rule", out var r) ? r.GetString() ?? string.Empty : string.Empty,
                        Message = warn.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty
                    });
                }
            }

            return new ValidationResultResponse
            {
                Valid = valid && errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch
        {
            return new ValidationResultResponse { Valid = false, Errors = new List<ValidationError>
            {
                new() { Parameter = "_system", Rule = "parse", Message = "Failed to parse validation result from rules engine." }
            }};
        }
    }

    private static (Dictionary<string, List<OptionResponse>>? options, List<string>? resetFields) ParseOptionsResult(JsonDocument doc)
    {
        try
        {
            JsonElement resultEl;
            if (!doc.RootElement.TryGetProperty("result", out resultEl))
                resultEl = doc.RootElement;

            List<string>? resetFields = null;
            if (resultEl.TryGetProperty("resetFields", out var resetEl) && resetEl.ValueKind == JsonValueKind.Array)
            {
                resetFields = resetEl.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(s => s is not null)
                    .Cast<string>()
                    .ToList();
            }

            Dictionary<string, List<OptionResponse>>? options = null;
            if (resultEl.TryGetProperty("availableOptions", out var optionsEl) && optionsEl.ValueKind == JsonValueKind.Object)
            {
                options = new Dictionary<string, List<OptionResponse>>();
                foreach (var prop in optionsEl.EnumerateObject())
                {
                    if (prop.Value.ValueKind != JsonValueKind.Array) continue;
                    var optList = new List<OptionResponse>();
                    foreach (var opt in prop.Value.EnumerateArray())
                    {
                        optList.Add(new OptionResponse
                        {
                            Code = opt.TryGetProperty("code", out var c) ? c.GetString() ?? string.Empty : string.Empty,
                            DisplayName = opt.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? string.Empty : string.Empty,
                            IsActive = !opt.TryGetProperty("isActive", out var ia) || ia.GetBoolean()
                        });
                    }
                    options[prop.Name] = optList;
                }
            }

            return (options, resetFields);
        }
        catch
        {
            return (null, null);
        }
    }
}
