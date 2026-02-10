using JiraLite.Application.Dtos.Issue;
using JiraLite.Application.Dtos.Comment;
using JiraLite.Application.Dtos.History;
using JiraLite.Domain.Enums;

namespace JiraLite.Application.Interfaces;

public interface IIssueService
{
    Task<IssueResponseDto> CreateIssueAsync(CreateIssueDto dto, Guid userId, string userName);
    Task<IssueResponseDto?> UpdateIssueAsync(Guid issueId, UpdateIssueDto dto, Guid userId, string userName);
    Task<IssueResponseDto?> GetIssueByIdAsync(Guid issueId);
    Task<List<IssueResponseDto>> GetAllIssuesAsync();
    Task<List<IssueResponseDto>> GetUserIssuesAsync(Guid userId);
    Task<ExcelUploadResultDto> BulkCreateFromExcelAsync(Stream excelStream, Guid userId, string userName);
    
    // Comment methods
    Task<CommentResponseDto> AddCommentAsync(Guid issueId, CreateCommentDto dto, Guid userId);
    Task<List<CommentResponseDto>> GetIssueCommentsAsync(Guid issueId);
    
    // History methods
    Task<List<HistoryResponseDto>> GetIssueHistoryAsync(Guid issueId);
    
    // Workflow methods
    Task<List<IssueStatus>> GetAllowedTransitionsForIssueAsync(Guid issueId);
}
