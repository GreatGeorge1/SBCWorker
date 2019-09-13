using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class BleCommandEventArgs
    {
        byte[] Ble;
        public BleCommandEventArgs(byte[] ble)
        {
            Ble = ble;
        }
    }
}
