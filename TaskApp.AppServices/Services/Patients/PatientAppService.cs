using Microsoft.Extensions.DependencyInjection;
using System;
using TaskApp.AppServices.Common;
using TaskApp.Domain.Patients;
using Microsoft.Extensions.Logging;
using TaskApp.AppServices.Dto;
using AutoMapper;

namespace TaskApp.AppServices.Services.Patients
{
    internal class PatientAppService : AppServiceBase,  IPatientAppService
    {
        private readonly Lazy<IMapper> mapper;

        public PatientAppService(IServiceScopeFactory scopeFactory, 
                                 ILogger<PatientAppService> logger,
                                 Lazy<IMapper> mapper) : base(scopeFactory, logger)
        {
            this.mapper = mapper;
        }

        public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync()
        {
            return ExecuteScopedAsync<IReadOnlyList<PatientModel>>(async scope =>
            {
                var repo = scope.GetRequiredService<IPatientRepository>();               

                var list = await repo.GetList();
                return list.Select(s=>mapper.Value.Map<PatientModel>(s)).ToList();
            });
        }
             

        public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take)
        {
            return ExecuteScopedAsync<IReadOnlyList<PatientModel>>(async scope =>
            {
                var repo = scope.GetRequiredService<IPatientRepository>();                
                var list = await repo.GetList(count: take, skip: skip);

                return list.Select(s => mapper.Value.Map<PatientModel>(s)).ToList();
            });
        }

        public Task<ActionResponse> UpsertPatientsBatchAsync(IEnumerable<PatientModel> patientModels)
        {
            return ExecuteScopedAsync(async scope =>
            {
                var repo = scope.GetRequiredService<IPatientRepository>();
                var entityList = patientModels.Select(s => mapper.Value.Map<Patient>(s)).ToList();
                await repo.UpsertBatchAsync(entityList);
            });
        }
    }
}
