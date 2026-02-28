namespace Cpq.Api.Models.Responses;

public class BomResponse
{
    public List<BomLineResponse> Lines { get; set; } = new();

    public decimal TotalWeight { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }
}

public class BomLineResponse
{
    public string PartSku { get; set; } = string.Empty;

    public string? PartName { get; set; }

    public string? Category { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public int? CutLengthMm { get; set; }

    public int SortOrder { get; set; }

    public string? Notes { get; set; }
}
