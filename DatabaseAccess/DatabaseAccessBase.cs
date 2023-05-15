using SqlKata.Execution;

namespace DungeonWarAPI.DatabaseAccess;

public class DatabaseAccessBase
{
	protected readonly ILogger _logger;
	protected readonly QueryFactory _queryFactory;
	protected DatabaseAccessBase(ILogger logger, QueryFactory queryFactory)
	{
		_logger = logger;
		_queryFactory = queryFactory;
	}
}