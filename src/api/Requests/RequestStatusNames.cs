namespace CivicConnect.Requests;

/// <summary>Maps the status names stored in request_status to the RequestStatus enum.</summary>
internal static class RequestStatusNames
{
    public static bool TryParse(string? name, out RequestStatus status)
    {
        switch (name)
        {
            case "Submitted":
                status = RequestStatus.Submitted;
                return true;
            case "Assigned":
                status = RequestStatus.Assigned;
                return true;
            case "In Progress":
                status = RequestStatus.InProgress;
                return true;
            case "Closed":
                status = RequestStatus.Closed;
                return true;
            default:
                // Includes "Resolved", which the database seeds but the enum does not have yet, I'll bring this one up to you guys in a meeting with you guys
                status = default;
                return false;
        }
    }
}