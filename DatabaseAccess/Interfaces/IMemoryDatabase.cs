using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IMemoryDatabase
{
    Task<ErrorCode> RegisterUserAsync(String id, String authToken, UserData userData);
    Task<(ErrorCode, List<String>)> LoadNotificationsAsync();
    Task<(ErrorCode, AuthUserData)> LoadAuthUserDataAsync(String email);
    Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
    Task<ErrorCode> UnLockUserRequestAsync(String key);
    Task<ErrorCode> StoreStageDataAsync(String key, List<KeyValuePair<String, Int32>> stageKeyValueList);
    Task<(ErrorCode, Int32 stageLevel)> LoadStageLevelAsync(String key);
    Task<(ErrorCode, Int32 itemAcquisitionCount)> LoadItemAcquisitionCountAsync(String key,Int32 itemCode);
    Task<ErrorCode> IncrementItemCountAsync(String key, Int32 itemCode, Int32 ItemCount);
    Task<(ErrorCode, Int32 npcKillCount)> LoadNpcKillCountAsync(String key,Int32 npcCode);
    Task<ErrorCode> IncrementNpcKillCountAsync(String key, Int32 npcCode);
    Task<(ErrorCode, Dictionary<String, Int32>)> LoadStageDataAsync(String key);
    Task<ErrorCode> DeleteStageDataAsync(String key);
    Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, Int32 pageNumber);
}