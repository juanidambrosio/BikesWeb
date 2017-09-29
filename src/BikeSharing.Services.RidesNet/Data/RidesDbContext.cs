using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BikeSharing.Services.RidesNet.Data
{
    public partial class RidesDbContext : DbContext
    {
        public virtual DbSet<Bikes> Bikes { get; set; }
        public virtual DbSet<RidePositions> RidePositions { get; set; }
        public virtual DbSet<Rides> Rides { get; set; }
        public virtual DbSet<Stations> Stations { get; set; }

        // Unable to generate entity type for table 'dbo.holidayDates'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlServer(@"Server=.\SQL2016;Database=bikesharing-services-rides;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bikes>(entity =>
            {
                entity.ToTable("bikes");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.SerialNumber).HasColumnType("varchar(16)");
            });

            modelBuilder.Entity<RidePositions>(entity =>
            {
                entity.ToTable("ridePositions");

                entity.HasIndex(e => e.RideId)
                    .HasName("ix_RidePositions_RideId");

                entity.Property(e => e.Latitude).HasColumnType("numeric");

                entity.Property(e => e.Longitude).HasColumnType("numeric");

                entity.Property(e => e.Ts).HasColumnName("TS");

                entity.HasOne(d => d.Ride)
                    .WithMany(p => p.RidePositions)
                    .HasForeignKey(d => d.RideId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ridePositions_rides");
            });

            modelBuilder.Entity<Rides>(entity =>
            {
                entity.ToTable("rides");

                // Switch to In-Memory
                //entity.ToTable("rides").ForSqlServerIsMemoryOptimized();

                entity.Property(e => e.EventName).HasColumnType("varchar(512)");

                entity.HasOne(d => d.EndStation)
                    .WithMany(p => p.RidesEndStation)
                    .HasForeignKey(d => d.EndStationId)
                    .HasConstraintName("FK_rides_ToEndStation");

                entity.HasOne(d => d.StartStation)
                    .WithMany(p => p.RidesStartStation)
                    .HasForeignKey(d => d.StartStationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_rides_ToStartStation");
            });

            modelBuilder.Entity<Stations>(entity =>
            {
                entity.ToTable("stations");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Latitude).HasColumnType("numeric");

                entity.Property(e => e.Longitude).HasColumnType("numeric");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.Slots)
                    .HasColumnType("numeric")
                    .HasDefaultValueSql("30");
            });
        }
    }
}