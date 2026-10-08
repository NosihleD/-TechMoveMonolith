using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.ViewModels;

namespace TechMove.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FileValidationService _fileValidation;
    private readonly ContractWorkflowService _workflow;
    private readonly IWebHostEnvironment _environment;

    public ContractsController(ApplicationDbContext context, FileValidationService fileValidation,
        ContractWorkflowService workflow, IWebHostEnvironment environment)
    {
        _context = context;
        _fileValidation = fileValidation;
        _workflow = workflow;
        _environment = environment;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? status)
    {
        var query = _context.Contracts.Include(c => c.Client).AsQueryable();
        if (startDate.HasValue) query = query.Where(c => c.StartDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(c => c.EndDate <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);

        var vm = new ContractIndexViewModel
        {
            StartDate = startDate, EndDate = endDate, Status = status,
            Contracts = await query.OrderByDescending(c => c.StartDate).ToListAsync(),
            Statuses = ContractStatuses.All.Select(s => new SelectListItem(s, s, s == status))
        };
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "ClientId", "Name");
        return View(new Contract { StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1), Status = ContractStatuses.Active });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement)
    {
        try
        {
            _workflow.ValidateContractDates(contract);
            if (signedAgreement is not null) _fileValidation.ValidatePdf(signedAgreement);
        }
        catch (Exception ex) when (ex is InvalidOperationException or InvalidDataException)
        {
            ModelState.AddModelError("", ex.Message);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        if (signedAgreement is not null)
            contract.SignedAgreementPath = await SaveAgreementAsync(signedAgreement);

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.ContractId == id);
        return contract is null ? NotFound() : View(contract);
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract?.SignedAgreementPath is null) return NotFound();
        var fullPath = Path.Combine(_environment.ContentRootPath, contract.SignedAgreementPath);
        if (!System.IO.File.Exists(fullPath)) return NotFound();
        return PhysicalFile(fullPath, "application/pdf", Path.GetFileName(fullPath));
    }

    private async Task<string> SaveAgreementAsync(IFormFile file)
    {
        var relativeFolder = Path.Combine("Uploads", "Agreements");
        var folder = Path.Combine(_environment.ContentRootPath, relativeFolder);
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(folder, fileName);
        await using var stream = new FileStream(fullPath, FileMode.CreateNew);
        await file.CopyToAsync(stream);
        return Path.Combine(relativeFolder, fileName).Replace('\\', '/');
    }
}
