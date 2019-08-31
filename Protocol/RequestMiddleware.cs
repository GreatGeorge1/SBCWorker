using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Protocol
{
    public static class RequestMiddleware
    {
        public static bool Process(byte[] message, out ExecutedMethod executedMethod, out byte checksum, out MessageType mtype)
        {
            executedMethod = new ExecutedMethod();
            message=message.TakeWhile(x => !(x.Equals(0x0D))).ToArray();
            if (IsValid(message))
            {
               // message = message.SkipLast(n).ToArray();
                if(IsValidType(message, out mtype))
                {
                    CommandHeader command;
                    if(IsValidCommand(message, out command))
                    {
                        Method methodInfo;
                        Protocol.GetMethods().TryGetValue(command, out methodInfo);
                        executedMethod.MethodInfo = methodInfo;
                        if(IsValidCheckSum(message, out checksum))
                        {
                            var value = message.Skip(4).Take(message[3]).ToArray();
                            var strValue = Encoding.ASCII.GetString(value);
                            executedMethod.CommandValue = strValue;
                            return true;
                        } 
                    }
                }
            }
            mtype = MessageType.NULL;
            checksum = new byte();
            return false;
        }
        public static bool IsValid(byte[] message)
        {
            int k = 0;

            Debug.Assert(message[0] == 0x02);
            Debug.Assert(message[message.Length - 1] == 0x03);

            if (message[0] ==0x02)
            {
                k++;
            }

            if (message[message.Length-1] == 0x03)
            {
                k++;
            }

            if (k == 2)
            {
                return true;
            }
            return false;
        }

        public static bool IsValidType(byte[] message, out MessageType mtype)
        {
            var temp = message[1];
           // Debug.Assert(message[1] == 0xd5);
            mtype = MessageType.NULL;
            switch (temp)
            {
                case 0xD5:
                    mtype = MessageType.REQ;
                    return true;
                case 0xD6:
                    mtype = MessageType.RES;
                    return true;
                case 0xD7:
                    mtype = MessageType.ACK;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsValidCommand(byte[] message, out CommandHeader command)
        {
            var temp = message[2];
           // Debug.Assert(message[2] == 0xC7);
            command = CommandHeader.NotSet;
            switch (temp)
            {
                case (byte)0xC7:
                    command = CommandHeader.Card;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsValidCheckSum(byte[] message, out byte newChecksum)
        {
            var temp = message[3];
            var bytes = message.Skip(4).Take(temp).ToArray();
            var checksum = message.Skip(4 + (int)temp).SkipLast(1).First();
            //Debug.Assert(checksum == 0x0B);
            Console.WriteLine(checksum.ToString());
            newChecksum = CalCheckSum(bytes, bytes.Length);
            Console.WriteLine(newChecksum.ToString());
            Debug.Assert(checksum == newChecksum);
            if (checksum == newChecksum)
            {
                return true;
            }
            return false;

        }

        public static byte CalCheckSum(byte[] _PacketData, int PacketLength)
        {
            Byte _CheckSumByte = 0x00;
            for (int i = 0; i < PacketLength; i++)
                _CheckSumByte ^= _PacketData[i];

            return _CheckSumByte;
        }
    }

    public enum MessageType
    {
        NULL,
        REQ,
        RES,
        ACK
    }
}
