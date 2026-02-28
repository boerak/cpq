namespace Cpq.Api.Services.Specs;

public interface IProductSpecRepository
{
    Task<Dictionary<string, object>> GetSpecContextAsync(Guid productTypeId, CancellationToken ct = default);
}
