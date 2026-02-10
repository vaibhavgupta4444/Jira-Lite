namespace JiraLite.Application.Dtos.Issue;

public class ExcelUploadResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<IssueResponseDto> CreatedIssues { get; set; } = new();
    public List<ExcelRowErrorDto> Errors { get; set; } = new();
}

public class ExcelRowErrorDto
{
    public int RowNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
}
