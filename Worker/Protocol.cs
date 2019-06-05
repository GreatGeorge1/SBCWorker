using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Worker.Host
{
    public enum ProtocolCommands
    {
        [Display(Name = "CARD")]
        Card=0,
        [Display(Name = "BLE")]
        Ble,
        [Display(Name = "BLE")]
        Finger,
        [Display(Name = "FINGER_TIMEOUT")]
        FingerTimeout,
        [Display(Name = "ERROR")]
        Error,
        [Display(Name = "OTA")]
        Ota,
        [Display(Name = "WiFi_INIT")]
        WifiInit,
        [Display(Name = "WiFI_SPOTS")]
        WifiSpots,
        [Display(Name = "FINGER_WRITE_IN_BASE")]
        FingerWriteInBase,
        [Display(Name = "FINGER_SET_TIMEOUT")]
        FingerSetTimeout,
        [Display(Name = "FINGER_TIMEOUT_CURRENT")]
        FingerTimeoutCurrent,
        [Display(Name = "FINGER_DELETE_ID")]
        FingerDeleteId,
        [Display(Name = "FINGER_DELETE_ALL")]
        FingerDeleteAll,
        [Display(Name = "TERMINAL_CONF")]
        TerminalConf,
        [Display(Name = "TERMINAL_RESET")]
        TerminalReset,
        [Display(Name = "START")]
        Start
    }

    public enum ProtocolResponse
    {
        [Display(Name = "CARD_OK")]
        CardOk=0,
        [Display(Name = "CARD_ERR")]
        CardError,
        [Display(Name = "BLE_OK")]
        BleOk,
        [Display(Name = "BLE_ERR")]
        BleError,
        [Display(Name = "FINGER_OK")]
        FingerOk,
        [Display(Name = "FINGER_ERR")]
        FingerError,
        [Display(Name = "FINGER_FAIL")]
        FingerFail,
        [Display(Name = "FINGER_FULL")]
        FingerFull,
        [Display(Name = "FINGER_EXIST")]
        FingerExist,
        [Display(Name = "WiFi_OK")]
        WifiOk,
        [Display(Name = "0x05")]
        WifiError,
        [Display(Name = "TERMINAL_OK")]
        TerminalOk,
        [Display(Name = "TERMINAL_FAIL")]
        TerminalFail
    }

    public enum ProtocolErrors
    {
        [Display(Name = "0x01")]
        Uart=0,
        [Display(Name = "0x02")]
        Ble,
        [Display(Name = "0x03")]
        System,
        [Display(Name = "0x04")]
        Ota,
        [Display(Name = "0x05")]
        Wifi,
        [Display(Name = "0x06")]
        WifiSsid
    }

    public class ProtocolMethod
    {
        public ProtocolCommands CommandHeader { get; set; }
        public List<ProtocolResponse> ResponseHeaders { get; set; }
        public bool HasResponseHeader { get; set; }
        public bool HasResponseValue { get; set; }
        public bool HasCommandValue { get; set; }
        public bool IsHashable { get; set; }
        public bool IsControllerHosted { get; set; }
    }

    public class ExecutedMethod
    {
        public ProtocolMethod MethodInfo { get; set; }
        public string CommandValue { get; set; }
        public string ResponseValue { get; set; }
        public string Hash { get; set; }
    }

    public class Protocol
    {
        public static Dictionary<ProtocolCommands, ProtocolMethod> Methods {get;}=new Dictionary<ProtocolCommands, ProtocolMethod>{
            {ProtocolCommands.Card, new ProtocolMethod{
                CommandHeader=ProtocolCommands.Card,
                ResponseHeaders=new List<ProtocolResponse>{ ProtocolResponse.CardOk,ProtocolResponse.FingerError},
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                IsControllerHosted=false,
                IsHashable=true} },
            {ProtocolCommands.Ble,new ProtocolMethod{
                CommandHeader=ProtocolCommands.Ble,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.BleOk,ProtocolResponse.BleError},
                HasCommandValue=true,
                HasResponseValue=false,
                HasResponseHeader=true,
                IsControllerHosted=false,
                IsHashable=true} },
            {ProtocolCommands.Finger,new ProtocolMethod{
                CommandHeader=ProtocolCommands.Finger,
                ResponseHeaders=new List<ProtocolResponse>{ ProtocolResponse.FingerOk, ProtocolResponse.FingerError},
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                IsControllerHosted=false,
                IsHashable=true} },
            {ProtocolCommands.FingerTimeout,new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerTimeout,
                HasResponseHeader=false,
                HasResponseValue=false,
                HasCommandValue=false,
                IsControllerHosted=false,
                IsHashable=false} },
            {ProtocolCommands.Error,new ProtocolMethod{
                CommandHeader=ProtocolCommands.Error,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                IsHashable=false,
                IsControllerHosted=false,} },
            {ProtocolCommands.Ota,new ProtocolMethod{
                CommandHeader=ProtocolCommands.Ota,
                HasResponseHeader=false,
                HasResponseValue=true,
                HasCommandValue=true,
                IsControllerHosted=true,
                IsHashable=true,} },
            {ProtocolCommands.WifiInit,new ProtocolMethod{
                CommandHeader=ProtocolCommands.WifiInit,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                IsControllerHosted=true,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.WifiOk, ProtocolResponse.WifiError},
                IsHashable =false } },
            {ProtocolCommands.WifiSpots, new ProtocolMethod{
                CommandHeader=ProtocolCommands.WifiSpots,
                HasResponseHeader=false,
                HasResponseValue=true,
                HasCommandValue=false,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.FingerWriteInBase, new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerWriteInBase,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.FingerOk,ProtocolResponse.FingerFail,
                    ProtocolResponse.FingerFull,ProtocolResponse.FingerExist },
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.FingerSetTimeout,new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerSetTimeout,
                HasCommandValue=true,
                HasResponseHeader=true,
                HasResponseValue=false,
                IsControllerHosted=true,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.FingerOk,ProtocolResponse.FingerFail},
                IsHashable=false} },
            {ProtocolCommands.FingerTimeoutCurrent, new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerTimeoutCurrent,
                HasCommandValue=false,
                HasResponseHeader=false,
                HasResponseValue=true,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.FingerDeleteId, new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerDeleteId,
                HasCommandValue=true,
                HasResponseHeader=true,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.FingerOk,ProtocolResponse.FingerFail},
                HasResponseValue=false,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.FingerDeleteAll, new ProtocolMethod{
                CommandHeader=ProtocolCommands.FingerDeleteAll,
                HasCommandValue=false,
                HasResponseHeader=true,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.FingerOk,ProtocolResponse.FingerFail },
                HasResponseValue=false,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.TerminalConf,new ProtocolMethod{
                CommandHeader=ProtocolCommands.TerminalConf,
                HasCommandValue=true,
                HasResponseHeader=true,
                ResponseHeaders=new List<ProtocolResponse>{ProtocolResponse.TerminalOk,ProtocolResponse.TerminalFail},
                HasResponseValue=false,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.TerminalReset,new ProtocolMethod{
                CommandHeader=ProtocolCommands.TerminalReset,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                IsControllerHosted=true,
                IsHashable=false} },
            {ProtocolCommands.Start,new ProtocolMethod{
                CommandHeader=ProtocolCommands.Start,
                HasCommandValue=true,
                HasResponseHeader=false,
                HasResponseValue=false,
                IsControllerHosted=false,
                IsHashable=false} }
        };
        public static ProtocolCommands GetCommand(string command)
        {
            var enums = (ProtocolCommands[])Enum.GetValues(typeof(ProtocolCommands));
            foreach (var com in enums)
            {
                if (command.Trim().Equals(com.GetDisplayName()))
                {
                    return com;
                }
            }
            throw new CommandNotFoundException($"command :{command} not found");
        }

        public static string CreateQuery(ProtocolCommands command, string value)
        {
            var com = command.GetDisplayName();
            return $"{com}\r\n{value}\r\n";
        }

        public static string CreateCommand(ProtocolCommands command)
        {
            return $"{command.GetDisplayName()}\r\n";
        }
    }

    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException(string message) : base(message)
        {
        }
    }
}
