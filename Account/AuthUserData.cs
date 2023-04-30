namespace DungeonWarAPI.ModelDatabase;

public class AuthUserData
{
	public String Email { get; set; }
	public String AuthToken { get; set; }
	public Int32 GameUserId { get; set; }
	public Int32 PlayerId { get; set; }
}