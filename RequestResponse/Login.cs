
using DungeonWarAPI.Game;

namespace DungeonWarAPI.ModelPacket;

public class LoginRequest
{
	public String Email { get; set; }

	public String Password { get; set; }
}

public class LoginResponse
{
	public ErrorCode Result { get; set; }

	public Int32 UserLevel { get; set; }


	public String AuthToken { get; set; }

	public List<String> Notifications { get; set; }
}

