namespace CivicConnect.Requests;

/// <summary>Maps persisted request status names to the domain enum.</summary>
internal static class RequestStatusNames
{
    public static bool TryParse(string? name, out RequestStatus status)
    {
        switch (name)
        {
            case "Submitted": status = RequestStatus.Submitted; return true;
            case "Assigned": status = RequestStatus.Assigned; return true;
            case "In Progress": status = RequestStatus.InProgress; return true;
            case "Resolved": status = RequestStatus.Resolved; return true;
            case "Closed": status = RequestStatus.Closed; return true;
            default: status = default; return false;
        }
    }
}
