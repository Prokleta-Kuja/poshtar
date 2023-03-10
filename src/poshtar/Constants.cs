using System.Globalization;

namespace poshtar;

public static class C
{
    public static readonly bool IsDebug;
    public const string CRT_FILE = "cert.crt";
    public const string KEY_FILE = "cert.key";
    public static readonly string Hostname;
    public static readonly string PostgresConnectionString;
    public static readonly string MysqlConnectionString;
    public static readonly DbContextType DbContextType;
    public static readonly TimeZoneInfo TZ;
    public static readonly CultureInfo Locale;
    static C()
    {
        IsDebug = Environment.GetEnvironmentVariable("DEBUG") == "1";
        Hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? string.Empty;
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
    public static class Paths
    {
        static string Root => IsDebug ? Path.Join(Environment.CurrentDirectory, "/data") : string.Empty;
        public static readonly string CertData = $"{Root}/certs";
        public static string CertDataFor(string file) => Path.Combine(CertData, file);
        public static readonly string ConfigData = $"{Root}/config";
        public static string ConfigDataFor(string file) => Path.Combine(ConfigData, file);
        public static readonly string LogData = $"{Root}/logs";
        public static string LogDataFor(string file) => Path.Combine(LogData, file);
        public static readonly string MailData = $"{Root}/mail";
        public static readonly string CertCrt = CertDataFor(CRT_FILE);
        public static readonly string CertKey = CertDataFor(KEY_FILE);
        public static readonly string Sqlite = ConfigDataFor("app.db");
        public static readonly string AppDbConnectionString = $"Data Source={Sqlite}";
    }
}

public enum DbContextType
{
    SQLite,
    PostgreSQL,
    MySQL,
}