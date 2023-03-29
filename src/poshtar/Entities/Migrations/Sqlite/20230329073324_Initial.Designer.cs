﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using poshtar.Entities;

#nullable disable

namespace poshtar.Entities.Migrations.Sqlite
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20230329073324_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("AddressUser", b =>
                {
                    b.Property<int>("AddressesAddressId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("addresses_address_id");

                    b.Property<int>("UsersUserId")
                        .HasColumnType("INTEGER")
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
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("TEXT")
                        .HasColumnName("friendly_name");

                    b.Property<string>("Xml")
                        .HasColumnType("TEXT")
                        .HasColumnName("xml");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Address", b =>
                {
                    b.Property<int>("AddressId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("address_id");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<long?>("Disabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("disabled");

                    b.Property<int>("DomainId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("domain_id");

                    b.Property<string>("Expression")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("TEXT")
                        .HasColumnName("expression")
                        .HasComputedColumnSql("CASE type WHEN 0 THEN pattern WHEN 1 THEN pattern || '%' WHEN 2 THEN '%' || pattern ELSE NULL END", true);

                    b.Property<string>("Pattern")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("pattern");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER")
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
                        .HasColumnType("INTEGER")
                        .HasColumnName("domain_id");

                    b.Property<long?>("Disabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("disabled");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("host");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("password");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER")
                        .HasColumnName("port");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("username");

                    b.HasKey("DomainId")
                        .HasName("pk_domains");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_domains_name");

                    b.ToTable("domains", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.LogEntry", b =>
                {
                    b.Property<int>("LogEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("log_entry_id");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("message");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT")
                        .HasColumnName("properties");

                    b.Property<long>("Timestamp")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timestamp");

                    b.Property<int>("TransactionId")
                        .HasColumnType("INTEGER")
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
                        .HasColumnType("INTEGER")
                        .HasColumnName("recipient_id");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("data");

                    b.Property<bool>("Delivered")
                        .HasColumnType("INTEGER")
                        .HasColumnName("delivered");

                    b.Property<int>("TransactionId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("transaction_id");

                    b.Property<int?>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("RecipientId")
                        .HasName("pk_recipients");

                    b.HasIndex("TransactionId")
                        .HasDatabaseName("ix_recipients_transaction_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_recipients_user_id");

                    b.ToTable("recipients", (string)null);
                });

            modelBuilder.Entity("poshtar.Entities.Transaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("transaction_id");

                    b.Property<string>("Client")
                        .HasColumnType("TEXT")
                        .HasColumnName("client");

                    b.Property<bool>("Complete")
                        .HasColumnType("INTEGER")
                        .HasColumnName("complete");

                    b.Property<Guid>("ConnectionId")
                        .HasColumnType("TEXT")
                        .HasColumnName("connection_id");

                    b.Property<long>("End")
                        .HasColumnType("INTEGER")
                        .HasColumnName("end");

                    b.Property<string>("From")
                        .HasColumnType("TEXT")
                        .HasColumnName("from");

                    b.Property<int?>("FromUserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("from_user_id");

                    b.Property<long>("Start")
                        .HasColumnType("INTEGER")
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
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<long?>("Disabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("disabled");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("hash");

                    b.Property<bool>("IsMaster")
                        .HasColumnType("INTEGER")
                        .HasColumnName("is_master");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("password");

                    b.Property<int?>("Quota")
                        .HasColumnType("INTEGER")
                        .HasColumnName("quota");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("TEXT")
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
