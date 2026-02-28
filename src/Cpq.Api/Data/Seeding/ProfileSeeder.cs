using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProfileSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Profile>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var profiles = new[]
        {
            new Profile
            {
                Id = new Guid("30000000-0000-0000-0000-000000000001"),
                Code = "ALU-39",
                Name = "Aluminium profiel 39mm",
                MaterialCode = "ALU",
                HeightMm = 39m,
                ThicknessMm = 8.5m,
                WeightPerMeterKg = 1.85m,
                MinWidthMm = 400,
                MaxWidthMm = 3000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Profile
            {
                Id = new Guid("30000000-0000-0000-0000-000000000002"),
                Code = "PVC-37",
                Name = "PVC profiel 37mm",
                MaterialCode = "PVC",
                HeightMm = 37m,
                ThicknessMm = 8.0m,
                WeightPerMeterKg = 1.45m,
                MinWidthMm = 400,
                MaxWidthMm = 2500,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Profile
            {
                Id = new Guid("30000000-0000-0000-0000-000000000003"),
                Code = "ALU-INS-55",
                Name = "Geïsoleerd aluminium profiel 55mm",
                MaterialCode = "ALU_INSULATED",
                HeightMm = 55m,
                ThicknessMm = 14m,
                WeightPerMeterKg = 3.2m,
                MinWidthMm = 500,
                MaxWidthMm = 3000,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Profile
            {
                Id = new Guid("30000000-0000-0000-0000-000000000004"),
                Code = "ALU-42",
                Name = "Aluminium profiel 42mm",
                MaterialCode = "ALU",
                HeightMm = 42m,
                ThicknessMm = 9.5m,
                WeightPerMeterKg = 2.1m,
                MinWidthMm = 400,
                MaxWidthMm = 3200,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(profiles);
        await context.SaveChangesAsync();
    }
}
