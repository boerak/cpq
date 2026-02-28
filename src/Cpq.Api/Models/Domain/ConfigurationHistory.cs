using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class ConfigurationHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConfigurationId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public JsonDocument? SelectionsSnapshot { get; set; }

    public JsonDocument? ValidationSnapshot { get; set; }

    public List<string>? ChangedFields { get; set; }

    [MaxLength(200)]
    public string? PerformedBy { get; set; }

    public DateTimeOffset PerformedAt { get; set; }

    public Configuration Configuration { get; set; } = null!;
}
