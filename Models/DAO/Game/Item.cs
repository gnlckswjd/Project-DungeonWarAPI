namespace DungeonWarAPI.Models.Database.Game;

public class Item
{
    public int ItemCode { get; set; }
    public string ItemName { get; set; }
    public int AttributeCode { get; set; }
    public int Sell { get; set; }
    public int Buy { get; set; }
    public short UseLevel { get; set; }
    public short Attack { get; set; }
    public short Defence { get; set; }
    public short Magic { get; set; }
    public short EnhanceMaxCount { get; set; }
    public bool Stackability { get; set; }
}