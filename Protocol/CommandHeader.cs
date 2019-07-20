using System;
using System.ComponentModel.DataAnnotations;

namespace Protocol
{
    public enum CommandHeader
    {
        [Display(Name = "CARD")]
        Card = 0,
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
        [Display(Name = "TERMINAL_MODE")]
        TerminalMode,
        [Display(Name = "TERMINAL_RESET")]
        TerminalReset,
        [Display(Name = "START")]
        Start,
        [Display(Name = "NOTSET")]
        NotSet//fix
    }
}
