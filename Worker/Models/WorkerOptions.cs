using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Models
{
    public class WorkerOptions
    {
        public ICollection<Port> Ports { get; set; }

    }
    public class Port
    {
        public string PortName { get; set; }
        public bool IsRS485 { get; set; }
    }
}
