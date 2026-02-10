namespace JiraLite.Models;
public class Base
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "System";
    public string UpdatedBy { get; set; } = "System";
}