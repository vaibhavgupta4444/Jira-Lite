using System.ComponentModel.DataAnnotations;
using JiraLite.Domain.Enums;

namespace JiraLite.Models;
public class IssueHistory : Base
{
    [Required]
    public Guid IssueId { get; set; }
    public IssueStatus FromStatus  { get; set; }
    public IssueStatus ToStatus { get; set; } 
}