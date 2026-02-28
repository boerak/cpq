using System.ComponentModel.DataAnnotations;

namespace Cpq.Api.Models.Domain;

public class ProductType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FamilyId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Variant { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ProductFamily Family { get; set; } = null!;

    public ICollection<ProductParameter> Parameters { get; set; } = new List<ProductParameter>();

    public ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();

    public ICollection<ProductSpec> Specs { get; set; } = new List<ProductSpec>();
}
