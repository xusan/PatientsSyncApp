using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models;

public class ServiceSettingsModel
{
    public int Id { get; set; }

    public string ImportFolder { get; set; }
    public string ImportSchedule { get; set; }
    public string ExportFolder { get; set; }
    public string ExportSchedule { get; set; }
    public bool IsPaused { get; set; }
}
