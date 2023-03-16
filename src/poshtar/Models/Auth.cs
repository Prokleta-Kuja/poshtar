using System.ComponentModel.DataAnnotations;

namespace poshtar.Models;

public class LoginModel
{
    [Required] public required string Username { get; set; }
    [Required] public required string Password { get; set; }
}

public class AuthStatusModel
{
    [Required] public required bool Authenticated { get; set; }
    [Required] public string? Username { get; set; }
}