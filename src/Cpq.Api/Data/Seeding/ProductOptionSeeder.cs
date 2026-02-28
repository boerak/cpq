using System.Text.Json;
using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class ProductOptionSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<ProductOption>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;
        var options = new List<ProductOption>();

        options.AddRange(BuildStandardOptions(now));
        options.AddRange(BuildInsulatedOptions(now));

        await dbSet.AddRangeAsync(options);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // roller_shutter_standard
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductOption> BuildStandardOptions(DateTimeOffset now)
    {
        var ptId = ProductTypeSeeder.RollerShutterStandardId;
        var options = new List<ProductOption>();

        // material
        options.AddRange(new[]
        {
            Opt(ptId, "material", "ALU",  "Aluminium",  1, now),
            Opt(ptId, "material", "PVC",  "PVC",         2, now)
        });

        // profile
        options.AddRange(new[]
        {
            Opt(ptId, "profile", "ALU-39", "Aluminium profiel 39mm",           1, now),
            Opt(ptId, "profile", "PVC-37", "PVC profiel 37mm",                 2, now),
            Opt(ptId, "profile", "ALU-42", "Aluminium profiel 42mm",           3, now)
        });

        // color — all 20
        options.AddRange(AllColors(ptId, now));

        // boxType
        options.AddRange(new[]
        {
            Opt(ptId, "boxType", "BOX-SM-165", "Surface Mount 165", 1, now),
            Opt(ptId, "boxType", "BOX-SM-180", "Surface Mount 180", 2, now),
            Opt(ptId, "boxType", "BOX-BI-200", "Built-In 200",      3, now)
        });

        // driveType
        options.AddRange(new[]
        {
            Opt(ptId, "driveType", "manual_strap", "Bandoproller",  1, now),
            Opt(ptId, "driveType", "manual_crank", "Lierhandwiel",  2, now),
            Opt(ptId, "driveType", "motor",         "Motor",         3, now)
        });

        // motorBrand
        options.AddRange(new[]
        {
            Opt(ptId, "motorBrand", "somfy",  "Somfy",  1, now),
            Opt(ptId, "motorBrand", "becker", "Becker", 2, now)
        });

        // controlType
        options.AddRange(new[]
        {
            Opt(ptId, "controlType", "io_homecontrol", "IO Homecontrol",      1, now),
            Opt(ptId, "controlType", "rts",            "RTS",                  2, now),
            Opt(ptId, "controlType", "rts_timer",      "RTS Timer",            3, now),
            Opt(ptId, "controlType", "wired_switch",   "Bedrade schakelaar",   4, now),
            Opt(ptId, "controlType", "wired_timer",    "Bedrade timer",        5, now)
        });

        // guideType
        options.AddRange(new[]
        {
            Opt(ptId, "guideType", "GR-STD-20",  "Standaard", 1, now),
            Opt(ptId, "guideType", "GR-WIND-25", "Windvast",  2, now)
        });

        // accessories
        options.AddRange(new[]
        {
            Opt(ptId, "accessories", "ACC-INSECT",      "Insectenhor",                    1, now),
            Opt(ptId, "accessories", "ACC-LOCK-STD",    "Standaard slot",                 2, now),
            Opt(ptId, "accessories", "ACC-LOCK-SEC",    "Veiligheidsslot",                3, now),
            Opt(ptId, "accessories", "ACC-SENSOR-WIND", "Windsensor",                     4, now),
            Opt(ptId, "accessories", "ACC-SENSOR-SUN",  "Zonsensor",                      5, now),
            Opt(ptId, "accessories", "ACC-REMOTE-1CH",  "Afstandsbediening 1-kanaal",     6, now),
            Opt(ptId, "accessories", "ACC-REMOTE-5CH",  "Afstandsbediening 5-kanaal",     7, now)
        });

        return options;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // roller_shutter_insulated
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductOption> BuildInsulatedOptions(DateTimeOffset now)
    {
        var ptId = ProductTypeSeeder.RollerShutterInsulatedId;
        var options = new List<ProductOption>();

        // material — insulated only
        options.Add(Opt(ptId, "material", "ALU_INSULATED", "Geïsoleerd aluminium", 1, now));

        // profile — insulated only
        options.Add(Opt(ptId, "profile", "ALU-INS-55", "Geïsoleerd aluminium profiel 55mm", 1, now));

        // color — 8 colors matching ALU_INSULATED material_colors
        options.AddRange(new[]
        {
            OptColor(ptId, "RAL9010", "Zuiver wit",      1, now),
            OptColor(ptId, "RAL9016", "Verkeerswit",     2, now),
            OptColor(ptId, "RAL7016", "Antracietgrijs",  3, now),
            OptColor(ptId, "RAL7035", "Lichtgrijs",      4, now),
            OptColor(ptId, "RAL7021", "Zwartgrijs",      5, now),
            OptColor(ptId, "DB703",   "Grijsaluminium",  6, now),
            OptColor(ptId, "RAL8017", "Chocoladebruin",  7, now),
            OptColor(ptId, "RAL9005", "Gitzwart",        8, now)
        });

        // boxType
        options.AddRange(new[]
        {
            Opt(ptId, "boxType", "BOX-SM-180", "Surface Mount 180", 1, now),
            Opt(ptId, "boxType", "BOX-BI-200", "Built-In 200",      2, now),
            Opt(ptId, "boxType", "BOX-CC-180", "Concealed 180",     3, now)
        });

        // driveType — motor only
        options.Add(Opt(ptId, "driveType", "motor", "Motor", 1, now));

        // motorBrand — Somfy only
        options.Add(Opt(ptId, "motorBrand", "somfy", "Somfy", 1, now));

        // controlType — IO only
        options.Add(Opt(ptId, "controlType", "io_homecontrol", "IO Homecontrol", 1, now));

        // guideType
        options.AddRange(new[]
        {
            Opt(ptId, "guideType", "GR-WIND-25", "Windvast", 1, now),
            Opt(ptId, "guideType", "GR-ZIP-30",  "Zip",      2, now)
        });

        // accessories
        options.AddRange(new[]
        {
            Opt(ptId, "accessories", "ACC-SENSOR-WIND", "Windsensor",                 1, now),
            Opt(ptId, "accessories", "ACC-SENSOR-SUN",  "Zonsensor",                  2, now),
            Opt(ptId, "accessories", "ACC-REMOTE-1CH",  "Afstandsbediening 1-kanaal", 3, now),
            Opt(ptId, "accessories", "ACC-REMOTE-5CH",  "Afstandsbediening 5-kanaal", 4, now)
        });

        return options;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static ProductOption Opt(
        Guid productTypeId,
        string parameterCode,
        string code,
        string displayName,
        int displayOrder,
        DateTimeOffset now) => new ProductOption
    {
        Id = Guid.NewGuid(),
        ProductTypeId = productTypeId,
        ParameterCode = parameterCode,
        Code = code,
        DisplayName = displayName,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now
    };

    private static ProductOption OptColor(
        Guid productTypeId,
        string colorCode,
        string colorName,
        int displayOrder,
        DateTimeOffset now) => new ProductOption
    {
        Id = Guid.NewGuid(),
        ProductTypeId = productTypeId,
        ParameterCode = "color",
        Code = colorCode,
        DisplayName = colorName,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedAt = now,
        UpdatedAt = now
    };

    /// <summary>Returns all 20 color options for the given product type.</summary>
    private static IEnumerable<ProductOption> AllColors(Guid productTypeId, DateTimeOffset now)
    {
        var colors = new (string Code, string Name)[]
        {
            ("RAL9010", "Zuiver wit"),
            ("RAL9001", "Crèmewit"),
            ("RAL9016", "Verkeerswit"),
            ("RAL7016", "Antracietgrijs"),
            ("RAL7035", "Lichtgrijs"),
            ("RAL7021", "Zwartgrijs"),
            ("RAL8014", "Sepiabruin"),
            ("RAL8003", "Leembruin"),
            ("RAL8017", "Chocoladebruin"),
            ("RAL6005", "Mosgroen"),
            ("RAL5010", "Gentiaanblauw"),
            ("RAL3000", "Vuurrood"),
            ("RAL1015", "Licht ivoor"),
            ("RAL6009", "Dennegroen"),
            ("DB703",   "Grijsaluminium"),
            ("RAL1013", "Parelwit"),
            ("RAL7022", "Ombergrijs"),
            ("RAL7039", "Kwartsgrijs"),
            ("RAL8019", "Grijsbruin"),
            ("RAL9005", "Gitzwart")
        };

        return colors.Select((c, i) => OptColor(productTypeId, c.Code, c.Name, i + 1, now));
    }
}
