using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.Issue;

public class UpdateIssueDto
{
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string? Title { get; set; }
    
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }
    
    public IssueType? Type { get; set; }
    
    public Priority? Priority { get; set; }
    
    public IssueStatus? Status { get; set; }
}
