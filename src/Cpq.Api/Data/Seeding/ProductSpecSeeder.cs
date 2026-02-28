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
