using System.Globalization;
using System.Net;
using poshtar.Services;

namespace poshtar;

public static class C
{
    public static readonly bool IsDebug;
    public static readonly bool StartApiOnly;
    public const string CRT_FILE = "cert.crt";
    public const string KEY_FILE = "cert.key";
    public static readonly int Uid;
    public static readonly int Gid;
    public static readonly string Hostname;
    public static readonly int MaxMessageSize;
    public static readonly string PostgresConnectionString;
    public static readonly string MysqlConnectionString;
    public static readonly DbContextType DbContextType;
    public static readonly TimeZoneInfo TZ;
    public static readonly CultureInfo Locale;
    static C()
    {
        IsDebug = Environment.GetEnvironmentVariable("DEBUG") == "1";
        StartApiOnly = Environment.GetEnvironmentVariable("START_API_ONLY") == "1";
        Uid = int.TryParse(Environment.GetEnvironmentVariable("UID"), out var uid) ? uid : 1000;
        Gid = int.TryParse(Environment.GetEnvironmentVariable("GID"), out var gid) ? gid : 1000;
        Hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? string.Empty;
        MaxMessageSize = int.TryParse(Environment.GetEnvironmentVariable("MAX_MESSAGE_SIZE_MB"), out var maxMessageSize) ? maxMessageSize * 1024 * 1024 : 0;
        PostgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES") ?? string.Empty;
        MysqlConnectionString = Environment.GetEnvironmentVariable("MYSQL") ?? string.Empty;
        DbContextType = !string.IsNullOrWhiteSpace(PostgresConnectionString) ? DbContextType.PostgreSQL :
            !string.IsNullOrWhiteSpace(MysqlConnectionString) ? DbContextType.MySQL :
            DbContextType.SQLite;

        try
        {
            TZ = TimeZoneInfo.FindSystemTimeZoneById(Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb");
        }
        catch (Exception)
        {
            TZ = TimeZoneInfo.Local;
        }

        try
        {
            Locale = CultureInfo.GetCultureInfo(Environment.GetEnvironmentVariable("LOCALE") ?? "en-UK");
        }
        catch (Exception)
        {
            Locale = CultureInfo.InvariantCulture;
        }

    }
    public static class Dovecot
    {
        public static readonly string MasterUser = "master-poshtar";
        public static readonly string MasterPassword = SecretGenerator.Password(24);
    }
    public static class CacheKeys
    {
        public static string FailedAuthCountFor(IPEndPoint ip) => $"IpFailedAuthCount-{ip.Address}";
    }
    public static class Paths
    {
        static string Root => IsDebug ? Path.Join(Environment.CurrentDirectory, "/data") : "/data";
        public static readonly string CertData = $"{Root}/certs";
        public static string CertDataFor(string file) => Path.Combine(CertData, file);
        public static readonly string ConfigData = $"{Root}/config";
        public static string ConfigDataFor(string file) => Path.Combine(ConfigData, file);
        public static readonly string QueueData = $"{Root}/queue";
        public static string QueueDataFor(string file) => Path.Combine(QueueData, file);
        public static readonly string MailData = $"{Root}/mail";
        public static readonly string CertCrt = CertDataFor(CRT_FILE);
        public static readonly string CertKey = CertDataFor(KEY_FILE);
        public static readonly string Sqlite = ConfigDataFor("app.db");
        public static readonly string Hangfire = QueueDataFor("queue.db");
        public static readonly string AppDbConnectionString = $"Data Source={Sqlite}";
        public static readonly string HangfireConnectionString = $"Data Source={Hangfire}";
    }
}

public enum DbContextType
{
    SQLite,
    PostgreSQL,
    MySQL,
}