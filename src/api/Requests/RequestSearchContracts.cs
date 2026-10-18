namespace CivicConnect.Requests.Requests;

/// <summary>Filters applied to the staff and admin request queues.</summary>
public sealed record RequestSearchFilters(
    string? Search = null,
    string? Id = null,
    string? Status = null,
    RequestSearchDateRange? DueDateRange = null,
    string? Category = null,
    string? Assignee = null,
    long? CategoryId = null,
    long? AssignedEmployeeId = null,
    bool Unassigned = false);

/// <summary>A page-free response for a filtered request queue.</summary>
public sealed record RequestSearchResponse(
    IReadOnlyList<RequestSearchItemDto> Requests,
    string? Message);

/// <summary>A service request shown in a staff or admin request queue.</summary>
public sealed record RequestSearchItemDto(
    long RequestId,
    long CategoryId,
    string Category,
    long? AssignedEmployeeId,
    string? Assignee,
    string Status,
    string Description,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ClosedAt,
    string CurrentStatus,
    string? AssignmentDecision);

/// <summary>Parses ISO date prefixes into UTC half-open timestamp ranges.</summary>
public sealed record RequestSearchDateRange(
    DateTimeOffset Start,
    DateTimeOffset EndExclusive)
{
    public static bool TryParse(string? value, out RequestSearchDateRange range)
    {
        range = default!;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        DateOnly startDate;
        DateOnly endDate;

        if (DateOnly.TryParseExact(
                text,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out startDate))
        {
            if (startDate == DateOnly.MaxValue)
            {
                return false;
            }

            endDate = startDate.AddDays(1);
        }
        else if (text.Length == 7
            && int.TryParse(text.AsSpan(0, 4), out var year)
            && int.TryParse(text.AsSpan(5, 2), out var month)
            && text[4] == '-'
            && year is >= 1 and < 9999
            && month is >= 1 and <= 12)
        {
            startDate = new DateOnly(year, month, 1);
            endDate = startDate.AddMonths(1);
        }
        else if (text.Length == 4
            && int.TryParse(text, out year)
            && year is >= 1 and < 9999)
        {
            startDate = new DateOnly(year, 1, 1);
            endDate = startDate.AddYears(1);
        }
        else
        {
            return false;
        }

        range = new RequestSearchDateRange(
            new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(endDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
        return true;
    }
}