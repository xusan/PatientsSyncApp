using Core.Contracts;
using Cronos;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.ServiceProcess;

namespace WorkerService;

public class Worker : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<Worker> logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Patient Sync Service Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var settingsRepo = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
                    var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                    
                    var settingsRes = await settingsRepo.GetAsync();

                    if (settingsRes.Success)
                    {
                        var settings = settingsRes.Result;
                        // 2. Check if WPF App set the Pause flag
                        if (settings.IsPaused)
                        {
                            logger.LogInformation("Service is PAUSED via database settings. Skipping...");
                        }
                        else
                        {
                            // 3. Check Cron for Inbound (Receiving)
                            if (IsTimeToRun(settings.ImportSchedule, "Import task"))
                            {
                                logger.LogInformation("Triggering Inbound Sync...");
                                await syncService.ImportPatientsFromCsvAsync(settings.ImportFolder);
                            }

                            // 4. Check Cron for Outbound (Sending)
                            if (IsTimeToRun(settings.ExportSchedule, "Export task"))
                            {
                                logger.LogInformation("Triggering Outbound Export...");
                                await syncService.ExportPatientsToCsvAsync(settings.ExportFolder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during sync cycle.");
            }

            // Wait 1 minute (Standard Cron resolution)
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
