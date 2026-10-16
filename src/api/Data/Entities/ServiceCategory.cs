namespace CivicConnect.Data.Entities;

public class ServiceCategory
{
    public long CategoryId { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}