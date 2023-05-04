using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO;

namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<(ErrorCode, Int32 )> CreateUserAsync(Int32 accountId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
	public Task<ErrorCode> RollbackCreateUserAsync(Int32 gameId);
	public Task<(ErrorCode, UserData )> LoadUserData(Int32 playerId);
	public Task<(ErrorCode, List<OwnedItem> )> LoadUserItems(Int32 gameUserId);
	public Task<(ErrorCode, List<MailWithItems> )> LoadMailList(Int32 gameUserId, Int32 pageNumber);
	public Task<ErrorCode> MarkMailAsRead(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> MarkMailItemAsReceive(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> ReceiveItemAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);
	public Task<(ErrorCode, DateTime lastLoginDate, short attendanceCount)> UpdateLoginDateAsync(Int32 gameUserId);
	public Task<ErrorCode> CreateAttendanceRewardMailAsync(AttendanceReward reward, Int32 gameUserId);

	public Task<ErrorCode> RollbackLoginDate(Int32 gameUserId, DateTime lastLoginDate, Int16 attendanceCount );
}