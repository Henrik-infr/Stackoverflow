namespace StackOverflow.Web.Models;

public class PostHistory
{
    public int Id { get; set; }
    public int PostHistoryTypeId { get; set; }
    public int PostId { get; set; }
    public string? RevisionGUID { get; set; }
    public DateTime CreationDate { get; set; }
    public int? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? Comment { get; set; }
    public string? Text { get; set; }
    public string? ContentLicense { get; set; }

    // Post history types
    public const int InitialTitle = 1;
    public const int InitialBody = 2;
    public const int InitialTags = 3;
    public const int EditTitle = 4;
    public const int EditBody = 5;
    public const int EditTags = 6;
    public const int RollbackTitle = 7;
    public const int RollbackBody = 8;
    public const int RollbackTags = 9;

    public string TypeDescription => PostHistoryTypeId switch
    {
        InitialTitle => "Initial Title",
        InitialBody => "Initial Body",
        InitialTags => "Initial Tags",
        EditTitle => "Edit Title",
        EditBody => "Edit Body",
        EditTags => "Edit Tags",
        RollbackTitle => "Rollback Title",
        RollbackBody => "Rollback Body",
        RollbackTags => "Rollback Tags",
        _ => $"Type {PostHistoryTypeId}"
    };

    // Navigation
    public User? User { get; set; }
}
