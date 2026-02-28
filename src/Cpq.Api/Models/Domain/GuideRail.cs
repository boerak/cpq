using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class GuideRail
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

    [Required]
    [MaxLength(50)]
    public string MaterialCode { get; set; } = string.Empty;

    public decimal WidthMm { get; set; }

    public decimal DepthMm { get; set; }

    public int MaxHeightMm { get; set; }

    public decimal WeightPerMeterKg { get; set; }

    public int BracketSpacingMm { get; set; } = 600;

    public List<string>? CompatibleProfiles { get; set; }

    public int? WindClass { get; set; }

    public bool IsActive { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Material Material { get; set; } = null!;
}
