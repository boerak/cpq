using Cpq.Api.Models.Responses;

namespace Cpq.Api.Services.Mes;

/// <summary>
/// Thin wrapper service for MES export operations.
/// Delegates to the actual IMesExporter implementation.
/// </summary>
public class MesExportService
{
    private readonly IMesExporter _exporter;

    public MesExportService(IMesExporter exporter)
    {
        _exporter = exporter;
    }

    public Task<MesExportResponse> ExportAsync(Guid configurationId, CancellationToken ct = default)
        => _exporter.ExportAsync(configurationId, ct);

    public Task<MesExportResponse> GetExportAsync(Guid exportId, CancellationToken ct = default)
        => _exporter.GetExportAsync(exportId, ct);
}
