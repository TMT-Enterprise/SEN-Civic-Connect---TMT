namespace CivicConnect.Requests;

public sealed record StatusTransitionResult(
    bool IsAllowed,
    string? Error);

public sealed class RequestStatusTransitionValidator
{
    public StatusTransitionResult Validate(
        RequestStatus current,
        RequestStatus requested,
        string? completionDescription = null)
    {
        if (!Enum.IsDefined(current) || !Enum.IsDefined(requested))
        {
            return Reject("The current or requested status is invalid.");
        }

        if (current == requested)
        {
            return Reject("The request is already in that status.");
        }

        if (!StatusTransitionMap.IsAllowed(current, requested))
        {
            return Reject(
                $"Transition from {current} to {requested} is not permitted.");
        }

        if (requested == RequestStatus.Closed &&
            string.IsNullOrWhiteSpace(completionDescription))
        {
            return Reject(
                "A valid completion description is required to close the request.");
        }

        return new StatusTransitionResult(true, null);
    }

    private static StatusTransitionResult Reject(string error)
    {
        return new StatusTransitionResult(false, error);
    }
}