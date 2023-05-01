using DungeonWarAPI.Game;
using DungeonWarAPI.ModelDatabase;

namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<(ErrorCode, Int32 )> CreateUserAsync(Int32 accountId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
	public Task<ErrorCode> RollbackCreateUserAsync(Int32 gameId);

	public Task<(ErrorCode, UserData )> LoadUserData(Int32 playerId);
	public Task<(ErrorCode, List<OwnedItem> )> LoadUserItems(Int32 gameUserId);

	public Task<(ErrorCode, List<Mail> )> LoadUserMails(Int32 gameUserId, Int32 pageNumber);

	public Task<ErrorCode> VerifyMailOwnerId(Int32 gameUserId, Int64 mailId);

	public Task<ErrorCode> MarkMailAsRead(Int32 gameUserId, Int64 mailId);
	public Task<(ErrorCode, Mail)> MarkMailItemAsReceive(int gameUserId, long mailId);

	public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> ReceiveItemAsync(int gameUserId, Mail mail);
}