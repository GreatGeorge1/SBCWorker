using System;
using System.ComponentModel.DataAnnotations;

namespace Protocol
{
    public enum Error
    {
        [Display(Name = "0x01")]
        Uart = 0,
        [Display(Name = "0x02")]
        Ble,
        [Display(Name = "0x03")]
        System,
        [Display(Name = "0x04")]
        Ota,
        [Display(Name = "0x05")]
        Wifi,
        [Display(Name = "0x06")]
        WifiSsid,
        [Display(Name = "0x07")]
        Timeout,
        [Display(Name = "0x08")]
        UserExist,
        [Display(Name = "0x09")]
        FingerExist,
        [Display(Name = "0x0A")]
        Fingerprint,
        [Display(Name = "0x0B")]
        DbFull,
        [Display(Name = "0x0C")]
        Terminal
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
