using AclsTracker.Models;
using SQLite;
using SQLiteException = SQLite.SQLiteException;

namespace AclsTracker.Services.Database;

public class SessionRepository : ISessionRepository
{
    private readonly SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
    private bool _initialized = false;

    public SessionRepository()
    {
        var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "aclstracker.db3");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized) return;

            await _database.CreateTableAsync<Session>().ConfigureAwait(false);
            await _database.CreateTableAsync<EventRecordEntity>().ConfigureAwait(false);
            await _database.CreateTableAsync<SyncQueueItem>().ConfigureAwait(false);

            // Migration: Add UserId column if not exists (graceful for existing installations)
            try
            {
                await _database.ExecuteAsync("ALTER TABLE Sessions ADD COLUMN UserId TEXT NULL")
                    .ConfigureAwait(false);
            }
            catch (SQLiteException)
            {
                // Column already exists — ignore
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync().ConfigureAwait(false);
        }
    }

    public async Task SaveSessionAsync(Session session, List<EventRecord> events)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        await _database.RunInTransactionAsync(db =>
        {
            db.Insert(session);

            foreach (var evt in events)
            {
                var entity = evt.ToEntity(session.Id);
                db.Insert(entity);
            }
        }).ConfigureAwait(false);
    }

    public async Task<List<Session>> SearchSessionsAsync(string? searchText, DateTime? fromDate, DateTime? toDate)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        var conditions = new List<string> { "1=1" };
        var args = new List<object>();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            conditions.Add("(PatientName LIKE ? OR PatientLastName LIKE ? OR PatientDNI LIKE ?)");
            var likePattern = "%" + searchText + "%";
            args.Add(likePattern);
            args.Add(likePattern);
            args.Add(likePattern);
        }

        if (fromDate.HasValue)
        {
            conditions.Add("SessionStartTime >= ?");
            args.Add(fromDate.Value);
        }

        if (toDate.HasValue)
        {
            conditions.Add("SessionStartTime <= ?");
            // Add 1 day for inclusive end-of-day boundary
            args.Add(toDate.Value.AddDays(1));
        }

        var sql = $"SELECT * FROM Sessions WHERE {string.Join(" AND ", conditions)} ORDER BY CreatedAt DESC";

        return await _database.QueryAsync<Session>(sql, args.ToArray()).ConfigureAwait(false);
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        return await _database.Table<Session>()
            .Where(s => s.Id == sessionId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<EventRecord>> GetSessionEventsAsync(string sessionId)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        var entities = await _database.Table<EventRecordEntity>()
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(e => e.ToModel()).ToList();
    }

    // ============ User-Scoped Operations ============

    public async Task<List<Session>> GetOrphanSessionsAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return await _database.Table<Session>()
            .Where(s => s.UserId == null)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        await _database.RunInTransactionAsync(db =>
        {
            // Get all session IDs for this user
            var sessionIds = db.Table<Session>()
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .ToList();

            // Delete events for these sessions
            foreach (var sessionId in sessionIds)
            {
                db.Execute("DELETE FROM EventRecords WHERE SessionId = ?", sessionId);
            }

            // Delete the sessions
            db.Execute("DELETE FROM Sessions WHERE UserId = ?", userId);
        }).ConfigureAwait(false);
    }

    public async Task UpdateSessionUserIdAsync(string sessionId, string userId)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        await _database.ExecuteAsync("UPDATE Sessions SET UserId = ? WHERE Id = ?", userId, sessionId)
            .ConfigureAwait(false);
    }

    public async Task InsertDownloadedSessionAsync(Session session, List<EventRecordEntity> events)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        await _database.RunInTransactionAsync(db =>
        {
            // INSERT OR IGNORE is the atomic guard against duplicate IDs.
            // A plain Insert + pre-check has a TOCTOU race when two concurrent
            // sync tasks both pass the existence check before either has committed.
            var inserted = db.Execute(
                "INSERT OR IGNORE INTO Sessions (Id, UserId, PatientName, PatientLastName, PatientDNI, SessionStartTime, SessionEndTime, CreatedAt) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                session.Id, session.UserId, session.PatientName, session.PatientLastName, session.PatientDNI, session.SessionStartTime, session.SessionEndTime, session.CreatedAt);
            if (inserted == 0)
            {
                // Session already exists locally — skip events too (immutable sync).
                return;
            }

            foreach (var evt in events)
            {
                db.Execute(
                    "INSERT OR IGNORE INTO EventRecords (Id, SessionId, Timestamp, ElapsedTicks, EventType, Description, Details) VALUES (?, ?, ?, ?, ?, ?, ?)",
                    evt.Id, evt.SessionId, evt.Timestamp, evt.ElapsedTicks, evt.EventType, evt.Description, evt.Details);
            }
        }).ConfigureAwait(false);
    }

    public async Task<List<Session>> GetSessionsByUserIdAsync(string userId)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return await _database.QueryAsync<Session>(
            "SELECT * FROM Sessions WHERE UserId = ? ORDER BY CreatedAt DESC", userId)
            .ConfigureAwait(false);
    }

    // ============ SyncQueue Operations ============

    public async Task<List<SyncQueueItem>> GetPendingSyncItemsAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return await _database.QueryAsync<SyncQueueItem>(
            "SELECT * FROM SyncQueue WHERE NextRetryAt <= ? ORDER BY CreatedAt",
            DateTime.UtcNow)
            .ConfigureAwait(false);
    }

    public async Task EnqueueSyncItemAsync(SyncQueueItem item)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        await _database.InsertOrReplaceAsync(item).ConfigureAwait(false);
    }

    public async Task RemoveSyncItemAsync(string id)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        await _database.DeleteAsync<SyncQueueItem>(id).ConfigureAwait(false);
    }
}
