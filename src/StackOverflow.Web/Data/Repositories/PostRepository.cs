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
            $@"SELECT p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
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
            @"SELECT p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
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
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Posts WHERE PostTypeId = 1 AND DeletionDate IS NULL");
    }

    public async Task<int> GetQuestionCountByTagAsync(string tagName)
    {
        using var connection = _connectionFactory.CreateConnection();
        var tagPattern = $"%<{tagName}>%";
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Posts WHERE PostTypeId = 1 AND DeletionDate IS NULL AND Tags LIKE @TagPattern",
            new { TagPattern = tagPattern });
    }

    public async Task<IEnumerable<Post>> GetRecentQuestionsAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post, User, Post>(
            @"SELECT TOP (@Count) p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
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
            @"SELECT TOP (@Count) *
              FROM Posts
              WHERE OwnerUserId = @UserId AND PostTypeId = 1 AND DeletionDate IS NULL
              ORDER BY CreationDate DESC",
            new { UserId = userId, Count = count });
    }

    public async Task<IEnumerable<Post>> GetUserAnswersAsync(int userId, int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Post>(
            @"SELECT TOP (@Count) a.*, q.Title
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
            @"SELECT p.*, u.Id, u.DisplayName, u.Reputation, u.ProfileImageUrl, u.EmailHash
              FROM Posts p
              LEFT JOIN Users u ON p.OwnerUserId = u.Id
              WHERE p.PostTypeId = 1 AND p.DeletionDate IS NULL
              AND (p.Title LIKE @SearchPattern OR p.Body LIKE @SearchPattern OR p.Tags LIKE @SearchPattern)
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

        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM Posts
              WHERE PostTypeId = 1 AND DeletionDate IS NULL
              AND (Title LIKE @SearchPattern OR Body LIKE @SearchPattern OR Tags LIKE @SearchPattern)",
            new { SearchPattern = searchPattern });
    }

    public async Task<int> CreateQuestionAsync(Post question)
    {
        using var connection = _connectionFactory.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Posts (PostTypeId, CreationDate, Score, ViewCount, Body, OwnerUserId, Title, Tags, AnswerCount, CommentCount, FavoriteCount)
              VALUES (1, @CreationDate, 0, 0, @Body, @OwnerUserId, @Title, @Tags, 0, 0, 0);
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
            @"INSERT INTO Posts (PostTypeId, ParentId, CreationDate, Score, Body, OwnerUserId, CommentCount)
              VALUES (2, @ParentId, @CreationDate, 0, @Body, @OwnerUserId, 0);
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
