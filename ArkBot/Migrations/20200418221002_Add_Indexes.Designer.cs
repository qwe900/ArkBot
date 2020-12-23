﻿// <auto-generated />
using System;
using ArkBot.Modules.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArkBot.Migrations
{
    [DbContext(typeof(EfDatabaseContext))]
    [Migration("20200418221002_Add_Indexes")]
    partial class Add_Indexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ArkBot.Modules.Database.Model.ChatMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("At")
                        .HasColumnType("datetime2");

                    b.Property<string>("CharacterName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Icon")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Mode")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("SteamId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("TribeName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SteamId");

                    b.HasIndex("ServerKey", "At");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("ArkBot.Modules.Database.Model.LoggedLocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("At")
                        .HasColumnType("datetime2");

                    b.Property<float>("Latitude")
                        .HasColumnType("real");

                    b.Property<float>("Longitude")
                        .HasColumnType("real");

                    b.Property<string>("ServerKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SteamId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<float>("X")
                        .HasColumnType("real");

                    b.Property<float>("Y")
                        .HasColumnType("real");

                    b.Property<float>("Z")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("At");

                    b.HasIndex("SteamId");

                    b.ToTable("LoggedLocations");
                });

            modelBuilder.Entity("ArkBot.Modules.Database.Model.Player", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("decimal(20,0)");

                    b.Property<bool>("IsOnline")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastServerKey")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("IsOnline", "LastServerKey");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("ArkBot.Modules.Database.Model.ChatMessage", b =>
                {
                    b.HasOne("ArkBot.Modules.Database.Model.Player", "Player")
                        .WithMany("ChatMessages")
                        .HasForeignKey("SteamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ArkBot.Modules.Database.Model.LoggedLocation", b =>
                {
                    b.HasOne("ArkBot.Modules.Database.Model.Player", "Player")
                        .WithMany("LoggedLocations")
                        .HasForeignKey("SteamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}