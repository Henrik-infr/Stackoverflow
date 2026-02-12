using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string tagName);
    Task<IEnumerable<Tag>> GetTagsAsync(int page, int pageSize, string? sortBy = null, string? filter = null);
    Task<int> GetTagCountAsync(string? filter = null);
    Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
    Task<IEnumerable<Tag>> GetTagsForPostAsync(string? tags);
    Task<IEnumerable<Tag>> SearchTagsAsync(string query, int limit = 10);
}
