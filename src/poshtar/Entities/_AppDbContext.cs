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
            e.HasIndex(e => e.Pattern).IsUnique();
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
            e.HasMany(e => e.Domains).WithMany(e => e.Users);
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
            Host = "abcd.ica.hr",
            Port = 587,
            IsSecure = true,
            Username = "home",
            Password = serverProtector.Protect("P@ssw0rd"),
        };
        Domains.Add(ica);

        var master = DovecotHasher.Hash("master");
        var masterUser = new User { Name = "master", IsMaster = true, Description = "Master user", Salt = master.Salt, Hash = master.Hash, Password = DovecotHasher.Password(master.Salt, master.Hash), };
        Users.Add(masterUser);
        ica.Users.Add(masterUser);

        var slave = DovecotHasher.Hash("slave");
        var slaveUser = new User { Name = "slave", Quota = 1024, Description = "Slave user", Salt = slave.Salt, Hash = slave.Hash, Password = DovecotHasher.Password(slave.Salt, slave.Hash), };
        Users.Add(slaveUser);
        ica.Users.Add(slaveUser);


        var mailMaster = new Address { Pattern = "master", Description = "Master main", Domain = ica, Type = AddressType.Exact, };
        var mailSlave = new Address { Pattern = "slave", Description = "Slave main", Domain = ica, Type = AddressType.Exact, };
        var mailSales = new Address { Pattern = "slave.", Description = "Slave prefix", Domain = ica, Type = AddressType.Prefix };
        mailMaster.Users.Add(masterUser);
        mailSlave.Users.Add(slaveUser);
        mailSales.Users.Add(slaveUser);
        Addresses.AddRange(mailMaster, mailSlave, mailSales);

        await SaveChangesAsync();
    }
}