
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Models;
using TechMove.Services;
using TechMove.ViewModels;

namespace TechMove.Controllers;

public class ContractsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FileValidationService _fileValidation;
    private readonly ContractWorkflowService _workflow;
    private readonly IWebHostEnvironment _environment;

    public ContractsController(
        IHttpClientFactory httpClientFactory,
        FileValidationService fileValidation,
        ContractWorkflowService workflow,
        IWebHostEnvironment environment)
    {
        _httpClientFactory = httpClientFactory;
        _fileValidation = fileValidation;
        _workflow = workflow;
        _environment = environment;
    }

    private HttpClient ApiClient =>
        _httpClientFactory.CreateClient("TechMoveApi");

    public async Task<IActionResult> Index(
        DateTime? startDate,
        DateTime? endDate,
        string? status)
    {
        var query = new List<string>();

        if (startDate.HasValue)
            query.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            query.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status)}");

        var url = "api/contracts";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        var contracts = await ApiClient
            .GetFromJsonAsync<List<ContractApiDto>>(url);

        var vm = new ContractIndexViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            Contracts = (contracts ?? new List<ContractApiDto>())
                .Select(MapContract)
                .ToList(),
            Statuses = ContractStatuses.All.Select(
                s => new SelectListItem(s, s, s == status))
        };

        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateClientsAsync();

        return View(new Contract
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            Status = ContractStatuses.Active
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Contract contract,
        IFormFile? signedAgreement)
    {
        try
        {
            _workflow.ValidateContractDates(contract);

            if (signedAgreement is not null)
                _fileValidation.ValidatePdf(signedAgreement);
        }
        catch (Exception ex) when (
            ex is InvalidOperationException or InvalidDataException)
        {
            ModelState.AddModelError("", ex.Message);
        }

        if (!ModelState.IsValid)
        {
            await PopulateClientsAsync(contract.ClientId);
            return View(contract);
        }

        if (signedAgreement is not null)
        {
            contract.SignedAgreementPath =
                await SaveAgreementAsync(signedAgreement);
        }

        var response = await ApiClient.PostAsJsonAsync(
            "api/contracts", contract);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                "The API could not save the contract. Please check the client and contract details.");

            await PopulateClientsAsync(contract.ClientId);
            return View(contract);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await ApiClient.GetFromJsonAsync<ContractApiDto>(
            $"api/contracts/{id}");

        if (dto is null)
            return NotFound();

        return View(MapContract(dto));
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var dto = await ApiClient.GetFromJsonAsync<ContractApiDto>(
            $"api/contracts/{id}");

        if (dto?.SignedAgreementPath is null)
            return NotFound();

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            dto.SignedAgreementPath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(
            fullPath,
            "application/pdf",
            Path.GetFileName(fullPath));
    }

    private async Task PopulateClientsAsync(int? selectedClientId = null)
    {
        var clients = await ApiClient.GetFromJsonAsync<List<Client>>(
            "api/clients") ?? new List<Client>();

        ViewBag.Clients = new SelectList(
            clients,
            "ClientId",
            "Name",
            selectedClientId);
    }

    private Contract MapContract(ContractApiDto dto)
    {
        return new Contract
        {
            ContractId = dto.ContractId,
            ClientId = dto.ClientId,
            Client = new Client
            {
                ClientId = dto.ClientId,
                Name = dto.ClientName ?? "Unknown client"
            },
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            ServiceLevel = dto.ServiceLevel,
            SignedAgreementPath = dto.SignedAgreementPath,
            ServiceRequests = dto.ServiceRequests
                .Select(s => new ServiceRequest
                {
                    ServiceRequestId = s.ServiceRequestId,
                    Description = s.Description,
                    Status = s.Status,
                    CostUsd = s.CostUsd,
                    ExchangeRate = s.ExchangeRate,
                    CostZar = s.CostZar
                })
                .ToList()
        };
    }

    private async Task<string> SaveAgreementAsync(IFormFile file)
    {
        var relativeFolder = Path.Combine("Uploads", "Agreements");

        var folder = Path.Combine(
            _environment.ContentRootPath,
            relativeFolder);

        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew);

        await file.CopyToAsync(stream);

        return Path.Combine(relativeFolder, fileName)
            .Replace('\\', '/');
    }
}

public class ContractApiDto
{
    public int ContractId { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string? SignedAgreementPath { get; set; }

    public List<ServiceRequestApiDto> ServiceRequests { get; set; }
        = new();
}

public class ServiceRequestApiDto
{
    public int ServiceRequestId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal CostUsd { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal CostZar { get; set; }
}
