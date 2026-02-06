using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts;

public interface ISettingsRepository
{
    public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync();
    public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings);
}
