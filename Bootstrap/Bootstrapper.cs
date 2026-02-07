using Core.Contracts;
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

        // 2. Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ISyncService, SyncService>();

        // 3. AutoMapper (assuming MappingProfile is in your Services project)
        // You need to point to a type inside the assembly where your profiles are
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }
}
