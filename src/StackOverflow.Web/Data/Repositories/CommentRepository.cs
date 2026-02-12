using Dapper;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CommentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Comment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Comment>(
            "SELECT * FROM Comments WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Comment, User, Comment>(
            @"SELECT c.*, u.Id, u.DisplayName, u.Reputation
              FROM Comments c
              LEFT JOIN Users u ON c.UserId = u.Id
              WHERE c.PostId = @PostId
              ORDER BY c.CreationDate ASC",
            (comment, user) =>
            {
                comment.User = user;
                return comment;
            },
            new { PostId = postId },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Comment>> GetByPostIdsAsync(IEnumerable<int> postIds)
    {
        using var connection = _connectionFactory.CreateConnection();
        var ids = postIds.ToList();

        if (!ids.Any())
            return Enumerable.Empty<Comment>();

        return await connection.QueryAsync<Comment, User, Comment>(
            @"SELECT c.*, u.Id, u.DisplayName, u.Reputation
              FROM Comments c
              LEFT JOIN Users u ON c.UserId = u.Id
              WHERE c.PostId IN @PostIds
              ORDER BY c.PostId, c.CreationDate ASC",
            (comment, user) =>
            {
                comment.User = user;
                return comment;
            },
            new { PostIds = ids },
            splitOn: "Id");
    }

    public async Task<int> CreateAsync(Comment comment)
    {
        using var connection = _connectionFactory.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Comments (PostId, Score, Text, CreationDate, UserId, UserDisplayName)
              VALUES (@PostId, 0, @Text, @CreationDate, @UserId, @UserDisplayName);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                comment.PostId,
                comment.Text,
                CreationDate = DateTime.UtcNow,
                comment.UserId,
                comment.UserDisplayName
            });

        // Update comment count on post
        await connection.ExecuteAsync(
            "UPDATE Posts SET CommentCount = CommentCount + 1 WHERE Id = @PostId",
            new { comment.PostId });

        return id;
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var comment = await GetByIdAsync(id);
        if (comment != null)
        {
            await connection.ExecuteAsync("DELETE FROM Comments WHERE Id = @Id", new { Id = id });

            // Update comment count on post
            await connection.ExecuteAsync(
                "UPDATE Posts SET CommentCount = CommentCount - 1 WHERE Id = @PostId AND CommentCount > 0",
                new { comment.PostId });
        }
    }
}
