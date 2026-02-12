namespace StackOverflow.Web.Models;

public class Vote
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int VoteTypeId { get; set; }
    public int? UserId { get; set; }
    public DateTime CreationDate { get; set; }
    public int? BountyAmount { get; set; }

    // Vote types
    public const int AcceptedByOriginator = 1;
    public const int UpMod = 2;
    public const int DownMod = 3;
    public const int Offensive = 4;
    public const int Favorite = 5;
    public const int Close = 6;
    public const int Reopen = 7;
    public const int BountyStart = 8;
    public const int BountyClose = 9;
    public const int Deletion = 10;
    public const int Undeletion = 11;
    public const int Spam = 12;

    public bool IsUpvote => VoteTypeId == UpMod;
    public bool IsDownvote => VoteTypeId == DownMod;
    public bool IsFavorite => VoteTypeId == Favorite;
}
