using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskApp.AppServices.Common;
using TaskApp.AppServices.Dto;
using TaskApp.Domain.Patients;
using TaskApp.Domain.Sync;
using static System.Formats.Asn1.AsnWriter;

namespace TaskApp.AppServices.Services.Sync;

public class SyncService : AppServiceBase, ISyncService
{
    private readonly Lazy<ICsvService> cSVReader;
    private readonly Lazy<Mapper> mapper;

    public SyncService(IServiceScopeFactory scopeFactory,
                       ILogger<SyncService> logger,
                       Lazy<ICsvService> cSVReader,                       
                       Lazy<Mapper> mapper) : base(scopeFactory, logger)
    {        
        this.cSVReader = cSVReader;        
        this.mapper = mapper;
    }

    public Task<ActionResponse> ImportPatientsFromCsvAsync(string inboxDir)
    {
        return ExecuteScopedAsync(async scope =>
        {
            var repo = scope.GetRequiredService<IPatientRepository>();

            if (!Directory.Exists(inboxDir))
            {
                throw new DirectoryNotFoundException($"Import directory does not exist inboxDir={inboxDir}");
            }

            var files = Directory.GetFiles(inboxDir, "*.csv");
            const int batchSize = 100;

            foreach (var file in files)
            {
                var records = await cSVReader.Value.ReadAsync<PatientModel>(file);
                var batch = new List<PatientModel>();

                foreach (var model in records)
                {
                    batch.Add(model);

                    if (batch.Count >= batchSize)
                    {
                        var entities = batch.Select(s => mapper.Value.Map<Patient>(s));
                        await repo.UpsertBatchAsync(entities);

                        batch.Clear();
                    }
                }

                if (batch.Any())
                {
                    var entities = batch.Select(s => mapper.Value.Map<Patient>(s));
                    await repo.UpsertBatchAsync(entities);
                }
            }
        });
        
    }

    public Task<ActionResponse> ExportPatientsToCsvAsync(string outboxDir)
    {
        return ExecuteScopedAsync(async scope =>
        {
            

            if (!Directory.Exists(outboxDir))
                throw new DirectoryNotFoundException(outboxDir);

            string fileName = $"export_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            string fullPath = Path.Combine(outboxDir, fileName);

            var patients = GetPatientsAsync(scope);
            await cSVReader.Value.WriteAsync(fullPath, patients);
        });
    }


    private async IAsyncEnumerable<PatientModel> GetPatientsAsync(IServiceProvider scope)//TODO: cancelation [EnumeratorCancellation] CancellationToken ct = default)
    {
        var repo = scope.GetRequiredService<IPatientRepository>();

        int skip = 0;
        const int pageSize = 100;

        while (true)
        {
            var list = await repo.GetList(skip, pageSize);
           

            if (list.Count <= 0)
                yield break;

            foreach (var entity in list)
            {
                if (entity is null) continue;

                yield return mapper.Value.Map<PatientModel>(entity);
            }

            skip += pageSize;
        }
    }
}
