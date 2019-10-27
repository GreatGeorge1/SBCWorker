using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace Worker.Host
{
    public partial class Listener<QueueT>
    {
        private async Task<bool> VerifyCard(byte[] card)
        {
            return true;
        }
    }
}

