using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Services;

public interface ISyncService
{
    Task<ActionResponse> ImportPatientsFromCsvAsync(string importDir);
    Task<ActionResponse> ExportPatientsToCsvAsync(string exportDir);
}
