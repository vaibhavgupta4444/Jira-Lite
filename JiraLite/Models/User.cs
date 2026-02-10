using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Models;
public class User : Base
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;

    public List<Issue> Issues { get; set; } = new();
} 