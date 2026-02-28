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

        // ── Screen parameters ───────────────────────────────────────────────
        parameters.AddRange(BuildScreenParameters(
            ProductTypeSeeder.ScreenZipId, now, "D1000000-0000-0000-0000"));
        parameters.AddRange(BuildScreenParameters(
            ProductTypeSeeder.ScreenStandardId, now, "D2000000-0000-0000-0000"));
        parameters.AddRange(BuildScreenParameters(
            ProductTypeSeeder.ScreenFixId, now, "D3000000-0000-0000-0000"));

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

    // ─────────────────────────────────────────────────────────────────────────
    // Screen parameters (shared across all 3 screen product types)
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductParameter> BuildScreenParameters(
        Guid productTypeId,
        DateTimeOffset now,
        string idPrefix)
    {
        int seq = 0;
        Guid NextId() => new Guid($"{idPrefix}-{(++seq):D12}");

        return new[]
        {
            // ── Step 1: Afmetingen ────────────────────────────────────────
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
                Metadata = JsonDocument.Parse("""{"min":500,"max":5500,"step":1,"label":"Breedte","help":"Meet de dagmaat"}"""),
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
                Metadata = JsonDocument.Parse("""{"min":500,"max":4000,"step":1,"label":"Hoogte"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 2: Doek ──────────────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "fabricType",
                Name = "Doektype",
                DataType = "select",
                Unit = null,
                StepNumber = 2,
                StepName = "Doek",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse("""{"label":"Doektype"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "fabricColor",
                Name = "Doekkleur",
                DataType = "select",
                Unit = null,
                StepNumber = 2,
                StepName = "Doek",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "fabricType" },
                Metadata = JsonDocument.Parse("""{"label":"Doekkleur"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "meshOpenness",
                Name = "Openheid",
                DataType = "select",
                Unit = null,
                StepNumber = 2,
                StepName = "Doek",
                DisplayOrder = 3,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "fabricType" },
                Metadata = JsonDocument.Parse("""{"label":"Openheid","help":"Percentage doek openheid"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 3: Frame & Kast ──────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "frameColor",
                Name = "Framekleur",
                DataType = "select",
                Unit = null,
                StepNumber = 3,
                StepName = "Frame & Kast",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse("""{"label":"Framekleur"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "housingType",
                Name = "Kasttype",
                DataType = "select",
                Unit = null,
                StepNumber = 3,
                StepName = "Frame & Kast",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "width" },
                Metadata = JsonDocument.Parse("""{"label":"Kasttype"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "mountType",
                Name = "Montagetype",
                DataType = "select",
                Unit = null,
                StepNumber = 3,
                StepName = "Frame & Kast",
                DisplayOrder = 3,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "housingType" },
                Metadata = JsonDocument.Parse("""{"label":"Montagetype"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 4: Geleiding ─────────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "guidanceType",
                Name = "Geleiding",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Geleiding",
                DisplayOrder = 1,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string>(),
                Metadata = JsonDocument.Parse("""{"label":"Geleidingstype"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "sideChannelColor",
                Name = "Zijgeleider kleur",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Geleiding",
                DisplayOrder = 2,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "frameColor", "guidanceType" },
                Metadata = JsonDocument.Parse("""{"label":"Kleur zijgeleider"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "bottomBar",
                Name = "Onderregel",
                DataType = "select",
                Unit = null,
                StepNumber = 4,
                StepName = "Geleiding",
                DisplayOrder = 3,
                IsRequired = true,
                DefaultValue = null,
                DependsOn = new List<string> { "guidanceType" },
                Metadata = JsonDocument.Parse("""{"label":"Onderregel"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 5: Aandrijving ───────────────────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "driveType",
                Name = "Aandrijving",
                DataType = "select",
                Unit = null,
                StepNumber = 5,
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
                StepNumber = 5,
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
                Code = "motorType",
                Name = "Motortype",
                DataType = "select",
                Unit = null,
                StepNumber = 5,
                StepName = "Aandrijving",
                DisplayOrder = 3,
                IsRequired = false,
                DefaultValue = null,
                DependsOn = new List<string> { "motorBrand", "width", "height" },
                Metadata = JsonDocument.Parse("""{"label":"Motortype","visibleWhen":"driveType == 'motor'"}"""),
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
                StepNumber = 5,
                StepName = "Aandrijving",
                DisplayOrder = 4,
                IsRequired = false,
                DefaultValue = null,
                DependsOn = new List<string> { "motorBrand", "motorType" },
                Metadata = JsonDocument.Parse("""{"label":"Besturingstype","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Step 6: Automatisering & Extra's ──────────────────────────
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "windAutomation",
                Name = "Windautomatisering",
                DataType = "select",
                Unit = null,
                StepNumber = 6,
                StepName = "Automatisering & Extra's",
                DisplayOrder = 1,
                IsRequired = false,
                DefaultValue = "none",
                DependsOn = new List<string> { "driveType" },
                Metadata = JsonDocument.Parse("""{"label":"Windautomatisering","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "remoteControl",
                Name = "Afstandsbediening",
                DataType = "select",
                Unit = null,
                StepNumber = 6,
                StepName = "Automatisering & Extra's",
                DisplayOrder = 2,
                IsRequired = false,
                DefaultValue = "none",
                DependsOn = new List<string> { "driveType", "controlType" },
                Metadata = JsonDocument.Parse("""{"label":"Afstandsbediening","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductParameter
            {
                Id = NextId(),
                ProductTypeId = productTypeId,
                Code = "integration",
                Name = "Domotica-integratie",
                DataType = "select",
                Unit = null,
                StepNumber = 6,
                StepName = "Automatisering & Extra's",
                DisplayOrder = 3,
                IsRequired = false,
                DefaultValue = "none",
                DependsOn = new List<string> { "controlType" },
                Metadata = JsonDocument.Parse("""{"label":"Domotica-integratie","visibleWhen":"driveType == 'motor'"}"""),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }
}
