using TaskApp.AppServices.Common;
using TaskApp.AppServices.Dto;

namespace TaskApp.AppServices.Services.Settings
{
    public interface ISettingsAppService
    {
        public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync();
        public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings);
    }
}
