using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class SkuMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string ProductFamilyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public JsonDocument MatchCriteria { get; set; } = JsonDocument.Parse("{}");

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    public int Priority { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ProductFamily ProductFamily { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
