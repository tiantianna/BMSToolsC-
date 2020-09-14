using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace bms.startup.SDK
{
    public class CANSDK
    {
        public struct VCI_BOARD_INFO
        {
            public UInt16 hw_Version;
            public UInt16 fw_Version;
            public UInt16 dr_Version;
            public UInt16 in_Version;
            public UInt16 irq_Num;
            public byte can_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Reserved;
        }

        public struct VCI_CAN_OBJ
        {
            public UInt32 ID;
            public UInt32 TimeStamp;
            public byte TimeFlag;
            public byte SendType;
            public byte RemoteFlag;
            public byte ExternFlag;
            public byte DataLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;

            public void Init()
            {
                Data = new byte[8];
                Reserved = new byte[3];
            }
        }

        public struct VCI_CAN_STATUS
        {
            public byte ErrInterrupt;
            public byte regMode;
            public byte regStatus;
            public byte regALCapture;
            public byte regECCapture;
            public byte regEWLimit;
            public byte regRECounter;
            public byte regTECounter;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Reserved;
        }

        public struct VCI_ERR_INFO
        {
            public UInt32 ErrCode;
            public byte Passive_ErrData1;
            public byte Passive_ErrData2;
            public byte Passive_ErrData3;
            public byte ArLost_ErrData;
        }

        public struct VCI_INIT_CONFIG
        {
            public UInt32 AccCode;
            public UInt32 AccMask;
            public UInt32 Reserved;
            public byte Filter;
            public byte Timing0;
            public byte Timing1;
            public byte Mode;
        }

        public struct CHGDESIPANDPORT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] szpwd;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] szdesip;
            public Int32 desport;

            public void Init()
            {
                szpwd = new byte[10];
                szdesip = new byte[20];
            }
        }


        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        public static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        public static UInt32 m_devtype = 4;
        public static UInt32 m_bOpen = 0;
        public static UInt32 m_canind = 0;
        public static UInt32 m_devind = 0;
        public static UInt32 m_reserved = 0;
        public static VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[50];
        public static UInt32[] m_arrdevtype = new UInt32[20];

        public static UInt32 m_editcode = 0x00000000;
        public static UInt32 m_editmask = 0xffffffff;
        public static UInt32 m_filtertype = 1;
        public static UInt32 m_mode = 0;
        public static UInt32 m_timing0 = 0x01;
        public static UInt32 m_timing1 = 0x1c;
        public static VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
        public static VCI_CAN_OBJ recobj = new VCI_CAN_OBJ();

        public static void Caninit(byte time0 = 0x01, byte time1 = 0x1c)
        {
            config.AccCode = 0;
            config.AccMask = 0xffffffff;
            config.Timing0 = time0;
            config.Timing1 = time1;
            config.Filter = 1;
            config.Mode = 0;
            //uint r = VCI_InitCAN(4, 0, 0, ref config);
        }

    }
}
