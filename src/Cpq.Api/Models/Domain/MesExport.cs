using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class MesExport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConfigurationId { get; set; }

    public JsonDocument Payload { get; set; } = JsonDocument.Parse("{}");

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending";

    public DateTimeOffset? SentAt { get; set; }

    public JsonDocument? Response { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Configuration Configuration { get; set; } = null!;
}
