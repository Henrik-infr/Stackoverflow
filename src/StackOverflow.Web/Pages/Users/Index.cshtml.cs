using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<IndexModel> _logger;

    public UserListViewModel ViewModel { get; set; } = new();

    public IndexModel(IUserRepository userRepository, ILogger<IndexModel> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task OnGetAsync(int pg = 1, string? sort = null, string? filter = null)
    {
        try
        {
            ViewModel.Page = pg;
            ViewModel.SortBy = sort ?? "reputation";
            ViewModel.Filter = filter;
            ViewModel.TotalCount = await _userRepository.GetUserCountAsync(filter);
            ViewModel.Users = (await _userRepository.GetUsersAsync(pg, ViewModel.PageSize, sort, filter)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            ViewModel.Users = new List<Models.User>();
        }
    }
}
