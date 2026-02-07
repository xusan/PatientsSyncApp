using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts;

public interface ISyncService
{
    Task<ActionResponse> ImportPatientsFromCsvAsync(string inboxDir);
    Task<ActionResponse> ExportPatientsToCsvAsync(string outboxDir);
}
