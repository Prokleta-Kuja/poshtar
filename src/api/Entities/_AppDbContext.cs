using System.Diagnostics;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using poshtar.Services;

namespace poshtar.Entities;

public class SqliteDbContext : AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => AdditionalConfiguration(options.UseSqlite(C.Paths.AppDbConnectionString));
}
public class PostgresDbContext : AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
         => AdditionalConfiguration(options.UseNpgsql(C.PostgresConnectionString));
}
public class MysqlDbContext : AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (string.IsNullOrWhiteSpace(C.MysqlConnectionString))
            AdditionalConfiguration(options.UseMySql(MariaDbServerVersion.LatestSupportedServerVersion));
        else
            AdditionalConfiguration(options.UseMySql(C.MysqlConnectionString, ServerVersion.AutoDetect(C.MysqlConnectionString)));
    }
}
public partial class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<BlockedIp> BlockedIps => Set<BlockedIp>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<Relay> Relays => Set<Relay>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LogEntry> Logs => Set<LogEntry>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<CalendarObject> CalendarObjects => Set<CalendarObject>();
    public DbSet<CalendarUser> CalendarUsers => Set<CalendarUser>();

    protected void AdditionalConfiguration(DbContextOptionsBuilder options)
    {
        options.UseSnakeCaseNamingConvention();
        if (C.IsDebug)
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
        }
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BlockedIp>(e =>
        {
            e.HasKey(e => e.Address);
        });

        builder.Entity<Address>(e =>
        {
            e.HasKey(e => e.AddressId);
            e.HasOne(e => e.Domain).WithMany(e => e.Addresses).HasForeignKey(e => e.DomainId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            e.Property(e => e.Expression).HasComputedColumnSql("CASE type WHEN 0 THEN pattern WHEN 1 THEN pattern || '%' WHEN 2 THEN '%' || pattern ELSE NULL END", true);
        });

        builder.Entity<Domain>(e =>
        {
            e.HasKey(e => e.DomainId);
            e.HasIndex(e => e.Name);
        });

        builder.Entity<Relay>(e =>
        {
            e.HasKey(e => e.RelayId);
            e.HasIndex(e => e.Name);
            e.HasMany(e => e.Domains).WithOne(e => e.Relay).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<User>(e =>
        {
            e.HasKey(e => e.UserId);
            e.HasMany(e => e.Addresses).WithMany(e => e.Users);
            e.HasMany(e => e.Transactions).WithOne(e => e.FromUser);
        });

        builder.Entity<Transaction>(e =>
        {
            e.HasKey(e => e.TransactionId);
            e.HasMany(e => e.Logs).WithOne(e => e.Transaction).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(e => e.Recipients).WithOne(e => e.Transaction).OnDelete(DeleteBehavior.Cascade);
            e.Ignore(e => e.InternalUsers);
            e.Ignore(e => e.ExternalAddresses);
        });

        builder.Entity<LogEntry>(e =>
        {
            e.HasKey(e => e.LogEntryId);
        });

        builder.Entity<Recipient>(e =>
        {
            e.HasKey(e => e.RecipientId);
            e.HasOne(e => e.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Calendar>(e =>
        {
            e.HasKey(e => e.CalendarId);
        });

        builder.Entity<CalendarObject>(e =>
        {
            e.HasKey(e => e.CalendarObjectId);
            e.HasOne(e => e.Calendar).WithMany(e => e.CalendarObjects).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CalendarUser>(e =>
        {
            e.HasKey(e => new { e.CalendarId, e.UserId });
            e.HasOne(e => e.Calendar).WithMany(e => e.CalendarUsers).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.User).WithMany(e => e.CalendarUsers).OnDelete(DeleteBehavior.Cascade);
        });

        // SQLite conversions
        if (Database.IsSqlite())
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var dtProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));
                foreach (var property in dtProperties)
                    builder.Entity(entityType.Name).Property(property.Name).HasConversion(new DateTimeToBinaryConverter());

                var decProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));
                foreach (var property in decProperties)
                    builder.Entity(entityType.Name).Property(property.Name).HasConversion<double>();

                var spanProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(TimeSpan) || p.PropertyType == typeof(TimeSpan?));
                foreach (var property in spanProperties)
                    builder.Entity(entityType.Name).Property(property.Name).HasConversion<double>();
            }
    }
    public async ValueTask InitializeDefaults(IDataProtectionProvider dpProvider)
    {
        if (!Debugger.IsAttached)
            return;

        // var serverProtector = dpProvider.CreateProtector(nameof(Domain));
        var fakeRelay = new Relay
        {
            Name = "Fake Relay",
            Host = "relay.ica.hr",
            Port = 587,
            Username = "fake",
            Password = "P@ssw0rd",
        };
        var ica = new Domain
        {
            Name = "ica.hr",
            Relay = fakeRelay,
        };
        var nan = new Domain
        {
            Name = "nan.hr",
        };
        Domains.AddRange(ica, nan);

        var admin = DovecotHasher.Hash("admin");
        var user = DovecotHasher.Hash("user");

        var icaAdmin = new User { Name = "icaadmin", IsMaster = true, Description = "ica _admin_ user", Salt = admin.Salt, Hash = admin.Hash, Password = DovecotHasher.Password(admin.Salt, admin.Hash), };
        var nanAdmin = new User { Name = "nanadmin", IsMaster = true, Description = "nan _admin_ user", Salt = admin.Salt, Hash = admin.Hash, Password = DovecotHasher.Password(admin.Salt, admin.Hash), };
        var icaUser = new User { Name = "icauser", Quota = 1024 * 1024, Description = "ica regular _user_", Salt = user.Salt, Hash = user.Hash, Password = DovecotHasher.Password(user.Salt, user.Hash), };
        var nanUser = new User { Name = "nanuser", Quota = 1024 * 1024, Description = "nan regular _user_", Salt = user.Salt, Hash = user.Hash, Password = DovecotHasher.Password(user.Salt, user.Hash), };
        Users.AddRange(icaAdmin, nanAdmin, icaUser, nanUser);

        var icaMailAdmin = new Address { Pattern = "admin", Description = "admin@ica.hr", Domain = ica, Type = AddressType.Exact };
        var nanMailAdmin = new Address { Pattern = "admin", Description = "admin@nan.hr", Domain = nan, Type = AddressType.Exact };
        var icaMailUser = new Address { Pattern = "user", Description = "user@ica.hr", Domain = ica, Type = AddressType.Exact };
        var nanMailUser = new Address { Pattern = "user", Description = "user@nan.hr", Domain = nan, Type = AddressType.Exact };
        var icaMailPrefix = new Address { Pattern = "prefix", Description = "prefix@ica.hr", Domain = ica, Type = AddressType.Prefix };
        var nanMailPrefix = new Address { Pattern = "prefix", Description = "prefix@nan.hr", Domain = nan, Type = AddressType.Prefix };
        var icaMailSuffix = new Address { Pattern = "suffix", Description = "suffix@ica.hr", Domain = ica, Type = AddressType.Suffix };
        var nanMailSuffix = new Address { Pattern = "suffix", Description = "suffix@nan.hr", Domain = nan, Type = AddressType.Suffix };
        Addresses.AddRange(icaMailAdmin, nanMailAdmin, icaMailUser, nanMailUser, icaMailPrefix, nanMailPrefix, icaMailSuffix, nanMailSuffix);

        icaAdmin.Addresses.Add(icaMailAdmin);
        nanAdmin.Addresses.Add(nanMailAdmin);
        icaUser.Addresses.Add(icaMailUser); icaUser.Addresses.Add(icaMailPrefix); icaUser.Addresses.Add(icaMailSuffix);
        nanUser.Addresses.Add(nanMailUser); nanUser.Addresses.Add(nanMailPrefix); nanUser.Addresses.Add(nanMailSuffix);

        await SaveChangesAsync();

        var icaAdminCalendar = new Calendar { DisplayName = "ICA Admin Calendar" };
        var icaUserCalendar = new Calendar { DisplayName = "ICA User Calendar" };
        var nanAdminCalendar = new Calendar { DisplayName = "NaN Admin Calendar" };
        var nanUserCalendar = new Calendar { DisplayName = "NaN User Calendar" };
        Calendars.AddRange(icaAdminCalendar, icaUserCalendar, nanAdminCalendar, nanUserCalendar);

        await SaveChangesAsync();

        CalendarUsers.Add(new() { Calendar = icaAdminCalendar, User = icaAdmin, CanWrite = true });
        CalendarUsers.Add(new() { Calendar = icaUserCalendar, User = icaAdmin });
        CalendarUsers.Add(new() { Calendar = icaUserCalendar, User = icaUser, CanWrite = true });
        CalendarUsers.Add(new() { Calendar = nanAdminCalendar, User = nanAdmin, CanWrite = true });
        CalendarUsers.Add(new() { Calendar = nanUserCalendar, User = nanAdmin });
        CalendarUsers.Add(new() { Calendar = nanUserCalendar, User = nanUser, CanWrite = true });

        await SaveChangesAsync();
    }
}