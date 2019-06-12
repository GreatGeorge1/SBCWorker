using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Display(Name = "FINGER")]
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
        Start,
        [Display(Name = "NOTSET")]
        NotSet//fix
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
        TerminalFail,
        [Display(Name = "NOTSET")]
        NotSet
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
        public virtual List<ProtocolResponse> ResponseHeaders { get; set; }
        public bool HasResponseHeader { get; set; }
        public bool HasResponseValue { get; set; }
        public bool HasCommandValue { get; set; }
        public bool IsHashable { get; set; }
        public bool IsControllerHosted { get; set; }
    }

    public class ExecutedMethod:INotifyPropertyChanged
    {
        public ProtocolMethod MethodInfo { get; set; }
        private string commandValue;
        public string CommandValue
        {
            get {return commandValue; }
            set {
                commandValue = value;
                OnPropertyChanged("CommandValue");}
        }
        private ProtocolResponse responseHeader;
        public ProtocolResponse ResponseHeader
        {
            get { return responseHeader; }
            set {
                responseHeader = value;
                OnPropertyChanged("ResponseHeader");}
        }
        private string responseValue;
        public string ResponseValue
        {
            get { return this.responseValue; }
            set {
                responseValue = value;
                OnPropertyChanged("ResponseValue");}
        }
        private string hash;
        public string Hash
        {
            get { return hash; }
            set {
                hash = value;
                OnPropertyChanged("Hash"); }
        }
        private bool isFired;
        public bool IsFired
        {
            get { return isFired; }
            set { isFired = value; OnPropertyChanged("IsFired"); }
        }
        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; OnPropertyChanged("IsCompleted"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public string CreateResponse()
        {
            var command = this.ResponseHeader;
            return $"{command.GetDisplayName()}\r\n";
        }
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
        public static ProtocolCommands GetCommandHeader(string command)
        {
            var enums = (ProtocolCommands[])Enum.GetValues(typeof(ProtocolCommands));
            foreach (var com in enums)
            {
                if (command.Equals(com.GetDisplayName()))
                {
                    return com;
                }
            }
            throw new CommandHeaderNotFoundException($"command :{command} not found");
        }

        public static ProtocolResponse GetResponseHeader(string response)
        {
            var enums = (ProtocolResponse[])Enum.GetValues(typeof(ProtocolResponse));
            foreach (var com in enums)
            {
                if (response.Equals(com.GetDisplayName()))
                {
                    return com;
                }
            }
            throw new ResponseHeaderNotFoundException($"response :{response} not found");
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

        public static string CreateResponse(ProtocolResponse command)
        {
            return $"{command.GetDisplayName()}\r\n";
        }


        public static bool CheckReadyTerminalHosted(ExecutedMethod executedMethod)
        {
            bool ready = true;
            if (!executedMethod.MethodInfo.IsControllerHosted)
            {
                if(executedMethod.MethodInfo.HasCommandValue)
                {
                    if (string.IsNullOrWhiteSpace(executedMethod.CommandValue))
                    {
                        ready = false;
                    }
                }
                if (executedMethod.MethodInfo.IsHashable)
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
            if (executedMethod.MethodInfo.IsControllerHosted)
            {
                if (executedMethod.MethodInfo.HasResponseHeader)
                {
                    if (executedMethod.ResponseHeader == ProtocolResponse.NotSet)
                    {
                        ready = false;
                    }
                }
                if (executedMethod.MethodInfo.HasResponseValue)
                {
                    if (string.IsNullOrWhiteSpace(executedMethod.ResponseValue))
                    {
                        ready = false;
                    }
                }
            }
            return ready;
        }
    }

    public class CommandHeaderNotFoundException : Exception
    {
        public CommandHeaderNotFoundException(string message) : base(message)
        {
        }
    }

    public class ResponseHeaderNotFoundException : Exception
    {
        public ResponseHeaderNotFoundException(string message): base(message)
        {
        }
    }
}
