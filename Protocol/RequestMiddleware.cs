﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public static class RequestMiddleware
    {
        public static Message Process(byte[] message)
        {
            if(message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (IsValid(message))
            {
                if (IsValidType(message, out MessageType mtype))
                {
                    if (IsValidCommand(message, out CommandHeader command))
                    {
                        Protocol.Data.GetMethods().TryGetValue(command, out Method methodInfo);
                        if (IsValidCheckSum(message, out _))
                        {
                            var value = message
                                .Skip(6)
                                .Take(
                                    HighLowToInt(message.ElementAt(4),message.ElementAt(5))
                                )
                                .ToArray();
                            //  var strValue = Encoding.ASCII.GetString(value);
                            // resMsg = new Message(methodInfo, value, mtype);
                            return new Message(methodInfo, value, mtype);
                        }
                    }
                }
            }
            Message resMsg = new Message(new Method(),Array.Empty<byte>(), MessageType.NotSet, false);
            Console.WriteLine($"rm:not valid message, bytes:'{BitConverter.ToString(message)}'");
            return resMsg;
        }
        
        private static bool IsValid(byte[] message)
        {
            int k = 0;
            if (message[0] ==0x02)
            {
                k++;
            }
            if (k == 1)
            {
                return true;
            }
            return false;
        }

        private static bool IsValidType(byte[] message, out MessageType mtype)
        {
            var temp = message[1];
            mtype = MessageType.NotSet;
            foreach (var item in Enum.GetValues(typeof(MessageType)))
            {
                if (temp == Convert.ToInt32(item))
                {
                    mtype = (MessageType)item;
                    return true;
                }
            }
            return false;
        }

        private static bool IsValidCommand(byte[] message, out CommandHeader command)
        {
            var temp = message[2];
            command = CommandHeader.NotSet;
            foreach(var item in Enum.GetValues(typeof(CommandHeader)))
            {
                if (temp == Convert.ToInt32(item))
                {
                    command =(CommandHeader)item;
                    return true;
                }
            }
            return false;
        }

        private static bool IsValidCheckSum(byte[] message, out byte newChecksum)
        {
            if(message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            try
            {
                var temp = HighLowToInt(message[4], message[5]);
                var bytes = message.Skip(6).Take(temp).ToArray();
                var checksum = message[3];
                newChecksum = CalCheckSum(bytes, bytes.Length);
                if (checksum == newChecksum)
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine($"Not valid message length (IsValidChecksum): {e.ToString()}");
            }
            newChecksum = 0x00; 
            return false;
        }

        public static byte CalCheckSum(byte[] PacketData, int PacketLength)
        {
            if (PacketData is null)
            {
                throw new ArgumentNullException(nameof(PacketData));
            }
            Byte _CheckSumByte = 0x00;
            for (int i = 0; i < PacketLength; i++)
                _CheckSumByte ^= PacketData[i];

            return _CheckSumByte;
        }
        /// <summary>
        /// byte[] length 2
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] IntToHighLow(int input)
        {
            int test = input;
            byte low = (byte)(test & 0xff);
            byte high = (byte)((test >> 8) & 0xff);
           // int res = low | (high << 8);
            return new byte[] { high, low };
        }

        public static int HighLowToInt(byte high, byte low)
        {
            int res = low | (high << 8);
            return res;
        }
    }

    public class Message
    {
        public bool IsValid { get; set; } = true;
        public Method Method { get; private set; }
        public byte[] Value { get; private set; }
        public MessageType Type { get; private set; }
        public Message(Method method, byte[] value, MessageType mtype, bool valid = true)
        {
            Method = method;
            Value = value;
            Type = mtype;
            IsValid = valid;
        }
    }

    public enum MessageType
    {
        NotSet = 0,
        REQ = 0xD5,
        RES = 0xD6,
        ACK = 0xD7,
        NACK = 0xD8
    }
} 
