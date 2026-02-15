using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Models.ViewModels;
using StackOverflow.Web.Services;

namespace StackOverflow.Web.Pages.Search;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexModel> _logger;

    public SearchResultsViewModel ViewModel { get; set; } = new();

    public IndexModel(ISearchService searchService, ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task OnGetAsync(string? q = null, int pg = 1, string? sort = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                ViewModel = await _searchService.SearchAsync(q, pg, 15, sort);
                ViewModel.SortBy = sort;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for '{Query}'", q);
            ViewModel.Query = q ?? string.Empty;
        }
    }
}
