using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMove.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 100000000)]
    [Display(Name = "Cost (USD)")]
    public decimal CostUsd { get; set; }

    [Display(Name = "Current USD → ZAR Rate")]
    public decimal ExchangeRate { get; set; }

    [Display(Name = "Calculated Cost (ZAR)")]
    public decimal CostZar { get; set; }

    public IEnumerable<SelectListItem> Contracts { get; set; } = Enumerable.Empty<SelectListItem>();
}
