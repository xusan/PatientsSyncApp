using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskApp.Domain.Common;
using TaskApp.Domain.Patients;
using TaskApp.Domain.Settings;
using TaskApp.Domain.Sync;
using TaskApp.Infrastructures.Repository;


namespace TaskApp.Infrastructures
{
    public static class RegistratorExtension
    {
        public static void RegisterInfrastructures(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IPatientRepository), typeof(PatientRepository));
            services.AddScoped(typeof(ISettingsRepository), typeof(SettingsRepository));

            services.AddSingleton(typeof(ICsvService), typeof(CsvReader));
        }
    }
}
