using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO;

namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<(ErrorCode, Int32 )> CreateUserAsync(Int32 accountId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
	public Task<ErrorCode> RollbackCreateUserAsync(Int32 gameId);
	public Task<(ErrorCode, UserData )> LoadUserDataAsync(Int32 playerId);
	public Task<(ErrorCode, List<OwnedItem> )> LoadUserItemsAsync(Int32 gameUserId);
	public Task<(ErrorCode, List<MailWithItems> )> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber);
	public Task<ErrorCode> MarkMailAsReadAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> MarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> ReceiveItemAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);
	public Task<(ErrorCode, DateTime lastLoginDate, short attendanceCount)> UpdateLoginDateAsync(Int32 gameUserId);
	public Task<ErrorCode> CreateAttendanceRewardMailAsync(int gameUserId, AttendanceReward reward);
	public Task<ErrorCode> RollbackLoginDateAsync(Int32 gameUserId, DateTime lastLoginDate, Int16 attendanceCount );
	public Task<(ErrorCode, Int32)> StoreReceiptAsync(Int32 gameUserId, String purchaseId, Int32 packageId);
	public Task<ErrorCode> CreateInAppMailAsync(Int32 gameUserId, List<PackageItem> packageItems);
	public Task<ErrorCode> RollbackStoreReceiptAsync(Int32 receiptId);

	public Task<(ErrorCode, Int32 itemCode, Int32 enhancementCount)> LoadItemAsync(Int32 gameUserId, Int64 itemId );

	public Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold);

	public Task<ErrorCode> UpdateEnhancementCountAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount);

	public Task<ErrorCode> DestroyItemAsync(Int32 gameUserId, Int64 itemId);

	public Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Boolean isSuccess);

	public Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold);

	public Task<ErrorCode> RollbackUpdateEnhancementCountAsync(Int64 itemId);

	public Task<ErrorCode> RollbackDestroyItem(Int64 itemId);
}