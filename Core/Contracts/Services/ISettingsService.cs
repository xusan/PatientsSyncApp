using Core.Common;
using Core.Models;

namespace Core.Contracts.Services;

public interface ISettingsService
{
    public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync();
    public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings);
}
