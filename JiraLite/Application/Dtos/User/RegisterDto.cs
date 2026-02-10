using System.ComponentModel.DataAnnotations;

namespace JiraLite.Application.Dtos.User;
public class RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100, MinimumLength = 8, 
        ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character"
    )]
    public string Password { get; set; }  = string.Empty;
}