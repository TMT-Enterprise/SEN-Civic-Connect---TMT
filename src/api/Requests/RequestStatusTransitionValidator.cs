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

    public StatusTransitionResult ValidateOverdueFlag(
        RequestStatus current,
        DateTimeOffset? dueAt,
        DateTimeOffset now,
        bool alreadyFlagged)
    {
        if (!Enum.IsDefined(current))
        {
            return Reject("The current status is invalid.");
        }

        if (current is RequestStatus.Resolved or RequestStatus.Closed)
        {
            return Reject("A resolved or closed request cannot become overdue.");
        }

        if (alreadyFlagged)
        {
            return Reject("The request is already flagged overdue.");
        }

        if (dueAt is null)
        {
            return Reject("The request has no due date.");
        }

        // Overdue once the due time has passed: at exactly the due time it is still on time.
        if (now <= dueAt.Value)
        {
            return Reject("The request is not past its due date yet.");
        }

        return new StatusTransitionResult(true, null);
    }

    private static StatusTransitionResult Reject(string error)
    {
        return new StatusTransitionResult(false, error);
    }
}