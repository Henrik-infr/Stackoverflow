using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public interface IVoteRepository
{
    Task<Vote?> GetUserVoteAsync(int postId, int userId, int voteTypeId);
    Task<IEnumerable<Vote>> GetVotesByPostIdAsync(int postId);
    Task<int> CreateAsync(Vote vote);
    Task DeleteAsync(int id);
    Task<bool> HasUserVotedAsync(int postId, int userId, int voteTypeId);
    Task<VoteCounts> GetVoteCountsAsync(int postId);
}

public class VoteCounts
{
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public int Score => UpVotes - DownVotes;
}
