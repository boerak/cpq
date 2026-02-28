using Cpq.Api.Models.Responses;

namespace Cpq.Api.Services.Mes;

public interface IMesExporter
{
    Task<MesExportResponse> ExportAsync(Guid configurationId, CancellationToken ct = default);

    Task<MesExportResponse> GetExportAsync(Guid exportId, CancellationToken ct = default);
}
