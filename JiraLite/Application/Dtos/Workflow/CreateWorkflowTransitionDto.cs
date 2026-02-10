using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.Workflow;

public class CreateWorkflowTransitionDto
{
    [Required]
    public IssueStatus FromStatus { get; set; }
    
    [Required]
    public IssueStatus ToStatus { get; set; }
}
