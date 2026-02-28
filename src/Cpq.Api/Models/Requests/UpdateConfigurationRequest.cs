using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Requests;

public class UpdateConfigurationRequest
{
    [Required]
    public Dictionary<string, JsonElement> Selections { get; set; } = new();

    [Required]
    public int ExpectedVersion { get; set; }

    public string? PerformedBy { get; set; }
}
