-- One open (Pending or Accepted) assignment per request.
-- Rejected assignments are kept for audit and do not block re-assignment.
CREATE UNIQUE INDEX IF NOT EXISTS ux_request_assignment_one_open
    ON request_assignment (request_id)
    WHERE decision IN ('Pending', 'Accepted');

-- request_status_history is insert-and-read only.
CREATE OR REPLACE FUNCTION forbid_history_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'request_status_history is insert-only (% blocked)', TG_OP;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_history_no_update_delete ON request_status_history;
CREATE TRIGGER trg_history_no_update_delete
    BEFORE UPDATE OR DELETE ON request_status_history
    FOR EACH ROW EXECUTE FUNCTION forbid_history_mutation();