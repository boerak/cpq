namespace Cpq.Api.Services.Rules;

public class RulesEngineException : Exception
{
    public string DecisionPath { get; }
    public int? StatusCode { get; }

    public RulesEngineException(string decisionPath, string message)
        : base(message)
    {
        DecisionPath = decisionPath;
    }

    public RulesEngineException(string decisionPath, int statusCode, string message)
        : base($"Rules engine returned HTTP {statusCode} for '{decisionPath}': {message}")
    {
        DecisionPath = decisionPath;
        StatusCode = statusCode;
    }

    public RulesEngineException(string decisionPath, string message, Exception innerException)
        : base($"Rules engine error for '{decisionPath}': {message}", innerException)
    {
        DecisionPath = decisionPath;
    }
}
