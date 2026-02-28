using System.Text.Json;
using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class PartsCatalogSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        await SeedPartsAsync(context);
        await SeedSkuMappingsAsync(context);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Parts
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedPartsAsync(DbContext context)
    {
        var dbSet = context.Set<Part>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var parts = new[]
        {
            // ── Slats ──────────────────────────────────────────────────────
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000001"),
                Sku = "SLAT-ALU-39",
                Name = "Aluminium lamel 39mm",
                Category = "slat",
                Unit = "pcs",
                IsCuttable = true,
                WeightKg = 0.185m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000002"),
                Sku = "SLAT-PVC-37",
                Name = "PVC lamel 37mm",
                Category = "slat",
                Unit = "pcs",
                IsCuttable = true,
                WeightKg = 0.145m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000003"),
                Sku = "SLAT-ALU-INS-55",
                Name = "Geïsoleerde lamel 55mm",
                Category = "slat",
                Unit = "pcs",
                IsCuttable = true,
                WeightKg = 0.32m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000004"),
                Sku = "SLAT-ALU-42",
                Name = "Aluminium lamel 42mm",
                Category = "slat",
                Unit = "pcs",
                IsCuttable = true,
                WeightKg = 0.21m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Motors ─────────────────────────────────────────────────────
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000005"),
                Sku = "MOTOR-SOMFY-IO-15",
                Name = "Somfy Ilmo 2 IO 15/17",
                Category = "motor",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000006"),
                Sku = "MOTOR-SOMFY-IO-25",
                Name = "Somfy Oximo IO 25/17",
                Category = "motor",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000007"),
                Sku = "MOTOR-SOMFY-IO-40",
                Name = "Somfy Oximo IO 40/17",
                Category = "motor",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000008"),
                Sku = "MOTOR-SOMFY-RTS-20",
                Name = "Somfy Ilmo 2 RTS 20/17",
                Category = "motor",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000009"),
                Sku = "MOTOR-BECKER-R12",
                Name = "Becker R12-17C",
                Category = "motor",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Boxes ──────────────────────────────────────────────────────
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000010"),
                Sku = "BOX-SM-165",
                Name = "Kast surface mount 165mm",
                Category = "box",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000011"),
                Sku = "BOX-SM-180",
                Name = "Kast surface mount 180mm",
                Category = "box",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000012"),
                Sku = "BOX-BI-200",
                Name = "Kast inbouw 200mm",
                Category = "box",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000013"),
                Sku = "BOX-CC-180",
                Name = "Kast concealed 180mm",
                Category = "box",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Guide rails ────────────────────────────────────────────────
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000014"),
                Sku = "GR-STD-20-RAIL",
                Name = "Geleider standard 20mm",
                Category = "guide",
                Unit = "pcs",
                IsCuttable = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000015"),
                Sku = "GR-WIND-25-RAIL",
                Name = "Geleider windvast 25mm",
                Category = "guide",
                Unit = "pcs",
                IsCuttable = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000016"),
                Sku = "GR-ZIP-30-RAIL",
                Name = "Geleider zip 30mm",
                Category = "guide",
                Unit = "pcs",
                IsCuttable = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Hardware ───────────────────────────────────────────────────
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000017"),
                Sku = "BRACKET-STD",
                Name = "Standaard beugel",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000018"),
                Sku = "BRACKET-WIND",
                Name = "Windvaste beugel",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000019"),
                Sku = "ENDCAP-SM",
                Name = "Eindkap surface mount",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000020"),
                Sku = "ENDCAP-BI",
                Name = "Eindkap inbouw",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000021"),
                Sku = "AXLE-60",
                Name = "Askoker 60mm",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000022"),
                Sku = "STRAP-14",
                Name = "Band 14mm",
                Category = "hardware",
                Unit = "set",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000023"),
                Sku = "CRANK-STD",
                Name = "Lierhandwiel standaard",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000024"),
                Sku = "SCREW-6X60",
                Name = "Houtschroef 6x60mm",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = new Guid("C0000000-0000-0000-0000-000000000025"),
                Sku = "PLUG-8MM",
                Name = "Muurplug 8mm",
                Category = "hardware",
                Unit = "pcs",
                IsCuttable = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(parts);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SKU Mappings
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedSkuMappingsAsync(DbContext context)
    {
        var dbSet = context.Set<SkuMapping>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;
        const string family = "roller_shutter";

        var mappings = new[]
        {
            // ── Slats ──────────────────────────────────────────────────────
            Mapping(family, "slat",  """{"profile":"ALU-39"}""",      "SLAT-ALU-39",      0, now),
            Mapping(family, "slat",  """{"profile":"PVC-37"}""",      "SLAT-PVC-37",      0, now),
            Mapping(family, "slat",  """{"profile":"ALU-INS-55"}""",  "SLAT-ALU-INS-55",  0, now),
            Mapping(family, "slat",  """{"profile":"ALU-42"}""",      "SLAT-ALU-42",      0, now),

            // ── Guide rails ────────────────────────────────────────────────
            Mapping(family, "guide", """{"guideType":"GR-STD-20"}""",  "GR-STD-20-RAIL",  0, now),
            Mapping(family, "guide", """{"guideType":"GR-WIND-25"}""", "GR-WIND-25-RAIL", 0, now),
            Mapping(family, "guide", """{"guideType":"GR-ZIP-30"}""",  "GR-ZIP-30-RAIL",  0, now),

            // ── Motors ─────────────────────────────────────────────────────
            Mapping(family, "motor", """{"motorCode":"SOMFY-IO-15"}""",  "MOTOR-SOMFY-IO-15",  0, now),
            Mapping(family, "motor", """{"motorCode":"SOMFY-IO-25"}""",  "MOTOR-SOMFY-IO-25",  0, now),
            Mapping(family, "motor", """{"motorCode":"SOMFY-IO-40"}""",  "MOTOR-SOMFY-IO-40",  0, now),
            Mapping(family, "motor", """{"motorCode":"SOMFY-RTS-20"}""", "MOTOR-SOMFY-RTS-20", 0, now),
            Mapping(family, "motor", """{"motorCode":"BECKER-R12"}""",   "MOTOR-BECKER-R12",   0, now),

            // ── Boxes ──────────────────────────────────────────────────────
            Mapping(family, "box", """{"boxType":"BOX-SM-165"}""", "BOX-SM-165", 0, now),
            Mapping(family, "box", """{"boxType":"BOX-SM-180"}""", "BOX-SM-180", 0, now),
            Mapping(family, "box", """{"boxType":"BOX-BI-200"}""", "BOX-BI-200", 0, now),
            Mapping(family, "box", """{"boxType":"BOX-CC-180"}""", "BOX-CC-180", 0, now),

            // ── Drive-type hardware ────────────────────────────────────────
            Mapping(family, "hardware", """{"driveType":"manual_strap"}""", "STRAP-14",   0, now),
            Mapping(family, "hardware", """{"driveType":"manual_crank"}""", "CRANK-STD",  0, now),

            // ── Axle (always present) ──────────────────────────────────────
            Mapping(family, "hardware", """{"component":"axle"}""", "AXLE-60", 0, now),

            // ── Fasteners (always present) ─────────────────────────────────
            Mapping(family, "hardware", """{"component":"screw"}""", "SCREW-6X60", 0, now),
            Mapping(family, "hardware", """{"component":"plug"}""",  "PLUG-8MM",   0, now),

            // ── Brackets by guide type ─────────────────────────────────────
            Mapping(family, "hardware", """{"guideType":"GR-STD-20","component":"bracket"}""",  "BRACKET-STD",  1, now),
            Mapping(family, "hardware", """{"guideType":"GR-WIND-25","component":"bracket"}""", "BRACKET-WIND", 1, now),
            Mapping(family, "hardware", """{"guideType":"GR-ZIP-30","component":"bracket"}""",  "BRACKET-WIND", 1, now),

            // ── Endcaps by box type ────────────────────────────────────────
            Mapping(family, "hardware", """{"boxType":"BOX-SM-165","component":"endcap"}""", "ENDCAP-SM", 1, now),
            Mapping(family, "hardware", """{"boxType":"BOX-SM-180","component":"endcap"}""", "ENDCAP-SM", 1, now),
            Mapping(family, "hardware", """{"boxType":"BOX-BI-200","component":"endcap"}""", "ENDCAP-BI", 1, now),
            Mapping(family, "hardware", """{"boxType":"BOX-CC-180","component":"endcap"}""", "ENDCAP-BI", 1, now)
        };

        await dbSet.AddRangeAsync(mappings);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────
    private static SkuMapping Mapping(
        string productFamilyCode,
        string category,
        string matchCriteriaJson,
        string sku,
        int priority,
        DateTimeOffset now) => new SkuMapping
    {
        Id = Guid.NewGuid(),
        ProductFamilyCode = productFamilyCode,
        Category = category,
        MatchCriteria = JsonDocument.Parse(matchCriteriaJson),
        Sku = sku,
        Priority = priority,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now
    };
}
