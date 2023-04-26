using firstAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace firstAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class CreateAccountController : ControllerBase
	{
		private readonly IAccountDatabase _accountDatabase;
		private readonly ILogger<CreateAccountController> _logger;

		public CreateAccountController(IAccountDatabase accountDatabase, ILogger<CreateAccountController> logger)
		{
			_accountDatabase = accountDatabase;
			_logger = logger;
		}

		[HttpPost]
		public async Task<PkCreateAccountResponse> Post(PkCreateAccountRequest packet)
		{
			var response = new PkCreateAccountResponse();


			var errorCode= await _accountDatabase.CreateAccountAsync(packet.Email, packet.Password);
			response.Result = errorCode;
			if (errorCode != ErrorCode.None)
			{
				return response;
			}

			Console.WriteLine("Account is Created!");
			return response;
		}
	}

	public class PkCreateAccountRequest
	{
		public String Email { get; set; }

		public String Password { get; set; }
	}

	public class PkCreateAccountResponse
	{
		public ErrorCode Result { get; set; }
	}

}
