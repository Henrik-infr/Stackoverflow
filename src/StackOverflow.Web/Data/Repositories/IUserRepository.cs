using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByIdWithStatsAsync(int id);
    Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize, string? sortBy = null, string? filter = null);
    Task<int> GetUserCountAsync(string? filter = null);
    Task<IEnumerable<User>> GetTopUsersByReputationAsync(int count);
    Task UpdateReputationAsync(int userId, int change);
    Task<User?> GetByDisplayNameAsync(string displayName);
    Task<int> CreateUserAsync(User user);
}
