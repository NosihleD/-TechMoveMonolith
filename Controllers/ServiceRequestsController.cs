
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using TechMove.Models;
using TechMove.Services;
using TechMove.ViewModels;

namespace TechMove.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CurrencyService _currency;
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        IHttpClientFactory httpClientFactory,
        CurrencyService currency,
        ILogger<ServiceRequestsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _currency = currency;
        _logger = logger;
    }

    private HttpClient GetApiClient() =>
        _httpClientFactory.CreateClient("TechMoveApi");

    public async Task<IActionResult> Index()
    {
        var api = GetApiClient();

        var requests = await api.GetFromJsonAsync<List<ServiceRequestApiDto>>(
            "api/servicerequests");

        var result = (requests ?? new())
            .Select(s => new ServiceRequest
            {
                ServiceRequestId = s.ServiceRequestId,
                ContractId = s.ContractId,
                Description = s.Description,
                CostUsd = s.CostUsd,
                ExchangeRate = s.ExchangeRate,
                CostZar = s.CostZar,
                Status = s.Status,
                Contract = new Contract
                {
                    ContractId = s.Contract?.ContractId ?? s.ContractId,
                    Status = s.Contract?.Status ?? "Unknown",
                    Client = new Client
                    {
                        Name = s.Contract?.ClientName ?? "Unknown"
                    }
                }
            }).ToList();

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ServiceRequestCreateViewModel();

        await PopulateContractsAsync(vm);

        try
        {
            vm.ExchangeRate = await _currency.GetUsdToZarRateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve exchange rate.");
            vm.ExchangeRate = 0;
            ViewBag.CurrencyError =
                "The live currency service is unavailable. Please try again.";
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
    {
        ModelState.Remove(nameof(vm.CostZar));
        ModelState.Remove(nameof(vm.ExchangeRate));

        if (ModelState.IsValid)
        {
            try
            {
                vm.ExchangeRate = await _currency.GetUsdToZarRateAsync();
                vm.CostZar = CurrencyService.ConvertUsdToZar(
                    vm.CostUsd, vm.ExchangeRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency calculation failed.");
                ModelState.AddModelError("",
                    "Unable to retrieve the current exchange rate.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateContractsAsync(vm);
            return View(vm);
        }

        try
        {
            var api = GetApiClient();

            var response = await api.PostAsJsonAsync(
                "api/servicerequests",
                new
                {
                    vm.ContractId,
                    vm.Description,
                    vm.CostUsd,
                    vm.ExchangeRate,
                    vm.CostZar
                });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
                await PopulateContractsAsync(vm);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API communication failed.");
            ModelState.AddModelError("",
                "Unable to communicate with the API.");

            await PopulateContractsAsync(vm);
            return View(vm);
        }
    }

    private async Task PopulateContractsAsync(
        ServiceRequestCreateViewModel vm)
    {
        var api = GetApiClient();

        var contracts = await api.GetFromJsonAsync<List<ContractApiDto>>(
            "api/contracts");

        vm.Contracts = (contracts ?? new())
            .Select(c => new SelectListItem
            {
                Text = $"{c.ClientName} - Contract #{c.ContractId} ({c.Status})",
                Value = c.ContractId.ToString(),
                Selected = c.ContractId == vm.ContractId
            }).ToList();
    }

    private sealed class ServiceRequestApiDto
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = "";
        public decimal CostUsd { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal CostZar { get; set; }
        public string Status { get; set; } = "";
        public ContractInfoDto? Contract { get; set; }
    }

    private sealed class ContractInfoDto
    {
        public int ContractId { get; set; }
        public string Status { get; set; } = "";
        public string ClientName { get; set; } = "";
    }

    private sealed class ContractApiDto
    {
        public int ContractId { get; set; }
        public string ClientName { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
