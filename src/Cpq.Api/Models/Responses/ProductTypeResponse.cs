namespace Cpq.Api.Models.Responses;

public class ProductTypeResponse
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Variant { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProductFamilyResponse Family { get; set; } = null!;

    public int DisplayOrder { get; set; }
}
