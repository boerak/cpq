using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class ProductSpec
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SpecGroup { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SpecKey { get; set; } = string.Empty;

    public JsonDocument SpecValue { get; set; } = JsonDocument.Parse("null");

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ProductType ProductType { get; set; } = null!;
}
