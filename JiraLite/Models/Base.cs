namespace JiraLite.Models;
public class Base
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public string CreatedBy { get; set; } = "System";
    public string UpdatedBy { get; set; } = "System";
}