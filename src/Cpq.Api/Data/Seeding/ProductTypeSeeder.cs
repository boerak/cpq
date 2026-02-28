using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductTypeSeeder
{
    public static readonly Guid RollerShutterStandardId = new Guid("90000000-0000-0000-0000-000000000001");
    public static readonly Guid RollerShutterInsulatedId = new Guid("90000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<ProductType>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var productTypes = new[]
        {
            new ProductType
            {
                Id = RollerShutterStandardId,
                FamilyId = ProductFamilySeeder.RollerShutterFamilyId,
                Code = "roller_shutter_standard",
                Name = "Standaard Rolluik",
                Variant = "standard",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductType
            {
                Id = RollerShutterInsulatedId,
                FamilyId = ProductFamilySeeder.RollerShutterFamilyId,
                Code = "roller_shutter_insulated",
                Name = "Geïsoleerd Rolluik",
                Variant = "insulated",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(productTypes);
        await context.SaveChangesAsync();
    }
}
