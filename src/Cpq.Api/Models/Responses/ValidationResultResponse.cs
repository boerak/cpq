namespace Cpq.Api.Models.Responses;

public class ValidationResultResponse
{
    public bool Valid { get; set; }

    public List<ValidationError> Errors { get; set; } = new();

    public List<ValidationWarning> Warnings { get; set; } = new();
}

public class ValidationError
{
    public string Parameter { get; set; } = string.Empty;

    public string Rule { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public class ValidationWarning
{
    public string Parameter { get; set; } = string.Empty;

    public string Rule { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
