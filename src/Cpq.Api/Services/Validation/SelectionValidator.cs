using System.Text.Json;
using Cpq.Api.Data;
using Cpq.Api.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Services.Validation;

public class SelectionValidatorResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class SelectionValidator
{
    private readonly CpqDbContext _db;
    private readonly ILogger<SelectionValidator> _logger;

    public SelectionValidator(CpqDbContext db, ILogger<SelectionValidator> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Validates the selections in an UpdateConfigurationRequest against known parameters for the product type.
    /// Checks that parameter codes exist and that enum values are valid options.
    /// </summary>
    public async Task<SelectionValidatorResult> ValidateAsync(
        Guid productTypeId,
        UpdateConfigurationRequest request,
        CancellationToken ct = default)
    {
        var result = new SelectionValidatorResult { IsValid = true };

        var parameters = await _db.ProductParameters
            .Where(p => p.ProductTypeId == productTypeId && p.IsActive)
            .ToListAsync(ct);

        var parameterByCode = parameters.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        // Load options for enum parameters
        var enumParamCodes = parameters
            .Where(p => p.DataType == "enum" || p.DataType == "select")
            .Select(p => p.Code)
            .ToList();

        var options = await _db.ProductOptions
            .Where(o => o.ProductTypeId == productTypeId && enumParamCodes.Contains(o.ParameterCode) && o.IsActive)
            .ToListAsync(ct);

        var optionsByParam = options
            .GroupBy(o => o.ParameterCode)
            .ToDictionary(g => g.Key, g => g.Select(o => o.Code).ToHashSet(StringComparer.OrdinalIgnoreCase));

        foreach (var (key, value) in request.Selections)
        {
            if (!parameterByCode.TryGetValue(key, out var param))
            {
                // Unknown parameter — warn but allow (rules engine may handle it)
                _logger.LogDebug("Unknown parameter code in selections: {Code}", key);
                continue;
            }

            // Validate null/required
            if (value.ValueKind == JsonValueKind.Null && param.IsRequired)
            {
                result.IsValid = false;
                result.Errors.Add($"Parameter '{key}' is required and cannot be null.");
                continue;
            }

            if (value.ValueKind == JsonValueKind.Null)
                continue;

            // Validate data type
            var validType = param.DataType switch
            {
                "number" or "decimal" or "integer" => value.ValueKind == JsonValueKind.Number,
                "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
                "string" or "text" => value.ValueKind == JsonValueKind.String,
                "enum" or "select" => value.ValueKind == JsonValueKind.String,
                _ => true // unknown type — pass through
            };

            if (!validType)
            {
                result.IsValid = false;
                result.Errors.Add($"Parameter '{key}' expects type '{param.DataType}' but got '{value.ValueKind}'.");
                continue;
            }

            // Validate enum values
            if ((param.DataType == "enum" || param.DataType == "select")
                && optionsByParam.TryGetValue(key, out var validOptions))
            {
                var selectedValue = value.GetString();
                if (!string.IsNullOrEmpty(selectedValue) && !validOptions.Contains(selectedValue))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Parameter '{key}' value '{selectedValue}' is not a valid option.");
                }
            }

            // Validate numeric ranges if applicable
            if ((param.DataType == "number" || param.DataType == "integer" || param.DataType == "decimal")
                && param.Metadata is not null)
            {
                try
                {
                    var numValue = value.GetDouble();
                    if (param.Metadata.RootElement.TryGetProperty("min", out var minEl)
                        && numValue < minEl.GetDouble())
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Parameter '{key}' value {numValue} is below minimum {minEl.GetDouble()}.");
                    }
                    if (param.Metadata.RootElement.TryGetProperty("max", out var maxEl)
                        && numValue > maxEl.GetDouble())
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Parameter '{key}' value {numValue} exceeds maximum {maxEl.GetDouble()}.");
                    }
                }
                catch
                {
                    // Ignore metadata parsing errors
                }
            }
        }

        return result;
    }
}
