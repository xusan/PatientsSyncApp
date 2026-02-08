using Core.Common;

namespace Core.Contracts.Services;

public interface ISyncService
{
    Task<ActionResponse> ImportPatientsFromCsvAsync(string importDir);
    Task<ActionResponse> ExportPatientsToCsvAsync(string exportDir);
}
