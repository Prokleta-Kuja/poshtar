using System.ComponentModel.DataAnnotations;
using poshtar.Services;

namespace poshtar.Models;

public class LoginModel
{
    [Required] public required string Username { get; set; }
    [Required] public required string Password { get; set; }
    public int? Totp { get; set; }
}

public class AuthStatusModel
{
    [Required] public required bool Authenticated { get; set; }
    [Required] public required bool HasOtp { get; set; }
    public string? Username { get; set; }
    public DateTime? Expires { get; set; }
}

public class TotpVM
{
    [Required] public required string Qr { get; set; }
    [Required] public required string ChunkedSecret { get; set; }
}

public class TotpCM
{
    [Required] public required string ChunkedSecret { get; set; }
    public int? Code { get; set; }
    public bool IsInvalid(byte[] securityToken, out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(ChunkedSecret))
            errorModel.Errors.Add(nameof(ChunkedSecret), "Required");

        if (!Code.HasValue)
            errorModel.Errors.Add(nameof(Code), "Required");
        else if (Code.Value.ToString().Length != TotpService.DIGITS)
            errorModel.Errors.Add(nameof(Code), $"Must be {TotpService.DIGITS} digits long");
        else if (!TotpService.ValidateCode(securityToken, Code.Value))
            errorModel.Errors.Add(nameof(Code), "Invalid code");

        return errorModel.Errors.Count > 0;
    }
}