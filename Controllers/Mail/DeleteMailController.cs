using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;

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
        var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new DeleteMailResponse();

        var ownerId = userAuthAndState.GameUserId;

        var errorCode = await _mailDataCRUD.DeleteMailAsync(ownerId, request.MailId);

        response.Error = errorCode;
        return response;

    }

}