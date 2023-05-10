using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services;

namespace DungeonWarAPI;


public class OwnedItemFactory
{
	private readonly MasterDataManager _masterDataManager;

	public OwnedItemFactory(MasterDataManager masterDataManager)
	{
		_masterDataManager = masterDataManager;
	}

	public OwnedItem CreateOwnedItem(int gameUserId, int itemCode, int enhancementCount = 0, int itemCount = 1)
	{
		var item = _masterDataManager.GetItem(itemCode);

		if (item == null)
		{
			throw new ArgumentException($"Invalid item code: {itemCode}");
		}

		return new OwnedItem
		{
			GameUserId = gameUserId,
			ItemCode = itemCode,
			EnhancementCount = enhancementCount,
			ItemCount = itemCount,
			Attack = item.Attack,
			Defense = item.Defence
		};
	}
}
