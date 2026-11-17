// Simulates Database queries for fetching user segments in batches
public interface IUserRepository
{
    Task<List<RecipientInfo>> GetUsersBySegmentInBatchesAsync(string segment, int pageNumber, int pageSize);
    Task<int> GetTotalUsersInSegmentAsync(string segment);
}