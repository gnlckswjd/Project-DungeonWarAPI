using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Chat;

public class ChannelChangeRequest
{
	public Int32 ChannelNumber { get; set; }
}
public class ChannelChangeResponse
{
	public ErrorCode Error { get; set; }
}