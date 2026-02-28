using System.Text.Json;

namespace Cpq.Api.Models.Responses;

public class MesExportResponse
{
    public Guid Id { get; set; }

    public Guid ConfigurationId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
