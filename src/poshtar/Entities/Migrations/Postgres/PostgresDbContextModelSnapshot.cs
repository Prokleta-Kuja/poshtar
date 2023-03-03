﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using poshtar.Entities;

#nullable disable

namespace poshtar.Entities.Migrations.Postgres
{
    [DbContext(typeof(PostgresDbContext))]
    partial class PostgresDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AddressUser", b =>
                {
                    b.Property<int>("AddressesAddressId")
                        .HasColumnType("integer")
                        .HasColumnName("addresses_address_id");

                    b.Property<int>("UsersUserId")
                        .HasColumnType("integer")
                        .HasColumnName("users_user_id");

                    b.HasKey("AddressesAddressId", "UsersUserId")
                        .HasName("pk_address_user");

                    b.HasIndex("UsersUserId")
                        .HasDatabaseName("ix_address_user_users_user_id");

                    b.ToTable("address_user", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text")
                        .HasColumnName("friendly_name");

                    b.Property<string>("Xml")
                        .HasColumnType("text")
                        .HasColumnName("xml");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Address", b =>
                {
                    b.Property<int>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("address_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AddressId"));

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTime?>("Disabled")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("disabled");

                    b.Property<int>("DomainId")
                        .HasColumnType("integer")
                        .HasColumnName("domain_id");

                    b.Property<string>("Expression")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("text")
                        .HasColumnName("expression")
                        .HasComputedColumnSql("CASE type WHEN 0 THEN pattern WHEN 1 THEN pattern || '%' WHEN 2 THEN '%' || pattern ELSE NULL END", true);

                    b.Property<string>("Pattern")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pattern");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("AddressId")
                        .HasName("pk_addresses");

                    b.HasIndex("DomainId")
                        .HasDatabaseName("ix_addresses_domain_id");

                    b.ToTable("addresses", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Domain", b =>
                {
                    b.Property<int>("DomainId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("domain_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DomainId"));

                    b.Property<DateTime?>("Disabled")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("disabled");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("host");

                    b.Property<bool>("IsSecure")
                        .HasColumnType("boolean")
                        .HasColumnName("is_secure");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<int>("Port")
                        .HasColumnType("integer")
                        .HasColumnName("port");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("DomainId")
                        .HasName("pk_domains");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_domains_name");

                    b.ToTable("domains", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserId"));

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTime?>("Disabled")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("disabled");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("hash");

                    b.Property<bool>("IsMaster")
                        .HasColumnType("boolean")
                        .HasColumnName("is_master");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<int?>("Quota")
                        .HasColumnType("integer")
                        .HasColumnName("quota");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("salt");

                    b.HasKey("UserId")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("AddressUser", b =>
                {
                    b.HasOne("poshtar.Entities.Address", null)
                        .WithMany()
                        .HasForeignKey("AddressesAddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_address_user_addresses_addresses_address_id");

                    b.HasOne("poshtar.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_address_user_users_users_user_id");
                });

            modelBuilder.Entity("poshtar.Entities.Address", b =>
                {
                    b.HasOne("poshtar.Entities.Domain", "Domain")
                        .WithMany("Addresses")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_addresses_domains_domain_id");

                    b.Navigation("Domain");
                });

            modelBuilder.Entity("poshtar.Entities.Domain", b =>
                {
                    b.Navigation("Addresses");
                });
#pragma warning restore 612, 618
        }
    }
}
