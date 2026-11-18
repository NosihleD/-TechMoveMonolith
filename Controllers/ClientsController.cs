using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TechMove.Models;

namespace TechMove.Controllers;

public class ClientsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ClientsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var api = _httpClientFactory.CreateClient("TechMoveApi");

        var clients = await api.GetFromJsonAsync<List<Client>>(
            "api/clients");

        return View(clients ?? new List<Client>());
    }

    public IActionResult Create()
    {
        return View(new Client());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid)
            return View(client);

        var api = _httpClientFactory.CreateClient("TechMoveApi");

        var response = await api.PostAsJsonAsync(
            "api/clients",
            new
            {
                client.Name,
                client.ContactDetails,
                client.Region
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                "Unable to create the client through the API.");

            return View(client);
        }

        return RedirectToAction(nameof(Index));
    }
}
