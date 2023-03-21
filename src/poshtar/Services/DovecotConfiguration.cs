using System.Text;
using MySqlConnector;
using Npgsql;

namespace poshtar.Services;

public static class DovecotConfiguration
{
    public static readonly string LogPath = C.Paths.LogDataFor("dovecot.log");
    public static readonly string DovecotRoot = C.Paths.ConfigDataFor("dovecot");
    public static readonly string MainPath = Path.Join(DovecotRoot, "dovecot.conf");
    public static readonly string UsersPath = Path.Join(DovecotRoot, "users.conf");
    public static readonly string PasswordsPath = Path.Join(DovecotRoot, "passes.conf");
    public static readonly string MastersPath = Path.Join(DovecotRoot, "masters.conf");
    static void GenerateMain()
    {
        var main = new StringBuilder();
        if (C.IsDebug)
            main.AppendLine("auth_debug = yes");

        main.AppendLine(@$"
ssl=required
ssl_cert = <{C.Paths.CertCrt}
ssl_key = <{C.Paths.CertKey}
protocols = imap lmtp
mail_location = maildir:{C.Paths.MailData}/%Ln
auth_master_user_separator=*
mail_plugins = $mail_plugins quota
mailbox_list_index = yes
mail_always_cache_fields = date.save
mailbox_list_index = yes
protocol imap {{
  mail_plugins = $mail_plugins imap_quota
}}
protocol !indexer-worker {{
  mail_vsize_bg_after_count = 100
}}
plugin {{
  quota_exceeded_message = Quota exceeded, please go to delete some messages first.
  quota = count:User quota
  quota_vsizes = yes
  #quota_rule = *:storage=1G # This will be the default overrriden by SQL.
}}

protocol lmtp {{
  #postmaster_address = postmaster@ica.hr   # required
  #mail_plugins = quota sieve
}}
service lmtp {{
  unix_listener /var/spool/postfix/private/dovecot-lmtp {{
    mode = 0600
    user = postfix
    group = postfix
  }}
}}
auth_mechanisms = plain login
service auth {{
  unix_listener /var/spool/postfix/private/auth {{
    mode = 0660
    user = postfix
    group = postfix
  }}
}}

namespace {{
  inbox = yes
  separator = /

  mailbox Drafts {{
    special_use = \Drafts
    auto = subscribe
  }}

  mailbox Junk {{
    special_use = \Junk
    auto = create
    autoexpunge = 14d
  }}

  mailbox Spam {{
    special_use = \Junk
    auto = no
  }}

  mailbox Trash {{
    special_use = \Trash
    auto = subscribe
    autoexpunge = 14d
  }}

  mailbox Sent {{
    special_use = \Sent
    auto = subscribe
  }}

  mailbox ""Sent Mail"" {{
    special_use = \Sent
    auto = no
  }}

  mailbox Archive {{
        special_use = \Archive
        auto = create
  }}
}}

log_path={LogPath}
info_log_path={LogPath}
debug_log_path={LogPath}");

        main.AppendLine($@"
mail_uid = {C.Uid}
mail_gid = {C.Gid}");

        // Permissions
        main.AppendLine($@"
passdb {{
  driver = sql
  args = {MastersPath}
  master = yes
  result_success = continue-ok
}}
passdb {{
  driver = sql
  args = {PasswordsPath}
}}
userdb {{
  driver = sql
  args = {UsersPath}
}}");

        File.WriteAllText(MainPath, main.ToString());
    }
    static void GenerateUsers(string sqlFilePrefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => @"iterate_query = SELECT name AS username FROM users WHERE disabled IS NULL
user_query = SELECT name AS user, '*:bytes=' || quota AS quota_rule FROM users u WHERE disabled IS NULL AND u.name = '%Lu'",
            DbContextType.SQLite => @"iterate_query = SELECT Name AS username FROM Users WHERE Disabled IS NULL
user_query = SELECT '*:bytes=' || Quota AS quota_rule FROM users WHERE Name = '%Lu' AND Disabled IS NULL",
            _ => "Not verified",
        };

        File.WriteAllText(UsersPath, string.Concat(sqlFilePrefix, query));
    }
    static void GeneratePasswords(string sqlFilePrefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => @"password_query = SELECT Name AS username, password, '*:bytes=' || quota AS quota_rule \
FROM users WHERE name = '%Lu' AND disabled IS NULL --AND is_master = false",
            DbContextType.SQLite => @"password_query = SELECT Name AS username, Password AS password, '*:bytes=' || Quota AS quota_rule \
FROM Users WHERE Name = '%Lu' AND Disabled IS NULL --AND IsMaster = 0",
            _ => "Not verified",
        };

        File.WriteAllText(PasswordsPath, string.Concat(sqlFilePrefix, query));
    }
    static void GenerateMasters(string sqlFilePrefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => @"password_query = SELECT Name AS username, password, '*:bytes=' || quota AS quota_rule \
FROM users WHERE name = '%Lu' AND disabled IS NULL AND is_master = true",
            DbContextType.SQLite => @"password_query = SELECT Name AS username, Password AS password, '*:bytes=' || Quota AS quota_rule \
FROM Users WHERE Name = '%Lu' AND Disabled IS NULL AND IsMaster = 1",
            _ => "Not verified",
        };

        File.WriteAllText(MastersPath, string.Concat(sqlFilePrefix, query));
    }
    public static void Generate()
    {
        string? sqlFilePrefix;
        switch (C.DbContextType)
        {
            case DbContextType.PostgreSQL:
                var postgres = new NpgsqlConnectionStringBuilder(C.PostgresConnectionString);
                sqlFilePrefix = $@"driver = pgsql
connect = host={postgres.Host} dbname={postgres.Database} user={postgres.Username} password={postgres.Password}
";
                break;
            case DbContextType.MySQL:
                throw new NotImplementedException();
                var mysql = new MySqlConnectionStringBuilder(C.MysqlConnectionString);
                sqlFilePrefix = $@"driver = mysql
connect = host={mysql.Server} dbname={mysql.Database} user={mysql.UserID} password={mysql.Password}
";
                break;
            default:
                sqlFilePrefix = $@"driver = sqlite
dbpath = {C.Paths.Sqlite}
";
                break;
        }

        Directory.CreateDirectory(DovecotRoot);
        File.Create(LogPath).Dispose();
        GenerateMain();
        GenerateUsers(sqlFilePrefix);
        GeneratePasswords(sqlFilePrefix);
        GenerateMasters(sqlFilePrefix);
    }
}