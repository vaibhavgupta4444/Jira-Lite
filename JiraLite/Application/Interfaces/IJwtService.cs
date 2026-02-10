using JiraLite.Models;

namespace JiraLite.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
