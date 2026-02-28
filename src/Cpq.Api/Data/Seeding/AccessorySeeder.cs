using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class AccessorySeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Accessory>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var accessories = new[]
        {
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000001"),
                Code = "ACC-INSECT",
                Name = "Insectenhor",
                Category = "insect_screen",
                RequiresMotor = false,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000002"),
                Code = "ACC-LOCK-STD",
                Name = "Standaard slot",
                Category = "security",
                RequiresMotor = false,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000003"),
                Code = "ACC-LOCK-SEC",
                Name = "Veiligheidsslot",
                Category = "security",
                RequiresMotor = false,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000004"),
                Code = "ACC-SENSOR-WIND",
                Name = "Windsensor",
                Category = "automation",
                RequiresMotor = true,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000005"),
                Code = "ACC-SENSOR-SUN",
                Name = "Zonsensor",
                Category = "automation",
                RequiresMotor = true,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000006"),
                Code = "ACC-REMOTE-1CH",
                Name = "Afstandsbediening 1-kanaal",
                Category = "remote_control",
                RequiresMotor = true,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Accessory
            {
                Id = new Guid("70000000-0000-0000-0000-000000000007"),
                Code = "ACC-REMOTE-5CH",
                Name = "Afstandsbediening 5-kanaal",
                Category = "remote_control",
                RequiresMotor = true,
                CompatibleFamilies = new List<string> { "roller_shutter" },
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(accessories);
        await context.SaveChangesAsync();
    }
}
