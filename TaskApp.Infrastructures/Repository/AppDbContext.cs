using Microsoft.EntityFrameworkCore;
using TaskApp.Domain.Patients;
using TaskApp.Domain.Settings;

namespace TaskApp.Infrastructures;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<ServiceSetting> ServiceSettings => Set<ServiceSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Primary Keys and Required Fields
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();            
        });


        modelBuilder.Entity<ServiceSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // SEED DATA: Essential for the Service to have a starting point
        modelBuilder.Entity<ServiceSetting>().HasData(new ServiceSetting
        {
            Id = 1,
            SendingSchedule = "0 0 * * *",      // Daily at Midnight
            ReceivingSchedule = "0 */2 * * *",  // Every 2 hours
            InboxFolder = @"C:\Temp\Sync\Inbox",
            OutboxFolder = @"C:\Temp\Sync\Outbox"
        });

        modelBuilder.Entity<Patient>().HasData(
                new Patient { Id = 1, Name = "John", Surname = "Doe", Email = "j.doe@example.com", DateOfBirth = new DateTime(1985, 5, 12) },
                new Patient { Id = 2, Name = "Jane", Surname = "Smith", Email = "j.smith@health.org", DateOfBirth = new DateTime(1990, 8, 24) },
                new Patient { Id = 3, Name = "Robert", Surname = "Brown", Email = "rbrown@med.net", DateOfBirth = new DateTime(1978, 12, 05) },
                new Patient { Id = 4, Name = "Emily", Surname = "Davis", Email = "emily.d@test.com", DateOfBirth = new DateTime(1995, 3, 15) },
                new Patient { Id = 5, Name = "Michael", Surname = "Wilson", Email = "m.wilson@clinic.com", DateOfBirth = new DateTime(1982, 11, 30) },
                new Patient { Id = 6, Name = "Sarah", Surname = "Miller", Email = "smiller@hospitals.uk", DateOfBirth = new DateTime(1988, 07, 22) },
                new Patient { Id = 7, Name = "David", Surname = "Garcia", Email = "dgarcia@gmail.com", DateOfBirth = new DateTime(1975, 01, 10) },
                new Patient { Id = 8, Name = "Jessica", Surname = "Taylor", Email = "jtaylor@outlook.com", DateOfBirth = new DateTime(1999, 06, 18) },
                new Patient { Id = 9, Name = "Chris", Surname = "Anderson", Email = "chris.a@sync.io", DateOfBirth = new DateTime(1992, 09, 09) },
                new Patient { Id = 10, Name = "Amanda", Surname = "Thomas", Email = "athomas@care.com", DateOfBirth = new DateTime(1980, 04, 27) }
            );
    }
}