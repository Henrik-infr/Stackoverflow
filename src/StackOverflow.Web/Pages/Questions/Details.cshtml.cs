using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Questions;

[IgnoreAntiforgeryToken]
public class DetailsModel : PageModel
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<DetailsModel> _logger;

    public QuestionViewModel? ViewModel { get; set; }

    public DetailsModel(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILogger<DetailsModel> logger)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var question = await _postRepository.GetQuestionWithDetailsAsync(id);
            if (question == null)
            {
                return Page();
            }

            ViewModel = new QuestionViewModel
            {
                Question = question
            };

            // Get answers
            ViewModel.Answers = (await _postRepository.GetAnswersForQuestionAsync(id)).ToList();

            // Get comments for question
            ViewModel.QuestionComments = (await _commentRepository.GetByPostIdAsync(id)).ToList();

            // Get comments for answers
            var answerIds = ViewModel.Answers.Select(a => a.Id).ToList();
            if (answerIds.Any())
            {
                var allAnswerComments = await _commentRepository.GetByPostIdsAsync(answerIds);
                ViewModel.AnswerComments = allAnswerComments
                    .GroupBy(c => c.PostId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }

            // Get related and linked questions
            ViewModel.RelatedQuestions = (await _postRepository.GetRelatedQuestionsAsync(id)).ToList();
            ViewModel.LinkedQuestions = (await _postRepository.GetLinkedQuestionsAsync(id)).ToList();

            // Get edit history
            ViewModel.EditHistory = (await _postRepository.GetPostHistoryAsync(id)).ToList();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question {Id}", id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAnswerAsync(int id, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return RedirectToPage(new { id });
        }

        try
        {
            var answer = new Post
            {
                ParentId = id,
                Body = body,
                // In a real app, this would come from authentication
                OwnerUserId = null
            };

            var answerId = await _postRepository.CreateAnswerAsync(answer);
            return Redirect($"/Questions/Details/{id}#answer-{answerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating answer for question {QuestionId}", id);
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostDeleteQuestionAsync(int id)
    {
        try
        {
            await _postRepository.DeleteAsync(id);
            return Redirect("/Questions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {Id}", id);
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostDeleteAnswerAsync(int questionId, int answerId)
    {
        try
        {
            await _postRepository.DeleteAsync(answerId);
            return Redirect($"/Questions/Details/{questionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting answer {AnswerId}", answerId);
            return RedirectToPage(new { id = questionId });
        }
    }
}
