using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApp.Domain.Patients;
using TaskApp.Domain.Settings;

namespace TaskApp.Infrastructures.Repository
{
    internal class SettingsRepository : EfRepository<ServiceSetting>, ISettingsRepository
    {
        public SettingsRepository(AppDbContext db) : base(db)
        {
        }
    }
}
