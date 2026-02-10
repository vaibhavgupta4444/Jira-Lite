using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Application.Dtos.User;
using Microsoft.EntityFrameworkCore;
using JiraLite.Infrastructure.Security;
using JiraLite.Models;

namespace JiraLite.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
 
    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already exists");
        
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = PasswordHasher.Hash(dto.Password)
        };
        
        // Set CreatedBy and UpdatedBy after user is created to use its own ID
        user.CreatedBy = user.Id;
        user.UpdatedBy = user.Id;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Message = "Registration successful. Please login.",
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            throw new InvalidOperationException("Invalid email or password");

        if (!PasswordHasher.Verify(dto.Password, user.Password))
            throw new InvalidOperationException("Invalid email or password");

        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id.ToString(),
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            Message = "Login successful"
        };
    }
}