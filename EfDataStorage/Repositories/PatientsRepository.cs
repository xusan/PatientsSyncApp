using AutoMapper;
using Core.Contracts.Repositories;
using Core.Models;
using EfDataStorage;
using EfDataStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Services;

public class PatientsRepository : IPatientsRepository
{
    private readonly AppDbContext database;
    private readonly IMapper mapper;
    private readonly ILogger<PatientsRepository> logger;

    public PatientsRepository(AppDbContext database, IMapper mapper, ILogger<PatientsRepository> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<PatientModel>> GetAllAsync()
    {        
        var entities = await database.Patients.ToListAsync();
        var patients = mapper.Map<IReadOnlyList<PatientModel>>(entities);
        return patients;
    }

    public async Task<int> UpsertBatchAsync(IEnumerable<PatientModel> patientModels)
    {
        var incomingEntities = mapper.Map<IEnumerable<PatientEntity>>(patientModels);
        var incomingIds = incomingEntities.Select(p => p.Id).ToList();

        // 2. Fetch existing records to check for Updates vs Inserts
        var existingEntities = await database.Patients
            .Where(p => incomingIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var incomingEntity in incomingEntities)
        {
            if (existingEntities.TryGetValue(incomingEntity.Id, out var existingEntity))
            {
                // Intelligent update (only changes what is different)
                database.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
            }
            else
            {
                await database.Patients.AddAsync(incomingEntity);
            }
        }

        return await database.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PatientModel>> GetPageAsync(int skip, int take)
    {
        // 1. Fetch the data from SQL Server using Paging
        var entities = await database.Patients
            .AsNoTracking()
            .OrderBy(p => p.Id) // Required for Skip/Take
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        // 2. Map to Models before returning to the Service
        var patients = mapper.Map<IReadOnlyList<PatientModel>>(entities);
        return patients;
    }
}
