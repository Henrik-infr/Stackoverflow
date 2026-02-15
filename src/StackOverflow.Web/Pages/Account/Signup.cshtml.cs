using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Pages.Account;

[IgnoreAntiforgeryToken]
public class SignupModel : PageModel
{
    private readonly IUserRepository _userRepository;

    public string? ErrorMessage { get; set; }

    public SignupModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string displayName, string password, string confirmPassword, string? location)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            ErrorMessage = "Display name is required.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Password is required.";
            return Page();
        }

        if (password != confirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        var existing = await _userRepository.GetByDisplayNameAsync(displayName.Trim());
        if (existing != null)
        {
            ErrorMessage = "A user with that display name already exists. Try logging in instead.";
            return Page();
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            DisplayName = displayName.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim()
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        var userId = await _userRepository.CreateUserAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax
        };

        Response.Cookies.Append("UserId", userId.ToString(), cookieOptions);
        Response.Cookies.Append("UserDisplayName", user.DisplayName ?? "", cookieOptions);

        return Redirect("/");
    }
}
