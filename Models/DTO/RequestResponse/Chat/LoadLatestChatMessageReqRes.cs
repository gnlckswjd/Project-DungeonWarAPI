using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Chat;

public class LoadLatestChatMessageRequest
{
	public String Email { get; set; }
	public String MessageId { get; set; }
}

public class LoadLatestChatMessageResponse
{
	public ErrorCode Error { get; set; }
	public ChatMessageReceived ChatMessage { get; set; }
}