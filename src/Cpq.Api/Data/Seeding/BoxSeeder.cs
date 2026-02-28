using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class BoxSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Box>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var boxes = new[]
        {
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000001"),
                Code = "BOX-SM-165",
                Name = "Surface Mount 165",
                Type = "surface_mount",
                InnerDiameterMm = 165,
                OuterHeightMm = 185,
                CompatibleMaterials = new List<string> { "ALU", "PVC", "ALU_INSULATED" },
                MaxWidthMm = 4000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000002"),
                Code = "BOX-SM-180",
                Name = "Surface Mount 180",
                Type = "surface_mount",
                InnerDiameterMm = 180,
                OuterHeightMm = 200,
                CompatibleMaterials = new List<string> { "ALU", "PVC", "ALU_INSULATED" },
                MaxWidthMm = 4000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000003"),
                Code = "BOX-BI-200",
                Name = "Built-In 200",
                Type = "built_in",
                InnerDiameterMm = 200,
                OuterHeightMm = 220,
                CompatibleMaterials = new List<string> { "ALU", "ALU_INSULATED" },
                MaxWidthMm = 3500,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000004"),
                Code = "BOX-CC-180",
                Name = "Concealed 180",
                Type = "concealed",
                InnerDiameterMm = 180,
                OuterHeightMm = 200,
                CompatibleMaterials = new List<string> { "ALU", "ALU_INSULATED" },
                MaxWidthMm = 3000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Screen housings ──────────────────────────────────────────
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000005"),
                Code = "SCR-HSNG-SQ-100",
                Name = "Screenkasten vierkant 100mm",
                Type = "screen_square",
                InnerDiameterMm = 100,
                OuterHeightMm = 110,
                CompatibleMaterials = new List<string> { "SCREEN_FABRIC" },
                MaxWidthMm = 3000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000006"),
                Code = "SCR-HSNG-RD-100",
                Name = "Screenkasten rond 100mm",
                Type = "screen_round",
                InnerDiameterMm = 100,
                OuterHeightMm = 105,
                CompatibleMaterials = new List<string> { "SCREEN_FABRIC" },
                MaxWidthMm = 3000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000007"),
                Code = "SCR-HSNG-SQ-120",
                Name = "Screenkasten vierkant 120mm",
                Type = "screen_square",
                InnerDiameterMm = 120,
                OuterHeightMm = 130,
                CompatibleMaterials = new List<string> { "SCREEN_FABRIC" },
                MaxWidthMm = 5500,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000008"),
                Code = "SCR-HSNG-RD-120",
                Name = "Screenkasten rond 120mm",
                Type = "screen_round",
                InnerDiameterMm = 120,
                OuterHeightMm = 125,
                CompatibleMaterials = new List<string> { "SCREEN_FABRIC" },
                MaxWidthMm = 5500,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Box
            {
                Id = new Guid("50000000-0000-0000-0000-000000000009"),
                Code = "SCR-HSNG-CONCEALED",
                Name = "Screenkasten inbouw",
                Type = "screen_concealed",
                InnerDiameterMm = 120,
                OuterHeightMm = 130,
                CompatibleMaterials = new List<string> { "SCREEN_FABRIC" },
                MaxWidthMm = 5500,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(boxes);
        await context.SaveChangesAsync();
    }
}
