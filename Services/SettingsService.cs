using AutoMapper;
using Core.Common;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Services;

public class SettingsService : BaseService, ISettingsService
{    
    public SettingsService(IServiceScopeFactory scopeFactory,
                              ILogger<PatientsService> logger) : base(scopeFactory, logger)
    {        
    }

    public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync()
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<ISettingsRepository>();

            var setting = await repo.GetAsync();            
            
            return setting;
        });
    }

    public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings)
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<ISettingsRepository>();            
            await repo.UpdateAsync(serviceSettings);
        });
    }
}
