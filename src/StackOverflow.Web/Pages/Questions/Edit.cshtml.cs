using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;

namespace StackOverflow.Web.Pages.Questions;

[IgnoreAntiforgeryToken]
public class EditModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<EditModel> _logger;

    [BindProperty]
    public InputModel? Input { get; set; }

    public int QuestionId { get; set; }

    public EditModel(IPostRepository postRepository, ILogger<EditModel> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null || post.PostTypeId != 1)
        {
            return RedirectToPage("/Questions/Index");
        }

        QuestionId = id;

        // Convert tags from <c#><asp.net> to comma-separated
        var tagsDisplay = string.Empty;
        if (!string.IsNullOrEmpty(post.Tags))
        {
            var tagNames = post.Tags.Split(new[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
            tagsDisplay = string.Join(", ", tagNames);
        }

        Input = new InputModel
        {
            Title = post.Title ?? string.Empty,
            Body = post.Body ?? string.Empty,
            Tags = tagsDisplay
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        QuestionId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null || post.PostTypeId != 1)
            {
                return RedirectToPage("/Questions/Index");
            }

            // Convert comma-separated tags to <tag1><tag2> format
            var formattedTags = string.Empty;
            if (!string.IsNullOrWhiteSpace(Input?.Tags))
            {
                var tagList = Input.Tags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Take(5);
                formattedTags = string.Join("", tagList.Select(t => $"<{t}>"));
            }

            post.Title = Input?.Title ?? post.Title;
            post.Body = Input?.Body ?? post.Body;
            post.Tags = formattedTags;

            await _postRepository.UpdateAsync(post);
            return Redirect($"/Questions/Details/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {Id}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
            return Page();
        }
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300, MinimumLength = 15, ErrorMessage = "Title must be between 15 and 300 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        [MinLength(30, ErrorMessage = "Body must be at least 30 characters")]
        public string Body { get; set; } = string.Empty;

        public string? Tags { get; set; }
    }
}
