using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class Color
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
    public string ColorSystem { get; set; } = string.Empty;

    [MaxLength(7)]
    public string? HexValue { get; set; }

    public bool IsStandard { get; set; } = true;

    public JsonDocument? Properties { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<MaterialColor> MaterialColors { get; set; } = new List<MaterialColor>();
}
