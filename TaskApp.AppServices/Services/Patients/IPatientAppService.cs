using TaskApp.AppServices.Common;
using TaskApp.AppServices.Dto;

namespace TaskApp.AppServices.Services.Patients
{
    public interface IPatientAppService
    {
        Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync();
        Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take);
        Task<ActionResponse> UpsertPatientsBatchAsync(IEnumerable<PatientModel> patientModels);
    }
}
