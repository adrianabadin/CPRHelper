---
status: resolved
trigger: "SQLite UNIQUE constraint fails on Sessions.Id when downloading remote sessions after closing and opening the user session (logout/login) a couple of times in a row"
created: 2026-04-01T00:00:00Z
updated: 2026-04-01T00:00:01Z
---

## Current Focus

hypothesis: CONFIRMED — Two independent duplicate checks (one in DownloadUserSessionsAsync, one in InsertDownloadedSessionAsync) both use SELECT-then-INSERT without any lock, creating a TOCTOU race. Two concurrent OnAuthStateChanged calls for the same login event both pass the existence check before either has written, then both attempt db.Insert(session) and the second throws UNIQUE constraint.
test: Traced the full call chain: OnAuthStateChanged -> HandleLoginSyncAsync -> DownloadUserSessionsAsync -> GetSessionAsync (check) -> InsertDownloadedSessionAsync -> GetSessionAsync (check again) -> db.Insert(session). Both checks are async and unguarded — a second trigger runs concurrently and passes both checks.
expecting: Fix: replace db.Insert(session) inside InsertDownloadedSessionAsync with db.InsertOrIgnore(session), and add a SemaphoreSlim in SessionSyncService to serialize concurrent HandleLoginSyncAsync calls.
next_action: Apply fix

## Symptoms

expected: Remote sessions should download and insert into local SQLite without errors, even after multiple logout/login cycles
actual: SQLite.SQLiteException: UNIQUE constraint failed: Sessions.Id — happens when closing and opening the session a couple of times in succession
errors: |
  [SessionSyncService] Downloading 1 remote session(s) for user e4f99058-a772-488c-bd62-04a20a9124bf
  [SessionSyncService] Downloaded session f152183d-e0c5-4e30-a0db-91122fb7877a with 2 event(s)
  Excepción producida: 'SQLite.SQLiteException' in SQLite-net.dll
  UNIQUE constraint failed: Sessions.Id
reproduction: Login, logout, login again (repeat a couple of times) — sync triggers download of same session that was already downloaded in a prior login
started: Likely since sync was implemented (phase 05.2)

## Eliminated

- hypothesis: InsertDownloadedSessionAsync uses InsertAsync without any prior existence check
  evidence: The method does call GetSessionAsync before inserting — both in DownloadUserSessionsAsync (line 158-163) AND again in InsertDownloadedSessionAsync (line 176-177). The check exists, it just is not atomic.
  timestamp: 2026-04-01T00:00:01Z

## Evidence

- timestamp: 2026-04-01T00:00:01Z
  checked: SessionSyncService.cs OnAuthStateChanged (line 71-84)
  found: async void handler — fires and forgets, no guard against concurrent calls. If AuthStateChanged fires twice in rapid succession (Supabase SDK may emit multiple events on login), two concurrent HandleLoginSyncAsync tasks run for the same userId.
  implication: Two sync tasks reach DownloadUserSessionsAsync simultaneously.

- timestamp: 2026-04-01T00:00:01Z
  checked: SessionSyncService.cs DownloadUserSessionsAsync (lines 157-163)
  found: GetSessionAsync check happens before InsertDownloadedSessionAsync. No lock between the check and the insert call. Both concurrent tasks see existing=null and both proceed to call InsertDownloadedSessionAsync.
  implication: Both tasks pass the first check. Race window is the entire async chain.

- timestamp: 2026-04-01T00:00:01Z
  checked: SessionRepository.cs InsertDownloadedSessionAsync (lines 171-187)
  found: Second GetSessionAsync check at line 176 before db.Insert(session). Still unguarded. Both concurrent callers arrive here, both see null (neither has committed yet), both call db.Insert(session) inside RunInTransactionAsync. The second transaction throws UNIQUE constraint failed.
  implication: This is the exact crash site. The double-check pattern does not help because both checks complete before either write is committed.

- timestamp: 2026-04-01T00:00:01Z
  checked: AuthViewModel.cs SignOutAsync (lines 387-427)
  found: On logout, DeleteLocalUserSessionsAsync runs before SignOutAsync. On next login, OnAuthStateChanged fires. The Supabase SDK (gotrue) is known to fire the state change event once for the session restore and potentially again for the sign-in confirmation, producing two rapid isLoggedIn=true events.
  implication: Confirms multiple concurrent sync triggers as the real-world trigger.

## Resolution

root_cause: |
  TOCTOU (time-of-check/time-of-use) race condition. The Supabase SDK fires AuthStateChanged
  multiple times on a single login event. Each trigger calls HandleLoginSyncAsync concurrently.
  Both concurrent tasks pass the "does session exist?" check before either has written to SQLite,
  then both reach db.Insert(session) inside a transaction. The second transaction throws
  UNIQUE constraint failed: Sessions.Id.

fix: |
  Two complementary fixes applied:
  1. SessionRepository.InsertDownloadedSessionAsync: replaced separate GetSessionAsync check +
     db.Insert with a single atomic db.InsertOrIgnore inside the transaction. InsertOrIgnore
     returns 0 when the row exists; if 0, events are also skipped (immutable sync preserved).
  2. SessionSyncService.HandleLoginSyncAsync: added SemaphoreSlim _loginSyncLock (1,1) with
     WaitAsync(0) (non-blocking try-acquire). If a login sync is already running, the duplicate
     trigger returns immediately without starting another sync.
  Also removed the now-redundant pre-check in DownloadUserSessionsAsync to avoid re-introducing
  the TOCTOU window.

verification: confirmed fixed by user (2026-04-01)
note: db.InsertOrIgnore does not exist in sqlite-net-pcl — replaced with db.Execute("INSERT OR IGNORE INTO ...") with all model columns
files_changed:
  - AclsTracker/Services/Database/SessionRepository.cs
  - AclsTracker/Services/Sync/SessionSyncService.cs
