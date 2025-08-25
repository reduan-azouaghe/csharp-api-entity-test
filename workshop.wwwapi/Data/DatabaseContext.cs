using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using workshop.wwwapi.Models;

namespace workshop.wwwapi.Data
{
    public class DatabaseContext : DbContext
    {
        private string _connectionString;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection")!;
            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Seeder seed = new Seeder();

            // Define structure
            modelBuilder.Entity<Appointment>()
                .HasKey(a => a.AppointmentId);
            
            // Define composite primary key for PrescriptionMedicine
            modelBuilder.Entity<PrescriptionMedicine>()
                .HasKey(pm => new { pm.PrescriptionId, pm.MedicineId });
            
            // PrescriptionMedicine -> prescription (many-to-one)
            modelBuilder.Entity<PrescriptionMedicine>()
                .HasOne(pm => pm.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(pm => pm.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // PrescriptionMedicine -> medicine (many-to-one)
            modelBuilder.Entity<PrescriptionMedicine>()
                .HasOne(pm => pm.Medicine)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(pm => pm.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Medicine
            modelBuilder.Entity<Medicine>()
                .Property(m => m.Name)
                .HasMaxLength(200)
                .IsRequired();
                
             //Prescription -> Appointment (many-to-one)
             modelBuilder.Entity<Prescription>()
                 .HasOne(p => p.Appointment)
                 .WithMany(a => a.Prescriptions)
                 .HasForeignKey(p => p.Appointment.AppointmentId)
                 .OnDelete(DeleteBehavior.Restrict);
            
            /*// Appointment -> Patient (many-to-one)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);*/
            
            // Appointment -> Doctor (many-to-one)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed data
            modelBuilder.Entity<Appointment>().HasData(seed.Appointments);
            modelBuilder.Entity<Patient>().HasData(seed.Patients);
            modelBuilder.Entity<Doctor>().HasData(seed.Doctors);
            modelBuilder.Entity<Medicine>().HasData(seed.Medicines);
            //modelBuilder.Entity<Prescription>().HasData(seed.Prescription);
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
            optionsBuilder.LogTo(message => Debug.WriteLine(message));
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Medicine> Medicine { get; set; }
        
    }
}