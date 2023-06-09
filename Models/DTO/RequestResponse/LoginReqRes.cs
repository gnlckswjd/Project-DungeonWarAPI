﻿using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class LoginRequest
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public class LoginResponse
{
    public ErrorCode Error { get; set; }

    public int UserLevel { get; set; }

    public List<OwnedItem> Items { get; set; }

    public string AuthToken { get; set; }

    public List<string> Notifications { get; set; }
}

