using Dapper;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public class TagRepository : ITagRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TagRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Tag?> GetByNameAsync(string tagName)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Tag>(
            "SELECT * FROM Tags WHERE TagName = @TagName",
            new { TagName = tagName });
    }

    public async Task<IEnumerable<Tag>> GetTagsAsync(int page, int pageSize, string? sortBy = null, string? filter = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var orderBy = sortBy?.ToLower() switch
        {
            "popular" => "Count DESC",
            "name" => "TagName ASC",
            "newest" => "Id DESC",
            _ => "Count DESC"
        };

        var offset = (page - 1) * pageSize;
        var whereClause = string.IsNullOrEmpty(filter)
            ? ""
            : "WHERE TagName LIKE @Filter";
        // Use prefix match for faster lookups
        var filterParam = string.IsNullOrEmpty(filter) ? null : $"{filter}%";

        return await connection.QueryAsync<Tag>(
            $@"SELECT * FROM Tags
               {whereClause}
               ORDER BY {orderBy}
               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Offset = offset, PageSize = pageSize, Filter = filterParam });
    }

    public async Task<int> GetTagCountAsync(string? filter = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        if (string.IsNullOrEmpty(filter))
        {
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Tags");
        }

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Tags WHERE TagName LIKE @Filter",
            new { Filter = $"{filter}%" });
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Tag>(
            "SELECT TOP (@Count) * FROM Tags ORDER BY Count DESC",
            new { Count = count });
    }

    public async Task<IEnumerable<Tag>> GetTagsForPostAsync(string? tags)
    {
        if (string.IsNullOrEmpty(tags))
            return Enumerable.Empty<Tag>();

        using var connection = _connectionFactory.CreateConnection();

        // Parse tags from format like "<c#><.net><asp.net>"
        var tagNames = tags
            .Split(new[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        if (!tagNames.Any())
            return Enumerable.Empty<Tag>();

        return await connection.QueryAsync<Tag>(
            "SELECT * FROM Tags WHERE TagName IN @TagNames ORDER BY Count DESC",
            new { TagNames = tagNames });
    }

    public async Task<IEnumerable<Tag>> SearchTagsAsync(string query, int limit = 10)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Tag>(
            @"SELECT TOP (@Limit) * FROM Tags
              WHERE TagName LIKE @Query
              ORDER BY Count DESC",
            new { Query = $"{query}%", Limit = limit });
    }
}
