using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Repositories;

public interface IPatientsRepository 
{
    Task<IReadOnlyList<PatientModel>> GetAllAsync();
    Task<IReadOnlyList<PatientModel>> GetPageAsync(int skip, int take);
    Task<int> UpsertBatchAsync(IEnumerable<PatientModel> patientModels);
}
