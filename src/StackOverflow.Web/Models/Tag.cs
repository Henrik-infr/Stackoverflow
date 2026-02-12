namespace StackOverflow.Web.Models;

public class Tag
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int? ExcerptPostId { get; set; }
    public int? WikiPostId { get; set; }

    // For display
    public string? Excerpt { get; set; }
}
