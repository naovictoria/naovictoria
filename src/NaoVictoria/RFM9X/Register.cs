using System;
using System.Collections.Generic;
using System.Text;

namespace RFM9X
{
    [Flags]
    public enum ModemConfig2Flag
    {
        SF_6 = 0b0110_0000,
        SF_7 = 0b0111_0000,
        SF_8 = 0b1000_0000,
        SF_9 = 0b1001_0000,
        ENABLE_CRC = 0b0000_0100,
    };

    [Flags]
    public enum ModemConfig1Flag
    {
        CR_5 = 0b0000_0010,
        CR_6 = 0b0000_0100,
        CR_7 = 0b0000_0110,
        CR_8 = 0b0000_1000,
        BW_7800 = 0b0000_0000,
        BW_10400 = 0b0001_0000,
        BW_15600 = 0b0010_0000,
        BW_20800 = 0b0011_0000,
        BW_31250 = 0b0100_0000,
        BW_41700 = 0b0101_0000,
        BW_62500 = 0b0110_0000,
        BW_125000 = 0b0111_0000,
        BW_250000 = 0b1000_0000
    };

    [Flags]
    public enum OperationModeFlag
    {
        SLEEP = 0b0000_0000,
        STANDBY = 0b0000_0001,
        FS_TX = 0b0000_0010,
        TX = 0b0000_0011,
        FS_RX = 0b0000_0100,
        RX = 0b0000_0101
    }

    /// <summary>
    /// All the documented registers for the MPU99250
    /// </summary>
    internal enum Register
    {
        FIFO = 0x00,
        OP_MODE = 0x01,
        FRF_MSB = 0x06,
        FRF_MID = 0x07,
        FRF_LSB = 0x08,
        PA_CONFIG = 0x09,
        PA_RAMP = 0x0a,
        OCP = 0x0b,
        LNA = 0x0c,
        FIFO_ADDR_PTR = 0x0d,
        FIFO_TX_BASE_ADDR = 0x0e,
        FIFO_RX_BASE_ADDR = 0x0f,
        FIFO_RX_CURRENT_ADDR = 0x10,
        IRQ_FLAGS_MASK = 0x11,
        IRQ_FLAGS = 0x12,
        RX_NB_BYTES = 0x13,
        RX_HEADER_CNT_VALUE_MSB = 0x14,
        RX_HEADER_CNT_VALUE_LSB = 0x15,
        RX_PACKET_CNT_VALUE_MSB = 0x16,
        RX_PACKET_CNT_VALUE_LSB = 0x17,
        MODEM_STAT = 0x18,
        PKT_SNR_VALUE = 0x19,
        PKT_RSSI_VALUE = 0x1a,
        RSSI_VALUE = 0x1b,
        HOP_CHANNEL = 0x1c,
        MODEM_CONFIG1 = 0x1d,
        MODEM_CONFIG2 = 0x1e,
        SYMB_TIMEOUT_LSB = 0x1f,
        PREAMBLE_MSB = 0x20,
        PREAMBLE_LSB = 0x21,
        PAYLOAD_LENGTH = 0x22,
        MAX_PAYLOAD_LENGTH = 0x23,
        HOP_PERIOD = 0x24,
        FIFO_RX_BYTE_ADDR = 0x25,
        MODEM_CONFIG3 = 0x26,
        DIO_MAPPING1 = 0x40,
        DIO_MAPPING2 = 0x41,
        VERSION = 0x42,
        TCXO = 0x4b,
        PA_DAC = 0x4d,
        FORMER_TEMP = 0x5b,
        AGC_REF = 0x61,
        AGC_THRESH1 = 0x62,
        AGC_THRESH2 = 0x63,
        AGC_THRESH3 = 0x64,
        DETECTION_OPTIMIZE = 0x31,
        DETECTION_THRESHOLD = 0x37
    }
}
