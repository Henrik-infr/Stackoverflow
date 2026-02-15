using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Services;

public interface ISearchService
{
    Task<SearchResultsViewModel> SearchAsync(string query, int page, int pageSize, string? sortBy = null, SearchFilters? filters = null);
    Task<IEnumerable<Post>> GetSuggestionsAsync(string query, int limit = 5);
}
