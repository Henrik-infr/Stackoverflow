namespace StackOverflow.Web.Models;

public class PostLink
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public int PostId { get; set; }
    public int RelatedPostId { get; set; }
    public int LinkTypeId { get; set; }

    // Link types
    public const int Linked = 1;
    public const int Duplicate = 3;

    public bool IsDuplicate => LinkTypeId == Duplicate;
    public bool IsLinked => LinkTypeId == Linked;

    // Navigation
    public Post? RelatedPost { get; set; }
}
