using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ColorSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Color>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var colors = new[]
        {
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000001"),
                Code = "RAL9010",
                Name = "Zuiver wit",
                ColorSystem = "RAL",
                HexValue = "#FFFFFF",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000002"),
                Code = "RAL9001",
                Name = "Crèmewit",
                ColorSystem = "RAL",
                HexValue = "#FDF4E3",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000003"),
                Code = "RAL9016",
                Name = "Verkeerswit",
                ColorSystem = "RAL",
                HexValue = "#F6F6F6",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000004"),
                Code = "RAL7016",
                Name = "Antracietgrijs",
                ColorSystem = "RAL",
                HexValue = "#293133",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000005"),
                Code = "RAL7035",
                Name = "Lichtgrijs",
                ColorSystem = "RAL",
                HexValue = "#CBD0CC",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000006"),
                Code = "RAL7021",
                Name = "Zwartgrijs",
                ColorSystem = "RAL",
                HexValue = "#2F3234",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000007"),
                Code = "RAL8014",
                Name = "Sepiabruin",
                ColorSystem = "RAL",
                HexValue = "#49392D",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000008"),
                Code = "RAL8003",
                Name = "Leembruin",
                ColorSystem = "RAL",
                HexValue = "#734222",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000009"),
                Code = "RAL8017",
                Name = "Chocoladebruin",
                ColorSystem = "RAL",
                HexValue = "#442F29",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000010"),
                Code = "RAL6005",
                Name = "Mosgroen",
                ColorSystem = "RAL",
                HexValue = "#0F4336",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000011"),
                Code = "RAL5010",
                Name = "Gentiaanblauw",
                ColorSystem = "RAL",
                HexValue = "#004F7C",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000012"),
                Code = "RAL3000",
                Name = "Vuurrood",
                ColorSystem = "RAL",
                HexValue = "#A72920",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000013"),
                Code = "RAL1015",
                Name = "Licht ivoor",
                ColorSystem = "RAL",
                HexValue = "#E6D2B5",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000014"),
                Code = "RAL6009",
                Name = "Dennegroen",
                ColorSystem = "RAL",
                HexValue = "#27352A",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000015"),
                Code = "DB703",
                Name = "Grijsaluminium",
                ColorSystem = "DB",
                HexValue = "#6A6E6E",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000016"),
                Code = "RAL1013",
                Name = "Parelwit",
                ColorSystem = "RAL",
                HexValue = "#E3D9C6",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000017"),
                Code = "RAL7022",
                Name = "Ombergrijs",
                ColorSystem = "RAL",
                HexValue = "#4B4D46",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000018"),
                Code = "RAL7039",
                Name = "Kwartsgrijs",
                ColorSystem = "RAL",
                HexValue = "#6B695F",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000019"),
                Code = "RAL8019",
                Name = "Grijsbruin",
                ColorSystem = "RAL",
                HexValue = "#3B3332",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000020"),
                Code = "RAL9005",
                Name = "Gitzwart",
                ColorSystem = "RAL",
                HexValue = "#0E0E10",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(colors);
        await context.SaveChangesAsync();
    }
}
