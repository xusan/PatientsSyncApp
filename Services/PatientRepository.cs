using AutoMapper;
using Core.Contracts;
using Core.Models;
using EfDataStorage;
using EfDataStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Services;

public class PatientRepository : IPatientRepository
{
    private readonly Lazy<AppDbContext> database;
    private readonly Lazy<IMapper> mapper;
    private readonly Lazy<ILogger<PatientRepository>> logger;

    public PatientRepository(Lazy<AppDbContext> database, Lazy<IMapper> mapper, Lazy<ILogger<PatientRepository>> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync()
    {
        var res = new ActionResultResponse<IReadOnlyList<PatientModel>>();

        try
        {
            var entities = await database.Value.Patients.ToListAsync();
            var patients = mapper.Value.Map<IReadOnlyList<PatientModel>>(entities);

            res.Success = true;
            res.Result = patients;
        }
        catch (Exception ex)
        {
            logger.Value.LogError(ex, "Error getting all patients from database");
            res.Msg = ex.Message;
        }

        return res;
    }

    public async Task<ActionResponse> UpsertPatientsBatchAsync(IEnumerable<PatientModel> patientModels)
    {
        var res = new ActionResponse();

        try
        {
            // 1. Map Models to Entities internally
            var incomingEntities = mapper.Value.Map<IEnumerable<PatientEntity>>(patientModels);
            var incomingIds = incomingEntities.Select(p => p.Id).ToList();

            // 2. Fetch existing records to check for Updates vs Inserts
            var existingEntities = await database.Value.Patients
                .Where(p => incomingIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var incomingEntity in incomingEntities)
            {
                if (existingEntities.TryGetValue(incomingEntity.Id, out var existingEntity))
                {
                    // Intelligent update (only changes what is different)
                    database.Value.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
                }
                else
                {
                    await database.Value.Patients.AddAsync(incomingEntity);
                }
            }

            await database.Value.SaveChangesAsync();

            res.Success = true;
        }
        catch (Exception ex)
        {
            logger.Value.LogError(ex, "Error in UpsertPatientsBatchAsync");
            res.Success = false;
            res.Msg = ex.Message;
        }

        return res;
    }

    public async Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take)
    {
        var res = new ActionResultResponse<IReadOnlyList<PatientModel>>();

        try
        {
            // 1. Fetch the data from SQL Server using Paging
            var entities = await database.Value.Patients
                .AsNoTracking()
                .OrderBy(p => p.Id) // Required for Skip/Take
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // 2. Map to Models before returning to the Service
            var patients = mapper.Value.Map<IReadOnlyList<PatientModel>>(entities);

            res.Success = true;
            res.Result = patients;
        }
        catch (Exception ex)
        {
            logger.Value.LogError(ex, "Error getting batch patients from database");
            res.Msg = ex.Message;
        }

        return res;
    }
}
