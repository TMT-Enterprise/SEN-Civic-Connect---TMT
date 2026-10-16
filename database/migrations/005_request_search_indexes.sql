CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX IF NOT EXISTS ix_service_request_due_at
    ON service_request (due_at);

CREATE INDEX IF NOT EXISTS ix_service_request_id_trgm
    ON service_request USING gin ((request_id::text) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_request_status_name_trgm
    ON request_status USING gin (name gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_service_category_type_trgm
    ON service_category USING gin (type gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_employee_name_trgm
    ON employee USING gin (name gin_trgm_ops);
