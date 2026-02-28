using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Accessory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public bool RequiresMotor { get; set; } = false;

    public List<string>? CompatibleFamilies { get; set; }

    public List<string>? IncompatibleWith { get; set; }

    public List<string>? PrerequisiteAccessories { get; set; }

    public bool IsActive { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
