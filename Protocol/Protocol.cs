﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Protocol
{
    public static class Protocol
    {
        public static Dictionary<CommandHeader, Method> GetMethods() => new Dictionary<CommandHeader, Method>{
            {CommandHeader.Card, new Method{
                CommandHeader=CommandHeader.Card,
                ResponseHeaders=new List<ResponseHeader>{ ResponseHeader.CardOk,ResponseHeader.FingerError},
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                DirectionTo=Direction.Controller,
                HasCheckSum=true} },
            {CommandHeader.Ble,new Method{
                CommandHeader=CommandHeader.Ble,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.BleOk,ResponseHeader.BleError},
                HasCommandValue=true,
                HasResponseValue=false,
                HasResponseHeader=true,
                DirectionTo=Direction.Controller,
                HasCheckSum=true} },
            {CommandHeader.Finger,new Method{
                CommandHeader=CommandHeader.Finger,
                ResponseHeaders=new List<ResponseHeader>{ ResponseHeader.FingerOk, ResponseHeader.FingerError},
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                DirectionTo=Direction.Controller,
                HasCheckSum=true} },
            {CommandHeader.FingerTimeout,new Method{
                CommandHeader=CommandHeader.FingerTimeout,
                HasResponseHeader=false,
                HasResponseValue=false,
                HasCommandValue=false,
                DirectionTo=Direction.Controller,
                HasCheckSum=true} },
            {CommandHeader.Error,new Method{
                CommandHeader=CommandHeader.Error,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                HasCheckSum=true,
                DirectionTo=Direction.Controller,} },
            {CommandHeader.Ota,new Method{
                CommandHeader=CommandHeader.Ota,
                HasResponseHeader=false,
                HasResponseValue=true,
                HasCommandValue=true,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true,} },
            {CommandHeader.WifiInit,new Method{
                CommandHeader=CommandHeader.WifiInit,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.WifiOk, ResponseHeader.WifiError},
                HasCheckSum =true } },
            {CommandHeader.WifiSpots, new Method{
                CommandHeader=CommandHeader.WifiSpots,
                HasResponseHeader=false,
                HasResponseValue=true,
                HasCommandValue=false,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.FingerWriteInBase, new Method{
                CommandHeader=CommandHeader.FingerWriteInBase,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.FingerOk,ResponseHeader.FingerFail,
                    ResponseHeader.FingerFull,ResponseHeader.FingerExist },
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.FingerSetTimeout,new Method{
                CommandHeader=CommandHeader.FingerSetTimeout,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.FingerOk,ResponseHeader.FingerFail},
                HasCheckSum=true} },
            {CommandHeader.FingerTimeoutCurrent, new Method{
                CommandHeader=CommandHeader.FingerTimeoutCurrent,
                HasCommandValue=false,
                HasResponseHeader=false,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.FingerDeleteId, new Method{
                CommandHeader=CommandHeader.FingerDeleteId,
                HasCommandValue=true,
                HasResponseHeader=true,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.FingerOk,ResponseHeader.FingerFail},
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.FingerDeleteAll, new Method{
                CommandHeader=CommandHeader.FingerDeleteAll,
                HasCommandValue=false,
                HasResponseHeader=true,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.FingerOk,ResponseHeader.FingerFail },
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.TerminalConf,new Method{
                CommandHeader=CommandHeader.TerminalConf,
                HasCommandValue=true,
                HasResponseHeader=true,
                ResponseHeaders=new List<ResponseHeader>{ResponseHeader.TerminalOk,ResponseHeader.TerminalFail},
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.TerminalReset,new Method{
                CommandHeader=CommandHeader.TerminalReset,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                DirectionTo=Direction.Terminal,
                HasCheckSum=true} },
            {CommandHeader.Start,new Method{
                CommandHeader=CommandHeader.Start,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                DirectionTo=Direction.Controller,
                HasCheckSum=true} }
        };
        public static CommandHeader GetCommandHeader(string command)
        {
            var enums = (CommandHeader[])Enum.GetValues(typeof(CommandHeader));
            foreach (var com in enums)
            {
                if (command.Equals(com.GetDisplayName()))
                {
                    return com;
                }
            }
            throw new CommandHeaderNotFoundException($"command :{command} not found");
        }

        public static ResponseHeader GetResponseHeader(string response)
        {
            var enums = (ResponseHeader[])Enum.GetValues(typeof(ResponseHeader));
            foreach (var com in enums)
            {
                if (response.Equals(com.GetDisplayName()))
                {
                    return com;
                }
            }
            throw new ResponseHeaderNotFoundException($"response :{response} not found");
        }

        public static string CreateQuery(CommandHeader command, string value)
        {
            var com = command.GetDisplayName();
            return $"{com}\r\n{value}\r\n";
        }

        public static string CreateCommand(CommandHeader command)
        {
            return $"{command.GetDisplayName()}\r\n";
        }

        public static string CreateResponse(ResponseHeader command)
        {
            return $"{command.GetDisplayName()}\r\n";
        }


        public static bool CheckReadyTerminalHosted(ExecutedMethod executedMethod)
        {
            bool ready = true;
            if (!executedMethod.MethodInfo.DirectionTo)
            {
                if (executedMethod.MethodInfo.HasCommandValue)
                {
                    if (string.IsNullOrWhiteSpace(executedMethod.CommandValue))
                    {
                        ready = false;
                    }
                }
                if (executedMethod.MethodInfo.HasCheckSum)
                {
                    if (string.IsNullOrWhiteSpace(executedMethod.Hash))
                    {
                        ready = false;
                    }
                }
            }
            return ready;
        }

        public static bool CheckReadyControllerHosted(ExecutedMethod executedMethod)
        {
            bool ready = true;
            //return ready;
            if (executedMethod.MethodInfo.DirectionTo)
            {
                if (executedMethod.MethodInfo.HasResponseHeader)
                {
                    if (executedMethod.ResponseHeader == ResponseHeader.NotSet)
                    {
                        ready = false;
                    }
                }
                if (executedMethod.MethodInfo.HasResponseValue)
                {
                    if (string.IsNullOrWhiteSpace(executedMethod.ResponseValue))
                    {//////////////////////////////
                       // ready = true;/////////////////////
                        ready = false;
                      ///////////////////////////
                    }
                }
            }
            return ready;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
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

        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
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
    }
    public class CommandHeaderNotFoundException : Exception
    {
        public CommandHeaderNotFoundException(string message) : base(message)
        {
        }

        public CommandHeaderNotFoundException()
        {
        }

        public CommandHeaderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ResponseHeaderNotFoundException : Exception
    {
        public ResponseHeaderNotFoundException(string message) : base(message)
        {
        }

        public ResponseHeaderNotFoundException()
        {
        }

        public ResponseHeaderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

    public delegate void RepeatCountReachedLimitEventHandler(object sender, RepeatCountReachedLimitArgs e);

    public class RepeatCountReachedLimitArgs
    {
        public RepeatCountReachedLimitArgs(int count, int limit)
        {
            Count = count;
            Limit = limit;
        }

        public int Count { get; set; }
        public int Limit { get; set; }
    }
}