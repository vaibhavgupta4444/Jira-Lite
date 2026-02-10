using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.Workflow;

public class WorkflowTransitionDto
{
    public Guid Id { get; set; }
    public IssueStatus FromStatus { get; set; }
    public string FromStatusName { get; set; } = string.Empty;
    public IssueStatus ToStatus { get; set; }
    public string ToStatusName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
