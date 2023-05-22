using System.Text;
using System.Text.Json;
using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.RequestResponse;


namespace DungeonWarAPI.Middleware;

public class UserAuthentication
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly MasterDataProvider _masterDataProvider;

	private readonly RequestDelegate _next;

	public UserAuthentication(IMemoryDatabase memoryDatabase, MasterDataProvider masterDataProvider, RequestDelegate next)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataProvider = masterDataProvider;
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		

		context.Request.EnableBuffering();

		var userLockKey = "";

		using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
		{
			var requestBody = await streamReader.ReadToEndAsync();
			if (await IsRequestBodyNullOrEmpty(requestBody, context))
			{
				return;
			}

			var bodyDocument = JsonDocument.Parse(requestBody);

			var( errorCode,  appVersion, masterDataVersion)= ExtractAppAndMasterDataVersion(bodyDocument,context);
			if (errorCode != ErrorCode.None)
			{
				return;
			}

			if (await IsWrongAppVersion(appVersion, context) || await IsWrongMasterDataVersion(masterDataVersion, context))
			{
				return;
			}

			if (IsLoginOrCreaetAccount(context))
			{
				context.Request.Body.Position = 0;
				await _next(context);
				return;
			}

			(errorCode,var email, var authToken) = ExtractEmailAndAuthToken(bodyDocument, context);
			if (errorCode != ErrorCode.None)
			{
				return;
			}

			(errorCode, var authUserData) = await _memoryDatabase.LoadAuthUserDataAsync(email);
			if (errorCode != ErrorCode.None)
			{
				return;
			}

			if (await IsAuthTokenWrong(authUserData, authToken, context))
			{
				return;
			}

			userLockKey = MemoryDatabaseKeyGenerator.MakeUserLockKey(authUserData.Email);

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

			context.Items[nameof(AuthenticatedUserState)] = authUserData;
		}

		context.Request.Body.Position = 0;
		await _next(context);
		//락해제

		await _memoryDatabase.UnLockUserRequestAsync(userLockKey);
	}

	(ErrorCode, String appVersion, String masterDataVersion) ExtractAppAndMasterDataVersion(JsonDocument bodyDocument, HttpContext context)
	{
		try
		{
			String appVersion = bodyDocument.RootElement.GetProperty("AppVersion").GetString();
			String masterDataVersion = bodyDocument.RootElement.GetProperty("MasterDataVersion").GetString();
			return (ErrorCode.None, appVersion, masterDataVersion);
		}
		catch (Exception e)
		{
			var jsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.InvalidRequestHttpBody
			});

			var bytes = Encoding.UTF8.GetBytes(jsonResponse);
			context.Response.Body.Write(bytes, 0, bytes.Length);

			return(ErrorCode.WrongAppVersionOrMasterDataVersion,"","");
		}
	}

	(ErrorCode, String email, String authToken) ExtractEmailAndAuthToken(JsonDocument bodyDocument, HttpContext context)
	{
		try
		{
			String email = bodyDocument.RootElement.GetProperty("Email").GetString();
			String authToken = bodyDocument.RootElement.GetProperty("AuthToken").GetString();	
			return (ErrorCode.None, email, authToken);
		}
		catch (Exception e)
		{
			var jsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.InvalidRequestHttpBody
			});

			var bytes = Encoding.UTF8.GetBytes(jsonResponse);
			context.Response.Body.Write(bytes, 0, bytes.Length);

			return (ErrorCode.WrongAuthTokenRequest, "", "");
		}
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
		if (String.CompareOrdinal(appVersion, _masterDataProvider.Versions.AppVersion) != 0)
		{
			var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.WrongAppVersionOrMasterDataVersion
			});
			var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

			return true;
		}

		return false;
	}

	async Task<bool> IsWrongMasterDataVersion(String masterDataVersion, HttpContext context)
	{
		if (String.CompareOrdinal(masterDataVersion, _masterDataProvider.Versions.AppVersion) != 0)
		{
			var errorJsonResponse = JsonSerializer.Serialize(new AuthenticationResponse
			{
				Error = ErrorCode.WrongMasterDataVersion
			});
			var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
			await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

			return true;
		}

		return false;
	}

	async Task<bool> IsAuthTokenWrong(AuthenticatedUserState authenticatedUserState, String authToken, HttpContext context)
	{
		if (String.CompareOrdinal(authenticatedUserState.AuthToken, authToken) != 0)
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