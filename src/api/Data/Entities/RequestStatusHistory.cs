namespace CivicConnect.Data.Entities;

public class RequestStatusHistory
{
    public long HistoryId { get; set; }
    public long RequestId { get; set; }
    public long? EmployeeId { get; set; }
    public long? RequesterId { get; set; }
    public long StatusId { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}