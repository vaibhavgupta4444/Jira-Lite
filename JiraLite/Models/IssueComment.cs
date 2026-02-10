using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Models;
public class IssueComment : Base
{
    [Required]
    public Guid IssueId { get; set; }
    [Required]
    public string Comment { get; set; } = string.Empty;
}