using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Models;
using StackOverflow.Web.Models.ViewModels;

namespace StackOverflow.Web.Pages.Questions;

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

    public async Task<IActionResult> OnPostAnswerAsync(int questionId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return RedirectToPage(new { id = questionId });
        }

        try
        {
            var answer = new Post
            {
                ParentId = questionId,
                Body = body,
                // In a real app, this would come from authentication
                OwnerUserId = null
            };

            var answerId = await _postRepository.CreateAnswerAsync(answer);
            return Redirect($"/Questions/Details/{questionId}#answer-{answerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating answer for question {QuestionId}", questionId);
            return RedirectToPage(new { id = questionId });
        }
    }
}
