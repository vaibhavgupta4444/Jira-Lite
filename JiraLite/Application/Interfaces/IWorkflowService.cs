using JiraLite.Application.Dtos.Workflow;
using JiraLite.Domain.Enums;

namespace JiraLite.Application.Interfaces;

public interface IWorkflowService
{
    Task<List<WorkflowTransitionDto>> GetAllTransitionsAsync();
    Task<WorkflowTransitionDto?> GetTransitionByIdAsync(Guid id);
    Task<WorkflowTransitionDto> CreateTransitionAsync(CreateWorkflowTransitionDto dto, Guid userId);
    Task<bool> DeleteTransitionAsync(Guid id);
    Task<bool> ToggleTransitionStatusAsync(Guid id);
    Task<bool> IsTransitionAllowedAsync(IssueStatus fromStatus, IssueStatus toStatus);
    Task<List<IssueStatus>> GetAllowedTransitionsAsync(IssueStatus fromStatus);
}
