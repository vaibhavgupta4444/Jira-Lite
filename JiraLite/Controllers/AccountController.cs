using JiraLite.Application.Dtos.User;
using JiraLite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _authService.RegisterAsync(model);
            TempData["SuccessMessage"] = response.Message;
            return RedirectToAction(nameof(Login));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            return View(model);
        }
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("Token") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _authService.LoginAsync(model);

            HttpContext.Session.SetString("Token", response.Token);
            HttpContext.Session.SetString("UserId", response.UserId);
            HttpContext.Session.SetString("UserName", response.Name);
            HttpContext.Session.SetString("UserEmail", response.Email);
            HttpContext.Session.SetString("UserRole", response.Role);

            TempData["SuccessMessage"] = response.Message;
            return RedirectToAction("Index", "Dashboard");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
            return View(model);
        }


    }

    [HttpGet]
    public IActionResult Logout()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LogoutConfirm()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction(nameof(Login));
    }
}
