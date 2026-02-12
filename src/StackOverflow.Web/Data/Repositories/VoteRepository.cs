using Dapper;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Vote?> GetUserVoteAsync(int postId, int userId, int voteTypeId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Vote>(
            "SELECT * FROM Votes WHERE PostId = @PostId AND UserId = @UserId AND VoteTypeId = @VoteTypeId",
            new { PostId = postId, UserId = userId, VoteTypeId = voteTypeId });
    }

    public async Task<IEnumerable<Vote>> GetVotesByPostIdAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Vote>(
            "SELECT * FROM Votes WHERE PostId = @PostId",
            new { PostId = postId });
    }

    public async Task<int> CreateAsync(Vote vote)
    {
        using var connection = _connectionFactory.CreateConnection();

        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Votes (PostId, VoteTypeId, UserId, CreationDate, BountyAmount)
              VALUES (@PostId, @VoteTypeId, @UserId, @CreationDate, @BountyAmount);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                vote.PostId,
                vote.VoteTypeId,
                vote.UserId,
                CreationDate = DateTime.UtcNow,
                vote.BountyAmount
            });

        // Update post score
        var scoreChange = vote.VoteTypeId == Vote.UpMod ? 1 : (vote.VoteTypeId == Vote.DownMod ? -1 : 0);
        if (scoreChange != 0)
        {
            await connection.ExecuteAsync(
                "UPDATE Posts SET Score = Score + @Change WHERE Id = @PostId",
                new { Change = scoreChange, vote.PostId });
        }

        return id;
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var vote = await connection.QueryFirstOrDefaultAsync<Vote>(
            "SELECT * FROM Votes WHERE Id = @Id",
            new { Id = id });

        if (vote != null)
        {
            await connection.ExecuteAsync("DELETE FROM Votes WHERE Id = @Id", new { Id = id });

            // Revert post score
            var scoreChange = vote.VoteTypeId == Vote.UpMod ? -1 : (vote.VoteTypeId == Vote.DownMod ? 1 : 0);
            if (scoreChange != 0)
            {
                await connection.ExecuteAsync(
                    "UPDATE Posts SET Score = Score + @Change WHERE Id = @PostId",
                    new { Change = scoreChange, vote.PostId });
            }
        }
    }

    public async Task<bool> HasUserVotedAsync(int postId, int userId, int voteTypeId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Votes WHERE PostId = @PostId AND UserId = @UserId AND VoteTypeId = @VoteTypeId",
            new { PostId = postId, UserId = userId, VoteTypeId = voteTypeId });
        return count > 0;
    }

    public async Task<VoteCounts> GetVoteCountsAsync(int postId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var upVotes = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Votes WHERE PostId = @PostId AND VoteTypeId = @VoteTypeId",
            new { PostId = postId, VoteTypeId = Vote.UpMod });

        var downVotes = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Votes WHERE PostId = @PostId AND VoteTypeId = @VoteTypeId",
            new { PostId = postId, VoteTypeId = Vote.DownMod });

        return new VoteCounts { UpVotes = upVotes, DownVotes = downVotes };
    }
}
