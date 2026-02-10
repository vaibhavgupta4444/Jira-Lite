using JiraLite.Application.Dtos.User;
using JiraLite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var response = await _authService.RegisterAsync(dto);

            return Ok(new
            {
                success = true,
                message = response.Message,
                data = new
                {
                    email = response.Email,
                    name = response.Name,
                    role = response.Role
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during registration",
                error = ex.Message
            });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var response = await _authService.LoginAsync(dto);

            // Also create server-side session for MVC form compatibility
            HttpContext.Session.SetString("Token", response.Token);
            HttpContext.Session.SetString("UserId", response.UserId);
            HttpContext.Session.SetString("UserName", response.Name);
            HttpContext.Session.SetString("UserEmail", response.Email);
            HttpContext.Session.SetString("UserRole", response.Role);

            return Ok(new
            {
                success = true,
                message = response.Message,
                data = new
                {
                    token = response.Token,
                    userId = response.UserId,
                    email = response.Email,
                    name = response.Name,
                    role = response.Role
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during login",
                error = ex.Message
            });
        }
    }
}
