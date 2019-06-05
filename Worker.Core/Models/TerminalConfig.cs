using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public class TerminalConfig : SoftEntity<long>, IExternal
    {
        public int FingetTimeoutMs { get; set; }
        public string WifiConf { get; set; }
        public TerminalMode Mode { get; set; }
        public string ViewName { get; set; }
        public string ExternalId { get; set; }
        public long CoreId { get; set; }
        public Terminal Core { get; set; }
    }

    public enum TerminalMode
    {
        Default=0
    }
}
