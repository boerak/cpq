using Cpq.Api.Models.Responses;
using Cpq.Api.Services.Mes;
using Microsoft.AspNetCore.Mvc;

namespace Cpq.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class MesExportController : ControllerBase
{
    private readonly IMesExporter _mesExporter;
    private readonly ILogger<MesExportController> _logger;

    public MesExportController(IMesExporter mesExporter, ILogger<MesExportController> logger)
    {
        _mesExporter = mesExporter;
        _logger = logger;
    }

    /// <summary>
    /// Export a finalized configuration to MES.
    /// </summary>
    [HttpPost("configurations/{id:guid}/export")]
    [ProducesResponseType(typeof(MesExportResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var result = await _mesExporter.ExportAsync(id, ct);
        return AcceptedAtAction(nameof(GetExport), new { exportId = result.Id }, result);
    }

    /// <summary>
    /// Get the status of a MES export.
    /// </summary>
    [HttpGet("exports/{exportId:guid}")]
    [ProducesResponseType(typeof(MesExportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExport(Guid exportId, CancellationToken ct)
    {
        var result = await _mesExporter.GetExportAsync(exportId, ct);
        return Ok(result);
    }
}
