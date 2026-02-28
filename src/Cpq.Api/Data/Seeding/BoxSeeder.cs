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
            }
        };

        await dbSet.AddRangeAsync(boxes);
        await context.SaveChangesAsync();
    }
}
