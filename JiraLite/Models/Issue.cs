using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Models;

public class Issue : Base
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    public IssueType Type { get; set; } = IssueType.Task;
    public Priority Priority { get; set; } = Priority.Low;
    public IssueStatus Status { get; set; } = IssueStatus.Open;

    public List<IssueComment> Comments { get; set; } = new();
    public List<IssueHistory> History { get; set; } = new();
}