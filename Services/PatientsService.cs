using Core.Common;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Services;

public class PatientsService : BaseService, IPatientsService
{    
    public PatientsService(IServiceScopeFactory scopeFactory,
                             ILogger<PatientsService> logger) : base(scopeFactory, logger)
    {        
    }

    public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync()
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();
            var list = await repo.GetAllAsync();

            return list;
        });
    }


    public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take)
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();
            var list = await repo.GetPagedListAsync(skip, take);

            return list;
        });
    }

    public Task<ActionResponse> UpsertPatientsBatchAsync(IReadOnlyList<PatientModel> patients)
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();            
            await repo.BatchInsertOrUpdateAsync(patients);
        });
    }
}
