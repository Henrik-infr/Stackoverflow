using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Post>? RecentQuestions { get; set; }
    public IEnumerable<Tag>? PopularTags { get; set; }
    public IEnumerable<User>? TopUsers { get; set; }

    public IndexModel(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository,
        ILogger<IndexModel> logger)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            RecentQuestions = await _postRepository.GetRecentQuestionsAsync(10);
            PopularTags = await _tagRepository.GetPopularTagsAsync(12);
            TopUsers = await _userRepository.GetTopUsersByReputationAsync(5);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page data");
            RecentQuestions = Enumerable.Empty<Post>();
            PopularTags = Enumerable.Empty<Tag>();
            TopUsers = Enumerable.Empty<User>();
        }
    }
}
