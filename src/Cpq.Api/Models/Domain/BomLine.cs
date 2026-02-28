using System.ComponentModel.DataAnnotations;

namespace Cpq.Api.Models.Domain;

public class BomLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConfigurationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PartSku { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? PartName { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public decimal Quantity { get; set; }

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    public int? CutLengthMm { get; set; }

    public int SortOrder { get; set; } = 0;

    public string? Notes { get; set; }

    public Configuration Configuration { get; set; } = null!;

    public Part? Part { get; set; }
}
