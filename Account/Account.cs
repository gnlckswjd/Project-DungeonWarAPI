namespace DungeonWarAPI.ModelDatabase;

public class Account
{
	public Int32 AccountId { get; set; }

	public String Email { get; set; }
	public String HashedPassword { get; set; }
	public String SaltValue { get; set; }
}