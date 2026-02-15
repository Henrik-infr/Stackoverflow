using Dapper;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Post>(
            "SELECT * FROM Posts WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<Post?> GetQuestionWithDetailsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var post = await connection.QueryFirstOrDefaultAsync<Post>(
            @"SELECT p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.Id = @Id AND p.PostTypeId = 1",
            new { Id = id });

        if (post != null)
        {
            // Get owner details
            if (post.OwnerUserId.HasValue)
            {
                post.Owner = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @Id",
                    new { Id = post.OwnerUserId.Value });
            }
        }

        return post;
    }

    public async Task<IEnumerable<Post>> GetQuestionsAsync(int page, int pageSize, string? sortBy = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var orderBy = sortBy?.ToLower() switch
        {
            "votes" => "p.Score DESC",
            "newest" => "p.CreationDate DESC",
            "active" => "p.LastActivityDate DESC",
            "unanswered" => "p.AnswerCount ASC, p.CreationDate DESC",
            _ => "p.LastActivityDate DESC"
        };

        var offset = (page - 1) * pageSize;

        return await connection.QueryAsync<Post, User, Post>(
            $@"SELECT p.Id, p.PostTypeId, p.AcceptedAnswerId, p.CreationDate, p.Score, p.ViewCount,
                      p.OwnerUserId, p.Title, p.Tags, p.AnswerCount, p.CommentCount, p.FavoriteCount,
                      p.LastActivityDate, p.ClosedDate,
                      u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
               FROM Posts p
               LEFT JOIN Users u ON p.OwnerUserId = u.Id
               WHERE p.PostTypeId = 1 AND p.DeletionDate IS NULL
               ORDER BY {orderBy}
               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            (post, user) =>
            {
                post.Owner = user;
                return post;
            },
            new { Offset = offset, PageSize = pageSize },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Post>> GetQuestionsByTagAsync(string tagName, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (page - 1) * pageSize;
        var tagPattern = $"%<{tagName}>%";

        return await connection.QueryAsync<Post, User, Post>(
            @"SELECT p.Id, p.PostTypeId, p.AcceptedAnswerId, p.CreationDate, p.Score, p.ViewCount,
                     p.OwnerUserId, p.Title, p.Tags, p.AnswerCount, p.CommentCount, p.FavoriteCount,
                     p.LastActivityDate, p.ClosedDate,
                     u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.PostTypeId = 1 AND p.DeletionDate IS NULL
              AND p.Tags LIKE @TagPattern
              ORDER BY p.LastActivityDate DESC
              OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            (post, user) =>
            {
                post.Owner = user;
                return post;
            },
            new { TagPattern = tagPattern, Offset = offset, PageSize = pageSize },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Post>> GetAnswersForQuestionAsync(int questionId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post, User, Post>(
            @"SELECT p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.ParentId = @QuestionId AND p.PostTypeId = 2 AND p.DeletionDate IS NULL
              ORDER BY
                CASE WHEN p.Id = (SELECT AcceptedAnswerId FROM Posts WHERE Id = @QuestionId) THEN 0 ELSE 1 END,
                p.Score DESC,
                p.CreationDate ASC",
            (post, user) =>
            {
                post.Owner = user;
                return post;
            },
            new { QuestionId = questionId },
            splitOn: "Id");
    }

    public async Task<int> GetQuestionCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        // Use filtered index partition stats for fast approximate count (instant vs 3+ seconds for COUNT)
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT CAST(rows AS INT) FROM sys.partitions
              WHERE object_id = OBJECT_ID('Posts')
              AND index_id = (SELECT index_id FROM sys.indexes WHERE object_id = OBJECT_ID('Posts') AND name = 'IX_Posts_Questions_Filtered')");
    }

    public async Task<int> GetQuestionCountByTagAsync(string tagName)
    {
        using var connection = _connectionFactory.CreateConnection();
        var tagPattern = $"%<{tagName}>%";
        // Cap at 10000 to avoid full table scan on LIKE query
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM (SELECT TOP 10000 Id FROM Posts WHERE PostTypeId = 1 AND DeletionDate IS NULL AND Tags LIKE @TagPattern) AS t",
            new { TagPattern = tagPattern });
    }

    public async Task<IEnumerable<Post>> GetRecentQuestionsAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post, User, Post>(
            @"SELECT TOP (@Count) p.Id, p.PostTypeId, p.AcceptedAnswerId, p.CreationDate, p.Score, p.ViewCount,
                     p.OwnerUserId, p.Title, p.Tags, p.AnswerCount, p.CommentCount, p.FavoriteCount,
                     p.LastActivityDate, p.ClosedDate,
                     u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.PostTypeId = 1 AND p.DeletionDate IS NULL
              ORDER BY p.CreationDate DESC",
            (post, user) =>
            {
                post.Owner = user;
                return post;
            },
            new { Count = count },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Post>> GetUserQuestionsAsync(int userId, int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post>(
            @"SELECT TOP (@Count) Id, PostTypeId, AcceptedAnswerId, CreationDate, Score, ViewCount,
                     OwnerUserId, Title, Tags, AnswerCount, CommentCount, FavoriteCount, LastActivityDate
              FROM Posts
              WHERE OwnerUserId = @UserId AND PostTypeId = 1 AND DeletionDate IS NULL
              ORDER BY CreationDate DESC",
            new { UserId = userId, Count = count });
    }

    public async Task<IEnumerable<Post>> GetUserAnswersAsync(int userId, int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post>(
            @"SELECT TOP (@Count) a.Id, a.PostTypeId, a.ParentId, a.CreationDate, a.Score,
                     a.OwnerUserId, a.CommentCount, q.Title
              FROM Posts a
              INNER JOIN Posts q ON a.ParentId = q.Id
              WHERE a.OwnerUserId = @UserId AND a.PostTypeId = 2 AND a.DeletionDate IS NULL
              ORDER BY a.CreationDate DESC",
            new { UserId = userId, Count = count });
    }

    public async Task<IEnumerable<Post>> SearchAsync(string query, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<Post, User, Post>(
            @"SELECT p.Id, p.PostTypeId, p.AcceptedAnswerId, p.CreationDate, p.Score, p.ViewCount,
                     p.OwnerUserId, p.Title, p.Tags, p.AnswerCount, p.CommentCount, p.FavoriteCount,
                     p.LastActivityDate, p.ClosedDate,
                     u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.PostTypeId = 1 AND p.DeletionDate IS NULL
              AND (p.Title LIKE @SearchPattern OR p.Tags LIKE @SearchPattern)
              ORDER BY p.Score DESC, p.LastActivityDate DESC
              OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            (post, user) =>
            {
                post.Owner = user;
                return post;
            },
            new { SearchPattern = searchPattern, Offset = offset, PageSize = pageSize },
            splitOn: "Id");
    }

    public async Task<int> GetSearchCountAsync(string query)
    {
        using var connection = _connectionFactory.CreateConnection();
        var searchPattern = $"%{query}%";

        // Cap at 10000 to avoid full table scan; search Title and Tags only (Body LIKE is too slow)
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM (SELECT TOP 10000 Id FROM Posts
              WHERE PostTypeId = 1 AND DeletionDate IS NULL
              AND (Title LIKE @SearchPattern OR Tags LIKE @SearchPattern)) AS t",
            new { SearchPattern = searchPattern });
    }

    public async Task<int> CreateQuestionAsync(Post question)
    {
        using var connection = _connectionFactory.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Posts (PostTypeId, CreationDate, LastActivityDate, Score, ViewCount, Body, OwnerUserId, Title, Tags, AnswerCount, CommentCount, FavoriteCount)
              VALUES (1, @CreationDate, @CreationDate, 0, 0, @Body, @OwnerUserId, @Title, @Tags, 0, 0, 0);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                CreationDate = DateTime.UtcNow,
                question.Body,
                question.OwnerUserId,
                question.Title,
                question.Tags
            });

        return id;
    }

    public async Task<int> CreateAnswerAsync(Post answer)
    {
        using var connection = _connectionFactory.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Posts (PostTypeId, ParentId, CreationDate, LastActivityDate, Score, Body, OwnerUserId, CommentCount)
              VALUES (2, @ParentId, @CreationDate, @CreationDate, 0, @Body, @OwnerUserId, 0);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                answer.ParentId,
                CreationDate = DateTime.UtcNow,
                answer.Body,
                answer.OwnerUserId
            });

        // Update answer count on parent question
        await connection.ExecuteAsync(
            "UPDATE Posts SET AnswerCount = AnswerCount + 1, LastActivityDate = @Now WHERE Id = @ParentId",
            new { Now = DateTime.UtcNow, answer.ParentId });

        return id;
    }

    public async Task UpdateAsync(Post post)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"UPDATE Posts SET
                Body = @Body,
                Title = @Title,
                Tags = @Tags,
                LastEditDate = @LastEditDate,
                LastEditorUserId = @LastEditorUserId,
                LastActivityDate = @LastActivityDate
              WHERE Id = @Id",
            new
            {
                post.Id,
                post.Body,
                post.Title,
                post.Tags,
                LastEditDate = DateTime.UtcNow,
                post.LastEditorUserId,
                LastActivityDate = DateTime.UtcNow
            });
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var now = DateTime.UtcNow;

        // Soft-delete the post
        await connection.ExecuteAsync(
            "UPDATE Posts SET DeletionDate = @Now WHERE Id = @Id",
            new { Now = now, Id = id });

        // If it's a question, also soft-delete its answers and decrement nothing (question is gone)
        // If it's an answer, decrement the parent's answer count
        var post = await connection.QueryFirstOrDefaultAsync<Post>(
            "SELECT PostTypeId, ParentId FROM Posts WHERE Id = @Id",
            new { Id = id });

        if (post?.PostTypeId == 1)
        {
            // Soft-delete all child answers
            await connection.ExecuteAsync(
                "UPDATE Posts SET DeletionDate = @Now WHERE ParentId = @Id AND PostTypeId = 2 AND DeletionDate IS NULL",
                new { Now = now, Id = id });
        }
        else if (post?.PostTypeId == 2 && post.ParentId.HasValue)
        {
            // Decrement parent question's answer count
            await connection.ExecuteAsync(
                "UPDATE Posts SET AnswerCount = CASE WHEN AnswerCount > 0 THEN AnswerCount - 1 ELSE 0 END WHERE Id = @ParentId",
                new { post.ParentId });
        }
    }

    public async Task<IEnumerable<PostLink>> GetRelatedQuestionsAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PostLink, Post, PostLink>(
            @"SELECT pl.*, p.Id, p.Title, p.Score, p.AnswerCount, p.AcceptedAnswerId
              FROM PostLinks pl
              INNER JOIN Posts p ON pl.RelatedPostId = p.Id
              WHERE pl.PostId = @PostId AND pl.LinkTypeId = 1
              ORDER BY p.Score DESC",
            (link, post) =>
            {
                link.RelatedPost = post;
                return link;
            },
            new { PostId = postId },
            splitOn: "Id");
    }

    public async Task<IEnumerable<PostLink>> GetLinkedQuestionsAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PostLink, Post, PostLink>(
            @"SELECT pl.*, p.Id, p.Title, p.Score, p.AnswerCount, p.AcceptedAnswerId
              FROM PostLinks pl
              INNER JOIN Posts p ON pl.RelatedPostId = p.Id
              WHERE pl.PostId = @PostId AND pl.LinkTypeId = 3
              ORDER BY p.Score DESC",
            (link, post) =>
            {
                link.RelatedPost = post;
                return link;
            },
            new { PostId = postId },
            splitOn: "Id");
    }

    public async Task<IEnumerable<PostHistory>> GetPostHistoryAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PostHistory, User, PostHistory>(
            @"SELECT ph.*, u.Id, u.DisplayName, u.Reputation
              FROM PostHistory ph
              LEFT JOIN Users u ON ph.UserId = u.Id
              WHERE ph.PostId = @PostId
              ORDER BY ph.CreationDate DESC",
            (history, user) =>
            {
                history.User = user;
                return history;
            },
            new { PostId = postId },
            splitOn: "Id");
    }
}
