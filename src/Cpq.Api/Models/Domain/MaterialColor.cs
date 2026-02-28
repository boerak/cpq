using System.ComponentModel.DataAnnotations;

namespace Cpq.Api.Models.Domain;

public class MaterialColor
{
    [Required]
    [MaxLength(50)]
    public string MaterialCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ColorCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Material Material { get; set; } = null!;

    public Color Color { get; set; } = null!;
}
