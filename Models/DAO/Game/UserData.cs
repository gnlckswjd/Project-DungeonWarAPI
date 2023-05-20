namespace DungeonWarAPI.Models.Database.Game;

public class UserData
{
    public Int32 GameUserId { get; set; }
    public Int32 PlayerId { get; set; }
    public Int32 UserLevel { get; set; }
    public Int32 Exp { get; set; }
    public Int64 Gold { get; set; }

}