using bms.startup.Model;
using bms.startup.SDK;
using bms.startup.util;
using FirstFloor.ModernUI.Windows.Controls;
using slaveUpperComputer.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace bms.startup.service
{
    class Gen2Service
    {
        private Dictionary<string, int> isWaitting = new Dictionary<string, int>();//判断是否正在等待接收回送数据包,0否1是，2数据重传
        //正则
        private const string SUMINFO = "0C0041[0123456789ABCDEF][0123456789ABCDEF]";//总信息
        private const string CELLVOL = "180[123456789ABCDEF]41[0123456789ABCDEF][0123456789ABCDEF]";//单体电压
        private const string TEMINFO = "181041[0123456789ABCDEF][0123456789ABCDEF]";//电池模块温度
        private const string SINGNALINFO = "181441[0123456789ABCDEF][0123456789ABCDEF]";//信号采集线连接状态
        private const string BALANCESTATUS = "181541[0123456789ABCDEF][0123456789ABCDEF]";//电池均衡状态
        private const string SUMINFO2 = "0C0141[0123456789ABCDEF][0123456789ABCDEF]";//总信息2
        private const string SUMINFO3 = "0C0241[0123456789ABCDEF][0123456789ABCDEF]";//总信息3
        private const string TEMINFO2 = "181841[0123456789ABCDEF][0123456789ABCDEF]";//电池模块温度2
        private const string CONFIG1 = "0C217F[0123456789ABCDEF][0123456789ABCDEF]";//读取配置信息1
        private const string CONFIG2 = "0C247F[0123456789ABCDEF][0123456789ABCDEF]";//读取配置信息2
        private const string TRACECODE1 = "0C227F[0123456789ABCDEF][0123456789ABCDEF]";//读取追溯码1
        private const string TRACECODE2 = "0C237F[0123456789ABCDEF][0123456789ABCDEF]";//读取追溯码2
        private const string TRACECODE3 = "0C257F[0123456789ABCDEF][0123456789ABCDEF]";//读取追溯码3

        public delegate void changeViewHandler(ViewEventArgs e);
        public event changeViewHandler changeViewForGen2VM;
        public class ViewEventArgs : EventArgs
        {
            private Object[] obj;
            public Object[] Obj
            {
                get { return obj; }
                set { obj = value; }
            }
            public ViewEventArgs(Object[] obj)
            {
                this.Obj = obj;
            }
        }
        public void parseDataThread(Object o)
        {
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + "start");
            CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)o;
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string id = DataConverter.byteToHexStrForId(intBuff);
            if (new Regex(SUMINFO).IsMatch(id))
            {
                //总信息
                if (isWaitting.ContainsKey("getSlaveId") && isWaitting["getSlaveId"] == 1)
                {
                    //该帧用于获取从机编号
                    isWaitting["getSlaveId"] = 0;
                    flushVM(new Object[] { FunCode.SLAVEID, id.Substring(6) });
                }
                else { 
                //该帧为实时信息
                    byte[] data=obj.Data;
                    Object[] results = new Object[11];
                    results[0] = FunCode.REALINFO + "";
                    results[1] = data[0]+"";
                    results[2] = data[1] * 0.4+"%";
                    results[3]= (data[3] << 8 | data[2])*10+"mV";
                    results[4] = ((data[5]&0x1F) << 8 | data[4]) + "mV";
                    //results[5] = (data[5] >> 5 & 0x03)+"";
                    //data[5] = 32;
                    results[5] = (data[5] >> 5 & 0x03)>1?((data[5] >> 5 & 0x03)==2?"0":"1"):((data[5] >> 5 & 0x03)==0?"2":"3");
                    results[6] = (data[5] >> 7) == 0 ? "0" : "1";
                    results[7] = ((data[7] & 0x1F) << 8 | data[6]) + "mV";
                    results[8] = (data[7] >> 5 & 0x01)==0?"0":"1";
                    results[9] = (data[7] >> 6 & 0x01) == 0 ? "0" : "1";
                    results[10] = (data[7] >> 7 & 0x01)==0?"1":"0";
                    flushVM(results);
                }
            }
            else if (new Regex(CELLVOL).IsMatch(id))
            {
                //单体电压
                byte[] data = obj.Data;
                Object[] results = new Object[6];
                results[0] = FunCode.VOLINFO + "";                
                results[1] = 4*(Convert.ToInt32(id.Substring(2,2),16)-1)+1;//电池起始编号
                results[2] = (data[1] << 8 | data[0]) + "mV";
                results[3] = (data[3] << 8 | data[2]) + "mV";
                results[4] = (data[5] << 8 | data[4]) + "mV";
                results[5] = (data[7] << 8 | data[6]) + "mV";
                flushVM(results);
            }
            else if (new Regex(TEMINFO).IsMatch(id))
            { 
                //电池模块温度
                byte[] data = obj.Data;
                Object[] results = new Object[9];
                results[0] = FunCode.TEMPINFO + "";
                results[1] = (data[0] - 40) + "℃";
                results[2] = (data[1] - 40) + "℃";
                results[3] = (data[2] - 40) + "℃";
                results[4] = (data[3] - 40) + "℃";
                results[5] = (data[4] - 40) + "℃";
                results[6] = (data[5] - 40) + "℃";
                results[7] = (data[6] - 40) + "℃";
                results[8] = data[7] + "";
                flushVM(results);
            }
            else if (new Regex(SINGNALINFO).IsMatch(id))
            {

                //信号采集线状态
                byte[] data = obj.Data;
                Object[] results = new Object[62];
                results[0] = FunCode.SIGNALINFO + "";
                for(int i=0;i<61;i++){
                    results[i + 1] = (data[i/8] >> i%8 & 0x01)==0?"0":"1";
                }
                flushVM(results);
            }
            //else if (new Regex(BALANCESTATUS).IsMatch(id))
            //{
            //    //电池均衡状态
            //    byte[] data = obj.Data;
            //    Object[] results = new Object[61];
            //    results[0] = FunCode.BALANCESTATUS + "";
            //    for (int i = 0; i < 56; i++)
            //    {
            //        results[i + 1] = (data[i / 8+1] >> i % 8 & 0x01) == 0 ? "关闭" : "开启";
            //    }
            //    results[57] = (data[0] & 0x01 )== 0 ? "关闭" : "开启";
            //    results[58] = (data[0] >> 1 & 0x01) == 0 ? "关闭" : "开启";
            //    results[59] = (data[0] >> 2 & 0x01) == 0 ? "关闭" : "开启";
            //    results[60] = (data[0] >> 3 & 0x01) == 0 ? "关闭" : "开启";
            //    flushVM(results);
            //}
            else if (new Regex(SUMINFO2).IsMatch(id))
            { 
                //总信息2
                byte[] data = obj.Data;
                Object[] results = new Object[6];
                results[0] = FunCode.REALINFO2 + "";
                results[1] = data[0] - 40 + "℃";
                results[2] = data[1] - 40 + "℃";
                results[3] = DataConverter.bytetoAscString(new byte[]{data[2]});
                results[4] = DataConverter.bytetoAscString(new byte[]{data[3]});
                results[5] = DataConverter.bytetoAscString(new byte[]{data[4]});
                flushVM(results);
            }
            else if (new Regex(SUMINFO3).IsMatch(id))
            {
                //总信息3
                byte[] data = obj.Data;
                Object[] results = new Object[9];
                results[0] = FunCode.REALINFO3 + "";
                for (int i = 0; i < 8;i++ )
                {
                    results[i+1] = DataConverter.bytetoAscString(new byte[] { data[i] });
                }
                flushVM(results);
            }
            else if (new Regex(TEMINFO2).IsMatch(id))
            { 
                //电池模块温度2
                byte[] data = obj.Data;
                Object[] results = new Object[5];
                results[0] = FunCode.TEMPINFO2 + "";
                results[1] = data[0] - 40 + "℃";
                results[2] = data[1] - 40 + "℃";
                results[3] = data[2] - 40 + "℃";
                results[4] = data[3] - 40 + "℃";
                flushVM(results);
            }
            else if (new Regex(CONFIG1).IsMatch(id))
            {
                //收到配置信息1
                byte[] data = obj.Data;
                Object[] results = new Object[7];
                results[0] = FunCode.CONFIG1 + "";
                results[1] = data[0] + "";
                results[2] = data[1].ToString("X2");
                results[3] = (data[3]<<8|data[2]) + "mV";
                results[4] = (data[5] << 8 | data[4]) + "mV";
                results[5] = data[6] - 40 + "℃";
                results[6] = data[7] - 40 + "℃";
                flushVM(results);
            }
            else if (new Regex(CONFIG2).IsMatch(id))
            {
                //收到配置信息2
                byte[] data = obj.Data;
                Object[] results = new Object[8];
                results[0] = FunCode.CONFIG2 + "";
                for (int i = 0; i < 6; i++) {
                    results[i+1] = data[i] + "";
                }
                results[7] = (data[7] << 8 | data[6]) * 0.1 + "A";    
                flushVM(results);
            }
            else if (new Regex(TRACECODE1).IsMatch(id))
            { 
                //收到追溯码1
                byte[] data = obj.Data;
                Object[] results = new Object[9];
                results[0] = FunCode.TRACECODE1 + "";
                for (int i = 0; i < 8; i++)
                {
                    results[i + 1] = DataConverter.bytetoAscString(new byte[] { data[i] });
                }
                flushVM(results);
            }
            else if (new Regex(TRACECODE2).IsMatch(id))
            {
                //收到追溯码2
                byte[] data = obj.Data;
                Object[] results = new Object[9];
                results[0] = FunCode.TRACECODE2 + "";
                for (int i = 0; i < 8; i++)
                {
                    results[i + 1] = DataConverter.bytetoAscString(new byte[] { data[i] });
                }
                flushVM(results);
            }
            else if (new Regex(TRACECODE3).IsMatch(id))
            {
                //收到追溯码3
                byte[] data = obj.Data;
                Object[] results = new Object[9];
                results[0] = FunCode.TRACECODE3 + "";
                for (int i = 0; i < 8; i++)
                {
                    results[i + 1] = DataConverter.bytetoAscString(new byte[] { data[i] });
                }
                flushVM(results);
            }
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + "end");
        }

        private void flushVM(Object[] o)
        {
            if (changeViewForGen2VM != null)
            {
                changeViewForGen2VM(new ViewEventArgs(o));
            }

        }

        //获取从机ID
        public void getSalveId()
        {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 1;//远程帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            isWaitting["getSlaveId"] = 1;
            //isWaitting["getSlaveId"] == 1为了防止还在发送循环中已经收到了回复，此时不需要继续发送
            for (int i = 0; i < 64 && isWaitting["getSlaveId"] == 1; i++)
            {
                obj.ID = uint.Parse("0C0041" + i.ToString("X2"), System.Globalization.NumberStyles.HexNumber);
               // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
            long starttime = DateTime.Now.Ticks;
            //等待回复
            while ((DateTime.Now.Ticks - starttime) / 10000 < 2000 && isWaitting["getSlaveId"] == 1)
            {
                if (isWaitting["getSlaveId"] == 0)
                {
                    break;
                }
            }
            if (isWaitting["getSlaveId"] == 1)
            {
                isWaitting["getSlaveId"] = 0;
                flushVM(new Object[] { FunCode.SHOWINFO, "getSlaveFailed" });
            }
        }

        //读取追溯码
        public void getTraceCode(Object id) {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 1;//远程帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            string slaveId = Convert.ToString(id);
            while (true)
            {
                for (int i = 2; i < 6; i++)
                {
                    if (i == 4)
                    {
                        continue;
                    }
                    obj.ID = uint.Parse("0C2" + i.ToString("X") + "7F" + slaveId, System.Globalization.NumberStyles.HexNumber);
                   // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    Thread.Sleep(20);
                }
                Thread.Sleep(200);
            }
        }

        //获取配置信息
        public void getConfig(Object id) {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 1;//远程帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            string slaveId = Convert.ToString(id);
            for (int i = 0; i < 5; i++) {
                if (i == 1 || i == 2 || i == 4) {
                    continue;
                }
                obj.ID = uint.Parse("0C2" + (i+1).ToString("X") + "7F" + slaveId, System.Globalization.NumberStyles.HexNumber);
                //Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                Thread.Sleep(20);
            }
        }

        public void startShishi(Object id)
        {
            while (true)
            {
                CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
                obj.Init();
                obj.RemoteFlag = 1;//远程帧
                obj.ExternFlag = 1;//扩展帧
                obj.SendType = 0;//正常发送
                string slaveId = Convert.ToString(id);
                for (int i = 0; i < 3; i++)
                {
                    obj.ID = uint.Parse("0C" + i.ToString("X2") + "41" + slaveId, System.Globalization.NumberStyles.HexNumber);
                   // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                     CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                     Thread.Sleep(20);
                }
                for (int i = 0; i < 17; i++)
                {
                    obj.ID = uint.Parse("18" + i.ToString("X2") + "41" + slaveId, System.Globalization.NumberStyles.HexNumber);
                   // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    Thread.Sleep(20);
                }

                for (int i = 20; i < 22; i++)
                {
                    if (i == 21) { continue; }
                    obj.ID = uint.Parse("18" + i.ToString("X2") + "41" + slaveId, System.Globalization.NumberStyles.HexNumber);
                   // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    Thread.Sleep(20);
                }

                obj.ID = uint.Parse("181841" + slaveId, System.Globalization.NumberStyles.HexNumber);
               // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                Thread.Sleep(20);
            }

        }


        //发送config1
        public void sendConfig1Pack(Object o)
        {
            ArrayList al = (ArrayList)o;
            string slaveId = Convert.ToString(al[0]);
            Gen2SlaveConfig g = (Gen2SlaveConfig)al[1];
            Gen2SlaveInfo gsi = (Gen2SlaveInfo)al[2];
            if ((g.Sid == null || g.Sid.Equals(""))&&(gsi.Sid != null && !gsi.Sid.Equals(""))) {
                g.Sid = DataConverter.string2Hex(gsi.Sid).ToString();
            }
            if ((g.Covth == null || g.Covth.Equals("")) && (gsi.Covth != null && !gsi.Covth.Equals("")))
            {
                g.Covth = gsi.Covth.Replace("mV",String.Empty);
            }
            if ((g.Cuvth == null || g.Cuvth.Equals("")) && (gsi.Cuvth != null && !gsi.Cuvth.Equals("")))
            {
                g.Cuvth = gsi.Cuvth.Replace("mV",String.Empty);
            }
            if ((g.Foth == null || g.Foth.Equals("")) && (gsi.Foth != null && !gsi.Foth.Equals("")))
            {
                g.Foth = gsi.Foth.Replace("℃",String.Empty);
            }
            if ((g.Fcth == null || g.Fcth.Equals("")) && (gsi.Fcth != null && !gsi.Fcth.Equals("")))
            {
                g.Fcth = gsi.Fcth.Replace("℃",String.Empty);
            }
            if (!(DataConverter.canStirng2int(g.Sid) && DataConverter.canStirng2int(g.Covth) && DataConverter.canStirng2int(g.Cuvth)
                && DataConverter.canStirng2int(g.Foth) && DataConverter.canStirng2int(g.Fcth)))
            {
                flushVM(new Object[] { FunCode.SHOWINFO, "wrongdata" });
                return;
            }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.ID = uint.Parse("0C21" + slaveId + "7F", System.Globalization.NumberStyles.HexNumber);
            byte[] b = new byte[8];
            b[1] = (byte)Convert.ToInt32(g.Sid);
            short d = (short)Convert.ToInt32(g.Covth);
            byte[] t = System.BitConverter.GetBytes(d);
            b[2] = t[0];
            b[3] = t[1];
            d = (short)Convert.ToInt32(g.Cuvth);
            t = System.BitConverter.GetBytes(d);
            b[4] = t[0];
            b[5] = t[1];
            b[6] = (byte)(Convert.ToInt32(g.Foth) + 40);
            b[7] = (byte)(Convert.ToInt32(g.Fcth) + 40);
            obj.Data = b;
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            flushVM(new Object[] { FunCode.SHOWINFO, "sendSuc" });
        }
        //发送config2
        public void sendConfig2Pack(Object o)
        {
            ArrayList al = (ArrayList)o;
            string slaveId = Convert.ToString(al[0]);
            Gen2SlaveConfig g = (Gen2SlaveConfig)al[1];
            Gen2SlaveInfo gsi = (Gen2SlaveInfo)al[2];
            if ((g.Bcnt_a == null || g.Bcnt_a.Equals("")) && (gsi.Bcnt_A != null && !gsi.Bcnt_A.Equals("")))
            {
                g.Bcnt_a = gsi.Bcnt_A;
            }
            if ((g.Bcnt_b == null || g.Bcnt_b.Equals("")) && (gsi.Bcnt_B != null && !gsi.Bcnt_B.Equals("")))
            {
                g.Bcnt_b = gsi.Bcnt_B;
            }
            if ((g.Bcnt_c == null || g.Bcnt_c.Equals("")) && (gsi.Bcnt_C != null && !gsi.Bcnt_C.Equals("")))
            {
                g.Bcnt_c = gsi.Bcnt_C;
            }
            if ((g.Bcnt_d == null || g.Bcnt_d.Equals("")) && (gsi.Bcnt_D != null && !gsi.Bcnt_D.Equals("")))
            {
                g.Bcnt_d = gsi.Bcnt_D;
            }
            if ((g.Bcnt_e == null || g.Bcnt_e.Equals("")) && (gsi.Bcnt_E != null && !gsi.Bcnt_E.Equals("")))
            {
                g.Bcnt_e = gsi.Bcnt_E;
            }
            if ((g.Bcnt_f == null || g.Bcnt_f.Equals("")) && (gsi.Bcnt_F != null && !gsi.Bcnt_F.Equals("")))
            {
                g.Bcnt_f = gsi.Bcnt_F;
            }
            if ((g.MaxCharge == null || g.MaxCharge.Equals("")) && (gsi.MaxChargeCur != null && !gsi.MaxChargeCur.Equals("")))
            {
                g.MaxCharge = gsi.MaxChargeCur.Replace("A", String.Empty);
            }
            if (!(DataConverter.canStirng2int(g.Bcnt_a) && DataConverter.canStirng2int(g.Bcnt_b) && DataConverter.canStirng2int(g.Bcnt_c)
                && DataConverter.canStirng2int(g.Bcnt_d) && DataConverter.canStirng2int(g.Bcnt_e) && DataConverter.canStirng2int(g.Bcnt_f) && DataConverter.canStirng2double(g.MaxCharge)))
            {
                flushVM(new Object[] { FunCode.SHOWINFO, "wrongdata" });
                return;
            }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.ID = uint.Parse("0C24" + slaveId + "7F", System.Globalization.NumberStyles.HexNumber);
            byte[] b = new byte[8];
            b[0] = (byte)Convert.ToInt32(g.Bcnt_a);
            b[1] = (byte)Convert.ToInt32(g.Bcnt_b);
            b[2] = (byte)Convert.ToInt32(g.Bcnt_c);
            b[3] = (byte)Convert.ToInt32(g.Bcnt_d);
            b[4] = (byte)Convert.ToInt32(g.Bcnt_e);
            b[5] = (byte)Convert.ToInt32(g.Bcnt_f);
            short d = (short)(Convert.ToInt32(g.MaxCharge) * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            b[6] = t[0];
            b[7] = t[1];
            obj.Data = b;
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            flushVM(new Object[] { FunCode.SHOWINFO, "sendSuc" });
        }
        //发送均衡控制指令
        public void sendBalancePack(Object o) {
            ArrayList al = (ArrayList)o;
            string slaveId = Convert.ToString(al[0]);
            Gen2SlaveConfig g = (Gen2SlaveConfig)al[1];
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.ID = uint.Parse("1815" + "41" + slaveId, System.Globalization.NumberStyles.HexNumber);
            obj.Data = g.getBalanceData();
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            flushVM(new Object[] { FunCode.SHOWINFO, "sendSuc" });
        }

        public void sendRelay2Pack(Object o) {
            ArrayList al = (ArrayList)o;
            string slaveId = Convert.ToString(al[0]);
            Gen2SlaveConfig g = (Gen2SlaveConfig)al[1];
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.ID = uint.Parse("181E" + slaveId + "41", System.Globalization.NumberStyles.HexNumber);
            //obj.Data = DataConverter.str2ASCII(g.TraceCode.Substring(0, 8));
            obj.DataLen = 8;

            byte[] b = new byte[8];
            //b[1] =(byte)( g.Realy2 == true ? 1 : 0);
            b[0] = (byte)((g.Realy2 == true ? 1 : 0 )<< 1 | 0x00);

            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            int year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int second = currentTime.Second;
            b[1] = DataConverter.strToHexByte((year - 2000).ToString())[0];
            b[2] = DataConverter.strToHexByte(month.ToString())[0];
            b[3] = DataConverter.strToHexByte(day.ToString())[0];
            b[4] = DataConverter.strToHexByte(hour.ToString())[0];
            b[5] = DataConverter.strToHexByte(minute.ToString())[0];
            b[6] = DataConverter.strToHexByte(second.ToString())[0];
            b[7] = (byte)Convert.ToInt32(g.CanLife);
            obj.Data = b;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //发送追溯码
        public void sendTraceCodePack(Object o)
        {
            ArrayList al = (ArrayList)o;
            string slaveId = Convert.ToString(al[0]);
            Gen2SlaveConfig g = (Gen2SlaveConfig)al[1];

            if (g.TraceCode==null|| g.TraceCode.Length != 24)
            {
                flushVM(new Object[] { FunCode.SHOWINFO, "wrongdata" });
                return;
            }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.ID = uint.Parse("0C22" + slaveId + "7F", System.Globalization.NumberStyles.HexNumber);
            obj.Data= DataConverter.str2ASCII(g.TraceCode.Substring(0, 8));
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Thread.Sleep(200);

            obj.ID = uint.Parse("0C23" + slaveId + "7F", System.Globalization.NumberStyles.HexNumber);
            obj.Data = DataConverter.str2ASCII(g.TraceCode.Substring(8, 8));
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Thread.Sleep(200);

            obj.ID = uint.Parse("0C25" + slaveId + "7F", System.Globalization.NumberStyles.HexNumber);
            obj.Data = DataConverter.str2ASCII(g.TraceCode.Substring(16, 8));
            obj.DataLen = 8;
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

    }
}
