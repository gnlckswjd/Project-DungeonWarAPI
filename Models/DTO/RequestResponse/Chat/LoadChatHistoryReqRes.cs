using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Chat;

public class LoadChatHistoryRequest
{
	public String MessageId { get; set; }
}

public class LoadChatHistoryResponse
{
	public ErrorCode Error { get; set; }
	public List<ChatMessageReceived> ChatHistory { get; set; }
}