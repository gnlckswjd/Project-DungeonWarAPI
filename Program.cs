using System.Text.Json;
using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Implementations;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Middleware;
using DungeonWarAPI.ModelConfiguration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DatabaseConfiguration>(configuration.GetSection(nameof(DatabaseConfiguration)));

DependencyInjection();

builder.Services.AddControllers();

SetLogger();

var app = builder.Build();
app.UseRouting();
app.UseUserAuthentication();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run(configuration["ServerAddress"]);

void DependencyInjection()
{
	builder.Services.AddTransient<IAccountDataCRUD, AccountDataCRUD>();
	builder.Services.AddTransient<IUserDataCRUD,UserDataCRUD>();
	builder.Services.AddTransient<IMailDataCRUD,MailDataCRUD>();
	builder.Services.AddTransient<IItemDATACRUD, ItemDataCRUD>();
	builder.Services.AddTransient<IInAppPurchaseDataCRUD, InAppPurchaseDataCRUD>();
	builder.Services.AddTransient<IAttendanceDataCRUD,AttendanceDataCRUD>();
	builder.Services.AddTransient<IEnhancementDataCRUD,EnhancementDataCRUD>();
	builder.Services.AddTransient<IStageDataCRUD, StageDataCRUD>();
	
	builder.Services.AddTransient<IMasterDataLoader, MasterDataLoader>();
	builder.Services.AddSingleton<IMemoryDatabase, RedisDatabase>();
	builder.Services.AddSingleton<MasterDataProvider>();
	builder.Services.AddSingleton<OwnedItemFactory>();
	builder.Services.AddSingleton<ChatRoomAllocator>();
	builder.Services.AddScoped<QueryFactory>(provider =>
	{
		var config = provider.GetRequiredService<IOptions<DatabaseConfiguration>>();
		var connection = new MySqlConnection(config.Value.GameDatabase);
		connection.Open();

		var queryFactory = new QueryFactory(connection, new MySqlCompiler());
		return queryFactory;
	});
}

void SetLogger()
{
	var logging = builder.Logging;
	logging.ClearProviders();

	var fileDirection = configuration["logDirection"];
	if (!Directory.Exists(fileDirection))
	{
		Directory.CreateDirectory(fileDirection);
	}

	logging.AddZLoggerFile($"{fileDirection}/MainLog.log");

	logging.AddZLoggerRollingFile(
		fileNameSelector: (dt, x) => $"{fileDirection}/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
		timestampPattern: x => x.ToLocalTime().Date,
		rollSizeKB: 1024,
		configure: options =>
		{
			options.EnableStructuredLogging = true;

			var timeName = JsonEncodedText.Encode("Timestamp");

			options.StructuredLoggingFormatter = (writer, info) =>
			{
				var timeValue = JsonEncodedText.Encode(info.Timestamp.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss.fff"));

				writer.WriteString(timeName, timeValue);
				info.WriteToJsonWriter(writer);
			};
		}
	);


	logging.AddZLoggerConsole(options =>
	{
		options.EnableStructuredLogging = true;

		var timeName = JsonEncodedText.Encode("Timestamp");
		options.StructuredLoggingFormatter = (writer, info) =>
		{
			var timeValue = JsonEncodedText.Encode(info.Timestamp.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss.fff"));

			writer.WriteString(timeName, timeValue);
			info.WriteToJsonWriter(writer);
		};
	});
}