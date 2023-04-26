﻿using firstAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlKata;

namespace firstAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly IAccountDatabase _accountDatabase;
		private readonly IMemoryDatabase _memoryDatabase;

		public LoginController(ILogger<LoginController> logger, IAccountDatabase accountDb, IMemoryDatabase memoryDb)
		{
		
			_accountDatabase = accountDb;
			_memoryDatabase = memoryDb;
		}

		[HttpPost]
		public async Task<PkLoginResponse> Post(PkLoginRequest request)
		{
			var response = new PkLoginResponse();

			//유저 정보 확인
			var (errorCode, accountId) = await _accountDatabase.VerifyAccount(request.Email, request.Password);
			if (errorCode != ErrorCode.None)
			{
				response.Result = errorCode;
				return response;
			}
			//토큰 발행 후 추가
			var authToken = Security.GetToken(); 


			errorCode = await _memoryDatabase.RegisterUserAsync(request.Email, authToken,accountId);

			if (errorCode != ErrorCode.None)
			{
				response.Result = errorCode;
				return response;
			}

			var (noticeErrorCode, notifications) = await _memoryDatabase.GetNoticeAsync();

			response.Notifications=notifications;
			response.AuthToken = authToken;
			return response;

		}

	}
	public class PkLoginRequest
	{
		public String Email { get; set; }

		public String Password { get; set; }
	}

	public class PkLoginResponse
	{
		public ErrorCode Result { get; set; }

		public string AuthToken { get; set; }

		public List<string> Notifications { get; set; }
	}
}
