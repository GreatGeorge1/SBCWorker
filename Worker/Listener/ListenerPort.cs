using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host
{
    public class ListenerPort
    {
        public readonly string PortName;
        public readonly bool IsRS485;

        public ListenerPort(string port, bool isRS485=false)
        {
            PortName = port;
            IsRS485 = isRS485;
        }
    }
}
