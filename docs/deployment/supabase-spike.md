# Supabase Connectivity Spike

**Date:** 2026-10-08  
**Author:** Mutshidzi  
**Task:** S1-10

---

## Project Details

| Field | Value |
|-------|-------|
| Region | Central EU (Frankfurt) — eu-central-1 |
| Postgres Version | 17.11.0.003 |
| Connection Mode | Direct connection |
| Connection Format | .NET key=value (Npgsql) |

---

## Connection String Format

Supabase provides a URI-style string by default. We use the .NET key=value format for Npgsql compatibility:
Host=db.qszjbviuzgchkxhhyzjt.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=REDACTED;SSL Mode=Require;Trust Server Certificate=true