namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class CreateAccountRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreateAccountResponse
{
    public ErrorCode Error { get; set; }
}