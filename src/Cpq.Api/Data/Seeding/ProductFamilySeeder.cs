using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductFamilySeeder
{
    public static readonly Guid RollerShutterFamilyId = new Guid("80000000-0000-0000-0000-000000000001");
    public static readonly Guid ScreenFamilyId = new Guid("80000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<ProductFamily>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var families = new[]
        {
            new ProductFamily
            {
                Id = RollerShutterFamilyId,
                Code = "roller_shutter",
                Name = "Rolluik",
                Description = "Externe rolluiken voor ramen en deuren",
                RulePrefix = "families/roller-shutter",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductFamily
            {
                Id = ScreenFamilyId,
                Code = "screen",
                Name = "Zonwering Screen",
                Description = "Zonwerende screens voor gevels en terrassen",
                RulePrefix = "families/screen",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(families);
        await context.SaveChangesAsync();
    }
}
