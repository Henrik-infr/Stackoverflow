namespace StackOverflow.Web.Models.ViewModels;

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<Post> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? SortBy { get; set; }
    public SearchFilters Filters { get; set; } = new();

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class SearchFilters
{
    public bool HasAcceptedAnswer { get; set; }
    public int? MinScore { get; set; }
    public int? MinAnswers { get; set; }
    public string? Tag { get; set; }
    public string? User { get; set; }
}

public class TagListViewModel
{
    public List<Tag> Tags { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 36;
    public string? SortBy { get; set; }
    public string? Filter { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
