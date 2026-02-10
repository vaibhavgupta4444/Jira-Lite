using JiraLite.Application.Dtos.Issue;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using JiraLite.Domain.Enums;
using JiraLite.Application.Dtos.Comment;
using JiraLite.Application.Dtos.History;

namespace JiraLite.Application.Services;

public class IssueService : IIssueService
{
    private readonly AppDbContext _context;
    private readonly IWorkflowService _workflowService;

    public IssueService(AppDbContext context, IWorkflowService workflowService)
    {
        _context = context;
        _workflowService = workflowService;
    }

    public async Task<IssueResponseDto> CreateIssueAsync(CreateIssueDto dto, Guid userId, string userName)
    {
        var issue = new Issue
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Priority = dto.Priority,
            UserId = userId,
            Status = Domain.Enums.IssueStatus.Open,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        return MapToResponseDto(issue, userName, userName);
    }

    public async Task<IssueResponseDto?> UpdateIssueAsync(Guid issueId, UpdateIssueDto dto, Guid userId, string userName)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        
        if (issue == null)
        {
            return null;
        }

        var oldStatus = issue.Status;

        if (!string.IsNullOrEmpty(dto.Title))
        {
            issue.Title = dto.Title;
        }

        if (!string.IsNullOrEmpty(dto.Description))
        {
            issue.Description = dto.Description;
        }

        if (dto.Type.HasValue)
        {
            issue.Type = dto.Type.Value;
        }

        if (dto.Priority.HasValue)
        {
            issue.Priority = dto.Priority.Value;
        }

        if (dto.Status.HasValue && dto.Status.Value != oldStatus)
        {
            // Validate workflow transition
            var isAllowed = await _workflowService.IsTransitionAllowedAsync(oldStatus, dto.Status.Value);
            
            if (!isAllowed)
            {
                throw new InvalidOperationException(
                    $"Transition from {oldStatus} to {dto.Status.Value} is not allowed by workflow configuration.");
            }
            
            issue.Status = dto.Status.Value;
            
            var history = new IssueHistory
            {
                IssueId = issueId,
                FromStatus = oldStatus,
                ToStatus = dto.Status.Value,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            _context.IssueHistories.Add(history);
        }

        issue.Updated = DateTime.UtcNow;
        issue.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        var createdByUser = await _context.Users.FindAsync(issue.CreatedBy);

        return MapToResponseDto(issue, createdByUser?.Name ?? "Unknown", userName);
    }

    public async Task<IssueResponseDto?> GetIssueByIdAsync(Guid issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        
        if (issue == null) return null;

        var createdByUser = await _context.Users.FindAsync(issue.CreatedBy);
        var updatedByUser = await _context.Users.FindAsync(issue.UpdatedBy);

        return MapToResponseDto(issue, createdByUser?.Name ?? "Unknown", updatedByUser?.Name ?? "Unknown");
    }

    public async Task<List<IssueResponseDto>> GetAllIssuesAsync()
    {
        var issues = await _context.Issues
            .OrderByDescending(i => i.Created)
            .ToListAsync();

        var userIds = issues
            .SelectMany(i => new[] { i.CreatedBy, i.UpdatedBy })
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        return issues.Select(i => MapToResponseDto(
            i,
            users.GetValueOrDefault(i.CreatedBy, "Unknown"),
            users.GetValueOrDefault(i.UpdatedBy, "Unknown")
        )).ToList();
    }

    public async Task<List<IssueResponseDto>> GetUserIssuesAsync(Guid userId)
    {
        var issues = await _context.Issues
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.Created)
            .ToListAsync();

        var userIds = issues
            .SelectMany(i => new[] { i.CreatedBy, i.UpdatedBy })
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);
    
        return issues.Select(i => MapToResponseDto(
            i,
            users.GetValueOrDefault(i.CreatedBy, "Unknown"),
            users.GetValueOrDefault(i.UpdatedBy, "Unknown")
        )).ToList();
    }

    public async Task<ExcelUploadResultDto> BulkCreateFromExcelAsync(Stream excelStream, Guid userId, string userName)
    {
        var result = new ExcelUploadResultDto();
        
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        
        if (worksheet == null)
        {
            result.Errors.Add(new ExcelRowErrorDto
            {
                RowNumber = 0,
                Title = "Excel File",
                ValidationErrors = new List<string> { "No worksheet found in Excel file" }
            });
            return result;
        }

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        
        if (rowCount <= 1)
        {
            result.Errors.Add(new ExcelRowErrorDto
            {
                RowNumber = 0,
                Title = "Excel File",
                ValidationErrors = new List<string> { "No data rows found in Excel file" }
            });
            return result;
        }

        result.TotalRows = rowCount - 1;

        for (int row = 2; row <= rowCount; row++)
        {
            var errors = new List<string>();
            var title = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? string.Empty;
            var description = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty;
            var typeStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty;
            var priorityStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add("Title is required");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add("Description is required");
            }

            IssueType issueType = IssueType.Task;;
            if (string.IsNullOrWhiteSpace(typeStr))
            {
                errors.Add("Type is required (Task, Bug, Feature, or Improvement)");
            }
            else if (!Enum.TryParse<IssueType>(typeStr, true, out issueType))
            {
                errors.Add($"Invalid Type '{typeStr}'. Must be: Task, Bug, Feature, or Improvement");
            }

            Priority priority = Priority.Low;
            if (string.IsNullOrWhiteSpace(priorityStr))
            {
                errors.Add("Priority is required (Low, Medium, High, or Critical)");
            }
            else if (!Enum.TryParse<Priority>(priorityStr, true, out priority))
            {
                errors.Add($"Invalid Priority '{priorityStr}'. Must be: Low, Medium, High, or Critical");
            }

            if (errors.Any())
            {
                result.Errors.Add(new ExcelRowErrorDto
                {
                    RowNumber = row,
                    Title = string.IsNullOrEmpty(title) ? $"Row {row}" : title,
                    ValidationErrors = errors
                });
                result.FailedCount++;
                continue;
            }

            try
            {
         
                var issue = new Issue
                {
                    Title = title,
                    Description = description,
                    Type = issueType,
                    Priority = priority,
                    UserId = userId,
                    Status = IssueStatus.Open,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                _context.Issues.Add(issue);
                await _context.SaveChangesAsync();

                result.CreatedIssues.Add(MapToResponseDto(issue, userName, userName));
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ExcelRowErrorDto
                {
                    RowNumber = row,
                    Title = title,
                    ValidationErrors = new List<string> { $"Database error: {ex.Message}" }
                });
                result.FailedCount++;
            }
        }

        return result;
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid issueId, CreateCommentDto dto, Guid userId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null)
        {
            throw new InvalidOperationException("Issue not found");
        }

        var comment = new IssueComment
        {
            IssueId = issueId,
            Comment = dto.Comment,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.IssueComments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return MapToCommentResponseDto(comment, user?.Name ?? "Unknown");
    }

    public async Task<List<CommentResponseDto>> GetIssueCommentsAsync(Guid issueId)
    {
        var comments = await _context.IssueComments
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.Created)
            .ToListAsync();

        var userIds = comments.Select(c => c.CreatedBy).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        return comments.Select(c => MapToCommentResponseDto(
            c,
            users.GetValueOrDefault(c.CreatedBy, "Unknown")
        )).ToList();
    }

    public async Task<List<HistoryResponseDto>> GetIssueHistoryAsync(Guid issueId)
    {
        var history = await _context.IssueHistories
            .Where(h => h.IssueId == issueId)
            .OrderByDescending(h => h.Created)
            .ToListAsync();

        var userIds = history.Select(h => h.CreatedBy).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        return history.Select(h => MapToHistoryResponseDto(
            h,
            users.GetValueOrDefault(h.CreatedBy, "Unknown")
        )).ToList();
    }
    
    private static IssueResponseDto MapToResponseDto(Issue issue, string createdByName, string updatedByName)
    {
        return new IssueResponseDto
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            UserId = issue.UserId,
            Type = issue.Type,
            Priority = issue.Priority,
            Status = issue.Status,
            Created = issue.Created,
            Updated = issue.Updated,
            CreatedBy = createdByName,
            UpdatedBy = updatedByName
        };
    }

    private static CommentResponseDto MapToCommentResponseDto(IssueComment comment, string createdByName)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            IssueId = comment.IssueId,
            Comment = comment.Comment,
            Created = comment.Created,
            CreatedBy = createdByName
        };
    }

    private static HistoryResponseDto MapToHistoryResponseDto(IssueHistory history, string createdByName)
    {
        return new HistoryResponseDto
        {
            Id = history.Id,
            IssueId = history.IssueId,
            FromStatus = history.FromStatus,
            ToStatus = history.ToStatus,
            Created = history.Created,
            CreatedBy = createdByName
        };
    }
    public async Task<List<IssueStatus>> GetAllowedTransitionsForIssueAsync(Guid issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        
        if (issue == null)
        {
            throw new InvalidOperationException($"Issue with ID {issueId} not found.");
        }

        return await _workflowService.GetAllowedTransitionsAsync(issue.Status);
    }}
