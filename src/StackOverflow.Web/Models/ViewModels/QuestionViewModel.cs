namespace StackOverflow.Web.Models.ViewModels;

public class QuestionViewModel
{
    public Post Question { get; set; } = new();
    public List<Post> Answers { get; set; } = new();
    public List<Comment> QuestionComments { get; set; } = new();
    public Dictionary<int, List<Comment>> AnswerComments { get; set; } = new();
    public List<PostLink> RelatedQuestions { get; set; } = new();
    public List<PostLink> LinkedQuestions { get; set; } = new();
    public List<PostHistory> EditHistory { get; set; } = new();
}

public class QuestionListViewModel
{
    public List<Post> Questions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? SortBy { get; set; }
    public string? TagFilter { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class AskQuestionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}
