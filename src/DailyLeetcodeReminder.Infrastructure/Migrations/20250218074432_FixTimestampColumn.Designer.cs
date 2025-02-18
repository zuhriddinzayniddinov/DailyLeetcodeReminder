﻿// <auto-generated />
using System;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DailyLeetcodeReminder.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250218074432_FixTimestampColumn")]
    partial class FixTimestampColumn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.Challenger", b =>
                {
                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("FirstName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<short>("Heart")
                        .HasColumnType("smallint");

                    b.Property<string>("LastName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("LeetcodeUserName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("TotalSolvedProblems")
                        .HasColumnType("integer");

                    b.HasKey("TelegramId");

                    b.HasIndex("LeetcodeUserName")
                        .IsUnique();

                    b.ToTable("Challengers", (string)null);
                });

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.ChallengerWithNoAttempt", b =>
                {
                    b.Property<string>("LeetcodeUserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint");

                    b.Property<int>("TotalSolvedProblems")
                        .HasColumnType("integer");

                    b.ToTable("ChallengerWithNoAttempt");
                });

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.DailyAttempt", b =>
                {
                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("SolvedProblems")
                        .HasColumnType("integer");

                    b.HasKey("Date", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("DailyAttempts", (string)null);
                });

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.DailyAttempt", b =>
                {
                    b.HasOne("DailyLeetcodeReminder.Domain.Entities.Challenger", "User")
                        .WithMany("DailyAttempts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.Challenger", b =>
                {
                    b.Navigation("DailyAttempts");
                });
#pragma warning restore 612, 618
        }
    }
}
