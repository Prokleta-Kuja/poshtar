namespace poshtar.Entities;

public class CalendarEvent
{
    public int CalendarEventId { get; set; }
    public int CalendarId { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime Modified { get; set; }

    public Calendar? Calendar { get; set; }
}