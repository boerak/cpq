using Cpq.Api.Models.Domain;

namespace Cpq.Api.Services.Specs;

public static class SpecContextBuilder
{
    /// <summary>
    /// Builds a nested spec context dictionary from flat ProductSpec rows.
    /// Groups by SpecGroup then SpecKey.
    /// </summary>
    public static Dictionary<string, object> BuildFromSpecs(IEnumerable<ProductSpec> specs)
    {
        var result = new Dictionary<string, object>();

        foreach (var spec in specs.Where(s => s.IsActive))
        {
            if (!result.ContainsKey(spec.SpecGroup))
            {
                result[spec.SpecGroup] = new Dictionary<string, object>();
            }

            var group = (Dictionary<string, object>)result[spec.SpecGroup];

            try
            {
                group[spec.SpecKey] = spec.SpecValue.RootElement.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.Number => spec.SpecValue.RootElement.GetDouble(),
                    System.Text.Json.JsonValueKind.String => spec.SpecValue.RootElement.GetString() ?? string.Empty,
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    _ => spec.SpecValue.RootElement.Clone()
                };
            }
            catch
            {
                group[spec.SpecKey] = spec.SpecValue.RootElement.Clone();
            }
        }

        return result;
    }

    /// <summary>
    /// Builds a materials context from Material + related data.
    /// </summary>
    public static List<Dictionary<string, object>> BuildMaterialsContext(IEnumerable<Material> materials)
    {
        return materials.Where(m => m.IsActive).Select(m => new Dictionary<string, object>
        {
            ["code"] = m.Code,
            ["name"] = m.Name,
            ["densityKgPerM3"] = m.DensityKgPerM3 ?? 0m,
            ["availableColors"] = m.MaterialColors
                .Where(mc => mc.IsActive && mc.Color?.IsActive == true)
                .Select(mc => mc.ColorCode)
                .ToList()
        }).ToList();
    }

    /// <summary>
    /// Builds a profiles context from Profile data.
    /// </summary>
    public static List<Dictionary<string, object>> BuildProfilesContext(IEnumerable<Models.Domain.Profile> profiles)
    {
        return profiles.Where(p => p.IsActive).Select(p => new Dictionary<string, object>
        {
            ["code"] = p.Code,
            ["name"] = p.Name,
            ["materialCode"] = p.MaterialCode,
            ["heightMm"] = p.HeightMm,
            ["thicknessMm"] = p.ThicknessMm,
            ["weightPerMeterKg"] = p.WeightPerMeterKg,
            ["maxWidthMm"] = p.MaxWidthMm,
            ["minWidthMm"] = p.MinWidthMm
        }).ToList();
    }

    /// <summary>
    /// Builds a motors context from Motor data.
    /// </summary>
    public static List<Dictionary<string, object>> BuildMotorsContext(IEnumerable<Motor> motors)
    {
        return motors.Where(m => m.IsActive).Select(m => new Dictionary<string, object>
        {
            ["code"] = m.Code,
            ["brand"] = m.Brand,
            ["model"] = m.Model,
            ["torqueNm"] = m.TorqueNm,
            ["maxWeightKg"] = m.MaxWeightKg ?? 0m,
            ["maxSurfaceM2"] = m.MaxSurfaceM2 ?? 0m,
            ["controlTypes"] = m.ControlTypes,
            ["tubeDiameterMm"] = m.TubeDiameterMm ?? 0
        }).ToList();
    }

    /// <summary>
    /// Builds guide rails context.
    /// </summary>
    public static List<Dictionary<string, object>> BuildGuideRailsContext(IEnumerable<GuideRail> rails)
    {
        return rails.Where(r => r.IsActive).Select(r => new Dictionary<string, object>
        {
            ["code"] = r.Code,
            ["name"] = r.Name,
            ["type"] = r.Type,
            ["materialCode"] = r.MaterialCode,
            ["maxHeightMm"] = r.MaxHeightMm,
            ["widthMm"] = r.WidthMm,
            ["depthMm"] = r.DepthMm,
            ["weightPerMeterKg"] = r.WeightPerMeterKg,
            ["bracketSpacingMm"] = r.BracketSpacingMm,
            ["compatibleProfiles"] = (object)(r.CompatibleProfiles ?? new List<string>()),
            ["windClass"] = r.WindClass ?? 0
        }).ToList();
    }

    /// <summary>
    /// Builds boxes context.
    /// </summary>
    public static List<Dictionary<string, object>> BuildBoxesContext(IEnumerable<Box> boxes)
    {
        return boxes.Where(b => b.IsActive).Select(b => new Dictionary<string, object>
        {
            ["code"] = b.Code,
            ["name"] = b.Name,
            ["type"] = b.Type,
            ["innerDiameterMm"] = b.InnerDiameterMm,
            ["outerHeightMm"] = b.OuterHeightMm,
            ["compatibleMaterials"] = b.CompatibleMaterials,
            ["maxWidthMm"] = b.MaxWidthMm ?? 0
        }).ToList();
    }

    /// <summary>
    /// Builds colors context.
    /// </summary>
    public static List<Dictionary<string, object>> BuildColorsContext(IEnumerable<Color> colors)
    {
        return colors.Where(c => c.IsActive).Select(c => new Dictionary<string, object>
        {
            ["code"] = c.Code,
            ["name"] = c.Name,
            ["colorSystem"] = c.ColorSystem,
            ["hexValue"] = c.HexValue ?? string.Empty,
            ["isStandard"] = c.IsStandard
        }).ToList();
    }
}
