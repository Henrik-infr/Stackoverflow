namespace StackOverflow.Web.Models.ViewModels;

public class UserProfileViewModel
{
    public User User { get; set; } = new();
    public List<Post> RecentQuestions { get; set; } = new();
    public List<Post> RecentAnswers { get; set; } = new();
    public List<Badge> Badges { get; set; } = new();
    public BadgeSummary BadgeSummary { get; set; } = new();
    public List<Tag> TopTags { get; set; } = new();
}

public class BadgeSummary
{
    public int GoldCount { get; set; }
    public int SilverCount { get; set; }
    public int BronzeCount { get; set; }
    public int TotalCount => GoldCount + SilverCount + BronzeCount;
}

public class UserListViewModel
{
    public List<User> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 36;
    public string? SortBy { get; set; }
    public string? Filter { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
