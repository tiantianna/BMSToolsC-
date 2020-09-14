using bms.startup.SDK;
using bms.startup.util;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace bms.startup.service
{
    class UDSTestService
    {
        private const string UDSSEND = "000007E1";//UDS发送ID
        private const string UDSRECEIVE = "000007E9";//UDS接收ID
        private const string UDSDONTRECEIVE = "000007[01][0123456789ABCDEF]";//UDS接收ID
        private const string RECEIVECOLOR = "#333399";//回复正常数据的颜色
        private const string ERRORCOLOR = "#FF0000";//错误响应的颜色
        private const string SENDCOLOR = "#000000";//发送数据的颜色
        private const string POSITIVERESPONSE = "#993399";//接收到正响应的颜色
        private bool isDoBootLoader = false;//标志是否在做bootloader操作
        private bool isECUFun = false;//判断是否在ECU Functional阶段，如是，则需要将发送ID变更为0x7DF
        public bool IsDoBootLoader
        {
            get { return isDoBootLoader; }
            set { isDoBootLoader = value; }
        }

        private byte[] seed = null;//保存接收的seed

        public byte[] data = null;
        //private static object isWaittingLock = new object();//用于同步isWaitting的锁

        private int ff_dl = 0;//计算还需要收到多少CF的数据

        public int Ff_dl
        {
            get { return ff_dl; }
            set { ff_dl = value; }
        }
        private int sendFF_dl = 0;//计算需要发送的数据字节数（多帧）
        public int SendFF_dl
        {
            get { return sendFF_dl; }
            set { sendFF_dl = value; }
        }

        private string sendID = "7E1";
        //private string sendID = "785";

        public string SendID
        {
            get { return sendID; }
            set { sendID = value; }
        }
        private string receiveID = "7E9";
        //private string receiveID = "78D";

        public string ReceiveID
        {
            get { return receiveID; }
            set { receiveID = value; }
        }
        //保存74发送的bootloader每次发送的blocksize大小
        private int blockSize = 0;
        public int BlockSize
        {
            get { return blockSize; }
            set { blockSize = value; }
        }
        private Dictionary<string, int> isWaitting = new Dictionary<string, int>();
        //判断是否正在等待接收回送数据包
        //{"waitCF",hex}表示等待接收CF帧，其第一个字节应是hex，值为0xFE表示接收CF出现问题，值为0x00表示接收完毕
        //key="FC"标志是否收到下位机发送的FC帧，值0表示否，值1表示是
        //key="BS"表示需要上位机发送的一轮CF帧的数量，0表示全部发送
        //key="stmin"表示需要上位机发送的每个CF帧的间隔时间
        //key=服务号，则value=1表示上位机发送了服务号为key的请求，正在等待下位机响应。value=0表示不在等待或已经收到了正响应，value=2表示收到了错误响应，
        //value=2X表示等待多帧数据，2X表示下一帧应当收到的帧头

        public Dictionary<string, int> IsWaitting
        {
            get { return isWaitting; }
            set { isWaitting = value; }
        }



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
            CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)o;
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string id = DataConverter.byteToHexStrForId(intBuff);
            byte[] data = obj.Data;
            if (new Regex(UDSDONTRECEIVE).IsMatch(id))
            {
                return;
            }
            if ((SendID != "" && ReceiveID != "" && SendID != null && ReceiveID != null) && (new Regex(SendID).IsMatch(id) || new Regex(ReceiveID).IsMatch(id)))
            //if (new Regex(UDSSEND).IsMatch(id) || new Regex(UDSRECEIVE).IsMatch(id) || new Regex("07E1").IsMatch(id) || new Regex("07E9").IsMatch(id))
            {
                //更改发送心跳基础时间
                Object[] results = new Object[1];
                results[0] = FunCode.CHANGEHEARTTIME + "";
                flushVM(results);
                results = new Object[3];
                switch (data[0] >> 4 & 0x0F)
                {
                    case 0:
                        //单帧SF
                        if ((data[1] == 0x67 && data[2] == 0x01) || (data[1] == 0x67 && data[2] == 0x03) || (data[1] == 0x67 && data[2] == 0x11))
                        {
                            //接收到2701
                            if (isWaitting.ContainsKey("27") && isWaitting["27"] == 0x01)
                            {
                                seed = data.Skip(3).Take(4).ToArray();//从索引3开始截取4个
                                                                      //seed[0] = 0x01;
                                                                      //seed[1] = 0x02;
                                                                      //seed[2] = 0x03;
                                                                      //seed[3] = 0x04;
                                                                      //isWaitting["27"] = 0x00;
                                                                      // Console.WriteLine("接收到2701，"+isWaitting["27"]);
                            }
                        }

                        results[0] = FunCode.SHOWUDSLOG + "";
                        results[1] = "receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
                        if (data[1] == 0x7F && data[3] != 0x78)
                        {
                            //错误响应                            
                            results[2] = ERRORCOLOR;
                            if (isWaitting.ContainsKey(DataConverter.hex2String(data[2])) && isWaitting[DataConverter.hex2String(data[2])] == 0x01)
                            {
                                //收到了错误响应
                                isWaitting[DataConverter.hex2String(data[2])] = 0x02;
                            }
                        }
                        else
                        {
                            if (data[1] != 0x7E && isWaitting.ContainsKey(DataConverter.hex2String((byte)(data[1] - 0x40))) && isWaitting[DataConverter.hex2String((byte)(data[1] - 0x40))] == 0x01)
                            {
                                //收到正确响应
                                isWaitting[DataConverter.hex2String((byte)(data[1] - 0x40))] = 0x00;

                                results[2] = POSITIVERESPONSE;
                                if (data[1] == 0x74)
                                {
                                    //收到34的正确响应，保存每块block的数据量参数
                                    int sizeL = data[2] >> 4;
                                    blockSize = 0;
                                    for (int i = 3; i < 3 + sizeL; i++)
                                    {
                                        blockSize = (blockSize << 8) | data[i];
                                    }
                                    // Console.WriteLine("blocksize;"+blockSize);
                                }
                            }
                            else
                            {
                                results[2] = RECEIVECOLOR;
                            }
                        }

                        //屏蔽心跳帧显示
                        //if (data[1] != 0x7E)
                        //{
                        //    flushVM(results);
                        //}
                        if (isDoBootLoader)
                        {
                            Console.WriteLine("receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                        }
                        else
                        {
                            flushVM(results);
                        }

                        break;
                    case 1:
                        //首帧FF
                        dealMultiFrame(obj);
                        break;
                    case 2:
                        //CF帧
                        if (isWaitting.ContainsKey("waitCF") && isWaitting.ContainsKey(DataConverter.hex2String((byte)isWaitting["waitCF"])))
                        //if (isWaitting.ContainsKey("waitCF") && isWaitting["waitCF"] == data[0])
                        {
                            for (int i = 1; i < 8; i++)
                            {
                                if (data[i] != 0xAA)
                                {
                                    ff_dl -= 1;
                                }
                            }
                            if (ff_dl < 0)
                            {
                                //收到的数据过长，超过了请求的长度，报错
                                results = new Object[2];
                                results[0] = FunCode.ERROR + "";
                                results[1] = "receiveDataTooLong";
                                flushVM(results);
                                isWaitting.Remove("waitCF");

                                isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] = 0x01;

                                return;
                            }

                            //接收到需要的CF帧，计算还有多少数据没有发
                            if (isDoBootLoader)
                            {
                                Console.WriteLine("receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                            }
                            else
                            {
                                results = new Object[3];
                                results[0] = FunCode.SHOWUDSLOG + "";
                                results[1] = "receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
                                results[2] = RECEIVECOLOR;
                                flushVM(results);
                            }

                            //ff_dl -= 7;
                            if (ff_dl > 0)
                            {
                                isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] = isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] + 1 > 0x2F ? 0x20 : (isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] + 1);
                                // isWaitting["waitCF"] = isWaitting["waitCF"] + 1 > 0x2F ? 0x20 : (isWaitting["waitCF"] + 1);
                            }
                            else if (ff_dl == 0)
                            {
                                //数据接收完毕，将isWaitting["waitCF"]置为0x00
                                isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] = 0x00;
                                isWaitting["waitCF"] = 0x00;

                            }
                        }
                        else if (isWaitting.ContainsKey("waitCF") && isWaitting["waitCF"] != data[0])
                        {
                            //接收数据出现问题，终止接收
                            isWaitting[DataConverter.hex2String((byte)isWaitting["waitCF"])] = 0x01;
                            isWaitting["waitCF"] = 0xFE;
                        }
                        break;
                    case 3:
                        //流控帧FC
                        switch (data[0] & 0x0F)
                        {

                            //FS
                            case 0:
                                if (isDoBootLoader)
                                {
                                    Console.WriteLine("receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                                }
                                else
                                {
                                    results = new Object[3];
                                    results[0] = FunCode.SHOWUDSLOG + "";
                                    results[1] = "receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
                                    results[2] = RECEIVECOLOR;
                                    flushVM(results);
                                }

                                //Continue to send                                
                                if (isWaitting.ContainsKey("BS"))
                                {
                                    isWaitting.Remove("BS");
                                }
                                isWaitting.Add("BS", data[1]);
                                if (isWaitting.ContainsKey("stmin"))
                                {
                                    isWaitting.Remove("stmin");
                                }
                                isWaitting.Add("stmin", data[2]);
                                if (isWaitting.ContainsKey("FC"))
                                {
                                    isWaitting.Remove("FC");
                                }
                                isWaitting.Add("FC", 0x01);
                                break;
                            case 1:
                                //wait
                                break;

                            case 2:
                                //overflow
                                break;
                        }
                        break;

                }

            }
        }

        //同步更改isWaitting信号量,key表示要更改的键值，isRemove=true表示删除,remOrAdd=false表示添加或更改
        //当isRemove=true时value无效，否则value表示值
        //private void syncIswaitingChange(string key,int value,bool isRemove) {
        //    lock (isWaittingLock) {
        //        if (isRemove)
        //        {
        //            if (isWaitting.ContainsKey(key))
        //            {
        //                isWaitting.Remove(key);
        //            }
        //        }
        //        else { 
        //        if()
        //        }
        //    }
        //}

        //收到多帧，进行处理
        private void dealMultiFrame(CANSDK.VCI_CAN_OBJ obj)
        {
            byte[] data = obj.Data;

            Object[] results = new Object[3];

            results[0] = FunCode.SHOWUDSLOG + "";
            results[1] = "receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
            if (isWaitting.ContainsKey(DataConverter.hex2String((byte)(data[2] - 0x40))) && isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] == 0x01)
            {
                //收到正确响应
                results[2] = POSITIVERESPONSE;
            }
            else
            {
                results[2] = RECEIVECOLOR;
            }
            // results[2] = RECEIVECOLOR; 
            if (isDoBootLoader)
            {
                Console.WriteLine("receUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            }
            else
            {
                flushVM(results);
            }


            ff_dl = data[0] & 0x0F << 8 | data[1];
            if (ff_dl >= 6)
            {
                ff_dl -= 6;
            }
            if (ff_dl > 0)
            {
                results = new Object[1];
                results[0] = FunCode.SENDFC + "";
                flushVM(results);//发送流控帧

                if (!isWaitting.ContainsKey("waitCF"))
                {
                    //isWaitting.Add("waitCF", 0x21);

                    isWaitting.Add("waitCF", data[2] - 0x40);
                    if (isWaitting.ContainsKey(DataConverter.hex2String((byte)(data[2] - 0x40))) && isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] == 0x01)
                    {
                        isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] = 0x21;
                    }
                }
                else
                {
                    // isWaitting["waitCF"] = 0x21;
                    isWaitting["waitCF"] = data[2] - 0x40;
                    if (isWaitting.ContainsKey(DataConverter.hex2String((byte)(data[2] - 0x40))) && isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] == 0x01)
                    {
                        isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] = 0x21;
                    }
                }
            }

        }
        //等待下位机响应，sid为服务号，millisecond为最多等待时时间
        //返回值0表示收到了正确响应(标志位置为0x00),1表示没有收到响应，2表示收到错误响应(标志位为0x02)
        private int waitForResponse(Object o)
        {
            Object[] os = (Object[])o;
            string sid = Convert.ToString(os[0]);
            int millisecond = (int)os[1];
            long starttime = DateTime.Now.Ticks;

            while ((DateTime.Now.Ticks - starttime) / 10000 < millisecond)
            {
                if (isWaitting.ContainsKey(sid) && isWaitting[sid] == 0x00)
                {
                    //收到了正确响应
                    return 0;
                }
                else if (isWaitting.ContainsKey(sid) && isWaitting[sid] == 0x02)
                {
                    //收到了错误响应
                    isWaitting[sid] = 0x00;
                    //if (sid.Equals("31"))
                    //{
                    //    //忽略31服务的错误响应，使之可以继续执行，根据需要进行修改
                    //    return 0;
                    //}
                    //else
                    //{
                        return 2;
                    //}

                }
            }
            return 1;
        }

        private bool isNext = false;

        public bool IsNext
        {
            get { return isNext; }
            set { isNext = value; }
        }

        public bool IsECUFun
        {
            get
            {
                return isECUFun;
            }

            set
            {
                isECUFun = value;
            }
        }

        //发送一帧
        public void sendFrame(Object o)
        {
            byte[] data = (byte[])o;

            switch (data[0] >> 4 & 0x0F)
            {
                case 0:

                    //发送单帧，记录服务号
                    //sub-function的bit7=0时需要肯定响应
                    if (data[1] != 0x3E)
                    {
                        //除心跳帧
                        if (isWaitting.ContainsKey(DataConverter.hex2String(data[1])))
                        {
                            isWaitting[DataConverter.hex2String(data[1])] = 0x01;
                        }
                        else
                        {
                            isWaitting.Add(DataConverter.hex2String(data[1]), 0x01);
                        }
                    }
                    // new Thread(new ParameterizedThreadStart(waitForResponse)).Start(new Object[] { DataConverter.hex2String(data[1]), 1000 });
                    break;
                case 1:
                    //发送多帧，记录服务号
                    //if (data[3] >> 7 == 0x00)
                    //{
                    if (isWaitting.ContainsKey(DataConverter.hex2String(data[2])))
                    {
                        isWaitting[DataConverter.hex2String(data[2])] = 0x01;
                    }
                    else
                    {
                        isWaitting.Add(DataConverter.hex2String(data[2]), 0x01);
                    }
                    //}

                    //new Thread(new ParameterizedThreadStart(waitForResponse)).Start(new Object[] { DataConverter.hex2String(data[1]), 1000 });
                    break;
            }

            if (data[0]>>4==0&&(data[1] == 0x27 && data[2] == 0x01))
            {
                //发送2701帧，需要接收回应并发出2702帧
                if (isWaitting.ContainsKey("27"))
                {
                    isWaitting["27"] = 0x01;
                }
                else
                {
                    isWaitting.Add("27", 0x01);
                }
                send(data);
                //CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                {
                    //收到了seed，计算key
                    byte[] temp = DataConverter.calKeyFromSeedOnUDS(seed, 2);
                    byte[] b = new byte[8];
                    b[0] = 0x06;
                    b[1] = 0x27;
                    b[2] = 0x02;
                    b[3] = temp[0];
                    b[4] = temp[1];
                    b[5] = temp[2];
                    b[6] = temp[3];
                    b[7] = 0xAA;
                    if (isWaitting.ContainsKey("27"))
                    {
                        isWaitting["27"] = 0x01;
                    }
                    else
                    {
                        isWaitting.Add("27", 0x01);
                    }
                    send(b);
                    if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                    {
                        isNext = true;
                        Console.WriteLine("T3");
                        //以下为2703校验，不需要则屏蔽

                        //接着发2703
                        //b[0] = 0x02;
                        //b[1] = 0x27;
                        //b[2] = 0x03;
                        //b[3] = 0xAA;
                        //b[4] = 0xAA;
                        //b[5] = 0xAA;
                        //b[6] = 0xAA;
                        //b[7] = 0xAA;
                        //if (isWaitting.ContainsKey("27"))
                        //{
                        //    isWaitting["27"] = 0x01;
                        //}
                        //else
                        //{
                        //    isWaitting.Add("27", 0x01);
                        //}
                        //send(b);
                        //if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                        //{
                        //    temp = DataConverter.calKeyFromSeedOnUDS(seed,4);
                        //    b[0] = 0x06;
                        //    b[1] = 0x27;
                        //    b[2] = 0x04;
                        //    b[3] = temp[0];
                        //    b[4] = temp[1];
                        //    b[5] = temp[2];
                        //    b[6] = temp[3];
                        //    b[7] = 0xAA;
                        //    if (isWaitting.ContainsKey("27"))
                        //    {
                        //        isWaitting["27"] = 0x01;
                        //    }
                        //    else
                        //    {
                        //        isWaitting.Add("27", 0x01);
                        //    }
                        //    send(b);
                        //    if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                        //    {
                        //        isNext = true;
                        //    }
                        //    else
                        //    {
                        //        isNext = false;
                        //    }
                        //}
                        //else {
                        //    isNext = false;
                        //}

                        //2703到此
                    }
                    else
                    {
                        isNext = false;
                    }
                }
                else
                {
                    isNext = false;
                }
            }
            //这个是用于2703的服务，如不需要删除这个
            else if (data[0] >> 4 == 0 && data[1] == 0x27 && data[2] == 0x03)
            {
                //发送2701帧，需要接收回应并发出2702帧
                if (isWaitting.ContainsKey("27"))
                {
                    isWaitting["27"] = 0x01;
                }
                else
                {
                    isWaitting.Add("27", 0x01);
                }
                send(data);
                //CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                {
                    //收到了seed，计算key
                    byte[] temp = DataConverter.calKeyFromSeedOnUDS(seed, 4);
                    byte[] b = new byte[8];
                    b[0] = 0x06;
                    b[1] = 0x27;
                    b[2] = 0x04;
                    b[3] = temp[0];
                    b[4] = temp[1];
                    b[5] = temp[2];
                    b[6] = temp[3];
                    b[7] = 0xAA;
                    if (isWaitting.ContainsKey("27"))
                    {
                        isWaitting["27"] = 0x01;
                    }
                    else
                    {
                        isWaitting.Add("27", 0x01);
                    }
                    send(b);
                    if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                    {
                        isNext = true;
                        Console.WriteLine("T4");
                    }
                    else
                    {
                        isNext = false;
                    }
                }
                else
                {
                    isNext = false;
                }
            }
            //这个是用于2711的服务，如不需要删除这个
            else if (data[0] >> 4 == 0&&data[1] == 0x27 && data[2] == 0x11)
            {
                //发送2701帧，需要接收回应并发出2702帧
                if (isWaitting.ContainsKey("27"))
                {
                    isWaitting["27"] = 0x01;
                }
                else
                {
                    isWaitting.Add("27", 0x01);
                }
                send(data);
                //CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                {
                    //收到了seed，计算key
                    byte[] temp = DataConverter.calKeyFromSeedOnUDS(seed, 2);
                    byte[] b = new byte[8];
                    b[0] = 0x06;
                    b[1] = 0x27;
                    b[2] = 0x12;
                    b[3] = temp[0];
                    b[4] = temp[1];
                    b[5] = temp[2];
                    b[6] = temp[3];
                    b[7] = 0xAA;
                    if (isWaitting.ContainsKey("27"))
                    {
                        isWaitting["27"] = 0x01;
                    }
                    else
                    {
                        isWaitting.Add("27", 0x01);
                    }
                    send(b);
                    if (waitForResponse(new Object[] { "27", 5000 }) == 0)
                    {
                        isNext = true;
                        Console.WriteLine("T4");
                    }
                    else
                    {
                        isNext = false;
                    }
                }
                else
                {
                    isNext = false;
                }
            }
            else
            {
                send(data);
                //（待测）该if可能可以屏蔽，不判断waitCF而将判断是否接收完全数据交给多帧的iswaitting状态，
                //但是此处会根据待接收数据长度计算等待时间，而删除后等待时间为固定值，可能存在隐患
                if ((data[0] >> 4 & 0x0F) == 3)
                {
                    //发送的是流控帧，判断数据是否接收完全，根据需要启用
                    if (waitForResponse(new Object[] { "waitCF", (Ff_dl / 7 + 1) * 100 > 5000 ? (Ff_dl / 7 + 1) * 100 : 5000 }) != 0)
                    // if (waitForResponse(new Object[] { "waitCF", (Ff_dl / 7 + 1) * 100 }) != 0)
                    {
                        object[] results = new object[2];
                        //没有收到下位机完整的数据回复
                        results[0] = FunCode.ERROR + "";
                        results[1] = "waitOvertime";
                        flushVM(results);
                        if (isWaitting.ContainsKey("waitCF"))
                        {
                            isWaitting["waitCF"] = 0x00;
                            if (isWaitting.ContainsKey(DataConverter.hex2String((byte)(data[2] - 0x40))))
                            {
                                isWaitting[DataConverter.hex2String((byte)(data[2] - 0x40))] = 0x00;
                            }
                        }
                        isNext = false;
                    }
                    else
                    {
                        isNext = true;
                        Console.WriteLine("T5");
                    }
                }
                else if ((data[0] >> 4 & 0x0F) == 0)
                {
                    if (data[1] == 0x3E)
                    {
                        //3E心跳不改变isNext状态
                        //isNext = true;
                        Console.WriteLine("T6");
                    }
                    else
                    {
                        if (waitForResponse(new Object[] { DataConverter.hex2String(data[1]), 5000 }) == 0)
                        {
                            isNext = true;
                            Console.WriteLine("T7");
                        }
                        else
                        {
                            isNext = false;
                        }
                    }

                }
                else
                {
                    //cf帧或FF帧
                    // isNext= true;
                    Console.WriteLine("T8");
                }
                // CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                //Object[] results = new Object[3];
                //results[0] = FunCode.SHOWUDSLOG + "";
                //results[1] = "sendUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
                //results[2] = SENDCOLOR;
                //flushVM(results);
                //UdsLog += "sendUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2") + "\r\n";

                //发送的是流控帧，判断数据是否接收完全，根据需要启用
                //if ((data[0] >> 4 & 0x0F) == 3)
                //{
                //    bool isEnd = false;
                //    long starttime = DateTime.Now.Ticks;
                //    // long end = (new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks - starttime) / 10000;
                //    long e = (DateTime.Now.Ticks - starttime) / 10000;//单位毫秒
                //    long e2 = Ff_dl / 7 * 50;//根据需要接收的帧数计算超时时间
                //    while (((DateTime.Now.Ticks - starttime) / 10000) < e2)
                //    {
                //        if (IsWaitting.ContainsKey("waitCF") && IsWaitting["waitCF"] == 0xFF)
                //        {
                //            isEnd = true;
                //            break;
                //        }
                //    }
                //    if (!isEnd)
                //    {
                //        object[] results = new object[2];
                //        //没有收到下位机完整的数据回复
                //        results[0] = FunCode.ERROR + "";
                //        results[1] = "waitOvertime";
                //        flushVM(results);
                //        //UdsLog += "receive overtime\r\n";
                //    }
                //}
            }
            if ((data[0] >> 4) == 0 && data[1] == 0x31 && data[2] == 0x01 && data[3] == 0x02 && data[4] == 0x03) {
                //31010203帧停顿2秒
                Thread.Sleep(2000);
            }
        }

        public void send(byte[] data)
        {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 0;//标准帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            string s = SendID;
            //if (IsECUFun) {
            //    s = "7DF";
            //}
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            obj.Data = data;

            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Object[] results = new Object[3];
            results[0] = FunCode.SHOWUDSLOG + "";
            results[1] = "sendUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2");
            results[2] = SENDCOLOR;
            //if (data[1] != 0x3E)
            //{
            //    flushVM(results);
            //}
            //每次发送数据更新心跳发送基础时间
            if (isDoBootLoader)
            {
                Console.WriteLine("sendUDSData   ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 3 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            }
            else
            {
                flushVM(results);
            }

            results = new Object[1];
            results[0] = FunCode.CHANGEHEARTTIME + "";
            flushVM(results);
        }

        //多帧发送
        public void sendMultiFrame(Object o)
        {
            Object[] os = (Object[])o;
            string frameS = (string)os[0];
            byte sid = (byte)os[1];
            String sf = frameS.Substring(0, 2);
            //byte did = (byte)os[2];//xg


            byte[] b = new byte[8];
            //b[0] = (byte)(1 << 4 | (((frameS.Length / 2 + 2) >> 8) & 0x0F));
            //b[0] = did==0xAA? (byte)(1 << 4 | (((frameS.Length / 2 + 1) >> 8) & 0x0F)):(byte)(1 << 4 | (((frameS.Length / 2 + 2) >> 8) & 0x0F));//xg
            b[0] = (byte)(1 << 4 | (((frameS.Length / 2 + 1) >> 8) & 0x0F));
            //b[1] = did == 0xAA ? (byte)((frameS.Length / 2 + 1) & 0x00FF):(byte)((frameS.Length / 2 + 2) & 0x00FF);//xg
            b[1] =  (byte)((frameS.Length / 2 + 1) & 0x00FF);
            b[2] = sid;
            //b[3] = did;

            //byte[] temp = DataConverter.strToHexByte(frameS.Substring(0, 8));
            //frameS = did == 0xAA ? frameS : (DataConverter.hex2String(did) + frameS);//xg
            //frameS =frameS : (DataConverter.hex2String(did) + frameS);
            //byte[] temp = did==0xAA? DataConverter.strToHexByte(frameS.Substring(0, 10)):DataConverter.strToHexByte((DataConverter.hex2String(did)+ frameS).Substring(0, 10));
            byte[] temp = DataConverter.strToHexByte(frameS.Substring(0, 10));

            Array.Copy(temp, 0, b, 3, temp.Length);
            frameS = frameS.Substring(10);
            //int addAACount = frameS.Length / 2 % 7 == 0 ? 0 : (7 - frameS.Length / 2 % 7);
            //for (int i = 0; i < addAACount; i++)
            //{
            //    frameS += "AA";
            //}
            //sendFF_dl = frameS.Length / 2;//待发送数据长度
            sendFrame(b);//发送FF帧
            while (frameS.Length > 0)
            {
                //等待下位机流控帧
                bool isFC = false;
                long starttime = DateTime.Now.Ticks;
                // long end = (new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks - starttime) / 10000;
                //long e = (DateTime.Now.Ticks - starttime) / 10000;//单位毫秒
                //long e2 = 1000;//根据需要接收的帧数计算超时时间

                while ((DateTime.Now.Ticks - starttime) / 10000 < 5000)
                {
                    if (IsWaitting.ContainsKey("FC") && IsWaitting["FC"] == 0x01)
                    {
                        //收到了流控帧准备发送
                        isFC = true;
                        break;
                    }
                }
                if (!isFC)
                {
                    //没有收到下位机的流控帧
                    Object[] results = new Object[2];
                    results[0] = FunCode.ERROR + "";
                    results[1] = "didntreceiveFC";
                    flushVM(results);
                    isWaitting.Remove("sendCFFirstByte");
                    isWaitting.Remove("FC");
                    isWaitting.Remove("BS");
                    isWaitting.Remove("stmin");
                    isNext = false;
                    return;
                    //UdsLog += (string)page.Resources["didntreceiveFC"] + "\r\n";
                }
                else
                {
                    //收到了流控帧，开始发送CF

                    int BS = isWaitting["BS"] == 0 ? 10000 : isWaitting["BS"];
                    int stmin = isWaitting["stmin"];
                    isWaitting.Remove("FC");
                    isWaitting.Remove("BS");
                    isWaitting.Remove("stmin");
                    if (BS * 7 >= frameS.Length / 2)
                    {
                        int addAACount = frameS.Length / 2 % 7 == 0 ? 0 : (7 - frameS.Length / 2 % 7);
                        for (int i = 0; i < addAACount; i++)
                        {
                            frameS += "AA";
                        }

                    }
                    sendFF_dl = frameS.Length / 2;//待发送数据长度
                    //一轮可以发完，需要补充AA凑成7的倍数
                    for (int i = 0; i < BS; i++)
                    {
                        byte[] d = new byte[8];
                        if (isWaitting.ContainsKey("sendCFFirstByte"))
                        {
                            d[0] = (byte)isWaitting["sendCFFirstByte"];
                            isWaitting["sendCFFirstByte"] = isWaitting["sendCFFirstByte"] + 1 > 0x2F ? 0x20 : (isWaitting["sendCFFirstByte"] + 1);
                        }
                        else
                        {
                            d[0] = 0x21;
                            isWaitting.Add("sendCFFirstByte", 0x22);
                        }
                        if (sendFF_dl >= 7)
                        {
                            temp = DataConverter.strToHexByte(frameS.Substring(0, 14));
                            Array.Copy(temp, 0, d, 1, temp.Length);
                            frameS = frameS.Substring(14);
                            sendFF_dl -= 7;
                            Thread.Sleep(stmin);
                            sendFrame(d);

                        }
                        else
                        {
                            //cf发送完毕
                            isWaitting.Remove("sendCFFirstByte");
                            break;
                        }
                    }
                    //置位，等待下一个FC帧
                }
            }//while
            ////补一帧心跳
            //if (sid == 0x31) {
            //    Thread.Sleep(3000);
            //    send(new byte[] { 0x02, 0x3E, 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            //}
            //if (sid == 0x31 && did == 0x01)
            if (sid == 0x31 && sf.Equals("01"))
                {
                if (waitForResponse(new Object[] { DataConverter.hex2String(sid), 20000 }) == 0)
                {
                    isNext = true;
                    Console.WriteLine("T1");
                }
                else
                {
                    isNext = false;
                }
            }
            else
            {
                if (waitForResponse(new Object[] { DataConverter.hex2String(sid), 5000 }) == 0)
                {
                    isNext = true;
                    Console.WriteLine("T2");
                }
                else
                {
                    isNext = false;
                }
            }


        }
        private void flushVM(Object[] o)
        {
            if (changeViewForGen2VM != null)
            {
                changeViewForGen2VM(new ViewEventArgs(o));
            }

        }
    }
}
