using Cpq.Api.Models.Requests;
using Cpq.Api.Models.Responses;
using Cpq.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cpq.Api.Controllers;

[ApiController]
[Route("api/v1/configurations")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationService configService, ILogger<ConfigurationController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new configuration.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationRequest request, CancellationToken ct)
    {
        var result = await _configService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// List configurations with optional filters and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ConfigurationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListConfigurationsRequest filter, CancellationToken ct)
    {
        var result = await _configService.ListAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a configuration by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _configService.GetAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update selections in a configuration (PATCH semantics with optimistic concurrency).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [EnableRateLimiting("configuration-patch")]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConfigurationRequest request, CancellationToken ct)
    {
        var result = await _configService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Validate a configuration against business rules.
    /// </summary>
    [HttpGet("{id:guid}/validate")]
    [ProducesResponseType(typeof(ValidationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(Guid id, CancellationToken ct)
    {
        var result = await _configService.ValidateAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Finalize a configuration (sets status to 'finalized' after validation).
    /// </summary>
    [HttpPost("{id:guid}/finalize")]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var result = await _configService.FinalizeAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Delete a draft configuration.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _configService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Clone an existing configuration.
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Clone(Guid id, CancellationToken ct)
    {
        var result = await _configService.CloneAsync(id, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
