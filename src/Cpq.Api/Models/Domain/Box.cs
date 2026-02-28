using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Box
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    public int InnerDiameterMm { get; set; }

    public int OuterHeightMm { get; set; }

    public List<string> CompatibleMaterials { get; set; } = new List<string>();

    public int? MaxWidthMm { get; set; }

    public bool IsActive { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
