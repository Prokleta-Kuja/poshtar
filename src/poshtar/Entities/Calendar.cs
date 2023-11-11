namespace poshtar.Entities;

public class Calendar
{
    public int CalendarId { get; set; }
    public required string DisplayName { get; set; }

    public virtual List<CalendarObject> CalendarObjects { get; set; } = new();
    public virtual List<CalendarUser> CalendarUsers { get; set; } = new();
}