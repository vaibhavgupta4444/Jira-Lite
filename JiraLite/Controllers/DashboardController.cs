using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Controllers;

[SessionAuthorization]
public class DashboardController : Controller
{
    private readonly IIssueService _issueService;

    public DashboardController(IIssueService issueService)
    {
        _issueService = issueService;
    }
    
    public async Task<IActionResult> Index()
    {
        var issues = await _issueService.GetAllIssuesAsync();
        return View(issues);
    }
}
