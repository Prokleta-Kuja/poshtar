using System.ComponentModel.DataAnnotations;
using poshtar.Entities;

namespace poshtar.Models;

public class CalendarVM
{
    public CalendarVM(Calendar c)
    {
        Id = c.CalendarId;
        DisplayName = c.DisplayName;
        Users = c.CalendarUsers.Select(cu => new CalendarUserVM
        {
            UserId = cu.UserId,
            UserName = cu.User?.Name ?? string.Empty,
            IsMaster = cu.User?.IsMaster ?? false,
            CanWrite = cu.CanWrite,
        }).ToArray();
    }
    [Required] public int Id { get; set; }
    [Required] public string DisplayName { get; set; }
    [Required] public CalendarUserVM[] Users { get; set; }
}
public class CalendarUserVM
{
    [Required] public int UserId { get; set; }
    [Required] public required string UserName { get; set; }
    [Required] public bool IsMaster { get; set; }
    [Required] public bool CanWrite { get; set; }
}
public class CalendarLM
{
    [Required] public int Id { get; set; }
    [Required] public required string DisplayName { get; set; }
    [Required] public int UserCount { get; set; }
    [Required] public int WriteUserCount { get; set; }
}
public class CalendarCM
{
    [Required] public required string DisplayName { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(DisplayName))
            errorModel.Errors.Add(nameof(DisplayName), "Required");

        return errorModel.Errors.Count > 0;
    }
}
public class CalendarUM
{
    [Required] public required string DisplayName { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(DisplayName))
            errorModel.Errors.Add(nameof(DisplayName), "Required");

        return errorModel.Errors.Count > 0;
    }
}
public class CalendarUserSM
{
    [Required] public int UserId { get; set; }
    [Required] public required string UserName { get; set; }
    [Required] public bool IsMaster { get; set; }
}