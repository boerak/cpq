namespace Cpq.Api.Services.Rules;

public class RulesEngineOptions
{
    public const string SectionName = "RulesEngine";

    public string BaseUrl { get; set; } = "http://gorules-agent:8080";

    public string ProjectSlug { get; set; } = "default";

    public int TimeoutSeconds { get; set; } = 10;

    public int RetryCount { get; set; } = 3;

    public int CircuitBreakerThreshold { get; set; } = 5;

    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
