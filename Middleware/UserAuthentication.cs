using System.Text;
using System.Text.Json;
using DungeonWarAPI.ModelDatabase;
using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using DungeonWarAPI.Utils;

namespace DungeonWarAPI.Middleware;

public class UserAuthentication
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly MasterDataManager _masterDataManager;

	private readonly RequestDelegate _next;

	public UserAuthentication(IMemoryDatabase memoryDatabase, MasterDataManager masterDataManager, RequestDelegate next)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataManager= masterDataManager;
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var requestUrlPath = context.Request.Path;
		var isLoginRequest = String.Compare(requestUrlPath, "/Login", StringComparison.OrdinalIgnoreCase);
		var isCreateAccountRequest =
			String.Compare(requestUrlPath, "/CreateAccount", StringComparison.OrdinalIgnoreCase);

		if (isLoginRequest == 0 || isCreateAccountRequest == 0)
		{
			await _next(context);
			return;
		}

		context.Request.EnableBuffering();

		String email;
		String authToken;
		String appVersion;
		String masterDataVersion;
		var userLockKey = "";

		using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
		{
			var requestBody = await streamReader.ReadToEndAsync();

			if (String.IsNullOrEmpty(requestBody) == true)
			{
				var jsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = ErrorCode.InvalidRequestHttpBody
				});
				// json 형태 에러코드 입력
				var bytes = Encoding.UTF8.GetBytes(jsonResponse);
				await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
				return;
			}

			var bodyDocument = JsonDocument.Parse(requestBody);

			try
			{
				//JsonDocument에서 데이터 추출
				email = bodyDocument.RootElement.GetProperty("Email").GetString();
				authToken = bodyDocument.RootElement.GetProperty("AuthToken").GetString();
				appVersion = bodyDocument.RootElement.GetProperty("AppVersion").GetString();
				masterDataVersion = bodyDocument.RootElement.GetProperty("MasterDataVersion").GetString();
			}
			catch (Exception e)
			{
				email = "";
				authToken = "";

				var jsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = ErrorCode.InvalidRequestHttpBody
				});

				var bytes = Encoding.UTF8.GetBytes(jsonResponse);
				context.Response.Body.Write(bytes, 0, bytes.Length);

				return;
			}

			if (String.CompareOrdinal(appVersion, _masterDataManager.Versions.AppVersion) != 0)
			{
				var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = ErrorCode.WrongAppVersion
				});
				var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
				await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

				return;
			}
			if (String.CompareOrdinal(masterDataVersion, _masterDataManager.Versions.MasterDataVersion) != 0)
			{
				var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = ErrorCode.WrongMasterDataVersion
				});
				var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
				await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

				return;
			}


			var (errorCode, authUserData) = await _memoryDatabase.LoadAuthUserDataAsync(email);

			if (errorCode != ErrorCode.None)
			{
				return;
			}

			if (String.CompareOrdinal(authUserData.AuthToken, authToken) != 0)
			{
				var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = ErrorCode.WrongAuthTokenRequest
				});
				var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
				await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

				return;
			}

			userLockKey = MemoryDatabaseKeyGenerator.MakeUserLockKey(authUserData.Email);

			var setLockError = await _memoryDatabase.LockUserRequestAsync(userLockKey,authToken);
			if (ErrorCode.None != setLockError)
			{
				var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					result = setLockError
				});
				var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
				await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
				return;
			}

			context.Items[nameof(AuthUserData)] = authUserData;
		}

		context.Request.Body.Position = 0;
		await _next(context);
		//락해제

		await _memoryDatabase.UnLockUserRequestAsync(userLockKey);
	}
}