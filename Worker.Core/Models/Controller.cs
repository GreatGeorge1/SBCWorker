using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public class Controller : SoftEntity<long>, IExternal
    {
        public string Alias { get; set; }
        public string SecretKey { get; set; }
        public DateTime LastSync { get; set; }
        public ICollection<Terminal> Terminals { get; set; }
        public string ExternalId { get; set; }
        public string Version { get; set; }
        public ControllerConfig Config { get; set; }
    }
}
