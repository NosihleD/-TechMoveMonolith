using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Repositories;
using TechMove.Models;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractRepository _repository;

    public ContractsController(IContractRepository repository)
    {
        _repository = repository;
    }

    // GET: api/contracts
    [HttpGet]
    public async Task<IActionResult> GetContracts(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? status)
    {
        if (startDate.HasValue && endDate.HasValue &&
            startDate.Value > endDate.Value)
        {
            return BadRequest("Start date cannot be after end date.");
        }

        var contracts = await _repository.GetAllAsync(
            startDate, endDate, status);

        var result = contracts.Select(c => new
        {
            c.ContractId,
            c.ClientId,
            ClientName = c.Client?.Name,
            c.StartDate,
            c.EndDate,
            c.Status,
            c.ServiceLevel,
            c.SignedAgreementPath
        });

        return Ok(result);
    }

    // GET: api/contracts/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetContract(int id)
    {
        var contract = await _repository.GetByIdAsync(id);

        if (contract == null)
            return NotFound();

        return Ok(new
        {
            contract.ContractId,
            contract.ClientId,
            ClientName = contract.Client?.Name,
            contract.StartDate,
            contract.EndDate,
            contract.Status,
            contract.ServiceLevel,
            contract.SignedAgreementPath,
            ServiceRequests = contract.ServiceRequests.Select(s => new
            {
                s.ServiceRequestId,
                s.Description,
                s.Status,
                s.CostUsd,
                s.ExchangeRate,
                s.CostZar
            })
        });
    }

    // POST: api/contracts
    [HttpPost]
    public async Task<IActionResult> CreateContract(
        [FromBody] Contract contract)
    {
        if (contract.StartDate > contract.EndDate)
            return BadRequest("End date must be after start date.");

        if (!ContractStatuses.All.Contains(contract.Status))
            return BadRequest("Invalid contract status.");

        contract.ContractId = 0;
        contract.Client = null;
        contract.ServiceRequests = new List<ServiceRequest>();

        var created = await _repository.AddAsync(contract);

        return CreatedAtAction(
            nameof(GetContract),
            new { id = created.ContractId },
            created);
    }

    // PATCH: api/contracts/5/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateContractStatusRequest request)
    {
        if (request == null ||
            !ContractStatuses.All.Contains(request.Status))
        {
            return BadRequest(
                "Status must be Draft, Active, Expired or On Hold.");
        }

        var updated = await _repository.UpdateStatusAsync(
            id, request.Status);

        if (!updated)
            return NotFound();

        return Ok(new
        {
            ContractId = id,
            Status = request.Status
        });
    }
}

public class UpdateContractStatusRequest
{
    public string Status { get; set; } = string.Empty;
}