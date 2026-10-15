namespace CivicConnect.Data.Entities;

public class RequestStatusEntity
{
    public long StatusId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
}