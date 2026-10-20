-- Keep requester withdrawal in the request and status-history records.
INSERT INTO request_status (name)
VALUES ('Withdrawn')
ON CONFLICT (name) DO NOTHING;

ALTER TABLE request_status_history
    ADD COLUMN IF NOT EXISTS requester_id BIGINT
    REFERENCES requester(requester_id) ON DELETE RESTRICT;
