using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Models;

public class WorkflowTransition : Base
{
    [Required]
    public IssueStatus FromStatus { get; set; }
    
    [Required]
    public IssueStatus ToStatus { get; set; }
    
    public bool IsActive { get; set; } = true;
}
