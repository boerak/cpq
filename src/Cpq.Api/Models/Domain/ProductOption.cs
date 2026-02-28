using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class ProductOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ParameterCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public JsonDocument? Metadata { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ProductType ProductType { get; set; } = null!;
}
