// Mock User Database Repository
public class SqlUserRepository : IUserRepository
{
    // Simulating 12,000 active users in the database
    private readonly List<RecipientInfo> _databaseUsers;

    public SqlUserRepository()
    {
        _databaseUsers = Enumerable.Range(1, 12000)
            .Select(i => new RecipientInfo($"user-{i}", $"user{i}@omni.com"))
            .ToList();
    }

    public Task<int> GetTotalUsersInSegmentAsync(string segment)
    {
        return Task.FromResult(_databaseUsers.Count);
    }

    public Task<List<RecipientInfo>> GetUsersBySegmentInBatchesAsync(string segment, int pageNumber, int pageSize)
    {
        // Cursor / Paginated DB Query: SELECT * FROM users LIMIT pageSize OFFSET (pageNumber * pageSize)
        var batch = _databaseUsers
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(batch);
    }
}