using Core.Contracts;

namespace Core.Models;

public class ServiceSettingsModel : IEntity
{
    public int Id { get; set; }

    public string ImportFolder { get; set; }
    public string ImportSchedule { get; set; }
    public string ExportFolder { get; set; }
    public string ExportSchedule { get; set; }
    public bool IsPaused { get; set; }
}
