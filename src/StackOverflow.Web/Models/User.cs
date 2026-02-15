namespace StackOverflow.Web.Models;

public class User
{
    public int Id { get; set; }
    public int Reputation { get; set; }
    public DateTime CreationDate { get; set; }
    public string? DisplayName { get; set; }
    public DateTime LastAccessDate { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Location { get; set; }
    public string? AboutMe { get; set; }
    public int Views { get; set; }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? EmailHash { get; set; }
    public int? AccountId { get; set; }
    public string? PasswordHash { get; set; }

    // Computed properties
    public string GravatarUrl => !string.IsNullOrEmpty(EmailHash)
        ? $"https://www.gravatar.com/avatar/{EmailHash}?s=128&d=identicon"
        : $"https://www.gravatar.com/avatar/{Id}?s=128&d=identicon";

    // Statistics (populated by queries)
    public int QuestionCount { get; set; }
    public int AnswerCount { get; set; }
    public List<Badge> Badges { get; set; } = new();
}
