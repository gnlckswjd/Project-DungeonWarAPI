namespace DungeonWarAPI.Models.Database.Game;

public class Item
{
    public Int32 ItemCode { get; set; }
    public String ItemName { get; set; }
    public Int32 AttributeCode { get; set; }
    public Int32 Sell { get; set; }
    public Int32 Buy { get; set; }
    public Int16 UseLevel { get; set; }
    public Int16 Attack { get; set; }
    public Int16 Defence { get; set; }
    public Int16 Magic { get; set; }
    public Int16 EnhanceMaxCount { get; set; }
    public Boolean Stackability { get; set; }
}