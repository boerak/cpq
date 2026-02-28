using System.Text.Json;
using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductSpecSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<ProductSpec>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;
        var specs = new List<ProductSpec>();

        specs.AddRange(BuildStandardSpecs(now));
        specs.AddRange(BuildInsulatedSpecs(now));
        specs.AddRange(BuildScreenSpecs(ProductTypeSeeder.ScreenZipId, "zip", now));
        specs.AddRange(BuildScreenSpecs(ProductTypeSeeder.ScreenStandardId, "standard", now));
        specs.AddRange(BuildScreenSpecs(ProductTypeSeeder.ScreenFixId, "fix", now));

        await dbSet.AddRangeAsync(specs);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // roller_shutter_standard
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductSpec> BuildStandardSpecs(DateTimeOffset now)
    {
        var ptId = ProductTypeSeeder.RollerShutterStandardId;

        return new[]
        {
            Spec(ptId, "dimensions", "ALU",
                """{"minWidth":400,"maxWidth":3000,"minHeight":500,"maxHeight":3500,"maxArea":9.0}""",
                "Maatgrenzen voor aluminium rolluiken",
                now),

            Spec(ptId, "dimensions", "PVC",
                """{"minWidth":400,"maxWidth":2500,"minHeight":500,"maxHeight":3000,"maxArea":6.5}""",
                "Maatgrenzen voor PVC rolluiken",
                now),

            Spec(ptId, "weight", "ALU-39",
                """{"weightPerM2":4.5,"maxWeightManual":30,"maxWeightMotor":80}""",
                "Gewichtsspecificaties voor profiel ALU-39",
                now),

            Spec(ptId, "weight", "PVC-37",
                """{"weightPerM2":3.2,"maxWeightManual":25,"maxWeightMotor":60}""",
                "Gewichtsspecificaties voor profiel PVC-37",
                now),

            Spec(ptId, "weight", "ALU-42",
                """{"weightPerM2":5.1,"maxWeightManual":35,"maxWeightMotor":90}""",
                "Gewichtsspecificaties voor profiel ALU-42",
                now),

            Spec(ptId, "motor", "minTorque",
                """{"safetyFactor":1.3}""",
                "Veiligheidsfactor voor minimaal vereist motorkoppel",
                now),

            Spec(ptId, "motor", "maxArea",
                """{"perNm":0.3}""",
                "Maximaal oppervlak per Nm motorkoppel",
                now)
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // roller_shutter_insulated
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductSpec> BuildInsulatedSpecs(DateTimeOffset now)
    {
        var ptId = ProductTypeSeeder.RollerShutterInsulatedId;

        return new[]
        {
            Spec(ptId, "dimensions", "ALU_INSULATED",
                """{"minWidth":500,"maxWidth":3000,"minHeight":500,"maxHeight":3500,"maxArea":8.0}""",
                "Maatgrenzen voor geïsoleerde aluminium rolluiken",
                now),

            Spec(ptId, "weight", "ALU-INS-55",
                """{"weightPerM2":7.2,"maxWeightManual":null,"maxWeightMotor":100}""",
                "Gewichtsspecificaties voor profiel ALU-INS-55 (altijd gemotoriseerd)",
                now),

            Spec(ptId, "motor", "minTorque",
                """{"safetyFactor":1.4}""",
                "Hogere veiligheidsfactor voor geïsoleerde rolluiken",
                now),

            Spec(ptId, "thermal", "uValue",
                """{"target":1.2,"maxWidth":3000}""",
                "Thermische isolatiewaarde (U-waarde) doelstelling",
                now)
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Screen specs (shared across all 3 screen variants, rules use variant)
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductSpec> BuildScreenSpecs(
        Guid ptId, string variant, DateTimeOffset now)
    {
        var specs = new List<ProductSpec>
        {
            // ── Fabric → available colors mapping ────────────────────────
            Spec(ptId, "fabric", "soltis_86",
                """{"colors":["SCR-WHITE","SCR-LINEN","SCR-PEARL-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-CREAM","SCR-BEIGE","SCR-OFF-WHITE"],"openness":["3","5","10"],"maxArea":16.0,"weightPerM2":0.43}""",
                "Soltis 86 — 8 kleuren, 3 openheden", now),

            Spec(ptId, "fabric", "soltis_92",
                """{"colors":["SCR-WHITE","SCR-LINEN","SCR-SAND","SCR-PEARL-GREY","SCR-SILVER","SCR-ALU-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-CREAM","SCR-LIGHT-GREY"],"openness":["1","3","5"],"maxArea":18.0,"weightPerM2":0.49}""",
                "Soltis 92 — 10 kleuren, 3 openheden", now),

            Spec(ptId, "fabric", "soltis_96",
                """{"colors":["SCR-WHITE","SCR-PEARL-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-GRAPHITE","SCR-DARK-GREY"],"openness":["3","5"],"maxArea":14.0,"weightPerM2":0.52}""",
                "Soltis 96 — 6 kleuren, 2 openheden", now),

            Spec(ptId, "fabric", "serge_600",
                """{"colors":["SCR-WHITE","SCR-LINEN","SCR-SAND","SCR-PEARL-GREY","SCR-SILVER","SCR-ALU-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-BRONZE","SCR-TAUPE","SCR-STEEL","SCR-IVORY"],"openness":["3","5","10"],"maxArea":15.0,"weightPerM2":0.39}""",
                "Serge 600 — 12 kleuren, 3 openheden", now),

            Spec(ptId, "fabric", "sunworker_open",
                """{"colors":["SCR-WHITE","SCR-SAND","SCR-PEARL-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-CREAM","SCR-LIGHT-GREY","SCR-STONE"],"openness":["5","10","14"],"maxArea":12.0,"weightPerM2":0.36}""",
                "Sunworker Open — 8 kleuren, 3 openheden, max 12m²", now),

            Spec(ptId, "fabric", "sunworker_classic",
                """{"colors":["SCR-WHITE","SCR-LINEN","SCR-PEARL-GREY","SCR-ALU-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-BROWN","SCR-COFFEE","SCR-NAVY"],"openness":["3","5","10"],"maxArea":12.0,"weightPerM2":0.41}""",
                "Sunworker Classic — 9 kleuren, 3 openheden, max 12m²", now),

            Spec(ptId, "fabric", "copaco_300",
                """{"colors":["SCR-WHITE","SCR-PEARL-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-LIGHT-GREY","SCR-BEIGE"],"openness":["3","5"],"maxArea":8.0,"weightPerM2":0.30}""",
                "Copaco 300 — 6 kleuren, 2 openheden, max 8m²", now),

            Spec(ptId, "fabric", "copaco_350",
                """{"colors":["SCR-WHITE","SCR-PEARL-GREY","SCR-ALU-GREY","SCR-CHARCOAL","SCR-BLACK","SCR-DARK-GREY","SCR-TERRACOTTA"],"openness":["3","5"],"maxArea":8.0,"weightPerM2":0.35}""",
                "Copaco 350 — 7 kleuren, 2 openheden, max 8m²", now),

            // ── Housing constraints ──────────────────────────────────────
            Spec(ptId, "housing", "size_constraints",
                """{"small":["square_100","round_100"],"large":["square_120","round_120","concealed"],"smallMaxWidth":3000}""",
                "Kleine kasten max 3000mm breed", now),

            // ── Guidance constraints ─────────────────────────────────────
            Spec(ptId, "guidance", "compatibility",
                """{"zip":{"bottomBars":["zip"],"housings":["square_100","round_100","square_120","round_120","concealed"]},"cable":{"bottomBars":["flat","round"],"housings":["square_100","round_100","square_120","round_120"]},"free_hanging":{"bottomBars":["weighted"],"housings":["square_100","round_100","square_120","round_120","concealed"]}}""",
                "Geleiding → onderregel en kastcompatibiliteit", now),

            // ── Motor sizing ─────────────────────────────────────────────
            Spec(ptId, "motor", "minTorque",
                """{"safetyFactor":1.3}""",
                "Veiligheidsfactor voor minimaal vereist motorkoppel", now),

            // ── Dimensions ───────────────────────────────────────────────
            Spec(ptId, "dimensions", "screen",
                """{"minWidth":500,"maxWidth":5500,"minHeight":500,"maxHeight":4000}""",
                "Maatgrenzen voor screens", now)
        };

        // ── Variant-specific constraints (stored as spec for rules) ──────
        if (variant == "fix")
        {
            specs.Add(Spec(ptId, "variant", "constraints",
                """{"maxArea":6.0,"allowedDriveTypes":["manual_chain"],"description":"Fix screen: max 6m², alleen kettingbediening"}""",
                "Fix screen variant beperkingen", now));
        }
        else if (variant == "zip")
        {
            specs.Add(Spec(ptId, "variant", "constraints",
                """{"allowedGuidanceTypes":["zip"],"requiresMotor":true,"description":"ZIP screen: alleen zip geleiding, motor vereist"}""",
                "ZIP screen variant beperkingen", now));
        }
        else
        {
            specs.Add(Spec(ptId, "variant", "constraints",
                """{"allowedGuidanceTypes":["zip","cable","free_hanging"],"allowedDriveTypes":["motor","spring_assist","manual_chain"],"description":"Standaard screen: alle opties beschikbaar"}""",
                "Standaard screen variant — geen extra beperkingen", now));
        }

        return specs;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────
    private static ProductSpec Spec(
        Guid productTypeId,
        string specGroup,
        string specKey,
        string specValueJson,
        string description,
        DateTimeOffset now) => new ProductSpec
    {
        Id = Guid.NewGuid(),
        ProductTypeId = productTypeId,
        SpecGroup = specGroup,
        SpecKey = specKey,
        SpecValue = JsonDocument.Parse(specValueJson),
        Description = description,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now
    };
}
