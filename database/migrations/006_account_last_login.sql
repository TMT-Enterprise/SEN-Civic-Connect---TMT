ALTER TABLE public.requester
    ADD COLUMN IF NOT EXISTS last_login_at TIMESTAMPTZ;

ALTER TABLE public.employee
    ADD COLUMN IF NOT EXISTS last_login_at TIMESTAMPTZ;