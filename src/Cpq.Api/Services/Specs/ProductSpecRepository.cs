using Cpq.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Cpq.Api.Services.Specs;

public class ProductSpecRepository : IProductSpecRepository
{
    private readonly CpqDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductSpecRepository> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const string GlobalRefDataKey = "global_ref_data";

    public ProductSpecRepository(CpqDbContext db, IMemoryCache cache, ILogger<ProductSpecRepository> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GetSpecContextAsync(Guid productTypeId, CancellationToken ct = default)
    {
        // Load product-type-specific specs
        var specs = await _db.ProductSpecs
            .Where(s => s.ProductTypeId == productTypeId && s.IsActive)
            .ToListAsync(ct);

        var specContext = SpecContextBuilder.BuildFromSpecs(specs);

        // Load global reference data from cache
        var globalData = await _cache.GetOrCreateAsync(GlobalRefDataKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            _logger.LogDebug("Loading global reference data into cache");
            return await LoadGlobalRefDataAsync(ct);
        });

        // Merge product-specific specs into the top-level context
        // (so rules access specs.fabric, specs.dimensions, etc. directly)
        var context = new Dictionary<string, object>(specContext);

        if (globalData is not null)
        {
            context["materials"] = globalData.Materials;
            context["profiles"] = globalData.Profiles;
            context["motors"] = globalData.Motors;
            context["guideRails"] = globalData.GuideRails;
            context["boxes"] = globalData.Boxes;
            context["colors"] = globalData.Colors;
        }

        return context;
    }

    private async Task<GlobalRefData> LoadGlobalRefDataAsync(CancellationToken ct)
    {
        var materials = await _db.Materials
            .Include(m => m.MaterialColors)
                .ThenInclude(mc => mc.Color)
            .Where(m => m.IsActive)
            .ToListAsync(ct);

        var profiles = await _db.Profiles
            .Where(p => p.IsActive)
            .ToListAsync(ct);

        var motors = await _db.Motors
            .Where(m => m.IsActive)
            .ToListAsync(ct);

        var guideRails = await _db.GuideRails
            .Where(r => r.IsActive)
            .ToListAsync(ct);

        var boxes = await _db.Boxes
            .Where(b => b.IsActive)
            .ToListAsync(ct);

        var colors = await _db.Colors
            .Where(c => c.IsActive)
            .ToListAsync(ct);

        return new GlobalRefData
        {
            Materials = SpecContextBuilder.BuildMaterialsContext(materials),
            Profiles = SpecContextBuilder.BuildProfilesContext(profiles),
            Motors = SpecContextBuilder.BuildMotorsContext(motors),
            GuideRails = SpecContextBuilder.BuildGuideRailsContext(guideRails),
            Boxes = SpecContextBuilder.BuildBoxesContext(boxes),
            Colors = SpecContextBuilder.BuildColorsContext(colors)
        };
    }

    private class GlobalRefData
    {
        public List<Dictionary<string, object>> Materials { get; set; } = new();
        public List<Dictionary<string, object>> Profiles { get; set; } = new();
        public List<Dictionary<string, object>> Motors { get; set; } = new();
        public List<Dictionary<string, object>> GuideRails { get; set; } = new();
        public List<Dictionary<string, object>> Boxes { get; set; } = new();
        public List<Dictionary<string, object>> Colors { get; set; } = new();
    }
}
