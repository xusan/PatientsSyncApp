using Core.Common;
using Core.Models;

namespace Core.Contracts.Services;

public interface IPatientsService
{
    Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync();
    Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take);
    Task<ActionResponse> UpsertPatientsBatchAsync(IReadOnlyList<PatientModel> patients);
}
