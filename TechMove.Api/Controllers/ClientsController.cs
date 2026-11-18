
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Models;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/clients
    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _context.Clients
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.ClientId,
                c.Name,
                c.ContactDetails,
                c.Region
            })
            .ToListAsync();

        return Ok(clients);
    }

    // POST: api/clients
    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] Client client)
    {
        client.ClientId = 0;
        client.Contracts = new List<Contract>();

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            client.ClientId,
            client.Name,
            client.ContactDetails,
            client.Region
        });
    }
}
