using Dapper;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<User?> GetByIdWithStatsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id",
            new { Id = id });

        if (user != null)
        {
            // Get question count
            user.QuestionCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Posts WHERE OwnerUserId = @UserId AND PostTypeId = 1 AND DeletionDate IS NULL",
                new { UserId = id });

            // Get answer count
            user.AnswerCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Posts WHERE OwnerUserId = @UserId AND PostTypeId = 2 AND DeletionDate IS NULL",
                new { UserId = id });
        }

        return user;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize, string? sortBy = null, string? filter = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var orderBy = sortBy?.ToLower() switch
        {
            "reputation" => "Reputation DESC",
            "newest" => "CreationDate DESC",
            "name" => "DisplayName ASC",
            _ => "Reputation DESC"
        };

        var offset = (page - 1) * pageSize;
        var whereClause = string.IsNullOrEmpty(filter)
            ? ""
            : "WHERE DisplayName LIKE @Filter";
        var filterParam = string.IsNullOrEmpty(filter) ? null : $"%{filter}%";

        return await connection.QueryAsync<User>(
            $@"SELECT Id, Reputation, CreationDate, DisplayName, LastAccessDate, Location,
                      Views, UpVotes, DownVotes, ProfileImageUrl, EmailHash, AccountId
               FROM Users
               {whereClause}
               ORDER BY {orderBy}
               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Offset = offset, PageSize = pageSize, Filter = filterParam });
    }

    public async Task<int> GetUserCountAsync(string? filter = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        if (string.IsNullOrEmpty(filter))
        {
            // Use partition stats for instant approximate count
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT CAST(SUM(rows) AS INT) FROM sys.partitions
                  WHERE object_id = OBJECT_ID('Users') AND index_id IN (0,1)");
        }

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE DisplayName LIKE @Filter",
            new { Filter = $"%{filter}%" });
    }

    public async Task<IEnumerable<User>> GetTopUsersByReputationAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<User>(
            @"SELECT TOP (@Count) Id, Reputation, CreationDate, DisplayName, LastAccessDate, Location,
                     Views, UpVotes, DownVotes, ProfileImageUrl, EmailHash, AccountId
              FROM Users ORDER BY Reputation DESC",
            new { Count = count });
    }

    public async Task UpdateReputationAsync(int userId, int change)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE Users SET Reputation = Reputation + @Change WHERE Id = @UserId",
            new { UserId = userId, Change = change });
    }
}
