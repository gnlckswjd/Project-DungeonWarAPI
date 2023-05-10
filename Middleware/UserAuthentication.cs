using System.Text;
using System.Text.Json;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestRespose;
using DungeonWarAPI.Services;
using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Utilities;

namespace DungeonWarAPI.Middleware;

public class UserAuthentication
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly MasterDataManager _masterDataManager;

	private readonly RequestDelegate _next;

	public UserAuthentication(IMemoryDatabase memoryDatabase, MasterDataManager masterDataManager, RequestDelegate next)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataManager = masterDataManager;
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		

		context.Request.EnableBuffering();

		var userLockKey = "";
		String email;
		String authToken;
		String appVersion;
		String masterDataVersion;

		using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
		{
			var requestBody = await streamReader.ReadToEndAsync();
			if (await IsRequestBodyNullOrEmpty(requestBody, context))
			{
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
					Error = ErrorCode.InvalidRequestHttpBody
				});

				var bytes = Encoding.UTF8.GetBytes(jsonResponse);
				context.Response.Body.Write(bytes, 0, bytes.Length);

				return;
			}

			if (IsLoginOrCreaetAccount(context))
			{

				if (await IsWrongMasterDataVersion(masterDataVersion, context))
				{
					return;
				}

				if (await IsWrongAppVersion(appVersion, context))
				{
					return;
				}
				context.Request.Body.Position = 0;
				await _next(context);
				return;
			}

			var (errorCode, authUserData) = await _memoryDatabase.LoadAuthUserDataAsync(email);

			if (errorCode != ErrorCode.None)
			{
				return;
			}

			if (await IsAuthTokenWrong(authUserData, authToken, context))
			{
				return;
			}


			if (await IsWrongMasterDataVersion(masterDataVersion, context))
			{
				return;
			}

			if (await IsWrongAppVersion(appVersion, context))
			{
				return;
			}


			
			userLockKey = MemoryDatabaseKeyUtility.MakeUserLockKey(authUserData.Email);

			var setLockError = await _memoryDatabase.LockUserRequestAsync(userLockKey, authToken);
			if (ErrorCode.None != setLockError)
			{
				var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
				{
					Error = setLockError
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


	bool IsLoginOrCreaetAccount(HttpContext context)
	{
		var requestUrlPath = context.Request.Path;
		var isLoginRequest = String.Compare(requestUrlPath, "/Login", StringComparison.OrdinalIgnoreCase);
		var isCreateAccountRequest =
			String.Compare(requestUrlPath, "/CreateAccount", StringComparison.OrdinalIgnoreCase);

		if (isLoginRequest == 0 || isCreateAccountRequest == 0)
		{
			return true;
		}

		return false;
	}

	async Task<bool> IsRequestBodyNullOrEmpty(String requestBody, HttpContext context)
	{
		if (String.IsNullOrEmpty(requestBody) == true)
		{
			var jsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.InvalidRequestHttpBody
			});
			// json 형태 에러코드 입력
			var bytes = Encoding.UTF8.GetBytes(jsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
			return true;
		}

		return false;
	}

	async Task<bool> IsWrongAppVersion(String appVersion, HttpContext context)
	{
		if (String.CompareOrdinal(appVersion, _masterDataManager.Versions.AppVersion) != 0)
		{
			var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.WrongAppVersion
			});
			var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

			return true;
		}

		return false;
	}

	async Task<bool> IsWrongMasterDataVersion(String masterDataVersion, HttpContext context)
	{
		if (String.CompareOrdinal(masterDataVersion, _masterDataManager.Versions.AppVersion) != 0)
		{
			var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.WrongAppVersion
			});
			var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

			return true;
		}

		return false;
	}

	async Task<bool> IsAuthTokenWrong(AuthUserData authUserData, String authToken, HttpContext context)
	{
		if (String.CompareOrdinal(authUserData.AuthToken, authToken) != 0)
		{
			var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.WrongAuthTokenRequest
			});
			var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

			return true;
		}

		return false;
	}
}