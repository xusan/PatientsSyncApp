using Core.Contracts.Repositories;
using Core.Models;
using EfDataStorage;
using EfDataStorage.Repositories;

namespace Services;

public class PatientsRepository : Repository<PatientModel>, IPatientsRepository
{     
    public PatientsRepository(AppDbContext context) : base(context)
    {        
    }       
}
