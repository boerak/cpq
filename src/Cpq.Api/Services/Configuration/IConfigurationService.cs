using Cpq.Api.Models.Requests;
using Cpq.Api.Models.Responses;

namespace Cpq.Api.Services.Configuration;

public interface IConfigurationService
{
    Task<ConfigurationResponse> CreateAsync(CreateConfigurationRequest request, CancellationToken ct = default);

    Task<ConfigurationResponse> GetAsync(Guid id, CancellationToken ct = default);

    Task<PagedResponse<ConfigurationResponse>> ListAsync(ListConfigurationsRequest filter, CancellationToken ct = default);

    Task<ConfigurationResponse> UpdateAsync(Guid id, UpdateConfigurationRequest request, CancellationToken ct = default);

    Task<ValidationResultResponse> ValidateAsync(Guid id, CancellationToken ct = default);

    Task<ConfigurationResponse> FinalizeAsync(Guid id, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<ConfigurationResponse> CloneAsync(Guid id, CancellationToken ct = default);

    Task<BomResponse> GenerateBomAsync(Guid id, CancellationToken ct = default);
}
