using System.Text;
using MySqlConnector;
using Npgsql;

namespace poshtar.Services;

public static class PostfixConfiguration
{
    public static readonly string PostfixRoot = C.Paths.ConfigDataFor("postfix");
    public static string MainPath => Path.Combine(PostfixRoot, "main.cf");
    public static string MasterPath => Path.Combine(PostfixRoot, "master.cf");
    public static string AddressesPath => Path.Combine(PostfixRoot, "addresses.cf");
    public static string DomainsPath => Path.Combine(PostfixRoot, "domains.cf");
    public static string LoginsPath => Path.Combine(PostfixRoot, "logins.cf");
    public static string PasswordsPath => Path.Combine(PostfixRoot, "passwords.cf");
    public static string RelaysPath => Path.Combine(PostfixRoot, "relays.cf");
    static void GenerateMain(string dbProvider)
    {
        var main = @$"maillog_file = /var/log/postfix.log
compatibility_level = 3.6
append_dot_mydomain = no
readme_directory = no
smtpd_banner = $myhostname ESMTP $mail_name (Debian/GNU)
biff = no
myhostname = {C.Hostname}
mydestination = $myhostname, localhost
alias_maps =
mynetworks = 127.0.0.0/8 [::ffff:127.0.0.0]/104 [::1]/128
mailbox_size_limit = 0
inet_interfaces = all
inet_protocols = all

relayhost =

# Security - disable when debugging?
unknown_local_recipient_reject_code = 550
show_user_unknown_table_name = no
disable_vrfy_command = yes
smtpd_tls_protocols = !SSLv2 !SSLv3
smtpd_tls_mandatory_protocols = !SSLv2 !SSLv3
smtp_tls_protocols = !SSLv2 !SSLv3
smtp_tls_mandatory_protocols = !SSLv2 !SSLv3
lmtp_tls_protocols = !SSLv2 !SSLv3
lmtp_tls_mandatory_protocols = !SSLv2 !SSLv3
tls_random_source = dev:/dev/urandom
smtpd_reject_unlisted_recipient = yes
smtpd_reject_unlisted_sender = yes

# Incoming
smtpd_tls_cert_file={C.Paths.CertCrt}
smtpd_tls_key_file={C.Paths.CertKey}
smtpd_tls_security_level=may
smtpd_recipient_restrictions = permit_sasl_authenticated, reject_unauth_destination
smtpd_relay_restrictions = permit_sasl_authenticated, reject_unauth_destination
smtpd_sasl_security_options = noanonymous

virtual_mailbox_domains = {dbProvider}:{DomainsPath}
virtual_alias_maps = {dbProvider}:{AddressesPath}
virtual_transport = lmtp:unix:private/dovecot-lmtp
mailbox_transport = lmtp:unix:private/dovecot-lmtp

# Outgoing
smtp_use_tls = yes
smtp_tls_security_level = encrypt
smtp_sasl_auth_enable = yes
smtp_sasl_security_options = noanonymous
smtp_sasl_mechanism_filter = plain
smtp_always_send_ehlo = yes
smtp_sender_dependent_authentication = yes
sender_dependent_relayhost_maps = {dbProvider}:{RelaysPath}
smtp_sasl_password_maps = {dbProvider}:{PasswordsPath}";

        File.WriteAllText(MainPath, main);
    }
    static void GenerateMaster(string dbProvider)
    {
        var master = $@"#
smtp      inet  n       -       y       -       -       smtpd
submission inet n       -       y       -       -       smtpd
  -o syslog_name=postfix/submission
  -o smtpd_tls_security_level=encrypt
  -o smtpd_sasl_auth_enable=yes
  -o smtpd_tls_auth_only=yes
  -o smtpd_sasl_type=dovecot
  -o smtpd_sasl_path=private/auth
  -o smtpd_client_restrictions=permit_sasl_authenticated,reject
  -o smtpd_sasl_security_options=noanonymous
  -o smtpd_reject_unlisted_recipient=no
  -o smtpd_sender_restrictions=reject_sender_login_mismatch
  -o smtpd_recipient_restrictions=reject_non_fqdn_recipient,permit_sasl_authenticated,reject
  -o smtpd_relay_restrictions=permit_sasl_authenticated,reject
  -o milter_macro_daemon_name=ORIGINATING
  -o smtpd_sender_login_maps={dbProvider}:{LoginsPath}
  -o smtpd_tls_cert_file={C.Paths.CertCrt}
  -o smtpd_tls_key_file={C.Paths.CertKey}
pickup    unix  n       -       y       60      1       pickup
cleanup   unix  n       -       y       -       0       cleanup
qmgr      unix  n       -       n       300     1       qmgr
tlsmgr    unix  -       -       y       1000?   1       tlsmgr
rewrite   unix  -       -       y       -       -       trivial-rewrite
bounce    unix  -       -       y       -       0       bounce
defer     unix  -       -       y       -       0       bounce
trace     unix  -       -       y       -       0       bounce
verify    unix  -       -       y       -       1       verify
flush     unix  n       -       y       1000?   0       flush
proxymap  unix  -       -       n       -       -       proxymap
proxywrite unix -       -       n       -       1       proxymap
smtp      unix  -       -       y       -       -       smtp
relay     unix  -       -       y       -       -       smtp
        -o syslog_name=postfix/$service_name
showq     unix  n       -       y       -       -       showq
error     unix  -       -       y       -       -       error
retry     unix  -       -       y       -       -       error
discard   unix  -       -       y       -       -       discard
local     unix  -       n       n       -       -       local
virtual   unix  -       n       n       -       -       virtual
lmtp      unix  -       -       y       -       -       lmtp
anvil     unix  -       -       y       -       1       anvil
scache    unix  -       -       y       -       1       scache
postlog   unix-dgram n  -       n       -       1       postlogd
maildrop  unix  -       n       n       -       -       pipe
  flags=DRXhu user=vmail argv=/usr/bin/maildrop -d ${{recipient}}
ifmail    unix  -       n       n       -       -       pipe
  flags=F user=ftn argv=/usr/lib/ifmail/ifmail -r $nexthop ($recipient)
bsmtp     unix  -       n       n       -       -       pipe
  flags=Fq. user=bsmtp argv=/usr/lib/bsmtp/bsmtp -t$nexthop -f$sender $recipient
scalemail-backend unix -       n       n       -       2       pipe
  flags=R user=scalemail argv=/usr/lib/scalemail/bin/scalemail-store ${{nexthop}} ${{user}} ${{extension}}
mailman   unix  -       n       n       -       -       pipe
  flags=FRX user=list argv=/usr/lib/mailman/bin/postfix-to-mailman.py ${{nexthop}} ${{user}}
";

        File.WriteAllText(MasterPath, master);
    }
    static void GenerateAddresses(string prefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => "SELECT u.name || '@poshtar.local' FROM addresses a JOIN domains d USING(domain_id) JOIN address_user au ON au.addresses_address_id = a.address_id JOIN users u ON u.user_id = au.users_user_id WHERE d.disabled IS NULL AND a.disabled IS NULL AND u.disabled IS NULL AND d.name = '%d' AND ('%u' LIKE a.expression OR a.expression IS NULL)",
            _ => "Not verified",
        };

        File.WriteAllText(AddressesPath, string.Concat(prefix, query));
    }
    static void GenerateDomains(string prefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => "SELECT name FROM (SELECT name FROM domains WHERE disabled IS NULL UNION SELECT 'poshtar.local') v WHERE v.name = '%s'",
            _ => "Not verified",
        };

        File.WriteAllText(DomainsPath, string.Concat(prefix, query));
    }
    static void GenerateLogins(string prefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => "SELECT u.name FROM addresses a JOIN domains d USING(domain_id) JOIN address_user au ON au.addresses_address_id = a.address_id JOIN users u ON u.user_id = au.users_user_id WHERE d.disabled IS NULL AND a.disabled IS NULL AND u.disabled IS NULL AND d.name = '%d' AND ('%u' LIKE a.expression OR a.expression IS NULL)",
            _ => "Not verified",
        };

        File.WriteAllText(LoginsPath, string.Concat(prefix, query));
    }
    static void GeneratePasswords(string prefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => "SELECT username || ':' || password FROM domains WHERE name = '%d' AND disabled IS NULL",
            _ => "Not verified",
        };

        File.WriteAllText(PasswordsPath, string.Concat(prefix, query));
    }
    static void GenerateRelays(string prefix)
    {
        string query = C.DbContextType switch
        {
            DbContextType.PostgreSQL => "SELECT '[' || host || ']:' || port FROM domains WHERE name = '%d' AND disabled IS NULL",
            _ => "Not verified",
        };

        File.WriteAllText(RelaysPath, string.Concat(prefix, query));
    }
    public static void Generate()
    {
        string? dbProvider;
        string? sqlFilePrefix;
        switch (C.DbContextType)
        {
            case DbContextType.PostgreSQL:
                var postgres = new NpgsqlConnectionStringBuilder(C.PostgresConnectionString);
                dbProvider = "pgsql";
                sqlFilePrefix = $@"user = {postgres.Username}
password = {postgres.Password}
hosts = {postgres.Host}
dbname = {postgres.Database}
query = ";
                break;
            case DbContextType.MySQL:
                throw new NotImplementedException();
                var mysql = new MySqlConnectionStringBuilder(C.MysqlConnectionString);
                dbProvider = "mysql";
                sqlFilePrefix = $@"user = {mysql.UserID}
password = {mysql.Password}
hosts = {mysql.Server}
dbname = {mysql.Database}
query = ";
                break;
            default:
                throw new NotImplementedException();
                dbProvider = "sqlite";
                sqlFilePrefix = $@"dbpath = {C.Paths.Sqlite}
query = ";
                break;
        }

        Directory.CreateDirectory(PostfixRoot);
        GenerateMain(dbProvider);
        GenerateMaster(dbProvider);
        GenerateAddresses(sqlFilePrefix);
        GenerateDomains(sqlFilePrefix);
        GenerateLogins(sqlFilePrefix);
        GeneratePasswords(sqlFilePrefix);
        GenerateRelays(sqlFilePrefix);
    }
}