using AutoMapper;
using Core.Contracts;
using Core.Models;
using EfDataStorage;
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
            logger.Value.LogError(ex, null);
            res.Exception = ex;
        }

        return res;
    }

    public Task<ActionResponse> InsertAsync(IReadOnlyList<PatientModel> patients)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResponse> UpdateAsync(IReadOnlyList<PatientModel> patients)
    {
        throw new NotImplementedException();
    }
}
