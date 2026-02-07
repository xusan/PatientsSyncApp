using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Cronos;
using Microsoft.Extensions.Hosting.WindowsServices;
using Services;
using System.ServiceProcess;

namespace WorkerService;

public class Worker : BackgroundService
{    
    private readonly ISettingsService settingsService;    
    private readonly ISyncService syncService;
    private readonly ILogger<Worker> logger;

    public Worker(ISettingsService settingsService, ISyncService syncService, ILogger<Worker> logger)
    {        
        this.settingsService = settingsService;        
        this.syncService = syncService;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Patient Sync Service Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Fetch settings (AppService handles the DB scope)
            var settingsResult = await settingsService.GetAsync();

            if (settingsResult.Success)
            {
                var settings = settingsResult.Result;

                // 2. Check Global Pause Flag
                if (settings.IsPaused)
                {
                    logger.LogInformation("Service is PAUSED via settings.");
                }
                else
                {
                    // 3. Check Cron for Inbound
                    if (IsTimeToRun(settings.ImportSchedule, "Import task"))
                    {
                        logger.LogInformation("Running Import...");
                        await syncService.ImportPatientsFromCsvAsync(settings.ImportFolder);
                    }

                    // 4. Check Cron for Outbound
                    if (IsTimeToRun(settings.ExportSchedule, "Export task"))
                    {
                        logger.LogInformation("Running Export...");
                        await syncService.ExportPatientsToCsvAsync(settings.ExportFolder);
                    }
                }
            }
            else
            {
                logger.LogError("Could not retrieve settings: {Error}", settingsResult.Msg);
            }

            // Wait 1 minute for next cron check
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private bool IsTimeToRun(string cronExpression, string taskName)
    {
        try
        {            
            var cron = CronExpression.Parse(cronExpression);
            var lastMinute = DateTime.UtcNow.AddMinutes(-1.1);
            var occurrence = cron.GetNextOccurrence(lastMinute);

            // If the scheduled time is within the last 60 seconds, return true
            return occurrence.HasValue &&
                   occurrence.Value >= DateTime.UtcNow.AddMinutes(-1) &&
                   occurrence.Value <= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Invalid Cron for {taskName}");
        }

        return false;
    }
}
