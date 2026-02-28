namespace Cpq.Api.Models.Requests;

public class ListConfigurationsRequest
{
    public string? Status { get; set; }

    public string? ProductTypeCode { get; set; }

    public string? ProductFamilyCode { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; } = "created_at";

    public string? SortDirection { get; set; } = "desc";
}
