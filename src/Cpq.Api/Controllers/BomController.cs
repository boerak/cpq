using Cpq.Api.Models.Responses;
using Cpq.Api.Services.Configuration;
using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Controllers;

[ApiController]
[Route("api/v1/configurations/{id:guid}")]
public class BomController : ControllerBase
{
    private readonly IConfigurationService _configService;
    private readonly CpqDbContext _db;
    private readonly ILogger<BomController> _logger;

    public BomController(IConfigurationService configService, CpqDbContext db, ILogger<BomController> logger)
    {
        _configService = configService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Generate (or regenerate) the BOM for a configuration.
    /// </summary>
    [HttpPost("bom")]
    [ProducesResponseType(typeof(BomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateBom(Guid id, CancellationToken ct)
    {
        var result = await _configService.GenerateBomAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get the last generated BOM for a configuration.
    /// </summary>
    [HttpGet("bom")]
    [ProducesResponseType(typeof(BomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBom(Guid id, CancellationToken ct)
    {
        // Check configuration exists
        var exists = await _db.Configurations.AnyAsync(c => c.Id == id, ct);
        if (!exists)
        {
            throw new EntityNotFoundException(nameof(Models.Domain.Configuration), id);
        }

        var lines = await _db.BomLines
            .Where(b => b.ConfigurationId == id)
            .OrderBy(b => b.SortOrder)
            .ToListAsync(ct);

        if (!lines.Any())
        {
            return Ok(new BomResponse
            {
                Lines = new List<BomLineResponse>(),
                TotalWeight = 0,
                GeneratedAt = DateTimeOffset.UtcNow
            });
        }

        // Calculate total weight using part weight data
        var skus = lines.Select(l => l.PartSku).Distinct().ToList();
        var parts = await _db.Parts
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku, ct);

        decimal totalWeight = 0;
        var responseLines = lines.Select(line =>
        {
            parts.TryGetValue(line.PartSku, out var part);
            if (part?.WeightKg.HasValue == true)
            {
                var lengthFactor = line.CutLengthMm.HasValue ? (decimal)line.CutLengthMm.Value / 1000m : 1m;
                totalWeight += part.WeightKg.Value * line.Quantity * lengthFactor;
            }

            return new BomLineResponse
            {
                PartSku = line.PartSku,
                PartName = line.PartName,
                Category = line.Category,
                Quantity = line.Quantity,
                Unit = line.Unit,
                CutLengthMm = line.CutLengthMm,
                SortOrder = line.SortOrder,
                Notes = line.Notes
            };
        }).ToList();

        return Ok(new BomResponse
        {
            Lines = responseLines,
            TotalWeight = Math.Round(totalWeight, 2),
            GeneratedAt = DateTimeOffset.UtcNow
        });
    }
}
