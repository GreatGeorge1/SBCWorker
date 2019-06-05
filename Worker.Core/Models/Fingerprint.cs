using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public class Fingerprint: SoftEntity<long>
    {
        public Employer Employer { get; set; }
        public long EmployerId { get; set; }
        public string Hash { get; set; }
        public string InTerminalId { get; set; }
        public Terminal Terminal { get; set; }
    }
}
