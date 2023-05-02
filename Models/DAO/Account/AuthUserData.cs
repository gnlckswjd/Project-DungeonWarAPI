namespace DungeonWarAPI.Models.DAO.Account;

public class AuthUserData
{
    public string Email { get; set; }
    public string AuthToken { get; set; }
    public int GameUserId { get; set; }
    public int PlayerId { get; set; }
}