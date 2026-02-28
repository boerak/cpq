using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Motor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Model { get; set; } = string.Empty;

    public decimal TorqueNm { get; set; }

    public decimal? SpeedRpm { get; set; }

    public decimal? MaxWeightKg { get; set; }

    public decimal? MaxSurfaceM2 { get; set; }

    public List<string> ControlTypes { get; set; } = new List<string>();

    public int? TubeDiameterMm { get; set; }

    public bool IsActive { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
