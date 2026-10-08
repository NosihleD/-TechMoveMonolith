using Microsoft.AspNetCore.Mvc;

namespace TechMove.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
