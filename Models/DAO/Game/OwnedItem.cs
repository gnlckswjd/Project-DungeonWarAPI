﻿namespace DungeonWarAPI.Models.Database.Game;

public class OwnedItem
{
    public Int32 ItemId { get; set; }
    //public Int32 GameUserId { get; set; }
    public Int32 ItemCode { get; set; }
    public Int32 EnhancementCount { get; set; }
    public Int32 ItemCount { get; set; }

    public Boolean IsDestroyed { get; set; }
}