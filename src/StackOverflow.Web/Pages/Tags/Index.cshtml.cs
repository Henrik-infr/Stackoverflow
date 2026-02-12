using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Tags;

public class IndexModel : PageModel
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<IndexModel> _logger;

    public TagListViewModel ViewModel { get; set; } = new();

    public IndexModel(ITagRepository tagRepository, ILogger<IndexModel> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task OnGetAsync(int page = 1, string? sort = null, string? filter = null)
    {
        try
        {
            ViewModel.Page = page;
            ViewModel.SortBy = sort ?? "popular";
            ViewModel.Filter = filter;
            ViewModel.TotalCount = await _tagRepository.GetTagCountAsync(filter);
            ViewModel.Tags = (await _tagRepository.GetTagsAsync(page, ViewModel.PageSize, sort, filter)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tags");
            ViewModel.Tags = new List<Models.Tag>();
        }
    }
}
