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
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);
            Console.WriteLine($"New hash: {hashOfInput}");

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> VerifyCard(string card, string md5)
        {
            //string card = await ReadMessage();
           // string md5 = await ReadMessage();

            string etalon = "5300A3950E6B";
            //var byte1 = Encoding.UTF8.GetBytes(card);
            //var byte2 = Encoding.UTF8.GetBytes(etalon);
            //logger.LogInformation($"Card full hex {BitConverter.ToString(byte1)}");

            //logger.LogInformation($"etalon byte length {byte2.Length}");
            //logger.LogInformation($"card byte length {byte1.Length}");

            //if (byte1 == byte2)
            //{
            //    logger.LogInformation($"Card eq etalon");
            //}
            //else
            //{
            //    logger.LogInformation($"Card !eq etalon");
            //}
            //if (string.IsNullOrEmpty(card))
            //{
            //    logger.LogInformation($"Card is null {card}");
            //    return;
            //}
            logger.LogInformation($"Card secret recieved {card}");
            logger.LogInformation($"Card md5 recieved {md5}");


            var cardf = new string(card.Where(c => !char.IsControl(c)).ToArray());
            logger.LogInformation($"cardf byte length {Encoding.UTF8.GetBytes(cardf).Length}");

            var md5f = new string(md5.Where(c => !char.IsControl(c)).ToArray());

            using (MD5 md5Hash = MD5.Create())
            {
                //string hash = GetMd5Hash(md5Hash, cardf);

                //Console.WriteLine("The MD5 hash of " + cardf + " is: " + hash + ".");

                logger.LogInformation("Verifying the hash...");
                // Console.WriteLine($"New hash: {hash}");

                if (VerifyMd5Hash(md5Hash, cardf, md5f))
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

            //try
            //{
            //    var cardres = context.Cards.Where(c => c.SpecialNumber == cardf).AsEnumerable();
            //    if (cardres.Any())
            //    {
            //        foreach (var item in cardres)
            //        {
            //            logger.LogInformation($"Card authorized {item.SpecialNumber}");
            //            for (int i = 0; i < 3; i++)
            //            {
            //                try
            //                {
            //                    await WriteMessage(Encoding.UTF8.GetBytes(terminalCommands.Dictionary[terminalEnum.CardOk]));
            //                    break;
            //                }
            //                catch (Exception e)
            //                {
            //                    logger.LogWarning(e.ToString());
            //                    Thread.Sleep(2000);
            //                }
            //                i++;
            //            }

            //        }
            //        // ListenerState = State.Ready;
            //    }
            //    else
            //    {
            //        logger.LogInformation($"Card not found {cardres.ToString()}");
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.LogInformation($"{e.ToString()}");
            //}
            //ListenerState = State.Ready;
        }
    }
}

