using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    static class FunCode
    {
        //实时信息
        public const int SHOWINFO = 0;//显示信息
        public const int SLAVEID = 1;//从机ID
        public const int REALINFO = 2;//实时总信息
        public const int VOLINFO = 3;//实时单体电压信息
        public const int TEMPINFO = 4;//实时电池模块温度信息
        public const int SIGNALINFO = 5;//实时信号信号线连接状态
        public const int BALANCESTATUS = 6;//电池均衡状态
        public const int REALINFO2 = 7;//实时总信息2
        public const int REALINFO3 = 8;//实时总信息3
        public const int TEMPINFO2 = 9;//实时电池模块温度信息2
        //配置信息
        public const int CONFIG1 = 10;//配置信息1
        public const int CONFIG2 = 11;//配置信息1
        public const int TRACECODE1 = 12;//追溯码1
        public const int TRACECODE2 = 13;//追溯码2
        public const int TRACECODE3 = 14;//追溯码3

        //UDS
        public const int SHOWINFOFORUDS = 15;//显示信息
        public const int SHOWUDSLOG = 16;//显示UDSLog
        public const int SENDFC = 17;//发送流控帧（FC）
        public const int ERROR = 18;//报错
        public const int CHANGEHEARTTIME = 19;//更改心跳发送基础时间
    }
}
