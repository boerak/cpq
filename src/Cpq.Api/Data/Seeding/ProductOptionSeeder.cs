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
        options.AddRange(BuildScreenOptions(ProductTypeSeeder.ScreenZipId, now));
        options.AddRange(BuildScreenOptions(ProductTypeSeeder.ScreenStandardId, now));
        options.AddRange(BuildScreenOptions(ProductTypeSeeder.ScreenFixId, now));

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

    // ─────────────────────────────────────────────────────────────────────────
    // Screen options (shared structure, rules engine handles variant filtering)
    // ─────────────────────────────────────────────────────────────────────────
    private static IEnumerable<ProductOption> BuildScreenOptions(Guid ptId, DateTimeOffset now)
    {
        var options = new List<ProductOption>();

        // fabricType (8 options)
        options.AddRange(new[]
        {
            Opt(ptId, "fabricType", "soltis_86",        "Soltis 86",         1, now),
            Opt(ptId, "fabricType", "soltis_92",        "Soltis 92",         2, now),
            Opt(ptId, "fabricType", "soltis_96",        "Soltis 96",         3, now),
            Opt(ptId, "fabricType", "serge_600",        "Serge 600",         4, now),
            Opt(ptId, "fabricType", "sunworker_open",   "Sunworker Open",    5, now),
            Opt(ptId, "fabricType", "sunworker_classic", "Sunworker Classic", 6, now),
            Opt(ptId, "fabricType", "copaco_300",       "Copaco 300",        7, now),
            Opt(ptId, "fabricType", "copaco_350",       "Copaco 350",        8, now)
        });

        // fabricColor (25 colors — rules engine filters per fabric type)
        options.AddRange(AllFabricColors(ptId, now));

        // meshOpenness (5 options — rules engine filters per fabric type)
        options.AddRange(new[]
        {
            Opt(ptId, "meshOpenness", "1",  "1%",  1, now),
            Opt(ptId, "meshOpenness", "3",  "3%",  2, now),
            Opt(ptId, "meshOpenness", "5",  "5%",  3, now),
            Opt(ptId, "meshOpenness", "10", "10%", 4, now),
            Opt(ptId, "meshOpenness", "14", "14%", 5, now)
        });

        // frameColor (12 RAL colors for frame)
        options.AddRange(new[]
        {
            Opt(ptId, "frameColor", "RAL9010", "Zuiver wit",       1, now),
            Opt(ptId, "frameColor", "RAL9001", "Crèmewit",         2, now),
            Opt(ptId, "frameColor", "RAL9016", "Verkeerswit",       3, now),
            Opt(ptId, "frameColor", "RAL7016", "Antracietgrijs",    4, now),
            Opt(ptId, "frameColor", "RAL7035", "Lichtgrijs",        5, now),
            Opt(ptId, "frameColor", "RAL7021", "Zwartgrijs",        6, now),
            Opt(ptId, "frameColor", "RAL8014", "Sepiabruin",        7, now),
            Opt(ptId, "frameColor", "RAL8017", "Chocoladebruin",    8, now),
            Opt(ptId, "frameColor", "RAL9005", "Gitzwart",          9, now),
            Opt(ptId, "frameColor", "DB703",   "Grijsaluminium",   10, now),
            Opt(ptId, "frameColor", "RAL7022", "Ombergrijs",       11, now),
            Opt(ptId, "frameColor", "RAL8019", "Grijsbruin",       12, now)
        });

        // housingType (5 options)
        options.AddRange(new[]
        {
            Opt(ptId, "housingType", "square_100", "Vierkant 100mm",  1, now),
            Opt(ptId, "housingType", "round_100",  "Rond 100mm",      2, now),
            Opt(ptId, "housingType", "square_120", "Vierkant 120mm",  3, now),
            Opt(ptId, "housingType", "round_120",  "Rond 120mm",      4, now),
            Opt(ptId, "housingType", "concealed",  "Inbouw",          5, now)
        });

        // mountType (4 options)
        options.AddRange(new[]
        {
            Opt(ptId, "mountType", "face",    "Opbouw",         1, now),
            Opt(ptId, "mountType", "top",     "Bovenzijde",     2, now),
            Opt(ptId, "mountType", "ceiling", "Plafond",        3, now),
            Opt(ptId, "mountType", "recess",  "Inmetselen",     4, now)
        });

        // guidanceType (3 options)
        options.AddRange(new[]
        {
            Opt(ptId, "guidanceType", "zip",          "ZIP geleiding",     1, now),
            Opt(ptId, "guidanceType", "cable",        "Kabelgeleiding",    2, now),
            Opt(ptId, "guidanceType", "free_hanging", "Vrij hangend",      3, now)
        });

        // sideChannelColor (match frame colors — rules engine limits per guidance type)
        options.AddRange(new[]
        {
            Opt(ptId, "sideChannelColor", "RAL9010", "Zuiver wit",       1, now),
            Opt(ptId, "sideChannelColor", "RAL9016", "Verkeerswit",       2, now),
            Opt(ptId, "sideChannelColor", "RAL7016", "Antracietgrijs",    3, now),
            Opt(ptId, "sideChannelColor", "RAL7035", "Lichtgrijs",        4, now),
            Opt(ptId, "sideChannelColor", "RAL7021", "Zwartgrijs",        5, now),
            Opt(ptId, "sideChannelColor", "RAL9005", "Gitzwart",          6, now),
            Opt(ptId, "sideChannelColor", "DB703",   "Grijsaluminium",    7, now),
            Opt(ptId, "sideChannelColor", "RAL8019", "Grijsbruin",        8, now)
        });

        // bottomBar (4 options)
        options.AddRange(new[]
        {
            Opt(ptId, "bottomBar", "flat",     "Plat",       1, now),
            Opt(ptId, "bottomBar", "round",    "Rond",       2, now),
            Opt(ptId, "bottomBar", "weighted", "Verzwaard",  3, now),
            Opt(ptId, "bottomBar", "zip",      "ZIP",        4, now)
        });

        // driveType (3 options — rules engine limits per variant)
        options.AddRange(new[]
        {
            Opt(ptId, "driveType", "motor",         "Motor",            1, now),
            Opt(ptId, "driveType", "spring_assist",  "Veerassistentie",  2, now),
            Opt(ptId, "driveType", "manual_chain",   "Kettingbediening", 3, now)
        });

        // motorBrand (3 brands)
        options.AddRange(new[]
        {
            Opt(ptId, "motorBrand", "somfy",  "Somfy",  1, now),
            Opt(ptId, "motorBrand", "simu",   "Simu",   2, now),
            Opt(ptId, "motorBrand", "becker", "Becker", 3, now)
        });

        // motorType (~8 motors — rules engine filters by brand + size)
        options.AddRange(new[]
        {
            Opt(ptId, "motorType", "SOMFY-IO-15",  "Somfy Ilmo 2 IO 15Nm",    1, now),
            Opt(ptId, "motorType", "SOMFY-IO-25",  "Somfy Oximo IO 25Nm",     2, now),
            Opt(ptId, "motorType", "SOMFY-IO-40",  "Somfy Oximo IO 40Nm",     3, now),
            Opt(ptId, "motorType", "SOMFY-RTS-20", "Somfy Ilmo 2 RTS 20Nm",   4, now),
            Opt(ptId, "motorType", "BECKER-R12",   "Becker R12-17C 12Nm",     5, now),
            Opt(ptId, "motorType", "SIMU-T5-10",   "Simu T5 Hz 10Nm",         6, now),
            Opt(ptId, "motorType", "SIMU-T5-20",   "Simu T5 Hz 20Nm",         7, now),
            Opt(ptId, "motorType", "SIMU-T6-25",   "Simu T6 Hz 25Nm",         8, now)
        });

        // controlType (5 options — rules engine filters by motor brand)
        options.AddRange(new[]
        {
            Opt(ptId, "controlType", "io_homecontrol", "IO Homecontrol",    1, now),
            Opt(ptId, "controlType", "rts",            "RTS",                2, now),
            Opt(ptId, "controlType", "rts_timer",      "RTS Timer",          3, now),
            Opt(ptId, "controlType", "wired_switch",   "Bedrade schakelaar", 4, now),
            Opt(ptId, "controlType", "wired_timer",    "Bedrade timer",      5, now)
        });

        // windAutomation (4 options)
        options.AddRange(new[]
        {
            Opt(ptId, "windAutomation", "none",            "Geen",                  1, now),
            Opt(ptId, "windAutomation", "eolis_wirefree",  "Eolis Wirefree",        2, now),
            Opt(ptId, "windAutomation", "eolis_rts",       "Eolis RTS",             3, now),
            Opt(ptId, "windAutomation", "sunis_wirefree",  "Sunis Wirefree",        4, now)
        });

        // remoteControl (4 options)
        options.AddRange(new[]
        {
            Opt(ptId, "remoteControl", "none",       "Geen",                   1, now),
            Opt(ptId, "remoteControl", "situo_1ch",  "Situo 1-kanaal",         2, now),
            Opt(ptId, "remoteControl", "situo_5ch",  "Situo 5-kanaal",         3, now),
            Opt(ptId, "remoteControl", "smoove",     "Smoove wandzender",      4, now)
        });

        // integration (3 options)
        options.AddRange(new[]
        {
            Opt(ptId, "integration", "none",      "Geen",       1, now),
            Opt(ptId, "integration", "tahoma",    "TaHoma",     2, now),
            Opt(ptId, "integration", "connexoon", "Connexoon",  3, now)
        });

        return options;
    }

    private static IEnumerable<ProductOption> AllFabricColors(Guid productTypeId, DateTimeOffset now)
    {
        var colors = new (string Code, string Name)[]
        {
            ("SCR-WHITE",      "Wit"),
            ("SCR-LINEN",      "Linnen"),
            ("SCR-SAND",       "Zand"),
            ("SCR-PEARL-GREY", "Parelgrijs"),
            ("SCR-SILVER",     "Zilver"),
            ("SCR-ALU-GREY",   "Aluminium grijs"),
            ("SCR-CHARCOAL",   "Antraciet"),
            ("SCR-BLACK",      "Zwart"),
            ("SCR-BRONZE",     "Brons"),
            ("SCR-BROWN",      "Bruin"),
            ("SCR-DARK-GREY",  "Donkergrijs"),
            ("SCR-CREAM",      "Crème"),
            ("SCR-BEIGE",      "Beige"),
            ("SCR-LIGHT-GREY", "Lichtgrijs"),
            ("SCR-TAUPE",      "Taupe"),
            ("SCR-STEEL",      "Staalgrijs"),
            ("SCR-MOSS",       "Mosgroen"),
            ("SCR-NAVY",       "Marineblauw"),
            ("SCR-COFFEE",     "Koffie"),
            ("SCR-TERRACOTTA", "Terracotta"),
            ("SCR-OFF-WHITE",  "Gebroken wit"),
            ("SCR-GRAPHITE",   "Grafiet"),
            ("SCR-IVORY",      "Ivoor"),
            ("SCR-STONE",      "Steengrijs"),
            ("SCR-MIDNIGHT",   "Middernacht")
        };

        return colors.Select((c, i) => new ProductOption
        {
            Id = Guid.NewGuid(),
            ProductTypeId = productTypeId,
            ParameterCode = "fabricColor",
            Code = c.Code,
            DisplayName = c.Name,
            DisplayOrder = i + 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

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
