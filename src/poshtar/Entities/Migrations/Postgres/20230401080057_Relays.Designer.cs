﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using poshtar.Entities;

#nullable disable

namespace poshtar.Entities.Migrations.Postgres
{
    [DbContext(typeof(PostgresDbContext))]
    [Migration("20230401080057_Relays")]
    partial class Relays
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int?>("RelayId")
                        .HasColumnType("integer")
                        .HasColumnName("relay_id");

                    b.HasKey("DomainId")
                        .HasName("pk_domains");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_domains_name");

                    b.HasIndex("RelayId")
                        .HasDatabaseName("ix_domains_relay_id");

                    b.ToTable("domains", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.LogEntry", b =>
                {
                    b.Property<int>("LogEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("log_entry_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LogEntryId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<string>("Properties")
                        .HasColumnType("text")
                        .HasColumnName("properties");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<int>("TransactionId")
                        .HasColumnType("integer")
                        .HasColumnName("transaction_id");

                    b.HasKey("LogEntryId")
                        .HasName("pk_logs");

                    b.HasIndex("TransactionId")
                        .HasDatabaseName("ix_logs_transaction_id");

                    b.ToTable("logs", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Recipient", b =>
                {
                    b.Property<int>("RecipientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("recipient_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RecipientId"));

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("data");

                    b.Property<bool>("Delivered")
                        .HasColumnType("boolean")
                        .HasColumnName("delivered");

                    b.Property<int>("TransactionId")
                        .HasColumnType("integer")
                        .HasColumnName("transaction_id");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("RecipientId")
                        .HasName("pk_recipients");

                    b.HasIndex("TransactionId")
                        .HasDatabaseName("ix_recipients_transaction_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_recipients_user_id");

                    b.ToTable("recipients", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Relay", b =>
                {
                    b.Property<int>("RelayId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("relay_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RelayId"));

                    b.Property<DateTime?>("Disabled")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("disabled");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("host");

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

                    b.HasKey("RelayId")
                        .HasName("pk_relays");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_relays_name");

                    b.ToTable("relays", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Transaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("transaction_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TransactionId"));

                    b.Property<string>("Client")
                        .HasColumnType("text")
                        .HasColumnName("client");

                    b.Property<Guid>("ConnectionId")
                        .HasColumnType("uuid")
                        .HasColumnName("connection_id");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end");

                    b.Property<string>("From")
                        .HasColumnType("text")
                        .HasColumnName("from");

                    b.Property<int?>("FromUserId")
                        .HasColumnType("integer")
                        .HasColumnName("from_user_id");

                    b.Property<bool>("Secure")
                        .HasColumnType("boolean")
                        .HasColumnName("secure");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start");

                    b.HasKey("TransactionId")
                        .HasName("pk_transactions");

                    b.HasIndex("FromUserId")
                        .HasDatabaseName("ix_transactions_from_user_id");

                    b.ToTable("transactions", (string)null);
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
                    b.HasOne("poshtar.Entities.Relay", "Relay")
                        .WithMany("Domains")
                        .HasForeignKey("RelayId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_domains_relays_relay_id");

                    b.Navigation("Relay");
                });

            modelBuilder.Entity("poshtar.Entities.LogEntry", b =>
                {
                    b.HasOne("poshtar.Entities.Transaction", "Transaction")
                        .WithMany("Logs")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_logs_transactions_transaction_id");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("poshtar.Entities.Recipient", b =>
                {
                    b.HasOne("poshtar.Entities.Transaction", "Transaction")
                        .WithMany("Recipients")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_recipients_transactions_transaction_id");

                    b.HasOne("poshtar.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_recipients_users_user_id");

                    b.Navigation("Transaction");

                    b.Navigation("User");
                });

            modelBuilder.Entity("poshtar.Entities.Transaction", b =>
                {
                    b.HasOne("poshtar.Entities.User", "FromUser")
                        .WithMany("Transactions")
                        .HasForeignKey("FromUserId")
                        .HasConstraintName("fk_transactions_users_from_user_id");

                    b.Navigation("FromUser");
                });

            modelBuilder.Entity("poshtar.Entities.Domain", b =>
                {
                    b.Navigation("Addresses");
                });

            modelBuilder.Entity("poshtar.Entities.Relay", b =>
                {
                    b.Navigation("Domains");
                });

            modelBuilder.Entity("poshtar.Entities.Transaction", b =>
                {
                    b.Navigation("Logs");

                    b.Navigation("Recipients");
                });

            modelBuilder.Entity("poshtar.Entities.User", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
