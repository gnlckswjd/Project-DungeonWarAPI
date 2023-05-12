using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMemoryDatabase
{
    Task<ErrorCode> RegisterUserAsync(String id, String authToken, UserData userData);
    Task<(ErrorCode, List<String>)> LoadNotificationsAsync();

    Task<(ErrorCode, AuthUserData)> LoadAuthUserDataAsync(String email);

    Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
    Task<ErrorCode> UnLockUserRequestAsync(String key);
    Task<ErrorCode> InitializeStageDataAsync( String key, List<StageItem> items, List<StageNpc> npcs, Int32 stageLevel);

    Task<ErrorCode> IncrementItemCountAsync(String key, Int32 itemCode);
    Task<ErrorCode> IncrementNpcKillCountAsync(string key, int npcCode);
    Task<(ErrorCode, Dictionary<String, Int32>)> LoadStageDataAsync(String key);
    Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, Int32 pageNumber);
}