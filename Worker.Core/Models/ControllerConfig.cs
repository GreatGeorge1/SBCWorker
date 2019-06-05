using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public class ControllerConfig : SoftEntity<long>, IExternal
    {
        public long CoreId { get; set; }
        public SyncPolicy SyncPolicy { get; set; }
        public DateTime SyncTime { get; set; }
        public Controller Core { get; set; }
        public string ExternalId { get; set; }
    }
    public enum SyncPolicy
    {
        Default=0
    }
}
