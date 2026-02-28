using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cpq.Api.Models.Domain;

public class ProductParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DataType { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Unit { get; set; }

    public int StepNumber { get; set; }

    [MaxLength(200)]
    public string? StepName { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [MaxLength(200)]
    public string? DefaultValue { get; set; }

    public List<string> DependsOn { get; set; } = new List<string>();

    public JsonDocument? Metadata { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ProductType ProductType { get; set; } = null!;
}
