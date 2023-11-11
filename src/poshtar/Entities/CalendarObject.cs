namespace poshtar.Entities;

public class CalendarObject
{
    public int CalendarObjectId { get; set; }
    public int CalendarId { get; set; }
    public CalendarObjectType Type { get; set; }
    public DateTime? FirstOccurence { get; set; }
    public DateTime? LastOccurence { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime Modified { get; set; }

    public Calendar? Calendar { get; set; }
}

public enum CalendarObjectType
{
    Unknown = 0,
    Event = 1,
    Todo = 2,
    Journal = 3,
    FreeBusy = 4,
}