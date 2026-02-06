using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts;

public interface IPatientRepository 
{
    public Task<ActionResultResponse<List<PatientModel>>> GetAllAsync();
    public Task<ActionResponse> InsertAsync(IList<PatientModel> patients);
    public Task<ActionResponse> UpdateAsync(IList<PatientModel> patients);
}
