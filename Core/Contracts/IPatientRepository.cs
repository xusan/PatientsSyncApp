using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts;

public interface IPatientRepository 
{
    public Task<ActionResultResponse<IReadOnlyList<PatientModel>>> GetAllAsync();
    public Task<ActionResponse> InsertAsync(IReadOnlyList<PatientModel> patients);
    public Task<ActionResponse> UpdateAsync(IReadOnlyList<PatientModel> patients);
}
