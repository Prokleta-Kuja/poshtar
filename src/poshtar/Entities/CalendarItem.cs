namespace poshtar.Entities;

public class CalendarItem
{
    public int CalendarItemId { get; set; }
    public int CalendarId { get; set; }
    public CalendarItemType Type { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime Modified { get; set; }

    public Calendar? Calendar { get; set; }
}

public enum CalendarItemType
{
    Unknown = 0,
    Event = 1,
    Todo = 2,
    Journal = 3,
}