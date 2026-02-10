using JiraLite.Domain.Enums;

namespace JiraLite.Application.Dtos.History;

public class HistoryResponseDto
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public IssueStatus FromStatus { get; set; }
    public IssueStatus ToStatus { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string FromStatusName => FromStatus.ToString();
    public string ToStatusName => ToStatus.ToString();
}
