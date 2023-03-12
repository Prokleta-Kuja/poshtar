# TODO

A recurring job to check if there are maildirs for deleted users and/or delete it when deleting users
Reload certs https://doc.dovecot.org/admin_manual/doveadm_http_api/#doveadm-reload
API auth
MySql & Sqlite backend
Showing 1 to 0 of 0 entries.

# Important dovecot conf

- https://doc.dovecot.org/configuration_manual/lastlogin_plugin/

- https://doc.dovecot.org/settings/core/#core_setting-auth_master_user_separator
- https://doc.dovecot.org/settings/core/#core_setting-auth_username_chars
- https://doc.dovecot.org/settings/core/#core_setting-auth_username_format
- https://doc.dovecot.org/settings/core/#core_setting-auth_verbose
- https://doc.dovecot.org/settings/core/#core_setting-base_dir
- https://doc.dovecot.org/settings/core/#core_setting-imap_idle_notify_interval
- https://doc.dovecot.org/settings/core/#core_setting-listen
- https://doc.dovecot.org/settings/core/#core_setting-mail_location
- https://doc.dovecot.org/settings/core/#core_setting-mailbox_idle_check_interval
- https://doc.dovecot.org/settings/core/#core_setting-protocols
- https://doc.dovecot.org/settings/core/#core_setting-recipient_delimiter
- https://doc.dovecot.org/settings/core/#core_setting-ssl + odmah ispod za uri do certa

# Development

On first run a data folder is going to be created and it going to fail because you need to put cert.crt & cert.key in data/certs folder.

## Migrations

```
dotnet build
dotnet ef migrations add --no-build -c SqliteDbContext -o ./Entities/Migrations/Sqlite Initial
dotnet ef migrations add --no-build -c PostgresDbContext -o ./Entities/Migrations/Postgres Initial
dotnet ef migrations add --no-build -c MysqlDbContext -o ./Entities/Migrations/Mysql Initial
```

## postgres

```
SELECT *, pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE pid <> pg_backend_pid()
AND datname = 'dev_poshtar';

DROP DATABASE dev_poshtar;

CREATE DATABASE dev_poshtar
    WITH
    OWNER = dev
    ENCODING = 'UTF8'
    LC_COLLATE = 'hr_HR.utf8'
    LC_CTYPE = 'hr_HR.utf8'
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
```

## OpenAPI

```
cd src/client-app
npx openapi-typescript-codegen --useOptions --input http://localhost:5000/swagger/v1/swagger.json --output ./src/api
```
