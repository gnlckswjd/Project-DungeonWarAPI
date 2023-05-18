using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IItemService
{
	public Task<(ErrorCode, OwnedItem)> LoadItemAsync(Int32 gameUserId, Int64 itemId);

	public Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<MailItem> items);
	public Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<(Int32, Int32)> itemCodeList);
	public Task<ErrorCode> InsertNonStackableItemsAsync(Int32 gameUserId, List<OwnedItem> items);

	public Task<ErrorCode> DestroyItemAsync(Int32 gameUserId, Int64 itemId);

	public Task<ErrorCode> RollbackDestroyItemAsync(Int64 itemId);
}