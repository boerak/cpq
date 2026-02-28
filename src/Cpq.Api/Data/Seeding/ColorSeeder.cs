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
            },

            // ── Screen fabric colors ─────────────────────────────────────
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000021"),
                Code = "SCR-WHITE",
                Name = "Wit",
                ColorSystem = "FABRIC",
                HexValue = "#FFFFFF",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000022"),
                Code = "SCR-LINEN",
                Name = "Linnen",
                ColorSystem = "FABRIC",
                HexValue = "#E8DCC8",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000023"),
                Code = "SCR-SAND",
                Name = "Zand",
                ColorSystem = "FABRIC",
                HexValue = "#C2B280",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000024"),
                Code = "SCR-PEARL-GREY",
                Name = "Parelgrijs",
                ColorSystem = "FABRIC",
                HexValue = "#C0C0C0",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000025"),
                Code = "SCR-SILVER",
                Name = "Zilver",
                ColorSystem = "FABRIC",
                HexValue = "#A8A9AD",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000026"),
                Code = "SCR-ALU-GREY",
                Name = "Aluminium grijs",
                ColorSystem = "FABRIC",
                HexValue = "#8C8C8C",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000027"),
                Code = "SCR-CHARCOAL",
                Name = "Antraciet",
                ColorSystem = "FABRIC",
                HexValue = "#36454F",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000028"),
                Code = "SCR-BLACK",
                Name = "Zwart",
                ColorSystem = "FABRIC",
                HexValue = "#1A1A1A",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000029"),
                Code = "SCR-BRONZE",
                Name = "Brons",
                ColorSystem = "FABRIC",
                HexValue = "#665D1E",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000030"),
                Code = "SCR-BROWN",
                Name = "Bruin",
                ColorSystem = "FABRIC",
                HexValue = "#5C4033",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000031"),
                Code = "SCR-DARK-GREY",
                Name = "Donkergrijs",
                ColorSystem = "FABRIC",
                HexValue = "#4A4A4A",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000032"),
                Code = "SCR-CREAM",
                Name = "Crème",
                ColorSystem = "FABRIC",
                HexValue = "#FFFDD0",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000033"),
                Code = "SCR-BEIGE",
                Name = "Beige",
                ColorSystem = "FABRIC",
                HexValue = "#D4C5A9",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000034"),
                Code = "SCR-LIGHT-GREY",
                Name = "Lichtgrijs",
                ColorSystem = "FABRIC",
                HexValue = "#D3D3D3",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000035"),
                Code = "SCR-TAUPE",
                Name = "Taupe",
                ColorSystem = "FABRIC",
                HexValue = "#8B7D6B",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000036"),
                Code = "SCR-STEEL",
                Name = "Staalgrijs",
                ColorSystem = "FABRIC",
                HexValue = "#71797E",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000037"),
                Code = "SCR-MOSS",
                Name = "Mosgroen",
                ColorSystem = "FABRIC",
                HexValue = "#8A9A5B",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000038"),
                Code = "SCR-NAVY",
                Name = "Marineblauw",
                ColorSystem = "FABRIC",
                HexValue = "#000080",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000039"),
                Code = "SCR-COFFEE",
                Name = "Koffie",
                ColorSystem = "FABRIC",
                HexValue = "#6F4E37",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000040"),
                Code = "SCR-TERRACOTTA",
                Name = "Terracotta",
                ColorSystem = "FABRIC",
                HexValue = "#CC4E3C",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000041"),
                Code = "SCR-OFF-WHITE",
                Name = "Gebroken wit",
                ColorSystem = "FABRIC",
                HexValue = "#F5F0E1",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000042"),
                Code = "SCR-GRAPHITE",
                Name = "Grafiet",
                ColorSystem = "FABRIC",
                HexValue = "#383838",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000043"),
                Code = "SCR-IVORY",
                Name = "Ivoor",
                ColorSystem = "FABRIC",
                HexValue = "#FFFFF0",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000044"),
                Code = "SCR-STONE",
                Name = "Steengrijs",
                ColorSystem = "FABRIC",
                HexValue = "#928E85",
                IsStandard = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Color
            {
                Id = new Guid("20000000-0000-0000-0000-000000000045"),
                Code = "SCR-MIDNIGHT",
                Name = "Middernacht",
                ColorSystem = "FABRIC",
                HexValue = "#191970",
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
