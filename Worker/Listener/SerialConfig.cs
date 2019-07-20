using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host
{
    public class SerialConfig
    {
        public readonly string PortName;
        public readonly bool IsRS485;

        public SerialConfig(string port, bool isRS485=false)
        {
            PortName = port;
            IsRS485 = isRS485;
        }
    }
}
