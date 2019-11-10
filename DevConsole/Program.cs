using ConsoleTableExt;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;

namespace DevConsole
{
    class Program
    {
        static readonly ILogger log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        static async Task Main(string[] args)
        {
            var list = new List<ProtocolMethodView>();
            foreach(var item in Protocol.Data.GetMethods())
            {
                list.Add(ProtocolMethodView.MapProtocolMethod(item.Value));
            }
            ConsoleTableBuilder.From(list).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();

            Protocol.ConcurrentMessageBag<string> cqueue = new Protocol.ConcurrentMessageBag<string>();
            cqueue.EnqueueEvent += EnqueueAction;
            cqueue.Enqueue("item");
            cqueue.Enqueue("item2");
           // Console.WriteLine(cqueue.Count);
            log.Information("Count {@Cqueue}", cqueue);

            //byte[] message = 
            //    { 0x02, //start
            //    0xD5, //type
            //    0xC7, //comand
            //    0x08, //data length
            //    0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
            //    //4546344235464335423644 324234 443531 423530 334330 373633 453044 363245 hash
            //    0x45,0x46, 0x34,0x42,0x35,0x46,0x43,0x35,0x42,0x36,0x44,0x32,0x42,0x34,0x44,0x35,0x31,0x42,0x35,0x30,0x33,0x43,0x30,0x37,0x36,0x33,0x45,0x30,0x44,0x36,0x32,0x45,
            //    0x03 }; //end
            byte[] message =
               { 
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x04,
                0x03 
            }; //end

            string msg = "02-D5-C7-08-30-30-41-33-39-35-30-45-0B-03\r\n";

            msg = new string(msg.Where(c => !char.IsControl(c)).ToArray());
            string[] hexValuesSplit = msg.Split('-');
            List<byte> list2 = new List<byte>();
            foreach (string hex in hexValuesSplit)
            {
                int value = Convert.ToInt32(hex, 16);
                list2.Add((byte)value);
            }
            var arr = list2.ToArray();
            Debug.Assert(arr[0]==0x02);
            Debug.Assert(arr[arr.Length - 1] == 0x03);
            var StringByte = BitConverter.ToString(arr);

            Console.WriteLine(StringByte);

            Message message1;
            byte checksum;

            Console.WriteLine(Encoding.Default.GetString(arr));
            var res =RequestMiddleware.Process(arr);

            log.Information($"res {res.ToString()}");
            var cardbytes = message.Skip(4).Take(8).ToArray();

            var ts = new TestTransport();

        }
        public static void EnqueueAction(object sender, MessageQueueEnqueueEventArgs<string> e)
        {
            if (!String.IsNullOrWhiteSpace(e.Item))
            {
                Console.WriteLine($"Enqueue:{e.Item}");
            }
            else
            {
                Console.WriteLine("EnqueueAction error");
            }
        }


        public static byte CalCheckSum(byte[] _PacketData, int PacketLength)
        {
            Byte _CheckSumByte = 0x00;
            for (int i = 0; i < PacketLength; i++)
                _CheckSumByte ^= _PacketData[i];
            return _CheckSumByte;
        }


        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

    }



    class ProtocolMethodView : Protocol.Method
    {
        public static ProtocolMethodView MapProtocolMethod(Protocol.Method input)
        {
            var res = new ProtocolMethodView
            {
                CommandHeader=input.CommandHeader,
                HasCommandValue=input.HasCommandValue,
                HasResponseValue=input.HasResponseValue,
                DirectionTo=input.DirectionTo,
            };
            return res;
        }
    }

}
