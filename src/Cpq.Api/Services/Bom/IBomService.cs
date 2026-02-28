using Cpq.Api.Models.Responses;

namespace Cpq.Api.Services.Bom;

public interface IBomService
{
    Task<BomResponse> GenerateBomAsync(Guid configurationId, CancellationToken ct = default);
}
