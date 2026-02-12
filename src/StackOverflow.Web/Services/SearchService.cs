using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Services;

public class SearchService : ISearchService
{
    private readonly IPostRepository _postRepository;

    public SearchService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<SearchResultsViewModel> SearchAsync(string query, int page, int pageSize, SearchFilters? filters = null)
    {
        var results = await _postRepository.SearchAsync(query, page, pageSize);
        var totalCount = await _postRepository.GetSearchCountAsync(query);

        // Apply additional filters if provided
        var filteredResults = results.ToList();

        if (filters != null)
        {
            if (filters.HasAcceptedAnswer)
            {
                filteredResults = filteredResults.Where(p => p.AcceptedAnswerId.HasValue).ToList();
            }

            if (filters.MinScore.HasValue)
            {
                filteredResults = filteredResults.Where(p => p.Score >= filters.MinScore.Value).ToList();
            }

            if (filters.MinAnswers.HasValue)
            {
                filteredResults = filteredResults.Where(p => p.AnswerCount >= filters.MinAnswers.Value).ToList();
            }
        }

        return new SearchResultsViewModel
        {
            Query = query,
            Results = filteredResults,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Filters = filters ?? new SearchFilters()
        };
    }

    public async Task<IEnumerable<Post>> GetSuggestionsAsync(string query, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            return Enumerable.Empty<Post>();

        var results = await _postRepository.SearchAsync(query, 1, limit);
        return results;
    }
}
