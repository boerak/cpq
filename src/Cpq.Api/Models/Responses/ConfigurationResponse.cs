using System.Text.Json;

namespace Cpq.Api.Models.Responses;

public class ConfigurationResponse
{
    public Guid Id { get; set; }

    public ProductTypeResponse ProductType { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public JsonDocument? Config { get; set; }

    public ValidationResultResponse? Validation { get; set; }

    public Dictionary<string, List<OptionResponse>>? AvailableOptions { get; set; }

    public List<string>? ResetFields { get; set; }

    public bool IsComplete { get; set; }

    public bool CanFinalize { get; set; }

    public int Version { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }
}
