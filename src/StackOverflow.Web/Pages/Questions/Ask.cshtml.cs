using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;

namespace StackOverflow.Web.Pages.Questions;

[IgnoreAntiforgeryToken]
public class AskModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<AskModel> _logger;

    [BindProperty]
    public InputModel? Input { get; set; }

    public AskModel(IPostRepository postRepository, ILogger<AskModel> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public void OnGet()
    {
        Input = new InputModel();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Convert comma-separated tags to Stack Overflow format: <tag1><tag2>
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

            var question = new Post
            {
                Title = Input?.Title ?? string.Empty,
                Body = Input?.Body ?? string.Empty,
                Tags = formattedTags,
                // In a real app, this would come from authentication
                OwnerUserId = null
            };

            var questionId = await _postRepository.CreateQuestionAsync(question);
            return RedirectToPage("/Questions/Details", new { id = questionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            ModelState.AddModelError(string.Empty, "An error occurred while posting your question. Please try again.");
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
