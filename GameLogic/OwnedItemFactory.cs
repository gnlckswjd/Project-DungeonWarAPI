using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public class OwnedItemFactory
{
	private readonly MasterDataProvider _masterDataProvider;

	public OwnedItemFactory(MasterDataProvider masterDataProvider)
	{
		_masterDataProvider = masterDataProvider;
	}

	public OwnedItem CreateOwnedItem(Int32 gameUserId, Int32 itemCode, Int32 enhancementCount = 0, Int32 itemCount = 1)
	{
		var item = _masterDataProvider.GetItem(itemCode);

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

	public List<OwnedItem> CreateDefaultItems(Int32 gameUserId)
	{
		return new List<OwnedItem>
		{
			CreateOwnedItem(gameUserId, (Int32)ItemCode.SmallSword),
			CreateOwnedItem(gameUserId, (Int32)ItemCode.OrdinaryHat)
		};
	}
}