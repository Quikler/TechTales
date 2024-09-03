using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers.Extensions;
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
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

        var user = new UserEntity
        {
            Email = model.Email,
            UserName = model.UserName,
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (createResult.Succeeded)
        {
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }

        // foreach (var error in createResult.Errors)
        // {
        //     ModelState.AddModelError(error.Code, error.Description);
        // }

        this.SetModalMessage("Authorization", createResult.Errors.First().Description);
        return View(model);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

        var user = 
            await _userManager.FindByEmailAsync(model.UserNameOrEmail) ??
            await _userManager.FindByNameAsync(model.UserNameOrEmail);
        
        if (user is null)
        {
            this.SetModalMessage("Authorization", "No username or email found in database.");
            return View(model);
        }

        // todo remember me
        var signInResult = await _signInManager.PasswordSignInAsync(
            user, model.Password, true, true);
        
        if (signInResult.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        
        if (signInResult.IsLockedOut)
        {
            this.SetModalMessage("Authorization", "You have been locked.");
            return View(model);
        }
        
        this.SetModalMessage("Authorization", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}