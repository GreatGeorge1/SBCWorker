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
                DirectionTo=Direction.Terminal} },
            {CommandHeader.TerminalGetConf, new Method
            {
                CommandHeader=CommandHeader.TerminalGetConf,
                HasCommandValue=false,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal
            } },
            {CommandHeader.TerminalSysInfo,new Method
            {
                CommandHeader=CommandHeader.TerminalSysInfo,
                HasCommandValue=false,
                HasResponseValue=true,
                DirectionTo=Direction.Terminal
            } }
        };
     
   

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
