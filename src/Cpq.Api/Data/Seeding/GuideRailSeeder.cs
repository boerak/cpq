using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class GuideRailSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<GuideRail>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var guideRails = new[]
        {
            new GuideRail
            {
                Id = new Guid("60000000-0000-0000-0000-000000000001"),
                Code = "GR-STD-20",
                Name = "Standard 20mm",
                Type = "standard",
                MaterialCode = "ALU",
                WidthMm = 20m,
                DepthMm = 30m,
                MaxHeightMm = 3500,
                WeightPerMeterKg = 0.45m,
                BracketSpacingMm = 600,
                CompatibleProfiles = new List<string> { "ALU-39", "PVC-37", "ALU-42" },
                WindClass = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new GuideRail
            {
                Id = new Guid("60000000-0000-0000-0000-000000000002"),
                Code = "GR-WIND-25",
                Name = "Wind Resistant 25mm",
                Type = "wind_resistant",
                MaterialCode = "ALU",
                WidthMm = 25m,
                DepthMm = 35m,
                MaxHeightMm = 4000,
                WeightPerMeterKg = 0.65m,
                BracketSpacingMm = 500,
                CompatibleProfiles = new List<string> { "ALU-39", "ALU-INS-55", "ALU-42" },
                WindClass = 3,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new GuideRail
            {
                Id = new Guid("60000000-0000-0000-0000-000000000003"),
                Code = "GR-ZIP-30",
                Name = "Zip Guide 30mm",
                Type = "zip",
                MaterialCode = "ALU",
                WidthMm = 30m,
                DepthMm = 40m,
                MaxHeightMm = 3000,
                WeightPerMeterKg = 0.85m,
                BracketSpacingMm = 400,
                CompatibleProfiles = new List<string> { "ALU-INS-55" },
                WindClass = 5,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(guideRails);
        await context.SaveChangesAsync();
    }
}
