using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Data.Repositories;

public interface IBadgeRepository
{
    Task<IEnumerable<Badge>> GetByUserIdAsync(int userId);
    Task<BadgeSummary> GetUserBadgeSummaryAsync(int userId);
    Task<IEnumerable<Badge>> GetRecentBadgesAsync(int count);
}
