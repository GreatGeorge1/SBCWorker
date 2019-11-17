using System;
using System.ComponentModel.DataAnnotations;

namespace Protocol
{
    public enum TerminalError
    {
        [Display(Name = "0x01")]
        Uart = 0x01,
        [Display(Name = "0x02")]
        Ble = 0x02,
        [Display(Name = "0x03")]
        System = 0x03,
        [Display(Name = "0x07")]
        Timeout = 0x07,
        [Display(Name = "0x08")]
        UserExist =0x08,
        [Display(Name = "0x09")]
        FingerExist = 0x09,
        [Display(Name = "0x0A")]
        Fingerprint = 0x0A,
        [Display(Name = "0x0B")]
        DbFull = 0x0B,
        [Display(Name = "0x0C")]
        Terminal = 0x0C,
        FingerPrintScanner = 0x0E,
        NotSet = 0
    }

    public class ProtocolException : Exception {}
    public class UartException : ProtocolException { }
    public class BleException : ProtocolException { }
    public class SystemException : ProtocolException { }
    public class OtaException : ProtocolException { }
    public class WifiException : ProtocolException { }
    public class WifiSsidException : ProtocolException { }
    public class TimeoutException : ProtocolException { }
    public class UserExistException : ProtocolException { }
    public class FingerExistException : ProtocolException { }
    public class FingerprintException : ProtocolException { }
    public class DbFullException : ProtocolException { }
    public class TerminalException : ProtocolException { }
}
