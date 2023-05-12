using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class CreateAccountRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreateAccountResponse
{
    public ErrorCode Error { get; set; }
}