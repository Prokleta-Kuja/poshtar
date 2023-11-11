using System.Diagnostics;

namespace poshtar.Smtp;

[DebuggerDisplay("{User}@{Host}")]
public class EmailAddress
{
    public static readonly EmailAddress Empty = new(string.Empty, string.Empty);

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="user">The user/account name.</param>
    /// <param name="host">The host server.</param>
    public EmailAddress(string user, string host)
    {
        User = user;
        Host = host;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="address">The email address to create the EmailAddress from.</param>
    public EmailAddress(string address)
    {
        address = address.Replace(" ", String.Empty);

        var index = address.IndexOf('@');

        User = address[..index];
        Host = address[(index + 1)..];
    }

    /// <summary>
    /// Gets the user/account name.
    /// </summary>
    public string User { get; }

    /// <summary>
    /// Gets the host server.
    /// </summary>
    public string Host { get; }
    public override string ToString()
    {
        return $"{User}@{Host}";
    }
}