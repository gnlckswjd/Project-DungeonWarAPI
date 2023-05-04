using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO;

public class MailWithItems
{
	public Mail Mail { get; set; }
	public List<MailItem> Items { get; set; }

}