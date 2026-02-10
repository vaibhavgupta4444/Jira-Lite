using JiraLite.Application.Dtos.Workflow;
using JiraLite.Application.Interfaces;
using JiraLite.Domain.Enums;
using JiraLite.Infrastructure.Data;
using JiraLite.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraLite.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _context;

    public WorkflowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowTransitionDto>> GetAllTransitionsAsync()
    {
        var transitions = await _context.WorkflowTransitions
            .OrderBy(wt => wt.FromStatus)
            .ThenBy(wt => wt.ToStatus)
            .ToListAsync();

        return transitions.Select(MapToDto).ToList();
    }

    public async Task<WorkflowTransitionDto?> GetTransitionByIdAsync(Guid id)
    {
        var transition = await _context.WorkflowTransitions.FindAsync(id);
        return transition == null ? null : MapToDto(transition);
    }

    public async Task<WorkflowTransitionDto> CreateTransitionAsync(CreateWorkflowTransitionDto dto, Guid userId)
    {
        // Check if transition already exists
        var existingTransition = await _context.WorkflowTransitions
            .FirstOrDefaultAsync(wt => wt.FromStatus == dto.FromStatus && wt.ToStatus == dto.ToStatus);

        if (existingTransition != null)
        {
            throw new InvalidOperationException($"Transition from {dto.FromStatus} to {dto.ToStatus} already exists.");
        }

        var transition = new WorkflowTransition
        {
            FromStatus = dto.FromStatus,
            ToStatus = dto.ToStatus,
            IsActive = true,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.WorkflowTransitions.Add(transition);
        await _context.SaveChangesAsync();

        return MapToDto(transition);
    }

    public async Task<bool> DeleteTransitionAsync(Guid id)
    {
        var transition = await _context.WorkflowTransitions.FindAsync(id);
        
        if (transition == null)
        {
            return false;
        }

        _context.WorkflowTransitions.Remove(transition);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleTransitionStatusAsync(Guid id)
    {
        var transition = await _context.WorkflowTransitions.FindAsync(id);
        
        if (transition == null)
        {
            return false;
        }

        transition.IsActive = !transition.IsActive;
        transition.Updated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsTransitionAllowedAsync(IssueStatus fromStatus, IssueStatus toStatus)
    {
        // Check if any workflow transitions are defined
        var hasWorkflows = await _context.WorkflowTransitions.AnyAsync();
        
        if (!hasWorkflows)
        {
            // If no workflows are defined, allow all transitions (backward compatibility)
            return true;
        }

        // If the status is not changing, allow it
        if (fromStatus == toStatus)
        {
            return true;
        }

        // Check if transition is defined and active
        return await _context.WorkflowTransitions
            .AnyAsync(wt => wt.FromStatus == fromStatus 
                         && wt.ToStatus == toStatus 
                         && wt.IsActive);
    }

    public async Task<List<IssueStatus>> GetAllowedTransitionsAsync(IssueStatus fromStatus)
    {
        var hasWorkflows = await _context.WorkflowTransitions.AnyAsync();
        
        if (!hasWorkflows)
        {
            // If no workflows are defined, return all statuses
            return Enum.GetValues<IssueStatus>().ToList();
        }

        var allowedTransitions = await _context.WorkflowTransitions
            .Where(wt => wt.FromStatus == fromStatus && wt.IsActive)
            .Select(wt => wt.ToStatus)
            .ToListAsync();

        // Always allow staying in the same status
        if (!allowedTransitions.Contains(fromStatus))
        {
            allowedTransitions.Add(fromStatus);
        }

        return allowedTransitions;
    }

    private static WorkflowTransitionDto MapToDto(WorkflowTransition transition)
    {
        return new WorkflowTransitionDto
        {
            Id = transition.Id,
            FromStatus = transition.FromStatus,
            FromStatusName = transition.FromStatus.ToString(),
            ToStatus = transition.ToStatus,
            ToStatusName = transition.ToStatus.ToString(),
            IsActive = transition.IsActive
        };
    }
}
