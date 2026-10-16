using CivicConnect.Data.Entities;

namespace CivicConnect.Requests;

// Maps request_status rows to the RequestStatus enum and back.
internal sealed class StatusLookup
{
    private readonly Dictionary<long, RequestStatus> _byId = [];
    private readonly Dictionary<RequestStatus, long> _byStatus = [];

    public StatusLookup(IEnumerable<RequestStatusEntity> rows)
    {
        foreach (var row in rows)
        {
            // Rows the enum doesn't know about (e.g. "Resolved") are skipped.
            if (RequestStatusNames.TryParse(row.Name, out var status))
            {
                _byId[row.StatusId] = status;
                _byStatus[status] = row.StatusId;
            }
        }
    }

    public bool TryGetStatus(long statusId, out RequestStatus status) =>
        _byId.TryGetValue(statusId, out status);

    public long IdOf(RequestStatus status) => _byStatus[status];
}