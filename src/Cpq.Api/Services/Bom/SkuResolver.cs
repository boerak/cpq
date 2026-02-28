using System.Text.Json;
using Cpq.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Services.Bom;

public class SkuResolver
{
    private readonly CpqDbContext _db;
    private readonly ILogger<SkuResolver> _logger;

    public SkuResolver(CpqDbContext db, ILogger<SkuResolver> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Resolves a SKU from sku_mappings given a family code, category, and selection criteria.
    /// Returns the highest-priority matching SKU or null if none found.
    /// </summary>
    public async Task<string?> ResolveAsync(
        string productFamilyCode,
        string category,
        Dictionary<string, object> criteria,
        CancellationToken ct = default)
    {
        var mappings = await _db.SkuMappings
            .Where(m => m.ProductFamilyCode == productFamilyCode
                     && m.Category == category
                     && m.IsActive)
            .OrderByDescending(m => m.Priority)
            .ToListAsync(ct);

        foreach (var mapping in mappings)
        {
            if (MatchesCriteria(mapping.MatchCriteria, criteria))
            {
                _logger.LogDebug("Resolved SKU {Sku} for family={Family}, category={Category}",
                    mapping.Sku, productFamilyCode, category);
                return mapping.Sku;
            }
        }

        _logger.LogWarning("No SKU found for family={Family}, category={Category}, criteria={Criteria}",
            productFamilyCode, category, JsonSerializer.Serialize(criteria));
        return null;
    }

    private static bool MatchesCriteria(JsonDocument matchCriteria, Dictionary<string, object> criteria)
    {
        try
        {
            foreach (var property in matchCriteria.RootElement.EnumerateObject())
            {
                if (!criteria.TryGetValue(property.Name, out var criteriaValue))
                    return false;

                var criteriaString = criteriaValue?.ToString();
                var matchValue = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.ToString();

                // Support wildcard match
                if (matchValue == "*")
                    continue;

                // Support range match: {"min": x, "max": y}
                if (property.Value.ValueKind == JsonValueKind.Object
                    && property.Value.TryGetProperty("min", out var minEl)
                    && property.Value.TryGetProperty("max", out var maxEl))
                {
                    if (double.TryParse(criteriaString, out var numVal))
                    {
                        var min = minEl.GetDouble();
                        var max = maxEl.GetDouble();
                        if (numVal < min || numVal > max)
                            return false;
                        continue;
                    }
                    return false;
                }

                if (!string.Equals(criteriaString, matchValue, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
