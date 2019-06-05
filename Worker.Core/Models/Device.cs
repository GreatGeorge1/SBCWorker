using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public class Device : SoftEntity<long>, IExternal
    {
        public string BleUuid { get; set; }
        public Employer Employer { get; set; }
        public long EmployerId { get; set; }
        public string ExternalId { get; set; }
    }
}
