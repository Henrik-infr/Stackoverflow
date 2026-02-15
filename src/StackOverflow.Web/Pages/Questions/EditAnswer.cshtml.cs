using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;

namespace StackOverflow.Web.Pages.Questions;

[IgnoreAntiforgeryToken]
public class EditAnswerModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<EditAnswerModel> _logger;

    [BindProperty]
    public InputModel? Input { get; set; }

    public int AnswerId { get; set; }
    public int QuestionId { get; set; }

    public EditAnswerModel(IPostRepository postRepository, ILogger<EditAnswerModel> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null || post.PostTypeId != 2)
        {
            return RedirectToPage("/Questions/Index");
        }

        AnswerId = id;
        QuestionId = post.ParentId ?? 0;

        Input = new InputModel
        {
            Body = post.Body ?? string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        AnswerId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null || post.PostTypeId != 2)
            {
                return RedirectToPage("/Questions/Index");
            }

            QuestionId = post.ParentId ?? 0;
            post.Body = Input?.Body ?? post.Body;

            await _postRepository.UpdateAsync(post);
            return Redirect($"/Questions/Details/{QuestionId}#answer-{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating answer {Id}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
            return Page();
        }
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Body is required")]
        [MinLength(30, ErrorMessage = "Body must be at least 30 characters")]
        public string Body { get; set; } = string.Empty;
    }
}
