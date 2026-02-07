using Core.Common;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;

public class SyncService : ISyncService
{
    private readonly IPatientsService patientsService;
    private readonly ILogger<SyncService> logger;

    public SyncService(IPatientsService patientsService, ILogger<SyncService> logger)
    {
        this.patientsService = patientsService;
        this.logger = logger;
    }

    public async Task<ActionResponse> ImportPatientsFromCsvAsync(string inboxDir)
    {
        var res = new ActionResponse();

        try
        {           
            var files = Directory.GetFiles(inboxDir, "*.csv");
            const int batchSize = 100;

            foreach (var file in files)
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<PatientModel>();
                var batch = new List<PatientModel>();

                foreach (var model in records)
                {
                    batch.Add(model);

                    if (batch.Count >= batchSize)
                    {
                        await patientsService.UpsertPatientsBatchAsync(batch);
                        batch.Clear();
                    }
                }

                if (batch.Any())
                {
                    await patientsService.UpsertPatientsBatchAsync(batch);
                }
            }

            res.Success = true;
        }
        catch (Exception ex)
        {            
            logger.LogError(ex, "Error processing inbound files");
            res.Success = false;
            res.Msg = ex.Message;
        }

        return res;
    }

    public async Task<ActionResponse> ExportPatientsToCsvAsync(string outboxDir)
    {
        var res = new ActionResponse();

        try
        {           
            string fileName = $"export_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            string fullPath = Path.Combine(outboxDir, fileName);

            using (var writer = new StreamWriter(fullPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<PatientModel>();
                await csv.NextRecordAsync();

                int skip = 0;
                const int pageSize = 100;

                while (true)
                {                    
                    var patientsRes = await patientsService.GetPatientsPageAsync(skip, pageSize);
                    if (patientsRes.Success)
                    {
                        var patients = patientsRes.Result;                        
                        if (patients.Count == 0)
                        {
                            break;
                        }

                        // Write this batch to the CSV
                        csv.WriteRecords(patients);
                        await writer.FlushAsync();

                        skip += pageSize;
                    }
                    else
                    {
                        res.Msg = patientsRes.Msg;
                        return res;
                    }
                }

                res.Success = true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting patients to outbox");
            res.Success = false;
            res.Msg = ex.Message;
        }

        return res;
    }
}
