using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class MaterialSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Material>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var materials = new[]
        {
            new Material
            {
                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                Code = "ALU",
                Name = "Aluminium",
                DensityKgPerM3 = 2700m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Material
            {
                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                Code = "PVC",
                Name = "PVC",
                DensityKgPerM3 = 1400m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Material
            {
                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                Code = "ALU_INSULATED",
                Name = "Geïsoleerd aluminium",
                DensityKgPerM3 = 2500m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Material
            {
                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                Code = "SCREEN_FABRIC",
                Name = "Screendoek",
                DensityKgPerM3 = 350m,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(materials);
        await context.SaveChangesAsync();
    }
}
