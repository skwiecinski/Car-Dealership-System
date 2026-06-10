using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;



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
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=SalonSamochodowy_v7;Trusted_Connection=True;MultipleActiveResultSets=true");
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
            
            modelBuilder.Entity<VehicleFeature>().HasKey(vf => new { vf.VehicleID, vf.FeatureID });
            modelBuilder.Entity<TrimFeature>().HasKey(tf => new { tf.TrimID, tf.FeatureID });

            modelBuilder.Entity<AppRole>().HasKey(r => r.RoleID);
            modelBuilder.Entity<AppUser>().HasKey(u => u.UserID);
            modelBuilder.Entity<SalesOrder>().HasKey(s => s.OrderID);
            modelBuilder.Entity<TrimLevel>().HasKey(t => t.TrimID);
            modelBuilder.Entity<VehicleModel>().HasKey(v => v.ModelID);

            
            modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Client)
            .WithOne(c => c.User)
            .HasForeignKey<Client>(c => c.UserID);
            
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Worker)
                .WithOne(w => w.User)
                .HasForeignKey<Worker>(w => w.UserID);

            
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Worker)
                .WithMany(w => w.Jobs)
                .HasForeignKey(j => j.WorkerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Vehicle)
                .WithMany(v => v.Jobs)
                .HasForeignKey(j => j.VehicleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(s => s.Worker)
                .WithMany(w => w.SalesOrders)
                .HasForeignKey(s => s.WorkerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.SalesOrders)
                .HasForeignKey(s => s.VehicleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(s => s.Dealership)
                .WithMany(d => d.SalesOrders)
                .HasForeignKey(s => s.DealershipID)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
