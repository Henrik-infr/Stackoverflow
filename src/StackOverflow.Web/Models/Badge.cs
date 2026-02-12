namespace StackOverflow.Web.Models;

public class Badge
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Class { get; set; }
    public bool TagBased { get; set; }

    // Badge classes
    public const int Gold = 1;
    public const int Silver = 2;
    public const int Bronze = 3;

    public string ClassName => Class switch
    {
        Gold => "gold",
        Silver => "silver",
        Bronze => "bronze",
        _ => "bronze"
    };

    public string ClassDisplayName => Class switch
    {
        Gold => "Gold",
        Silver => "Silver",
        Bronze => "Bronze",
        _ => "Bronze"
    };
}
