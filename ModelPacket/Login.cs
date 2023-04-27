
namespace DungeonWarAPI.ModelPacket;

public class PkLoginRequest
{
	public String Email { get; set; }

	public String Password { get; set; }
}

public class PkLoginResponse
{
	public ErrorCode Result { get; set; }

	public string AuthToken { get; set; }

	public List<string> Notifications { get; set; }
}

