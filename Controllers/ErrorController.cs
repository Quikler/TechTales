
using Microsoft.AspNetCore.Mvc;

namespace TechTales.Controllers;

public class ErrorController : Controller
{
    public new IActionResult NotFound()
    {
        return View();
    }

    public IActionResult InternalServer()
    {
        return View();
    }

    public IActionResult Forbidden()
    {
        return View();
    }
}