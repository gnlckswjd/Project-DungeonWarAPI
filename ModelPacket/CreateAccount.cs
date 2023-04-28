using DungeonWarAPI;

namespace DungeonWarAPI.ModelPacket;

public class PkCreateAccountRequest
{
	public String Email { get; set; }
	public String Password { get; set; }
}

public class PkCreateAccountResponse
{
	public ErrorCode Result { get; set; }
}