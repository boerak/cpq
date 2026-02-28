using System.Text.Json;

namespace Cpq.Api.Services.Rules;

public interface IRulesEngineClient
{
    Task<T> EvaluateAsync<T>(string decisionPath, object context, CancellationToken ct = default);

    Task<JsonDocument> EvaluateRawAsync(string decisionPath, object context, CancellationToken ct = default);
}
