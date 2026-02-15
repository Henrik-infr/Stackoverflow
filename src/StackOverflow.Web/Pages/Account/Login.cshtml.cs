using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;

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

    public async Task<IActionResult> OnPostAsync(string displayName, string password)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            ErrorMessage = "Please enter a display name.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter a password.";
            return Page();
        }

        var user = await _userRepository.GetByDisplayNameAsync(displayName.Trim());
        if (user == null)
        {
            ErrorMessage = "Invalid display name or password.";
            return Page();
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            ErrorMessage = "This account has no password set. Please contact support.";
            return Page();
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            ErrorMessage = "Invalid display name or password.";
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
