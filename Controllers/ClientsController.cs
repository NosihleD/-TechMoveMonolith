using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Controllers;

public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ClientsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Clients.OrderBy(c => c.Name).ToListAsync());

    public IActionResult Create() => View(new Client());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid) return View(client);
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
