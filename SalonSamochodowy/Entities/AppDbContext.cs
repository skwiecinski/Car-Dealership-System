using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// https://learn.microsoft.com/pl-pl/ef/core/dbcontext-configuration/

namespace SalonSamochodowy.Entities
{
    public class AppDbContext:DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // sciezka wzgledna (zeby nie korzystac z pliku .db w folderze debug)
                optionsBuilder.UseSqlite(@"Data Source=..\..\..\SalonSamochodowy.db");
            }
        }
        

        public virtual DbSet<AppRole> AppRoles { get; set; }
        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Engine> Engines { get; set; }
        public virtual DbSet<Feature> Features { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<SalesOrder> SalesOrders { get; set; }
        public virtual DbSet<TrimFeature> TrimFeatures { get; set; }
        public virtual DbSet<TrimLevel> TrimLevels { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehicleFeature> VehicleFeatures { get; set; }
        public virtual DbSet<VehicleModel> VehicleModels { get; set; }
        public virtual DbSet<Worker> Workers { get; set; }
        public virtual DbSet<Dealership> Dealerships { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // klucze zlozone
            modelBuilder.Entity<VehicleFeature>().HasKey(vf => new { vf.VehicleID, vf.FeatureID });
            modelBuilder.Entity<TrimFeature>().HasKey(tf => new { tf.TrimID, tf.FeatureID });

            modelBuilder.Entity<AppRole>().HasKey(r => r.RoleID);
            modelBuilder.Entity<AppUser>().HasKey(u => u.UserID);
            modelBuilder.Entity<SalesOrder>().HasKey(s => s.OrderID);
            modelBuilder.Entity<TrimLevel>().HasKey(t => t.TrimID);
            modelBuilder.Entity<VehicleModel>().HasKey(v => v.ModelID);

            // tabela client posiada klucz obcy UserID
            modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Client)
            .WithOne(c => c.User)
            .HasForeignKey<Client>(c => c.UserID);
            // tabela worker posiada klucz obcy UserID
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Worker)
                .WithOne(w => w.User)
                .HasForeignKey<Worker>(w => w.UserID);
        }

    }
}
