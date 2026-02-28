using System.Text.Json;
using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductParameterSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<ProductParameter>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;
        var parameters = new List<ProductParameter>();

        // ── Standard product parameters ──────────────────────────────────────
        parameters.AddRange(BuildParameters(
            productTypeId: ProductTypeSeeder.RollerShutterStandardId,
            widthMaxMm: 3200,
            heightMaxMm: 4000,
            now: now,
            idPrefix: "A0000000-0000-0000-0000"));

        // ── Insulated product parameters ─────────────────────────────────────
        parameters.AddRange(BuildParameters(
            productTypeId: ProductTypeSeeder.RollerShutterInsulatedId,
            widthMaxMm: 3000,
            heightMaxMm: 3500,
            now: now,
            idPrefix: "B0000000-0000-0000-0000"));

        await dbSet.AddRangeAsync(parameters);
        await context.SaveChangesAsync();
    }

    private static IEnumerable<ProductParameter> BuildParameters(
        Guid productTypeId,
        int widthMaxMm,
        int heightMaxMm,
        DateTimeOffset now,
        string idPrefix)
    {
        // Each parameter gets a deterministic ID derived from the prefix and a counter.
        int seq = 0;
        Guid NextId() => new Guid($"{idPrefix}-{(++seq):D12}");

        return new[]
        {
            // ── Step 1: Afmetingen ──────────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "width",
                Name = "Breedte",
                DataType = "integer",
                Unit = "mm",
                StepNumber = 1,
                StepName = "Afmetingen",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse(
                    $$$"""{"min":400,"max":{{{widthMaxMm}}},"step":1,"label":"Breedte","help":"Meet de dagmaat van het raam"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "height",
                Name = "Hoogte",
                DataType = "integer",
                Unit = "mm",
                StepNumber = 1,
                StepName = "Afmetingen",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse(
                    $$$"""{"min":500,"max":{{{heightMaxMm}}},"step":1,"label":"Hoogte"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 2: Materiaal & Profiel ─────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "material",
                Name = "Materiaal",
                DataType = "select",
                Unit = null,
                StepNumber = 2,
                StepName = "Materiaal & Profiel",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse("""{"label":"Materiaal"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "profile",
                Name = "Profiel",
                DataType = "select",
                Unit = null,
                StepNumber = 2,
                StepName = "Materiaal & Profiel",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "material" },
                Metadata = JsonDocument.Parse("""{"label":"Profiel"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 3: Kleur & Kast ────────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "color",
                Name = "Kleur",
                DataType = "select",
                Unit = null,
                StepNumber = 3,
                StepName = "Kleur & Kast",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "material" },
                Metadata = JsonDocument.Parse("""{"label":"Kleur"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "boxType",
                Name = "Kasttype",
                DataType = "select",
                Unit = null,
                StepNumber = 3,
                StepName = "Kleur & Kast",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "material" },
                Metadata = JsonDocument.Parse("""{"label":"Kasttype"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 4: Aandrijving ─────────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "driveType",
                Name = "Aandrijving",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Aandrijving",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse("""{"label":"Aandrijving"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "motorBrand",
                Name = "Motormerk",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Aandrijving",
                DisplayOrder = 2,
                IsRequired = false,
                DefaultValue = null,
                DependsOn = new List<string> { "driveType" },
                Metadata = JsonDocument.Parse("""{"label":"Motormerk","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "controlType",
                Name = "Besturingstype",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Aandrijving",
                DisplayOrder = 3,
                IsRequired = false,
                DefaultValue = null,
                DependsOn = new List<string> { "driveType", "motorBrand" },
                Metadata = JsonDocument.Parse("""{"label":"Besturingstype","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 5: Geleiders & Toebehoren ─────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "guideType",
                Name = "Geleider",
                DataType = "select",
                Unit = null,
                StepNumber = 5,
                StepName = "Geleiders & Toebehoren",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "material", "profile" },
                Metadata = JsonDocument.Parse("""{"label":"Geleider"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "accessories",
                Name = "Toebehoren",
                DataType = "multi_select",
                Unit = null,
                StepNumber = 5,
                StepName = "Geleiders & Toebehoren",
                DisplayOrder = 2,
                IsRequired = false,
                DefaultValue = null,
                DependsOn = new List<string> { "driveType" },
                Metadata = JsonDocument.Parse("""{"label":"Toebehoren"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }
}
