using Core.Common;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Services;

public interface ISettingsService
{
    public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync();
    public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings);
}
