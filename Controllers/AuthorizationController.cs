using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Models;

namespace TechTales.Controllers;

public class AuthorizationController : Controller
{
    private readonly ILogger<AuthorizationController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;

    public AuthorizationController(ILogger<AuthorizationController> logger,
        AppDbContext context, UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Signup()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return Forbid();
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new UserEntity
        {
            Email = model.Email,
            UserName = model.UserName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return Forbid();
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail) 
            ?? await _userManager.FindByNameAsync(model.UserNameOrEmail);
        
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // todo remember me
        var signInResult = await _signInManager.PasswordSignInAsync(
            user, model.Password, true, true); // 3-d arg is remember me?
        
        if (signInResult.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        
        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "You have been locked.");
            return View();
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        //HttpContext.Session.Clear(); // Clear session
        Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Clear cookies
        return RedirectToAction("Index", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
