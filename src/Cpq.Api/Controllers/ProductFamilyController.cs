using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Cpq.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Controllers;

[ApiController]
[Route("api/v1/families")]
public class ProductFamilyController : ControllerBase
{
    private readonly CpqDbContext _db;
    private readonly ILogger<ProductFamilyController> _logger;

    public ProductFamilyController(CpqDbContext db, ILogger<ProductFamilyController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// List all active product families.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductFamilyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFamilies(CancellationToken ct)
    {
        var families = await _db.ProductFamilies
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

        var response = families.Select(f => new ProductFamilyResponse
        {
            Code = f.Code,
            Name = f.Name,
            Description = f.Description,
            IsActive = f.IsActive
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get all active product types for a family.
    /// </summary>
    [HttpGet("{code}/products")]
    [ProducesResponseType(typeof(List<ProductTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsByFamily(string code, CancellationToken ct)
    {
        var family = await _db.ProductFamilies
            .FirstOrDefaultAsync(f => f.Code == code && f.IsActive, ct);

        if (family is null)
        {
            throw new EntityNotFoundException(nameof(Models.Domain.ProductFamily), "Code", code);
        }

        var productTypes = await _db.ProductTypes
            .Include(pt => pt.Family)
            .Where(pt => pt.FamilyId == family.Id && pt.IsActive)
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Name)
            .ToListAsync(ct);

        var response = productTypes.Select(pt => new ProductTypeResponse
        {
            Code = pt.Code,
            Name = pt.Name,
            Variant = pt.Variant,
            Description = pt.Description,
            DisplayOrder = pt.DisplayOrder,
            Family = new ProductFamilyResponse
            {
                Code = pt.Family.Code,
                Name = pt.Family.Name,
                Description = pt.Family.Description,
                IsActive = pt.Family.IsActive
            }
        }).ToList();

        return Ok(response);
    }
}
