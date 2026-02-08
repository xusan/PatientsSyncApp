using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EfDataStorage;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    public DbSet<PatientModel> Patients => Set<PatientModel>();
    public DbSet<ServiceSettingsModel> ServiceSettings => Set<ServiceSettingsModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Primary Keys and Required Fields
        modelBuilder.Entity<PatientModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();            
        });

        modelBuilder.Entity<ServiceSettingsModel>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        //// SEED DATA: Essential for the Service to have a starting point
        //modelBuilder.Entity<ServiceSettingsModel>().HasData(new ServiceSettingsModel
        //{
        //    Id = 1,
        //    ExportSchedule = "0 0 * * *",      // Daily at Midnight
        //    ImportSchedule = "0 */2 * * *",  // Every 2 hours
        //    ExportFolder = @"C:\Temp\Sync\Outbox",
        //    ImportFolder = @"C:\Temp\Sync\Inbox",            
        //});

        //modelBuilder.Entity<PatientModel>().HasData(
        //        new PatientModel { Id = 1, Name = "John", Surname = "Doe", Email = "j.doe@example.com", DateOfBirth = new DateTime(1985, 5, 12) },
        //        new PatientModel { Id = 2, Name = "Jane", Surname = "Smith", Email = "j.smith@health.org", DateOfBirth = new DateTime(1990, 8, 24) },
        //        new PatientModel { Id = 3, Name = "Robert", Surname = "Brown", Email = "rbrown@med.net", DateOfBirth = new DateTime(1978, 12, 05) },
        //        new PatientModel { Id = 4, Name = "Emily", Surname = "Davis", Email = "emily.d@test.com", DateOfBirth = new DateTime(1995, 3, 15) },
        //        new PatientModel { Id = 5, Name = "Michael", Surname = "Wilson", Email = "m.wilson@clinic.com", DateOfBirth = new DateTime(1982, 11, 30) },
        //        new PatientModel { Id = 6, Name = "Sarah", Surname = "Miller", Email = "smiller@hospitals.uk", DateOfBirth = new DateTime(1988, 07, 22) },
        //        new PatientModel { Id = 7, Name = "David", Surname = "Garcia", Email = "dgarcia@gmail.com", DateOfBirth = new DateTime(1975, 01, 10) },
        //        new PatientModel { Id = 8, Name = "Jessica", Surname = "Taylor", Email = "jtaylor@outlook.com", DateOfBirth = new DateTime(1999, 06, 18) },
        //        new PatientModel { Id = 9, Name = "Chris", Surname = "Anderson", Email = "chris.a@sync.io", DateOfBirth = new DateTime(1992, 09, 09) },
        //        new PatientModel { Id = 10, Name = "Amanda", Surname = "Thomas", Email = "athomas@care.com", DateOfBirth = new DateTime(1980, 04, 27) }
        //    );
    }
}