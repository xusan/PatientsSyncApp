using Core.Common;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Services;

public interface IPatientsService
{
    Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync();
    Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetPatientsPageAsync(int skip, int take);
    Task<ActionResponse> UpsertPatientsBatchAsync(IEnumerable<PatientModel> patientModels);
}
