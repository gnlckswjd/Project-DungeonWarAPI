using DungeonWarAPI;

namespace DungeonWarAPI.ModelPacket;

public class CreateAccountRequest
{
	public String Email { get; set; }
	public String Password { get; set; }
}

public class CreateAccountResponse
{
	public ErrorCode Result { get; set; }
}