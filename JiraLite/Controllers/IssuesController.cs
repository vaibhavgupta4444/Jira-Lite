using JiraLite.Application.Dtos.Issue;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JiraLite.Application.Dtos.Comment;

namespace JiraLite.Controllers;

[SessionAuthorization]
public class IssuesController : Controller
{
    private readonly IIssueService _issueService;
    private readonly IJwtService _jwtService;

    public IssuesController(IIssueService issueService, IJwtService jwtService)
    {
        _issueService = issueService;
        _jwtService = jwtService;
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult BulkUpload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIssueDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        await _issueService.CreateIssueAsync(dto, userId, userName);
        
        TempData["SuccessMessage"] = "Issue created successfully!";
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        
        if (issue == null)
        {
            return NotFound();
        }

        return View(issue);
    }
    
    public async Task<IActionResult> Edit(Guid id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        
        if (issue == null)
        {
            return NotFound();
        }

        var updateDto = new UpdateIssueDto
        {
            Title = issue.Title,
            Description = issue.Description,
            Type = issue.Type,
            Priority = issue.Priority,
            Status = issue.Status
        };

        ViewBag.IssueId = id;
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateIssueDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.IssueId = id;
            return View(dto);
        }

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        try
        {
            var result = await _issueService.UpdateIssueAsync(id, dto, userId, userName);
            
            if (result == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Issue updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Status", ex.Message);
            ViewBag.IssueId = id;
            return View(dto);
        }
    }
    
    [HttpGet]
    [Route("api/issues")]
    public async Task<IActionResult> GetAllIssues()
    {
        var issues = await _issueService.GetAllIssuesAsync();
        return Ok(issues);
    }

    [HttpGet]
    [Route("api/issues/my")]
    public async Task<IActionResult> GetMyIssues()
    {
        var userId = GetCurrentUserId();
        var issues = await _issueService.GetUserIssuesAsync(userId);
        return Ok(issues);
    }

    [HttpGet]
    [Route("api/issues/{id}")]
    public async Task<IActionResult> GetIssue(Guid id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        
        if (issue == null)
        {
            return NotFound(new { message = "Issue not found" });
        }

        return Ok(issue);
    }

    [HttpPost]
    [Route("api/issues")]
    public async Task<IActionResult> CreateIssue([FromBody] CreateIssueDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        var issue = await _issueService.CreateIssueAsync(dto, userId, userName);
        
        return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, issue);
    }

    [HttpPut]
    [Route("api/issues/{id}")]
    public async Task<IActionResult> UpdateIssue(Guid id, [FromBody] UpdateIssueDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        var result = await _issueService.UpdateIssueAsync(id, dto, userId, userName);
        
        if (result == null)
        {
            return NotFound(new { message = "Issue not found" });
        }

        return Ok(result);
    }

    [HttpPost]
    [Route("api/issues/upload-excel")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (fileExtension != ".xlsx" && fileExtension != ".xls")
        {
            return BadRequest(new { message = "Invalid file format. Please upload an Excel file (.xlsx or .xls)" });
        }

        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var result = await _issueService.BulkCreateFromExcelAsync(stream, userId, userName);

            return Ok(new
            {
                success = true,
                message = $"Upload completed. {result.SuccessCount} issues created, {result.FailedCount} failed.",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing the Excel file",
                error = ex.Message
            });
        }
    }
    
    [HttpPost]
    [Route("api/issues/{issueId}/comments")]
    public async Task<IActionResult> AddComment(Guid issueId, [FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var comment = await _issueService.AddCommentAsync(issueId, dto, userId);
            
            return Ok(new
            {
                success = true,
                message = "Comment added successfully",
                data = comment
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while adding the comment",
                error = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("api/issues/{issueId}/comments")]
    public async Task<IActionResult> GetComments(Guid issueId)
    {
        try
        {
            var comments = await _issueService.GetIssueCommentsAsync(issueId);
            return Ok(new
            {
                success = true,
                data = comments
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching comments",
                error = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("api/issues/{issueId}/history")]
    public async Task<IActionResult> GetHistory(Guid issueId)
    {
        try
        {
            var history = await _issueService.GetIssueHistoryAsync(issueId);
            return Ok(new
            {
                success = true,
                data = history
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching history",
                error = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("api/issues/{issueId}/allowed-transitions")]
    public async Task<IActionResult> GetAllowedTransitions(Guid issueId)
    {
        try
        {
            var allowedStatuses = await _issueService.GetAllowedTransitionsForIssueAsync(issueId);
            return Ok(new
            {
                success = true,
                data = allowedStatuses
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching allowed transitions",
                error = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("api/issues/{issueId}/transition")]
    public async Task<IActionResult> TransitionStatus(Guid issueId, [FromBody] UpdateIssueDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            var result = await _issueService.UpdateIssueAsync(issueId, dto, userId, userName);
            
            if (result == null)
            {
                return NotFound(new { success = false, message = "Issue not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Status updated successfully",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while updating status",
                error = ex.Message
            });
        }
    }
    
    private (Guid userId, string userName) GetUserInfoFromToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = _jwtService.ValidateToken(token);
            
            if (principal != null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userNameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
                
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    return (userId, userNameClaim ?? "Unknown");
                }
            }
        }
 
        var sessionUserId = HttpContext.Session.GetString("UserId");
        var sessionUserName = HttpContext.Session.GetString("UserName");
        
        if (!string.IsNullOrEmpty(sessionUserId) && Guid.TryParse(sessionUserId, out var sessionId))
        {
            return (sessionId, sessionUserName ?? "Unknown");
        }
        
        return (Guid.Empty, "Unknown");
    }
    
    private Guid GetCurrentUserId()
    {
        return GetUserInfoFromToken().userId;
    }

    private string GetCurrentUserName()
    {
        return GetUserInfoFromToken().userName;
    }
}
