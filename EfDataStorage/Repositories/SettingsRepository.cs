using AutoMapper;
using Core.Contracts.Repositories;
using Core.Models;
using EfDataStorage;
using EfDataStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;

public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext database;
    private readonly IMapper mapper;
    private readonly ILogger<PatientsRepository> logger;

    public SettingsRepository(AppDbContext database, IMapper mapper, ILogger<PatientsRepository> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<ServiceSettingsModel> GetAsync()
    {
        var entity = await database.ServiceSettings.FirstOrDefaultAsync();
        var model = mapper.Map<ServiceSettingsModel>(entity);
        return model;
    }

    public async Task<int> UpdateAsync(ServiceSettingsModel serviceSettings)
    {
        var incomingEntity = mapper.Map<ServiceSettingsEntity>(serviceSettings);
        var existingEntity = await database.ServiceSettings.FirstOrDefaultAsync(s => s.Id == serviceSettings.Id);
        database.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
        return await database.SaveChangesAsync();
    }
}
