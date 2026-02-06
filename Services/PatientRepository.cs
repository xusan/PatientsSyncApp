using Core.Contracts;
using Core.Models;
using EfDataStorage;

namespace Services;

public class PatientRepository : IPatientRepository
{
    private readonly Lazy<AppDbContext> database;

    public PatientRepository(Lazy<AppDbContext> database)
    {
        this.database = database;
    }

    public Task<ActionResultResponse<List<PatientModel>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ActionResponse> InsertAsync(IList<PatientModel> patients)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResponse> UpdateAsync(IList<PatientModel> patients)
    {
        throw new NotImplementedException();
    }
}
