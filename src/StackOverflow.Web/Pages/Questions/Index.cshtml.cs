using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Questions;

public class IndexModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<IndexModel> _logger;

    public QuestionListViewModel ViewModel { get; set; } = new();

    public IndexModel(IPostRepository postRepository, ILogger<IndexModel> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task OnGetAsync(int pg = 1, string? sort = null)
    {
        try
        {
            ViewModel.Page = pg;
            ViewModel.SortBy = sort ?? "active";
            ViewModel.TotalCount = await _postRepository.GetQuestionCountAsync();
            ViewModel.Questions = (await _postRepository.GetQuestionsAsync(pg, ViewModel.PageSize, sort)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading questions");
            ViewModel.Questions = new List<Models.Post>();
        }
    }
}
