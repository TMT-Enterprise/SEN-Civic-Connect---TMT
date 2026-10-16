ALTER TABLE notification
    ADD COLUMN IF NOT EXISTS channel VARCHAR(20) NOT NULL DEFAULT 'Email',
    ADD COLUMN IF NOT EXISTS new_status VARCHAR(50) NOT NULL DEFAULT 'Unknown',
    ADD COLUMN IF NOT EXISTS failure_reason TEXT;

ALTER TABLE notification ALTER COLUMN new_status DROP DEFAULT;

-- Lets a later sweep find notifications that never left Pending.
CREATE INDEX IF NOT EXISTS ix_notification_pending
    ON notification (created_at)
    WHERE status = 'Pending';