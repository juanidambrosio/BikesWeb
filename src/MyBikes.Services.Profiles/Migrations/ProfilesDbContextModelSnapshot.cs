﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using BikeSharing.Services.Profiles.Data;

namespace BikeSharing.Services.Profiles.Migrations
{
    [DbContext(typeof(ProfilesDbContext))]
    partial class ProfilesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BikeSharing.Models.Profiles.PaymentData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreditCard")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("CreditCardType");

                    b.Property<DateTime>("ExpirationDate");

                    b.HasKey("Id");

                    b.ToTable("PaymentData");
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("ExpiresOn");

                    b.Property<int>("Status");

                    b.Property<int>("Type");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Subscriptions");

                    b.HasAnnotation("SqlServer:TableName", "Subscription");
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastLogin");

                    b.Property<string>("Password")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("TenantId")
                        .HasAnnotation("SqlServer:DefaultValue", 0);

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("BirthDate");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<Guid?>("FaceProfileId");

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("Gender");

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Mobile")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int?>("PaymentId");

                    b.Property<string>("Skype")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("UserId");

                    b.Property<Guid?>("VoiceProfileId");

                    b.Property<string>("VoiceSecretPhrase")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.HasIndex("PaymentId")
                        .IsUnique();

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.Subscription", b =>
                {
                    b.HasOne("BikeSharing.Models.Profiles.User", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.User", b =>
                {
                    b.HasOne("BikeSharing.Models.Profiles.Tenant", "Tenant")
                        .WithMany("Users")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BikeSharing.Models.Profiles.UserProfile", b =>
                {
                    b.HasOne("BikeSharing.Models.Profiles.PaymentData", "Payment")
                        .WithOne()
                        .HasForeignKey("BikeSharing.Models.Profiles.UserProfile", "PaymentId");

                    b.HasOne("BikeSharing.Models.Profiles.User", "User")
                        .WithOne("Profile")
                        .HasForeignKey("BikeSharing.Models.Profiles.UserProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
