namespace DungeonWarAPI.Models.Database.Game;

public class UserData
{
    public int GameUserId { get; set; }
    public int PlayerId { get; set; }
    public int UserLevel { get; set; }
    public Int32 Exp { get; set; }
    public Int64 Gold { get; set; }

}