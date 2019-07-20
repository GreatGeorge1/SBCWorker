using System;
using System.ComponentModel.DataAnnotations;

namespace Protocol
{
    public enum ResponseHeader
    {
        [Display(Name = "CARD_OK")]
        CardOk = 0,
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
}
