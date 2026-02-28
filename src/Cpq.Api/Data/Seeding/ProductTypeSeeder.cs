using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductTypeSeeder
{
    public static readonly Guid RollerShutterStandardId = new Guid("90000000-0000-0000-0000-000000000001");
    public static readonly Guid RollerShutterInsulatedId = new Guid("90000000-0000-0000-0000-000000000002");
    public static readonly Guid ScreenZipId = new Guid("90000000-0000-0000-0000-000000000003");
    public static readonly Guid ScreenStandardId = new Guid("90000000-0000-0000-0000-000000000004");
    public static readonly Guid ScreenFixId = new Guid("90000000-0000-0000-0000-000000000005");

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
            },

            // ── Screen product types ─────────────────────────────────────
            new ProductType
            {
                Id = ScreenZipId,
                FamilyId = ProductFamilySeeder.ScreenFamilyId,
                Code = "screen_zip",
                Name = "ZIP Screen",
                Variant = "zip",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductType
            {
                Id = ScreenStandardId,
                FamilyId = ProductFamilySeeder.ScreenFamilyId,
                Code = "screen_standard",
                Name = "Standaard Screen",
                Variant = "standard",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new ProductType
            {
                Id = ScreenFixId,
                FamilyId = ProductFamilySeeder.ScreenFamilyId,
                Code = "screen_fix",
                Name = "Fix Screen",
                Variant = "fix",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(productTypes);
        await context.SaveChangesAsync();
    }
}
