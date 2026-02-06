using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfDataStorage.Entities;

public class ServiceSettingsEntity : IEntity
{
    public int Id { get; set; } 

    public string SendingSchedule { get; set; } = string.Empty;
    public string ReceivingSchedule { get; set; } = string.Empty;

    public string OutboxFolder { get; set; } = string.Empty;
    public string InboxFolder { get; set; } = string.Empty;
}
