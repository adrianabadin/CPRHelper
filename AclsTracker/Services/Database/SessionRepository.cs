using AclsTracker.Models;
using SQLite;

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
}
