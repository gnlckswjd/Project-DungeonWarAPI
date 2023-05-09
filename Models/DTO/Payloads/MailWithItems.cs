using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO.Payloads;

public class MailWithItems
{
	

	public MailWithItems( Mail mail , List<MailItem> items)
	{
		MailId = mail.MailId;
		Title = mail.Title;
		IsRead = mail.IsRead;
		IsReceived = mail.IsReceived;
		ExpirationDate=mail.ExpirationDate.Date;
		Items = items;
	}

    public long MailId { get; set; }
    public string Title { get; set; }
	public bool IsRead { get; set; }
    public bool IsReceived { get; set; }
    public DateTime ExpirationDate { get; set; }
	public List<MailItem> Items { get; set; }

}