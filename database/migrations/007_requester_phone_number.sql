ALTER TABLE public.requester
    ADD COLUMN IF NOT EXISTS phone_number VARCHAR(30);