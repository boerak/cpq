using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cpq.Api.Data.Seeding;

/// <summary>
/// Orchestrates all database seeders in dependency order.
/// Safe to call on every startup — each individual seeder is idempotent.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAllAsync(DbContext context, ILogger? logger = null)
    {
        logger?.LogInformation("Starting database seeding…");

        // ── Tier 1: independent reference data ───────────────────────────────
        logger?.LogInformation("Seeding materials…");
        await MaterialSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding colors…");
        await ColorSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding motors…");
        await MotorSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding boxes…");
        await BoxSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding accessories…");
        await AccessorySeeder.SeedAsync(context);

        // ── Tier 2: depends on materials ──────────────────────────────────────
        logger?.LogInformation("Seeding material-color relationships…");
        await MaterialColorSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding profiles…");
        await ProfileSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding guide rails…");
        await GuideRailSeeder.SeedAsync(context);

        // ── Tier 3: product hierarchy ─────────────────────────────────────────
        logger?.LogInformation("Seeding product families…");
        await ProductFamilySeeder.SeedAsync(context);

        logger?.LogInformation("Seeding product types…");
        await ProductTypeSeeder.SeedAsync(context);

        // ── Tier 4: product configuration metadata ────────────────────────────
        // ProductParameter must exist before ProductOption (FK on product_type_id + code)
        logger?.LogInformation("Seeding product parameters…");
        await ProductParameterSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding product options…");
        await ProductOptionSeeder.SeedAsync(context);

        logger?.LogInformation("Seeding product specs…");
        await ProductSpecSeeder.SeedAsync(context);

        // ── Tier 5: parts catalog and SKU mappings ────────────────────────────
        // SkuMapping depends on ProductFamily and Part, so parts must be seeded first.
        logger?.LogInformation("Seeding parts catalog and SKU mappings…");
        await PartsCatalogSeeder.SeedAsync(context);

        logger?.LogInformation("Database seeding completed.");
    }
}
