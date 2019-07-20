using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class FingerCommandEventArgs
    {
        public readonly string Finger;
        public readonly string Md5Hash;

        public FingerCommandEventArgs(string finger, string md5Hash)
        {
            Finger = finger;
            Md5Hash = md5Hash;
        }
    }
}
