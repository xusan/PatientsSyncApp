using Core.Contracts;
using Core.Models;
using EfDataStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;

public class SettingsRepository : ISettingsRepository
{
    private readonly Lazy<AppDbContext> database;

    public SettingsRepository(Lazy<AppDbContext> database)
    {
        this.database = database;
    }

    public Task<ActionResultResponse<ServiceSettingsModel>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings)
    {
        throw new NotImplementedException();
    }
}
