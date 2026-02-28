using System.Text.Json;
using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Cpq.Api.Models.Domain;
using Cpq.Api.Models.Responses;
using Cpq.Api.Services.Rules;
using Cpq.Api.Services.Specs;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Services.Bom;

public class BomService : IBomService
{
    private readonly CpqDbContext _db;
    private readonly IRulesEngineClient _rulesEngine;
    private readonly IProductSpecRepository _specRepository;
    private readonly SkuResolver _skuResolver;
    private readonly ILogger<BomService> _logger;

    public BomService(
        CpqDbContext db,
        IRulesEngineClient rulesEngine,
        IProductSpecRepository specRepository,
        SkuResolver skuResolver,
        ILogger<BomService> logger)
    {
        _db = db;
        _rulesEngine = rulesEngine;
        _specRepository = specRepository;
        _skuResolver = skuResolver;
        _logger = logger;
    }

    public async Task<BomResponse> GenerateBomAsync(Guid configurationId, CancellationToken ct = default)
    {
        var configuration = await _db.Configurations
            .Include(c => c.ProductType)
                .ThenInclude(pt => pt.Family)
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct)
            ?? throw new EntityNotFoundException(nameof(Configuration), configurationId);

        if (configuration.Status == "draft")
        {
            _logger.LogWarning("Generating BOM for draft configuration {Id}", configurationId);
        }

        var specContext = await _specRepository.GetSpecContextAsync(configuration.ProductTypeId, ct);

        // Build rule context
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
        var bomPath = $"{rulePrefix}/bom";

        _logger.LogInformation("Generating BOM for configuration {Id} using rule {Path}", configurationId, bomPath);

        JsonDocument bomRulesResult;
        try
        {
            bomRulesResult = await _rulesEngine.EvaluateRawAsync(bomPath, ruleContext, ct);
        }
        catch (RulesEngineException ex)
        {
            _logger.LogError(ex, "BOM rules engine call failed for configuration {Id}", configurationId);
            throw;
        }

        // Parse BOM lines from rules result
        var bomLines = ParseBomLines(bomRulesResult, configuration.ProductType.Family.Code);

        // Enrich with part data
        var skus = bomLines.Select(l => l.PartSku).Distinct().ToList();
        var parts = await _db.Parts
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku, ct);

        var enrichedLines = bomLines.Select(line =>
        {
            parts.TryGetValue(line.PartSku, out var part);
            return new BomLineResponse
            {
                PartSku = line.PartSku,
                PartName = part?.Name ?? line.PartName,
                Category = part?.Category ?? line.Category,
                Quantity = line.Quantity,
                Unit = part?.Unit ?? line.Unit,
                CutLengthMm = line.CutLengthMm,
                SortOrder = line.SortOrder,
                Notes = line.Notes
            };
        }).OrderBy(l => l.SortOrder).ToList();

        // Persist BOM lines: clear existing and insert new
        var existingLines = await _db.BomLines
            .Where(b => b.ConfigurationId == configurationId)
            .ToListAsync(ct);
        _db.BomLines.RemoveRange(existingLines);

        var newDbLines = enrichedLines.Select((line, i) => new BomLine
        {
            ConfigurationId = configurationId,
            PartSku = line.PartSku,
            PartName = line.PartName,
            Category = line.Category,
            Quantity = line.Quantity,
            Unit = line.Unit,
            CutLengthMm = line.CutLengthMm,
            SortOrder = line.SortOrder,
            Notes = line.Notes
        }).ToList();

        await _db.BomLines.AddRangeAsync(newDbLines, ct);

        // Update configuration BOM data
        configuration.BomData = bomRulesResult;
        configuration.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Calculate total weight
        decimal totalWeight = 0;
        foreach (var line in enrichedLines)
        {
            if (parts.TryGetValue(line.PartSku, out var part) && part.WeightKg.HasValue)
            {
                var lengthFactor = line.CutLengthMm.HasValue ? (decimal)line.CutLengthMm.Value / 1000m : 1m;
                totalWeight += part.WeightKg.Value * line.Quantity * lengthFactor;
            }
        }

        return new BomResponse
        {
            Lines = enrichedLines,
            TotalWeight = Math.Round(totalWeight, 2),
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static Dictionary<string, object> DeserializeConfigData(JsonDocument configData)
    {
        var result = new Dictionary<string, object>();
        foreach (var prop in configData.RootElement.EnumerateObject())
        {
            result[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.Number => prop.Value.GetDouble(),
                JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                JsonValueKind.True => (object)true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => prop.Value.Clone()
            };
        }
        return result;
    }

    private static List<BomLineResponse> ParseBomLines(JsonDocument rulesResult, string familyCode)
    {
        var lines = new List<BomLineResponse>();

        if (!rulesResult.RootElement.TryGetProperty("result", out var resultEl))
            return lines;

        JsonElement linesEl;
        if (resultEl.ValueKind == JsonValueKind.Array)
        {
            linesEl = resultEl;
        }
        else if (resultEl.TryGetProperty("lines", out var nestedLines))
        {
            linesEl = nestedLines;
        }
        else
        {
            return lines;
        }

        int sortOrder = 0;
        foreach (var item in linesEl.EnumerateArray())
        {
            var line = new BomLineResponse
            {
                PartSku = item.TryGetProperty("partSku", out var sku) ? sku.GetString() ?? string.Empty
                         : item.TryGetProperty("sku", out var sku2) ? sku2.GetString() ?? string.Empty : string.Empty,
                PartName = item.TryGetProperty("name", out var name) ? name.GetString() : null,
                Category = item.TryGetProperty("category", out var cat) ? cat.GetString() : null,
                Quantity = item.TryGetProperty("quantity", out var qty) ? (decimal)qty.GetDouble() : 1m,
                Unit = item.TryGetProperty("unit", out var unit) ? unit.GetString() ?? "pcs" : "pcs",
                SortOrder = item.TryGetProperty("sortOrder", out var so) ? so.GetInt32() : sortOrder++,
                Notes = item.TryGetProperty("notes", out var notes) ? notes.GetString() : null
            };

            if (item.TryGetProperty("cutLengthMm", out var cutLen) && cutLen.ValueKind == JsonValueKind.Number)
            {
                line.CutLengthMm = cutLen.GetInt32();
            }

            if (!string.IsNullOrEmpty(line.PartSku))
            {
                lines.Add(line);
            }
        }

        return lines;
    }
}
