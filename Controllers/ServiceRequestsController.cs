using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.ViewModels;

namespace TechMove.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CurrencyService _currency;
    private readonly ContractWorkflowService _workflow;
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        ApplicationDbContext context,
        CurrencyService currency,
        ContractWorkflowService workflow,
        ILogger<ServiceRequestsController> logger)
    {
        _context = context;
        _currency = currency;
        _workflow = workflow;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(s => s.Contract)
            .ThenInclude(c => c!.Client)
            .OrderByDescending(s => s.ServiceRequestId)
            .ToListAsync();

        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ServiceRequestCreateViewModel();

        await PopulateContractsAsync(vm);

        try
        {
            vm.ExchangeRate =
                await _currency.GetUsdToZarRateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to retrieve USD to ZAR exchange rate.");

            vm.ExchangeRate = 0;

            ViewBag.CurrencyError =
                "The live currency service is currently unavailable. Please try again.";
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ServiceRequestCreateViewModel vm)
    {
        // CostZar and ExchangeRate are calculated by the server.
// Do not validate values posted back from the form.
ModelState.Remove(nameof(vm.CostZar));
ModelState.Remove(nameof(vm.ExchangeRate));
        _logger.LogInformation(
            "Creating Service Request. ContractId={ContractId}, CostUsd={CostUsd}, Description={Description}",
            vm.ContractId,
            vm.CostUsd,
            vm.Description);

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(
                c => c.ContractId == vm.ContractId);

        if (contract == null)
        {
            ModelState.AddModelError(
                "ContractId",
                "Please select a valid contract.");
        }

        // Expired and On Hold contracts cannot create requests.
        if (contract != null)
        {
            try
            {
                _workflow.EnsureCanCreateServiceRequest(contract);
                _logger.LogInformation(
                    "Contract validation passed. ContractId={ContractId}, Status={Status}",
                    contract.ContractId,
                    contract.Status);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    "Service Request rejected. ContractId={ContractId}, Status={Status}",
                    contract.ContractId,
                    contract.Status);

                ModelState.AddModelError(
                    "",
                    ex.Message);
            }
        }

        // Currency calculation
        if (ModelState.IsValid)
        {
            try
            {
                vm.ExchangeRate =
                    await _currency.GetUsdToZarRateAsync();

                vm.CostZar =
                    CurrencyService.ConvertUsdToZar(
                        vm.CostUsd,
                        vm.ExchangeRate);

                _logger.LogInformation(
                    "Currency calculation successful. Rate={Rate}, CostZar={CostZar}",
                    vm.ExchangeRate,
                    vm.CostZar);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Currency calculation failed.");

                ModelState.AddModelError(
                    "",
                    "Unable to retrieve the current USD to ZAR exchange rate. Please try again.");
            }
        }

        // Validation failed
      if (!ModelState.IsValid)
{
    _logger.LogWarning(
        "Service Request validation failed.");

    foreach (var entry in ModelState)
    {
        foreach (var error in entry.Value.Errors)
        {
            _logger.LogWarning(
                "VALIDATION ERROR: Field={Field}, Error={Error}",
                entry.Key,
                error.ErrorMessage);
        }
    }

    await PopulateContractsAsync(vm);

    return View(vm);
}

        // Create database entity
        var serviceRequest = new ServiceRequest
        {
            ContractId = vm.ContractId,
            Description = vm.Description
            ,CostUsd = vm.CostUsd,
            ExchangeRate = vm.ExchangeRate,
            CostZar = vm.CostZar,
            Status = "New"
        };

        try
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Service Request saved successfully. ID={ServiceRequestId}",
                serviceRequest.ServiceRequestId);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Service Request.");
            ModelState.AddModelError(
                "",
                "The Service Request could not be saved to the database.");

            await PopulateContractsAsync(vm);
            return View(vm);
        }
    }

    private async Task PopulateContractsAsync(
        ServiceRequestCreateViewModel vm)
    {
        vm.Contracts = await _context.Contracts
            .Include(c => c.Client)
            .OrderBy(c => c.Client!.Name)
            .ThenBy(c => c.ContractId)
            .Select(c => new SelectListItem
            {
                Text = $"{c.Client!.Name} — Contract #{c.ContractId} ({c.Status})",
                Value = c.ContractId.ToString(),
                Selected = c.ContractId == vm.ContractId
            })
            .ToListAsync();
    }
}