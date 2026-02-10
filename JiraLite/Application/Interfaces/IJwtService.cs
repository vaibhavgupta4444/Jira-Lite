using JiraLite.Models;
using System.Security.Claims;

namespace JiraLite.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
