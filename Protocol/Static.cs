using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Protocol
{
    public static class Static
    {
        public static Dictionary<CommandHeader, Method> GetMethods() => new Dictionary<CommandHeader, Method>{
            {CommandHeader.Card, new Method{
                CommandHeader=CommandHeader.Card,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Controller} },
            {CommandHeader.Ble,new Method{
                CommandHeader=CommandHeader.Ble,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Controller} },
            {CommandHeader.Finger,new Method{
                CommandHeader=CommandHeader.Finger,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Controller} },
            {CommandHeader.Error,new Method{
                CommandHeader=CommandHeader.Error,
                HasCommandValue=true,
                HasResponseValue=false,
                DirectionTo=Direction.Controller,} },
            {CommandHeader.FingerWriteInBase, new Method{
                CommandHeader=CommandHeader.FingerWriteInBase,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} },
            {CommandHeader.FingerSetTimeout,new Method{
                CommandHeader=CommandHeader.FingerSetTimeout,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} },
            {CommandHeader.FingerTimeoutCurrent, new Method{
                CommandHeader=CommandHeader.FingerTimeoutCurrent,
                HasCommandValue=false,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} },
            {CommandHeader.FingerDeleteId, new Method{
                CommandHeader=CommandHeader.FingerDeleteId,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} },
            {CommandHeader.FingerDeleteAll, new Method{
                CommandHeader=CommandHeader.FingerDeleteAll,
                HasCommandValue=false,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} },
            {CommandHeader.TerminalConf,new Method{
                CommandHeader=CommandHeader.TerminalConf,
                HasCommandValue=true,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal} }
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
