using StackOverflow.Web.Models;

namespace StackOverflow.Web.Data.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetQuestionWithDetailsAsync(int id);
    Task<IEnumerable<Post>> GetQuestionsAsync(int page, int pageSize, string? sortBy = null);
    Task<IEnumerable<Post>> GetQuestionsByTagAsync(string tagName, int page, int pageSize);
    Task<IEnumerable<Post>> GetAnswersForQuestionAsync(int questionId);
    Task<int> GetQuestionCountAsync();
    Task<int> GetQuestionCountByTagAsync(string tagName);
    Task<IEnumerable<Post>> GetRecentQuestionsAsync(int count);
    Task<IEnumerable<Post>> GetUserQuestionsAsync(int userId, int count);
    Task<IEnumerable<Post>> GetUserAnswersAsync(int userId, int count);
    Task<IEnumerable<Post>> SearchAsync(string query, int page, int pageSize);
    Task<int> GetSearchCountAsync(string query);
    Task<int> CreateQuestionAsync(Post question);
    Task<int> CreateAnswerAsync(Post answer);
    Task UpdateAsync(Post post);
    Task<IEnumerable<PostLink>> GetRelatedQuestionsAsync(int postId);
    Task<IEnumerable<PostLink>> GetLinkedQuestionsAsync(int postId);
    Task<IEnumerable<PostHistory>> GetPostHistoryAsync(int postId);
}
