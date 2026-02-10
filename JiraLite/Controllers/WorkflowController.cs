using JiraLite.Application.Dtos.Workflow;
using JiraLite.Application.Interfaces;
using JiraLite.Domain.Enums;
using JiraLite.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLite.Controllers;

[SessionAuthorization]
[Authorize(Roles = "Admin")]
public class WorkflowController : Controller
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public async Task<IActionResult> Index()
    {
        var transitions = await _workflowService.GetAllTransitionsAsync();
        ViewBag.AllStatuses = Enum.GetValues<IssueStatus>().ToList();
        return View(transitions);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkflowTransitionDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid workflow transition data.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _workflowService.CreateTransitionAsync(dto, userId);
            TempData["Success"] = $"Workflow transition from {dto.FromStatus} to {dto.ToStatus} created successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while creating the workflow transition.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var success = await _workflowService.ToggleTransitionStatusAsync(id);
        
        if (success)
        {
            TempData["Success"] = "Workflow transition status updated successfully.";
        }
        else
        {
            TempData["Error"] = "Workflow transition not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _workflowService.DeleteTransitionAsync(id);
        
        if (success)
        {
            TempData["Success"] = "Workflow transition deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Workflow transition not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllowedTransitions(IssueStatus fromStatus)
    {
        var allowedStatuses = await _workflowService.GetAllowedTransitionsAsync(fromStatus);
        return Json(allowedStatuses);
    }
}
