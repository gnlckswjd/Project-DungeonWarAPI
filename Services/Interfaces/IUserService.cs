using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IUserService
{
    public Task<(ErrorCode, int)> CreateUserAsync(int accountId);
    public Task<ErrorCode> CreateUserItemAsync(int gameId);
    public Task<ErrorCode> RollbackCreateUserAsync(int gameId);
    public Task<(ErrorCode, UserData)> LoadUserDataAsync(int playerId);
    public Task<(ErrorCode, List<OwnedItem>)> LoadUserItemsAsync(int gameUserId);
}