using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Configuration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductTypeId { get; set; }

    [MaxLength(255)]
    public string? Reference { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "draft";

    public JsonDocument ConfigData { get; set; } = JsonDocument.Parse("{}");

    public JsonDocument? ValidationResult { get; set; }

    public JsonDocument? BomData { get; set; }

    public int Version { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ICollection<ConfigurationHistory> History { get; set; } = new List<ConfigurationHistory>();

    public ICollection<BomLine> BomLines { get; set; } = new List<BomLine>();

    public ICollection<MesExport> MesExports { get; set; } = new List<MesExport>();
}
