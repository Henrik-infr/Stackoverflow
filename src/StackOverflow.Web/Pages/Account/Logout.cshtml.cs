using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StackOverflow.Web.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.Cookies.Delete("UserId");
        Response.Cookies.Delete("UserDisplayName");
        return Redirect("/");
    }
}
