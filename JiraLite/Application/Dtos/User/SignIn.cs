using System.ComponentModel.DataAnnotations;

namespace JiraLite.Application.Dtos.User;
public class SignIn
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}