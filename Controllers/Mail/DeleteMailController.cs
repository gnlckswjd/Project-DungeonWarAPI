using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers.Mail;

[Route("[controller]")]
[ApiController]
public class DeleteMailController : Controller
{
    private readonly IMailDataCRUD _mailDataCRUD;
    private readonly ILogger<DeleteMailController> _logger;

    public DeleteMailController(ILogger<DeleteMailController> logger, IMailDataCRUD mailDataCRUD)
    {
        _mailDataCRUD = mailDataCRUD;
        _logger = logger;
    }

    [HttpPost]
    public async Task<DeleteMailResponse> Post(DeleteMailRequest request)
    {
        var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new DeleteMailResponse();

        if (authenticatedUserState == null)
        {
	        response.Error = ErrorCode.WrongAuthenticatedUserState;
	        return response;
        }

		var gameUserId = authenticatedUserState.GameUserId;
        var mailId = request.MailId;

        var errorCode = await _mailDataCRUD.DeleteMailAsync(gameUserId, request.MailId);

        _logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, MailId= mailId }, "DeleteMail Success");

		response.Error = errorCode;
        return response;
    }

}