using System;
using System.ComponentModel.DataAnnotations;

namespace Protocol
{
    public enum CommandHeader
    {
        [Display(Name = "CARD")]
        Card = 0xC7,
        [Display(Name = "BLE")]
        Ble = 0xB7,
        [Display(Name = "FINGER")]
        Finger = 0xF7,
        [Display(Name = "ERROR")]
        Error = 0xFF,
        [Display(Name = "FINGER_WRITE_IN_BASE")]
        FingerWriteInBase = 0xF6,
        [Display(Name = "FINGER_SET_TIMEOUT")]
        FingerSetTimeout = 0xF3,
        [Display(Name = "FINGER_TIMEOUT_CURRENT")]
        FingerTimeoutCurrent = 0xF2,
        [Display(Name = "FINGER_DELETE_ID")]
        FingerDeleteId = 0xF5,
        [Display(Name = "FINGER_DELETE_ALL")]
        FingerDeleteAll = 0xF4,
        [Display(Name = "TERMINAL_CONF")]
        TerminalConf = 0xA1,
        [Display(Name = "SendConfig")]
        TerminalGetConf = 0xA2,
        [Display(Name = "GetConfig")]
        TerminalSysInfo = 0xA3,
        [Display(Name = "NotSet")]
        NotSet = 0
    }
}
