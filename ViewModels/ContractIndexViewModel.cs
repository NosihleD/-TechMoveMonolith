using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Models;

namespace TechMove.ViewModels;

public class ContractIndexViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public List<Contract> Contracts { get; set; } = new();
    public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
}
