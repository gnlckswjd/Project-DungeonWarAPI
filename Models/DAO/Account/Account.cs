namespace DungeonWarAPI.Models.DAO.Account;

public class Account
{
    public int AccountId { get; set; }

    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public string SaltValue { get; set; }
}