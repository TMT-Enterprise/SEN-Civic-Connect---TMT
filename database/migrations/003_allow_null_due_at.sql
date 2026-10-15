-- Allow requests to be submitted before an admin assigns a due date.
ALTER TABLE service_request
    ALTER COLUMN due_at DROP NOT NULL;
