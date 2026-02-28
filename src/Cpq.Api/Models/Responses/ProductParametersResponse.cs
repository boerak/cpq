using System.Text.Json;

namespace Cpq.Api.Models.Responses;

public class ProductParametersResponse
{
    public List<StepResponse> Steps { get; set; } = new();
}

public class StepResponse
{
    public int StepNumber { get; set; }

    public string? StepName { get; set; }

    public List<ParameterResponse> Parameters { get; set; } = new();
}

public class ParameterResponse
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public string? Unit { get; set; }

    public bool IsRequired { get; set; }

    public string? DefaultValue { get; set; }

    public List<string> DependsOn { get; set; } = new();

    public JsonDocument? Metadata { get; set; }

    public List<OptionResponse> Options { get; set; } = new();
}

public class OptionResponse
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
