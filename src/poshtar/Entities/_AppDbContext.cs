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
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<User> Users => Set<User>();

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

        builder.Entity<User>(e =>
        {
            e.HasKey(e => e.UserId);
            e.HasMany(e => e.Addresses).WithMany(e => e.Users);
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

        var serverProtector = dpProvider.CreateProtector(nameof(Domain));
        var ica = new Domain
        {
            Name = "ica.hr",
            Host = "relay.ica.hr",
            Port = 587,
            IsSecure = true,
            Username = "fake",
            Password = serverProtector.Protect("P@ssw0rd"),
        };
        var nan = new Domain
        {
            Name = "nan.hr",
            Host = "relay.nan.hr",
            Port = 587,
            IsSecure = true,
            Username = "fake",
            Password = serverProtector.Protect("P@ssw0rd"),
        };
        Domains.Add(nan);

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
    }
}