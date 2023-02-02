﻿// <auto-generated />
using System;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DailyLeetcodeReminder.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.Challenger", b =>
                {
                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint");

                    b.Property<short>("Attempts")
                        .HasColumnType("smallint");

                    b.Property<string>("FirstName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("LastName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("LeetcodeUserName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TotalSolvedProblems")
                        .HasColumnType("int");

                    b.HasKey("TelegramId");

                    b.HasIndex("LeetcodeUserName")
                        .IsUnique();

                    b.ToTable("Challengers", (string)null);
                });

            modelBuilder.Entity("DailyLeetcodeReminder.Domain.Entities.DailyAttempt", b =>
                {
                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("SolvedProblems")
                        .HasColumnType("int");

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
