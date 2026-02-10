using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.Issue;

public class CreateIssueDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Type is required")]
    public IssueType Type { get; set; }
    
    [Required(ErrorMessage = "Priority is required")]
    public Priority Priority { get; set; }
}
