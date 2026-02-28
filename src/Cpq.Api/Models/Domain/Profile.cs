using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Profile
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
    public string MaterialCode { get; set; } = string.Empty;

    public decimal HeightMm { get; set; }

    public decimal ThicknessMm { get; set; }

    public decimal WeightPerMeterKg { get; set; }

    public int MaxWidthMm { get; set; }

    public int MinWidthMm { get; set; } = 400;

    public bool IsActive { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Material Material { get; set; } = null!;
}
