using System.Text.Json;
using DungeonWarAPI.Middleware;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Services;
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
	builder.Services.AddTransient<IAccountDatabase, AccountDatabase>();
	builder.Services.AddTransient<IGameDatabase, GameDatabase>();
	builder.Services.AddTransient<IMasterDatabase, MasterGameDatabase>();
	builder.Services.AddSingleton<IMemoryDatabase, RedisDatabase>();
	builder.Services.AddSingleton<MasterDataManager>();
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

	logging.AddZLoggerFile("MailLog.log");

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