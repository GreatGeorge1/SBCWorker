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
        private async Task<bool> VerifyFinger(string finger, string md5)
        {
            logger.LogInformation($"Finger secret recieved {finger}");
            logger.LogInformation($"Finger md5 recieved {md5}");


            var cardf = new string(finger.Where(c => !char.IsControl(c)).ToArray());
            logger.LogInformation($"cardf byte length {Encoding.UTF8.GetBytes(cardf).Length}");

            var md5f = new string(md5.Where(c => !char.IsControl(c)).ToArray());

            using (MD5 md5Hash = MD5.Create())
            {
                //string hash = GetMd5Hash(md5Hash, cardf);

                //Console.WriteLine("The MD5 hash of " + cardf + " is: " + hash + ".");

                logger.LogInformation("Verifying the hash...");
                // Console.WriteLine($"New hash: {hash}");

                if (Protocol.Protocol.VerifyMd5Hash(md5Hash, cardf, md5f))
                {
                    logger.LogInformation("The hashes are the same.");
                    return true;
                }
                else
                {
                    logger.LogInformation("The hashes are not same.");
                    return false;
                }
            }
        }
    }
}
