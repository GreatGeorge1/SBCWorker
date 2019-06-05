using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host
{
    public static class PortHelpers
    {
        public static bool PortNameExists(string name)
        {
            string[] ports = GetPortNames();
            foreach (var port in ports)
            {
                if (port == name)
                {
                    return true;
                }
            }
            return false;
        }
        public static string[] GetPortNames()
        {
            int p = (int)Environment.OSVersion.Platform;
            List<string> serial_ports = new List<string>();

            // Are we on Unix?
            if (p == 4 || p == 128 || p == 6)
            {
                string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty*");
                foreach (string dev in ttys)
                {
                    if (dev.StartsWith("/dev/ttyS") || dev.StartsWith("/dev/ttyUSB") || dev.StartsWith("/dev/ttyACM") || dev.StartsWith("/dev/ttyAMA"))
                    {
                        serial_ports.Add(dev);
                        Console.WriteLine("Serial list: {0}", dev);
                    }
                }
            }
            return serial_ports.ToArray();
        }
    }
}
