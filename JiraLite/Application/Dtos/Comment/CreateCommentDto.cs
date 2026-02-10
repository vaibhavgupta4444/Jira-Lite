using System.ComponentModel.DataAnnotations;

namespace JiraLite.Application.Dtos.Comment;

public class CreateCommentDto
{
    [Required(ErrorMessage = "Comment text is required")]
    [MinLength(1, ErrorMessage = "Comment cannot be empty")]
    public string Comment { get; set; } = string.Empty;
}
