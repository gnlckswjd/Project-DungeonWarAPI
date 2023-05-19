using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Chat;

public class SendChatRequest
{
	public String Message { get; set; }
}
public class SendChatResponse
{
	public ErrorCode Error { get; set; }
}