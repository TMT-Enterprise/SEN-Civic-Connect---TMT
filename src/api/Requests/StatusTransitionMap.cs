namespace CivicConnect.Requests;

public static class StatusTransitionMap
{
    private static readonly HashSet<(RequestStatus Current, RequestStatus Next)>
        AllowedTransitions =
        [
            (RequestStatus.Submitted, RequestStatus.Assigned),
            (RequestStatus.Assigned, RequestStatus.InProgress),
            (RequestStatus.InProgress, RequestStatus.Closed)
        ];

    public static bool IsAllowed(
        RequestStatus current,
        RequestStatus requested)
    {
        return AllowedTransitions.Contains((current, requested));
    }
}