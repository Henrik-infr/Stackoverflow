using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;

namespace StackOverflow.Web.Pages.Account;

[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly IUserRepository _userRepository;

    public string? ErrorMessage { get; set; }

    public LoginModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            ErrorMessage = "Please enter a display name.";
            return Page();
        }

        var user = await _userRepository.GetByDisplayNameAsync(displayName.Trim());
        if (user == null)
        {
            ErrorMessage = "No user found with that display name.";
            return Page();
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax
        };

        Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
        Response.Cookies.Append("UserDisplayName", user.DisplayName ?? "", cookieOptions);

        return Redirect("/");
    }
}
