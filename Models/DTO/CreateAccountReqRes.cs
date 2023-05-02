namespace DungeonWarAPI.Models.DTO;

public class CreateAccountRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreateAccountResponse
{
    public ErrorCode Result { get; set; }
}