using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApp.Domain.Patients;

namespace TaskApp.Infrastructures.Repository
{
    internal class PatientRepository : EfRepository<Patient>, IPatientRepository
    {
        public PatientRepository(AppDbContext db) : base(db)
        {
        }
    }
}
