---
status: resolved
trigger: "supabase-sessions-table-missing — SessionSyncService fails with PGRST205"
created: 2026-03-31T00:00:00Z
updated: 2026-03-31T00:00:00Z
---

## Current Focus

hypothesis: confirmed — Supabase tables `sessions` and `events` were never created
test: read SessionSyncService.cs, SessionSupabase.cs, EventSupabase.cs and 05.2-RESEARCH.md
expecting: exact column names match a SQL migration script
next_action: DONE — SQL migration produced, user must execute in Supabase SQL editor

## Symptoms

expected: SessionSyncService uploads sessions to Supabase successfully after save
actual: Every upload/download attempt fails with PostgrestException PGRST205 — the `public.sessions` table does not exist in Supabase
errors: [SessionSyncService] Retry attempt 1 failed: {"code":"PGRST205","details":null,"hint":null,"message":"Could not find the table 'public.sessions' in the schema cache"}
reproduction: Log in and save a session — upload fires immediately, fails, retries every 60s/120s etc.
started: Phase 05.2 was just implemented. Tables were never created in Supabase (listed as prerequisite in VERIFICATION.md but not done yet).

## Eliminated

- hypothesis: code bug in SessionSyncService (wrong table name / wrong model attributes)
  evidence: SessionSupabase is decorated [Table("sessions")] and EventSupabase is [Table("events")], exactly matching the expected schema; no code defect found
  timestamp: 2026-03-31T00:00:00Z

- hypothesis: RLS policy misconfiguration blocking queries
  evidence: PGRST205 is a schema-cache miss, not a 403/RLS error; the table simply does not exist yet
  timestamp: 2026-03-31T00:00:00Z

## Evidence

- timestamp: 2026-03-31T00:00:00Z
  checked: AclsTracker/Models/SessionSupabase.cs
  found: columns id, user_id, patient_name, patient_last_name, patient_dni, session_start_time, session_end_time, created_at
  implication: SQL table must expose exactly these column names

- timestamp: 2026-03-31T00:00:00Z
  checked: AclsTracker/Models/EventSupabase.cs
  found: columns id, session_id, timestamp, elapsed_ticks, event_type, description, details
  implication: SQL events table must expose exactly these column names

- timestamp: 2026-03-31T00:00:00Z
  checked: AclsTracker/Services/Sync/SessionSyncService.cs
  found: UploadSessionInternalAsync inserts SessionSupabase then batches EventSupabase (100 rows/batch); DownloadUserSessionsAsync filters sessions WHERE user_id == userId then fetches events WHERE session_id == remote.Id
  implication: RLS must allow authenticated INSERT on sessions+events and SELECT on sessions+events; no DELETE or UPDATE calls in this phase

- timestamp: 2026-03-31T00:00:00Z
  checked: 05.2-RESEARCH.md § Supabase Table Creation SQL
  found: full schema and RLS policy SQL defined during research, including (SELECT auth.uid())::text cast for performance and ON DELETE CASCADE FK from events to sessions
  implication: migration script can be built directly from research SQL, adjusted to confirmed column types

## Resolution

root_cause: The Supabase PostgreSQL tables `sessions` and `events` were never created. The C# code (SessionSyncService, SessionSupabase, EventSupabase) is complete and correct — it references tables that do not yet exist in the remote database, causing PGRST205 on every Postgrest call.

fix: Run the SQL migration below in the Supabase project's SQL editor (Dashboard → SQL Editor → New query → paste → Run). No code changes needed.

verification: After executing the migration, log in and save a session. The [SessionSyncService] Upload succeeded log line should appear. Logging out then back in should trigger download and show the ☁️ indicator in Historial.

files_changed: []

---

## SQL Migration to Execute in Supabase

```sql
-- =============================================================
-- ACLS Tracker — Phase 05.2 Migration
-- Run once in: Supabase Dashboard → SQL Editor → New query
-- =============================================================

-- ── 1. sessions table ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS sessions (
    id               TEXT        PRIMARY KEY,
    user_id          UUID        REFERENCES auth.users(id) ON DELETE CASCADE,
    patient_name     TEXT        NOT NULL DEFAULT '',
    patient_last_name TEXT       NOT NULL DEFAULT '',
    patient_dni      TEXT        NOT NULL DEFAULT '',
    session_start_time TIMESTAMPTZ NOT NULL,
    session_end_time   TIMESTAMPTZ NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ── 2. events table ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS events (
    id            TEXT        PRIMARY KEY,
    session_id    TEXT        NOT NULL REFERENCES sessions(id) ON DELETE CASCADE,
    timestamp     TIMESTAMPTZ NOT NULL,
    elapsed_ticks BIGINT      NOT NULL DEFAULT 0,
    event_type    TEXT        NOT NULL DEFAULT '',
    description   TEXT        NOT NULL DEFAULT '',
    details       TEXT
);

-- ── 3. Indexes for RLS performance ───────────────────────────
CREATE INDEX IF NOT EXISTS idx_sessions_user_id   ON sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_events_session_id  ON events(session_id);

-- ── 4. Enable Row Level Security ─────────────────────────────
ALTER TABLE sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE events   ENABLE ROW LEVEL SECURITY;

-- ── 5. sessions policies ─────────────────────────────────────
-- SELECT: user sees only their own sessions
CREATE POLICY "Users can view their own sessions"
ON sessions FOR SELECT
TO authenticated
USING ((SELECT auth.uid())::text = user_id::text);

-- INSERT: user can only insert rows where user_id matches themselves
CREATE POLICY "Users can insert their own sessions"
ON sessions FOR INSERT
TO authenticated
WITH CHECK ((SELECT auth.uid())::text = user_id::text);

-- ── 6. events policies ───────────────────────────────────────
-- SELECT: user sees events only for sessions they own
CREATE POLICY "Users can view events for their sessions"
ON events FOR SELECT
TO authenticated
USING (
    session_id IN (
        SELECT id FROM sessions
        WHERE (SELECT auth.uid())::text = user_id::text
    )
);

-- INSERT: user can only insert events for their own sessions
CREATE POLICY "Users can insert events for their sessions"
ON events FOR INSERT
TO authenticated
WITH CHECK (
    session_id IN (
        SELECT id FROM sessions
        WHERE (SELECT auth.uid())::text = user_id::text
    )
);
```
