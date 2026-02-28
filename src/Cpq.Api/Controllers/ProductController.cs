using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Cpq.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly CpqDbContext _db;
    private readonly ILogger<ProductController> _logger;

    public ProductController(CpqDbContext db, ILogger<ProductController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get a product type by code.
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ProductTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(string code, CancellationToken ct)
    {
        var productType = await _db.ProductTypes
            .Include(pt => pt.Family)
            .FirstOrDefaultAsync(pt => pt.Code == code && pt.IsActive, ct);

        if (productType is null)
        {
            throw new EntityNotFoundException(nameof(Models.Domain.ProductType), "Code", code);
        }

        return Ok(new ProductTypeResponse
        {
            Code = productType.Code,
            Name = productType.Name,
            Variant = productType.Variant,
            Description = productType.Description,
            DisplayOrder = productType.DisplayOrder,
            Family = new ProductFamilyResponse
            {
                Code = productType.Family.Code,
                Name = productType.Family.Name,
                Description = productType.Family.Description,
                IsActive = productType.Family.IsActive
            }
        });
    }

    /// <summary>
    /// Get all parameters for a product type, organized by step.
    /// </summary>
    [HttpGet("{code}/parameters")]
    [ProducesResponseType(typeof(ProductParametersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParameters(string code, CancellationToken ct)
    {
        var productType = await _db.ProductTypes
            .FirstOrDefaultAsync(pt => pt.Code == code && pt.IsActive, ct);

        if (productType is null)
        {
            throw new EntityNotFoundException(nameof(Models.Domain.ProductType), "Code", code);
        }

        var parameters = await _db.ProductParameters
            .Where(p => p.ProductTypeId == productType.Id && p.IsActive)
            .OrderBy(p => p.StepNumber)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(ct);

        var options = await _db.ProductOptions
            .Where(o => o.ProductTypeId == productType.Id && o.IsActive)
            .OrderBy(o => o.DisplayOrder)
            .ToListAsync(ct);

        var optionsByParam = options
            .GroupBy(o => o.ParameterCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        var steps = parameters
            .GroupBy(p => new { p.StepNumber, p.StepName })
            .OrderBy(g => g.Key.StepNumber)
            .Select(g => new StepResponse
            {
                StepNumber = g.Key.StepNumber,
                StepName = g.Key.StepName,
                Parameters = g.Select(p => new ParameterResponse
                {
                    Code = p.Code,
                    Name = p.Name,
                    DataType = p.DataType,
                    Unit = p.Unit,
                    IsRequired = p.IsRequired,
                    DefaultValue = p.DefaultValue,
                    DependsOn = p.DependsOn,
                    Metadata = p.Metadata,
                    Options = optionsByParam.TryGetValue(p.Code, out var opts)
                        ? opts.Select(o => new OptionResponse
                        {
                            Code = o.Code,
                            DisplayName = o.DisplayName,
                            IsActive = o.IsActive
                        }).ToList()
                        : new List<OptionResponse>()
                }).ToList()
            }).ToList();

        return Ok(new ProductParametersResponse { Steps = steps });
    }
}
