using Core.Contracts.Repositories;
using Core.Models;
using EfDataStorage;
using EfDataStorage.Repositories;

namespace Services;

public class SettingsRepository : Repository<ServiceSettingsModel>, ISettingsRepository
{    
    public SettingsRepository(AppDbContext context) : base(context)
    { 
    }
}
