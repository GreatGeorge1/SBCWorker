using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class GetConfigEventArgs
    {

        public  byte[] Json { get; private set; }
        public  string Address { get; private set; }

        public GetConfigEventArgs(byte[] json, string address)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }
}
