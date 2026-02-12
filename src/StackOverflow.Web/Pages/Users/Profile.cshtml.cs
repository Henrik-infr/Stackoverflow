using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Users;

public class ProfileModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ILogger<ProfileModel> _logger;

    public UserProfileViewModel? ViewModel { get; set; }

    public ProfileModel(
        IUserRepository userRepository,
        IPostRepository postRepository,
        IBadgeRepository badgeRepository,
        ILogger<ProfileModel> logger)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _badgeRepository = badgeRepository;
        _logger = logger;
    }

    public async Task OnGetAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdWithStatsAsync(id);
            if (user == null)
            {
                return;
            }

            ViewModel = new UserProfileViewModel
            {
                User = user,
                RecentQuestions = (await _postRepository.GetUserQuestionsAsync(id, 10)).ToList(),
                RecentAnswers = (await _postRepository.GetUserAnswersAsync(id, 10)).ToList(),
                Badges = (await _badgeRepository.GetByUserIdAsync(id)).ToList(),
                BadgeSummary = await _badgeRepository.GetUserBadgeSummaryAsync(id)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user profile {UserId}", id);
        }
    }
}
