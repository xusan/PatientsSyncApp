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
    }
}