using System.ComponentModel.DataAnnotations;

namespace TechMove.Models;

public class ServiceRequest
{
    public int ServiceRequestId { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 100000000)]
    [Display(Name = "Cost (USD)")]
    public decimal CostUsd { get; set; }

    [Display(Name = "USD → ZAR Rate")]
    public decimal ExchangeRate { get; set; }

    [Display(Name = "Local Cost (ZAR)")]
    public decimal CostZar { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "New";

    public Contract? Contract { get; set; }
}
