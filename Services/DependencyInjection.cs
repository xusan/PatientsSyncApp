using Core.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Registering Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        return services;
    }
}
