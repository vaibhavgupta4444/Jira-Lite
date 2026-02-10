using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.Issue;

public class IssueResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public IssueType Type { get; set; }
    public Priority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
