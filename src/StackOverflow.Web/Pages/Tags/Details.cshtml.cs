using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Pages.Tags;

public class DetailsModel : PageModel
{
    private readonly ITagRepository _tagRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<DetailsModel> _logger;

    public string TagName { get; set; } = string.Empty;
    public Tag? Tag { get; set; }
    public List<Post> Questions { get; set; } = new();
    public int TotalCount { get; set; }
    public new int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? SortBy { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public DetailsModel(
        ITagRepository tagRepository,
        IPostRepository postRepository,
        ILogger<DetailsModel> logger)
    {
        _tagRepository = tagRepository;
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task OnGetAsync(string tagName, int page = 1, string? sort = null)
    {
        try
        {
            TagName = tagName;
            Page = page;
            SortBy = sort ?? "active";

            Tag = await _tagRepository.GetByNameAsync(tagName);
            TotalCount = await _postRepository.GetQuestionCountByTagAsync(tagName);
            Questions = (await _postRepository.GetQuestionsByTagAsync(tagName, page, PageSize)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tag {TagName}", tagName);
        }
    }
}
