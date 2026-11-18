using System.ComponentModel.DataAnnotations;

namespace TechMove.Models;

public class Contract
{
    public int ContractId { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = ContractStatuses.Draft;

    [Required, StringLength(100)]
    [Display(Name = "Service Level")]
    public string ServiceLevel { get; set; } = string.Empty;

    [Display(Name = "Signed Agreement")]
    public string? SignedAgreementPath { get; set; }

    public Client? Client { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}

public static class ContractStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string OnHold = "On Hold";

    public static readonly string[] All = [Draft, Active, Expired, OnHold];
}
