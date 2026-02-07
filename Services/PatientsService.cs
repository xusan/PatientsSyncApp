using AutoMapper;
using Core.Common;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Services;

public class PatientsService : BaseService, IPatientsService
{
    private readonly IMapper mapper;

    public PatientsService(IServiceScopeFactory scopeFactory,
                             ILogger<PatientsService> logger,
                             IMapper mapper) : base(scopeFactory, logger)
    {
        this.mapper = mapper;
    }

    public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync()
    {
        return ExecuteScopedAsync<IReadOnlyList<PatientModel>>(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();

            var list = await repo.GetAllAsync();
            return list.Select(s => mapper.Map<PatientModel>(s)).ToList();
        });
    }


    public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take)
    {
        return ExecuteScopedAsync<IReadOnlyList<PatientModel>>(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();
            var list = await repo.GetPageAsync(take, skip);

            return list.Select(s => mapper.Map<PatientModel>(s)).ToList();
        });
    }

    public Task<ActionResponse> UpsertPatientsBatchAsync(IEnumerable<PatientModel> patientModels)
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientsRepository>();
            var entityList = patientModels.Select(s => mapper.Map<PatientModel>(s)).ToList();
            await repo.UpsertBatchAsync(entityList);
        });
    }
}
