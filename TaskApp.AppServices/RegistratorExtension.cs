using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using TaskApp.AppServices.Dto;
using TaskApp.Domain.Patients;
using TaskApp.AppServices.Services.Patients;
using TaskApp.AppServices.Services.Settings;
using TaskApp.AppServices.Services.Sync;

namespace TaskApp.AppServices
{
    public static class RegistratorExtension
    {
        public static void RegisterAppServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            services.AddSingleton<IPatientAppService, PatientAppService>();
            services.AddSingleton<ISettingsAppService, SettingsAppService>();
            services.AddSingleton<ISyncService, SyncService>();
        }
    }
}
