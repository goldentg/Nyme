﻿// <auto-generated />
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Migrations
{
    [DbContext(typeof(brainKillerContext))]
    partial class brainKillerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Infrastructure.AutoRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("AutoRoles");
                });

            modelBuilder.Entity("Infrastructure.Rank", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("Ranks");
                });

            modelBuilder.Entity("Infrastructure.Server", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Background")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("Logs")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("Welcome")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("WelcomeDm")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("WelcomeDmMessage")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });
#pragma warning restore 612, 618
        }
    }
}
