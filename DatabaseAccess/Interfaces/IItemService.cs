using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IItemService
{
	public Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<MailItem> items);
}