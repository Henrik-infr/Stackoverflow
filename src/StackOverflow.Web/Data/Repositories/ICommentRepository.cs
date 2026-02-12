using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id);
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
    Task<IEnumerable<Comment>> GetByPostIdsAsync(IEnumerable<int> postIds);
    Task<int> CreateAsync(Comment comment);
    Task DeleteAsync(int id);
}
