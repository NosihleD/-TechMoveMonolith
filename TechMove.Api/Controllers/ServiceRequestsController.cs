using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Models;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _context.ServiceRequests
            .OrderByDescending(s => s.ServiceRequestId)
            .Select(s => new
            {
                s.ServiceRequestId,
                s.ContractId,
                s.Description,
                s.CostUsd,
                s.ExchangeRate,
                s.CostZar,
                s.Status,
                Contract = s.Contract == null ? null : new
                {
                    s.Contract.ContractId,
                    s.Contract.StartDate,
                    s.Contract.EndDate,
                    s.Contract.Status,
                    ClientName = s.Contract.Client == null
                        ? "Unknown"
                        : s.Contract.Client.Name
                }
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _context.ServiceRequests
            .Where(s => s.ServiceRequestId == id)
            .Select(s => new
            {
                s.ServiceRequestId,
                s.ContractId,
                s.Description,
                s.CostUsd,
                s.ExchangeRate,
                s.CostZar,
                s.Status,
                ClientName = s.Contract != null && s.Contract.Client != null
                    ? s.Contract.Client.Name
                    : "Unknown"
            })
            .FirstOrDefaultAsync();

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ServiceRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractId == request.ContractId);

        if (contract == null)
            return BadRequest("The selected contract does not exist.");

        if (contract.Status != ContractStatuses.Draft &&
            contract.Status != ContractStatuses.Active)
            return BadRequest("Cannot create a request for this contract.");

        if (contract.EndDate.Date < DateTime.Today)
            return BadRequest("The contract has expired.");

        if (request.CostUsd <= 0 || request.ExchangeRate <= 0)
            return BadRequest("Cost and exchange rate must be greater than zero.");

        request.CostZar = decimal.Round(
            request.CostUsd * request.ExchangeRate, 2);

        request.Status = "New";
        request.Contract = null;

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = request.ServiceRequestId },
            new
            {
                request.ServiceRequestId,
                request.ContractId,
                request.Description,
                request.CostUsd,
                request.ExchangeRate,
                request.CostZar,
                request.Status
            });
    }
}
