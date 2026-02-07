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
    private readonly Lazy<AppDbContext> database;
    private readonly Lazy<IMapper> mapper;
    private readonly Lazy<ILogger<PatientRepository>> logger;

    public SettingsRepository(Lazy<AppDbContext> database, Lazy<IMapper> mapper, Lazy<ILogger<PatientRepository>> logger)
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
            var entity = await database.Value.Patients.FirstOrDefaultAsync();
            var model = mapper.Value.Map<ServiceSettingsModel>(entity);

            res.Success = true;
            res.Result = model;
        }
        catch (Exception ex)
        {
            logger.Value.LogError(ex, "Error getting settings from database");
            res.Msg = ex.Message;
        }

        return res;
    }

    public async Task<ActionResponse> UpdateAsync(ServiceSettingsModel serviceSettings)
    {
        var res = new ActionResultResponse<ServiceSettingsModel>();

        try
        {
            var incomingEntity = mapper.Value.Map<ServiceSettingsEntity>(serviceSettings);
            var existingEntity = await database.Value.Patients.FirstOrDefaultAsync(s => s.Id == serviceSettings.Id);
            database.Value.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
            await database.Value.SaveChangesAsync();

            res.Success = true;            
        }
        catch (Exception ex)
        {
            logger.Value.LogError(ex, "Error saving settings to database");
            res.Msg = ex.Message;
        }

        return res;
    }
}
