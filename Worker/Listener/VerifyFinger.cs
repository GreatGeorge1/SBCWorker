using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Worker.Host
{
    public partial class Listener
    {
        private async Task<bool> VerifyFinger(byte[] finger)
        {
            return true;
        }
    }
}
