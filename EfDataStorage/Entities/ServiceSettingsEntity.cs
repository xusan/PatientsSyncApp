using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfDataStorage.Entities;

public class ServiceSettingsEntity : IEntity
{
    public int Id { get; set; }

    public string ImportFolder { get; set; } = string.Empty;
    public string ImportSchedule { get; set; } = string.Empty;
    public string ExportFolder { get; set; } = string.Empty;
    public string ExportSchedule { get; set; } = string.Empty;
    public bool IsPaused { get; set; }
}
