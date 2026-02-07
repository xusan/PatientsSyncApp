using TaskApp.AppServices.Common;

namespace TaskApp.AppServices.Services.Sync;

public interface ISyncService
{
    Task<ActionResponse> ImportPatientsFromCsvAsync(string inboxDir);
    Task<ActionResponse> ExportPatientsToCsvAsync(string outboxDir);
}
