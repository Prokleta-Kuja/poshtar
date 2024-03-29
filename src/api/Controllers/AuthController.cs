using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using poshtar.Entities;
using poshtar.Models;
using poshtar.Services;
using QRCoder;

namespace poshtar.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class AuthController : ControllerBase
{
    const string OTP_CLAIM = "OTP";
    const string GENERIC_ERROR_MESSAGE = "Invalid username, password and/or one time code";
    readonly ILogger<AuthController> _logger;
    readonly AppDbContext _db;
    readonly IDataProtectionProvider _dpProvider;

    public AuthController(ILogger<AuthController> logger, AppDbContext db, IDataProtectionProvider dpProvider)
    {
        _logger = logger;
        _db = db;
        _dpProvider = dpProvider;
    }

    [HttpGet(Name = "Status")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    public IActionResult Status()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var hasOtp = User.HasClaim(c => c.Type == OTP_CLAIM);
            var expires = DateTime.MinValue;
            var expiresStr = User.FindFirst(ClaimTypes.Expiration)?.Value;
            if (!string.IsNullOrWhiteSpace(expiresStr) && long.TryParse(expiresStr, out var expiresVal))
                expires = DateTime.FromBinary(expiresVal);

            return Ok(new AuthStatusModel
            {
                Authenticated = true,
                HasOtp = hasOtp,
                Username = User.Identity!.Name,
                Expires = expires
            });
        }
        else
            return Ok(new AuthStatusModel { Authenticated = false, HasOtp = false });
    }

    [Authorize]
    [HttpGet("totp", Name = "GetTotp")]
    [ProducesResponseType(typeof(TotpVM), StatusCodes.Status200OK)]
    public IActionResult GetTotp()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(User.Identity.Name))
            return BadRequest(new PlainError("Not authenticated or no username"));

        var token = TotpService.CreateAuthToken(nameof(poshtar), User.Identity.Name, nameof(poshtar));
        var chunks = token.Secret.Chunk(4).Select(c => new string(c));
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(token.Uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(5);

        var result = new TotpVM
        {
            ChunkedSecret = string.Join(' ', chunks),
            Qr = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}",
        };

        return Ok(result);
    }

    [Authorize]
    [HttpPost("totp", Name = "SaveTotp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveTotp(TotpCM model)
    {
        var secretArr = model.ChunkedSecret.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var secret = string.Join(string.Empty, secretArr);
        var key = Base32.FromBase32(secret);

        if (model.IsInvalid(key, out var errorModel))
            return BadRequest(errorModel);

        if (User.Identity == null || !User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(User.Identity.Name))
            return BadRequest(new PlainError("Not authenticated or no username"));

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Name == User.Identity.Name.ToLower());
        if (user == null)
            return BadRequest(new PlainError("User not found"));

        var protector = _dpProvider.CreateProtector(nameof(user.OtpKey));
        user.OtpKey = protector.Protect(key);

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch(Name = "AutoLogin")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AutoLoginAsync()
    {
        var hasMasters = await _db.Users.AsNoTracking().AnyAsync(u => u.IsMaster);
        if (hasMasters)
            return BadRequest(new PlainError("There are master users in database, autologin disabled."));

        _logger.LogInformation("No master users in database, autologing in...");
        var expires = DateTime.UtcNow.AddMinutes(10);
        var claims = new List<Claim> {
            new(ClaimTypes.Name, "temporary admin"),
            new(ClaimTypes.Role, "master"),
            new(ClaimTypes.Expiration, expires.ToBinary().ToString()),
            new(OTP_CLAIM,"1"),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { AllowRefresh = false, ExpiresUtc = expires, IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new AuthStatusModel { Authenticated = true, HasOtp = true, Username = "temporary admin", Expires = expires });
    }

    [HttpPost(Name = "Login")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginAsync(LoginModel model)
    {
        model.Username = model.Username.ToLower();
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Name == model.Username && u.IsMaster == true && u.Disabled == null);

        if (user == null || !DovecotHasher.Verify(user.Salt, user.Hash, model.Password))
            return BadRequest(new PlainError(GENERIC_ERROR_MESSAGE));

        var hasOtp = user.OtpKey != null;
        if (hasOtp)
        {
            var protector = _dpProvider.CreateProtector(nameof(user.OtpKey));
            var key = protector.Unprotect(user.OtpKey!);
            if (!model.Totp.HasValue || !TotpService.ValidateCode(key, model.Totp.Value))
                return BadRequest(new PlainError(GENERIC_ERROR_MESSAGE));
        }

        var expires = DateTime.UtcNow.AddHours(1);
        var claims = new List<Claim> {
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Sid, user.UserId.ToString()),
            new(ClaimTypes.Expiration, expires.ToBinary().ToString())
        };
        if (user.IsMaster)
            claims.Add(new Claim(ClaimTypes.Role, C.MASTER_ROLE));
        if (hasOtp)
            claims.Add(new Claim(OTP_CLAIM, "1"));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { AllowRefresh = false, ExpiresUtc = expires, IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new AuthStatusModel { Authenticated = true, HasOtp = hasOtp, Username = user.Name, Expires = expires });
    }

    [HttpDelete(Name = "Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogoutAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}