using Core.Contracts.Repositories;
using Core.Contracts.Services;
using EfDataStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;

namespace Bootstrap;

public static class Bootstrapper
{   
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // 1. Persistence & EF Core
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("AppDbContext");
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());

            if (environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        });
        
        services.AddScoped<IPatientsRepository, PatientsRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();


        services.AddSingleton<IPatientsService, PatientsService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISyncService, SyncService>();

        services.AddAutoMapper(typeof(EntitiesMappingProfile).Assembly);

        return services;
    }
}
