using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechTales.Models;

namespace TechTales.Controllers;

public class AuthorizationController(ILogger<AuthorizationController> logger) : Controller
{
    private readonly ILogger<AuthorizationController> _logger = logger;

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Signup()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
