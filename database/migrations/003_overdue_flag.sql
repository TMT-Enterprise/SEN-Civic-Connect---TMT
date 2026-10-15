-- flagged the request. current_status_id is never touched, so the underlying status is preserved.
ALTER TABLE service_request
    ADD COLUMN IF NOT EXISTS overdue_at TIMESTAMPTZ;

-- Candidate lookup for the processor: unflagged requests by due date.
CREATE INDEX IF NOT EXISTS ix_request_overdue_candidates
    ON service_request (due_at)
    WHERE overdue_at IS NULL;