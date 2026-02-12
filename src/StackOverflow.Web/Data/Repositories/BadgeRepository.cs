using Dapper;
using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Data.Repositories;

public class BadgeRepository : IBadgeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BadgeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Badge>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Badge>(
            @"SELECT * FROM Badges
              WHERE UserId = @UserId
              ORDER BY Class ASC, Date DESC",
            new { UserId = userId });
    }

    public async Task<BadgeSummary> GetUserBadgeSummaryAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var counts = await connection.QueryAsync<(int Class, int Count)>(
            @"SELECT Class, COUNT(*) as Count
              FROM Badges
              WHERE UserId = @UserId
              GROUP BY Class",
            new { UserId = userId });

        var summary = new BadgeSummary();
        foreach (var (badgeClass, count) in counts)
        {
            switch (badgeClass)
            {
                case Badge.Gold:
                    summary.GoldCount = count;
                    break;
                case Badge.Silver:
                    summary.SilverCount = count;
                    break;
                case Badge.Bronze:
                    summary.BronzeCount = count;
                    break;
            }
        }

        return summary;
    }

    public async Task<IEnumerable<Badge>> GetRecentBadgesAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Badge>(
            "SELECT TOP (@Count) * FROM Badges ORDER BY Date DESC",
            new { Count = count });
    }
}
