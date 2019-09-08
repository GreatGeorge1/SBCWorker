using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class FingerCommandEventArgs
    {
        public readonly byte[] Finger;

        public FingerCommandEventArgs(byte[] finger)
        {
            Finger = finger;
           
        }
    }
}
