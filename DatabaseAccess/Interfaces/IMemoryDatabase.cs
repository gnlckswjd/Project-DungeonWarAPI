using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IMemoryDatabase
{
    Task<ErrorCode> RegisterUserAsync(String id, String authToken, UserData userData);
    Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
    Task<ErrorCode> UnLockUserRequestAsync(String key);

    Task<ErrorCode> InsertStageDataAsync(String key, List<KeyValuePair<String, Int32>> stageKeyValueList);
    Task<ErrorCode> InsertChatMessageAsync(String key, ChatMessageSent chatMessageSent);

	Task<(ErrorCode, List<String>)> LoadNotificationsAsync();
    Task<(ErrorCode, AuthenticatedUserState)> LoadAuthUserDataAsync(String email);
    Task<(ErrorCode, Int32 stageLevel)> LoadStageLevelAsync(String key);
    Task<(ErrorCode, Int32 itemAcquisitionCount)> LoadItemAcquisitionCountAsync(String key,Int32 itemCode);
    Task<(ErrorCode, Int32 npcKillCount)> LoadNpcKillCountAsync(String key, Int32 npcCode);
    Task<(ErrorCode, Dictionary<String, Int32>)> LoadStageDataAsync(String key);
    Task<(ErrorCode, ChatMessageReceived )> LoadLatestChatMessageAsync(String key, String MessageId);
    Task<(ErrorCode, List<ChatMessageReceived>)> LoadLatestChatHistoryAsync(String key, String MessageId);


	Task<ErrorCode> IncrementItemCountAsync(String key, Int32 itemCode, Int32 ItemCount);
	Task<ErrorCode> IncrementNpcKillCountAsync(String key, Int32 npcCode);
	Task<ErrorCode> UpdateUserStateAsync(String key, AuthenticatedUserState authenticatedUserState, UserStateCode stateCode);
	Task<ErrorCode> UpdateChatChannelAsync(String key, AuthenticatedUserState authenticatedUserState, Int32 channelNumber);

	Task<ErrorCode> DeleteStageDataAsync(String key);



	
}