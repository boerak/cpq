using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Cpq.Api.Services.Rules;

public class RulesEngineClient : IRulesEngineClient
{
    private readonly HttpClient _httpClient;
    private readonly RulesEngineOptions _options;
    private readonly ILogger<RulesEngineClient> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RulesEngineClient(HttpClient httpClient, IOptions<RulesEngineOptions> options, ILogger<RulesEngineClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T> EvaluateAsync<T>(string decisionPath, object context, CancellationToken ct = default)
    {
        var doc = await EvaluateRawAsync(decisionPath, context, ct);

        try
        {
            var resultElement = doc.RootElement.GetProperty("result");
            var result = resultElement.Deserialize<T>(SerializerOptions);
            if (result is null)
            {
                throw new RulesEngineException(decisionPath, "Deserialized result was null.");
            }
            return result;
        }
        catch (KeyNotFoundException)
        {
            throw new RulesEngineException(decisionPath, "Response did not contain a 'result' property.");
        }
        catch (JsonException ex)
        {
            throw new RulesEngineException(decisionPath, "Failed to deserialize rules engine result.", ex);
        }
    }

    public async Task<JsonDocument> EvaluateRawAsync(string decisionPath, object context, CancellationToken ct = default)
    {
        // Ensure .json extension
        var path = decisionPath.EndsWith(".json") ? decisionPath : decisionPath + ".json";
        var url = $"/api/projects/{_options.ProjectSlug}/evaluate/{path}";

        var requestBody = new { context };
        var json = JsonSerializer.Serialize(requestBody, SerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("Calling rules engine: POST {Url}", url);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError(ex, "Rules engine call failed for {DecisionPath} after {Elapsed}ms", decisionPath, sw.ElapsedMilliseconds);
            throw new RulesEngineException(decisionPath, "HTTP request to rules engine failed.", ex);
        }

        sw.Stop();
        _logger.LogDebug("Rules engine responded {StatusCode} for {DecisionPath} in {Elapsed}ms",
            (int)response.StatusCode, decisionPath, sw.ElapsedMilliseconds);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Rules engine returned {StatusCode} for {DecisionPath}: {Body}",
                (int)response.StatusCode, decisionPath, errorBody);
            throw new RulesEngineException(decisionPath, (int)response.StatusCode,
                $"Rules engine error: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);

        try
        {
            return JsonDocument.Parse(responseJson);
        }
        catch (JsonException ex)
        {
            throw new RulesEngineException(decisionPath, "Failed to parse rules engine response as JSON.", ex);
        }
    }
}
