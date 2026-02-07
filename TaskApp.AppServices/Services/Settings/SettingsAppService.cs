using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskApp.AppServices.Common;
using TaskApp.AppServices.Dto;
using TaskApp.AppServices.Exceptions;
using TaskApp.AppServices.Services.Patients;
using TaskApp.Domain.Settings;

namespace TaskApp.AppServices.Services.Settings
{
    internal class SettingsAppService : AppServiceBase, ISettingsAppService
    {
        private Lazy<IMapper> mapper;

        public SettingsAppService(IServiceScopeFactory scopeFactory,
                                  ILogger<PatientAppService> logger,
                                  Lazy<IMapper> mapper) : base(scopeFactory, logger)
        {
            this.mapper = mapper;
        }

        public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync()
        {            
            return ExecuteScopedAsync(async scope =>
            {
                var repo = scope.GetRequiredService<ISettingsRepository>();

                var setting = await repo.FindById(1);
                if (setting == null)
                    throw new RecordNotFoundException();

                var model = mapper.Value.Map<ServiceSettingsModel>(setting);
                return model;   
            });
            
        }

        public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings)
        {           
            return ExecuteScopedAsync(async scope =>
            {
                var repo = scope.GetRequiredService<ISettingsRepository>();
                var entity = mapper.Value.Map<ServiceSetting>(serviceSettings);
                await repo.UpdateAsync(entity);
            });
        }
    }
}
