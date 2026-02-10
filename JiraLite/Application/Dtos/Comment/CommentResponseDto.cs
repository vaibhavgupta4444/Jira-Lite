namespace JiraLite.Application.Dtos.Comment;

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
