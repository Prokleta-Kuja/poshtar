namespace poshtar.Entities;

public class CalendarUser
{
    public int CalendarId { get; set; }
    public int UserId { get; set; }
    public bool CanWrite { get; set; }

    public Calendar? Calendar { get; set; }
    public User? User { get; set; }
}