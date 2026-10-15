using CivicConnect.Data.Entities;
namespace CivicConnect.Data;

public class ServiceRequest
{
    public long RequestId { get; set; }

    public long RequesterId { get; set; }

    public long CategoryId { get; set; }

    public long? AssignedEmployeeId { get; set; }

    public long CurrentStatusId { get; set; }

    public RequestStatusEntity? CurrentStatus { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
