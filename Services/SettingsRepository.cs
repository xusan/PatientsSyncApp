using AutoMapper;
using Core.Contracts;
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
    private readonly ILogger<PatientRepository> logger;

    public SettingsRepository(AppDbContext database, IMapper mapper, ILogger<PatientRepository> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<ActionResultResponse<ServiceSettingsModel>> GetAsync()
    {
        var res = new ActionResultResponse<ServiceSettingsModel>();

        try
        {
            var entity = await database.ServiceSettings.FirstOrDefaultAsync();
            var model = mapper.Map<ServiceSettingsModel>(entity);

            res.Success = true;
            res.Result = model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting settings from database");
            res.Msg = ex.Message;
        }

        return res;
    }

    public async Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings)
    {
        var res = new ActionResultResponse<ServiceSettingsModel>();

        try
        {
            var incomingEntity = mapper.Map<ServiceSettingsEntity>(serviceSettings);
            var existingEntity = await database.ServiceSettings.FirstOrDefaultAsync(s => s.Id == serviceSettings.Id);
            database.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
            await database.SaveChangesAsync();

            res.Success = true;            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving settings to database");
            res.Msg = ex.Message;
        }

        return res;
    }
}
