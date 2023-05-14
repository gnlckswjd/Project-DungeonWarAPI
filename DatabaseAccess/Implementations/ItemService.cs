using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.GameLogic;
using SqlKata.Execution;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class ItemService:IItemService
{
	private readonly ILogger<ItemService> _logger;
	private readonly OwnedItemFactory _ownedItemFactory;
	private readonly QueryFactory _queryFactory;

	public ItemService(ILogger<ItemService> logger, QueryFactory queryFactory,
		OwnedItemFactory ownedItemFactory)
	{
		_logger = logger;
		_ownedItemFactory = ownedItemFactory;

		_queryFactory = queryFactory;
	}

}