using Core.Contracts.Services;
using Cronos;

namespace WorkerService;

public class Worker : BackgroundService
{
    private readonly ISettingsService settingsService;
    private readonly ISyncService syncService;
    private readonly ILogger<Worker> logger;

    // Track the last time we checked each specific task to prevent missing windows
    private DateTime lastImportCheck;
    private DateTime lastExportCheck;

    public Worker(ISettingsService settingsService, ISyncService syncService, ILogger<Worker> logger)
    {
        this.settingsService = settingsService;
        this.syncService = syncService;
        this.logger = logger;

        // Initialize to current time so we don't sync historical data on startup
        this.lastImportCheck = DateTime.UtcNow;
        this.lastExportCheck = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Patient Sync Service Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {                
                var settingsResult = await settingsService.GetAsync();

                if (settingsResult.Success)
                {
                    var settings = settingsResult.Result;
                    var now = DateTime.UtcNow;

                    if (settings.IsPaused)
                    {
                        logger.LogInformation("Service is PAUSED via settings. Sync skipped.");

                        // Reset check times to 'now' while paused to prevent a "burst" of 
                        // catch-up syncs when the user clicks 'Resume'.
                        lastImportCheck = now;
                        lastExportCheck = now;
                    }
                    else
                    {
                        // 2. Check and Run Import Task
                        if (ShouldExecute(settings.ImportSchedule, ref lastImportCheck, "Import task"))
                        {
                            logger.LogInformation("Starting Import Process...");
                            await syncService.ImportPatientsFromCsvAsync(settings.ImportFolder);
                        }

                        // 3. Check and Run Export Task
                        if (ShouldExecute(settings.ExportSchedule, ref lastExportCheck, "Export task"))
                        {
                            logger.LogInformation("Starting Export Process...");
                            await syncService.ExportPatientsToCsvAsync(settings.ExportFolder);
                        }
                    }
                }
                else
                {
                    logger.LogError("Could not retrieve settings: {Error}", settingsResult.Error?.Message);
                }
            }
            catch (Exception ex)
            {
                // Ensures the worker loop continues even if a specific sync cycle fails
                logger.LogError(ex, "An unhandled error occurred in the Worker heartbeat loop.");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    /// <summary>
    /// Determines if a task should run based on its Cron expression and the last check time.
    /// Uses a "Jump to Now" strategy to prevent multiple catch-up runs after long delays.
    /// </summary>
    private bool ShouldExecute(string cronExpression, ref DateTime lastCheck, string taskName)
    {
        try
        {
            var now = DateTime.UtcNow;
            var cron = CronExpression.Parse(cronExpression);

            // Look for the next occurrence strictly AFTER our last successful check
            var occurrence = cron.GetNextOccurrence(lastCheck);

            // If a scheduled time exists and it is in the past (or exactly now)
            if (occurrence.HasValue && occurrence.Value <= now)
            {
                logger.LogInformation("{Task}: Sync triggered. Scheduled time {Time} reached.",
                    taskName, occurrence.Value.ToLocalTime());

                // IMPORTANT: We move lastCheck to 'now' rather than the 'occurrence' time.
                // This ensures that if we missed multiple cycles (e.g. 30 min delay), 
                // we only run ONCE and then wait for the next future schedule.
                lastCheck = now;
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Cron expression for {TaskName}: {Cron}", taskName, cronExpression);
        }

        return false;
    }
}
