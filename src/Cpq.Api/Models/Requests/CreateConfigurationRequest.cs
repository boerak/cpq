using System.ComponentModel.DataAnnotations;

namespace Cpq.Api.Models.Requests;

public class CreateConfigurationRequest
{
    [Required]
    public string ProductTypeCode { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public string? CreatedBy { get; set; }
}
