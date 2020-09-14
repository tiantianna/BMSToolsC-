using bms.startup.Model;
using bms.startup.SDK;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace bms.startup.service
{
    class ToolingService
    {
        private ToolingViewModel tvm;
        private int saveDataCount = 0;
        private byte tempData;//记录需要等待下一帧拼合的字节，安全性有待考量，如果下一帧并没有发出或者拼合了其他不是该帧所求的帧（例如步骤文件有误）
        double? resultData1 = null;
        double? resultData2 = null;
        double? resultData3 = null;
        double? resultData4 = null;
        private const string BOOTLOADER = "18A[0123][0123456789ABCDEF][0123456789ABCDEF]26";//bootloader
        private const string BOOTLOADERF3 = "F3";//下位机回复F3表示准备就绪
        private const string BOOTLOADERF2 = "F2";//F2表示忙碌
        private const string BOOTLOADERF1 = "F1";//F1表示重传
        private const string BOOTLOADERF0 = "F0";//F0表示全部代码刷写结束
        public ToolingService(ToolingViewModel toolingViewModel)
        {
            this.tvm = toolingViewModel;
        }
        private int WAITTIME = 4000;//等待数据回复的等待时间
        public void parseDataThread(Object o)
        {
            CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)o;
            byte[] data = obj.Data;
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string id = DataConverter.byteToHexStrForId(intBuff);
            //if (new Regex(tvm.ReceId).IsMatch(id))
            if (new Regex(tvm.ReceId).IsMatch(id) && (data[0] == tvm.ReceFrame||tvm.ReceFrame==0) && !tvm.IsNext)
            {
                ToolingStepsAttr tsa = tvm.NowToolingStepAttr;                
                switch (tsa.Type)
                {
                    case 0:
                        //设置读取工装软件版本号及设置工装参数配置
                        if (tvm.ReceCount > 1 && data[0] <= tvm.ReceCount)
                        {
                            tvm.CalDates[tvm.Indexs[data[0] - 1]] = data;
                            saveDataCount++;
                            if (saveDataCount == tvm.ReceCount)
                            {
                                saveDataCount = 0;
                                tvm.IsNext = true;
                            }
                        }
                        break;
                    case 1:
                    case 9:
                        //布尔
                        if (Convert.ToByte(tsa.Frame, 16) == data[0])
                        {
                            Console.WriteLine("接收：" + DataConverter.byteToHexStrForData(obj.Data) + ",ID:" + obj.ID.ToString("X2"));
                            tvm.Result = "OK";
                            tvm.IsNext = true;
                        }

                        break;
                    case 2:
                        //2+2
                        resultData1 = (data[2] << 8 | data[1]) * tsa.Resolution + tsa.Offset;
                        resultData2 = (data[4] << 8 | data[3]) * tsa.Resolution + tsa.Offset;
                        tvm.Result = resultData1 + "," + resultData2;
                        
                        if (resultData1 >=tsa.LowerLimit && resultData1 <= tsa.UpperLimit && resultData2 >= tsa.LowerLimit && resultData2 <= tsa.UpperLimit)
                        {
                            resultData1 = resultData2 = null;
                            tvm.IsNext = true;
                        }                        
                        break;
                    case 3:
                        //2+2+2+1
                        resultData1 = (data[2] << 8 | data[1]) * tsa.Resolution + tsa.Offset;
                        resultData2 = (data[4] << 8 | data[3]) * tsa.Resolution + tsa.Offset;
                        resultData3 = (data[6] << 8 | data[5]) * tsa.Resolution + tsa.Offset;
                        tempData = data[7];
                        tvm.Result = resultData1 + "," + resultData2 + "," + resultData3;
                        if (resultData1 >= tsa.LowerLimit && resultData1 <= tsa.UpperLimit && resultData2 >= tsa.LowerLimit && resultData2 <= tsa.UpperLimit&& resultData3 >=tsa.LowerLimit && resultData3 <= tsa.UpperLimit)
                        {
                            resultData1 = resultData2 =resultData3= null;
                            tvm.IsNext = true;
                        }
                        break;
                    case 4:
                        //1+2+2+2
                        resultData1 = (data[1] << 8 | tempData) * tsa.Resolution + tsa.Offset;
                        resultData2 = (data[3] << 8 | data[2]) * tsa.Resolution + tsa.Offset;
                        resultData3 = (data[5] << 8 | data[4]) * tsa.Resolution + tsa.Offset;
                        resultData4 = (data[7] << 8 | data[6]) * tsa.Resolution + tsa.Offset;
                        tvm.Result = resultData1 + "," + resultData2 + "," + resultData3 + "," + resultData4;
                        if (resultData1 >= tsa.LowerLimit && resultData1 <= tsa.UpperLimit && resultData2 >= tsa.LowerLimit && resultData2 <= tsa.UpperLimit
                            && resultData3 >= tsa.LowerLimit && resultData3 <= tsa.UpperLimit && resultData4 >= tsa.LowerLimit && resultData4 <= tsa.UpperLimit)
                        {
                            resultData1 = resultData2 = resultData3=resultData4 = null;
                            tvm.IsNext = true;
                        }
                        break;
                    case 5:
                        //2
                        resultData1 = (data[2] << 8 | data[1]) * tsa.Resolution + tsa.Offset;
                        tvm.Result = resultData1+"";
                        if (resultData1 >= tsa.LowerLimit && resultData1 <= tsa.UpperLimit) {
                            resultData1 = null;
                            tvm.IsNext = true;
                        }
                        break;
                    case 6:
                        //4
                        resultData1=(data[4]<<32|data[3]<<16|data[2]<<8|data[1]) * tsa.Resolution + tsa.Offset;
                        tvm.Result = resultData1 + "";
                        if (resultData1 >= tsa.LowerLimit && resultData1 <=tsa.UpperLimit)
                        {
                            resultData1 = null;
                            tvm.IsNext = true;
                        }
                        break;
                    case 7:
                        //2+2+2
                        resultData1 = (data[2] << 8 | data[1]) * tsa.Resolution + tsa.Offset;
                        resultData2 = (data[4] << 8 | data[3]) * tsa.Resolution + tsa.Offset;
                        resultData3 = (data[6] << 8 | data[5]) * tsa.Resolution + tsa.Offset;
                        tvm.Result = resultData1 + "," + resultData2 + "," + resultData3;
                        if (resultData1 >= tsa.LowerLimit && resultData1 <= tsa.UpperLimit && resultData2 >= tsa.LowerLimit && resultData2 <= tsa.UpperLimit
                           && resultData3 >= tsa.LowerLimit && resultData3 <= tsa.UpperLimit)
                        {
                            resultData1 = resultData2 = resultData3 = null;
                            tvm.IsNext = true;
                        }
                        break;
                    case 10:
                        //两帧。第一帧1(0x01)+2.第二帧1(0x02)+2+2+2
                        if (data[1] == 0x01)
                        {
                            resultData1 = (data[3] << 8 | data[2]) * tsa.Resolution + tsa.Offset;
                        }
                        else if (data[1] == 0x02) {
                            resultData2 = (data[3] << 8 | data[2]) * tsa.Resolution + tsa.Offset;
                            resultData3 = (data[5] << 8 | data[4]) * tsa.Resolution + tsa.Offset;
                            resultData4 = (data[7] << 8 | data[6]) * tsa.Resolution + tsa.Offset;
                        }
                        if (resultData1 != null && resultData2 != null & resultData3 != null & resultData4 != null) {
                            tvm.Result = resultData1 + "," + resultData2 + "," + resultData3+","+resultData4;
                            if (resultData1 >= tsa.LowerLimit && resultData1 <= tsa.UpperLimit && resultData2 >= tsa.LowerLimit && resultData2 <= tsa.UpperLimit
                            && resultData3 >= tsa.LowerLimit && resultData3 <=tsa.UpperLimit && resultData4 >= tsa.LowerLimit && resultData4 <= tsa.UpperLimit)
                            {
                                resultData1 = resultData2 = resultData3 = resultData4 = null;
                                tvm.IsNext = true;
                            }
                        }
                        break;
                    //case 31:
                    //    //计算帧
                    //    tvm.CalDates[tvm.Indexs[0]]=data;
                    //    tvm.IsNext = true;
                    //    break;
                    //case 32:
                    //case 33:
                    //    //计算帧
                    //    tvm.CalDates[tvm.Indexs[data[1] - 1]] = data;
                    //    saveDataCount++;
                    //    if (saveDataCount == tvm.ReceCount) {
                    //        saveDataCount = 0;
                    //        tvm.IsNext = true;
                    //    }                        
                    //    break;
                    //case 33:
                    //    //计算帧
                    //    tvm.CalDates[tvm.Indexs[data[1] - 1]] = data;
                    //    saveDataCount++;
                    //    if (saveDataCount == tvm.ReceCount)
                    //    {
                    //        saveDataCount = 0;
                    //        tvm.IsNext = true;
                    //    }
                    //    break;
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                        //计算帧，不做数据处理，仅保存数据，等待统一处理

                        if (tvm.ReceCount > 1&&data[1]<=tvm.ReceCount)
                        {
                            tvm.CalDates[tvm.Indexs[data[1]-1]] = data;
                            saveDataCount++;
                            if (saveDataCount == tvm.ReceCount) {
                                saveDataCount = 0;
                                tvm.IsNext = true;
                            }
                        }
                        else {
                            tvm.CalDates[tvm.Indexs[0]] = data;
                            tvm.IsNext = true;
                        }

                        ////if (tvm.ReceCount > 1)
                        //if (tvm.StepNum==1||tvm.StepNum == 2)
                        //{
                        //    tvm.CalDates[tvm.Indexs[0]] = data;
                        //    //saveDataCount++;
                        //    //if (saveDataCount == tvm.ReceCount)
                        //    //{
                        //    //    saveDataCount = 0;
                        //    //    tvm.IsNext = true;
                        //    //}
                        //    tvm.IsNext = true;
                        //}
                        //else if (tvm.StepNum == 3) { 
                        ////else if(tvm.ReceCount==1){
                        //    tvm.CalDates[tvm.Indexs[0]] = data;
                        //    tvm.IsNext = true;
                        //}
                        break;
                    case 35:
                        Console.WriteLine("接收：" + DataConverter.byteToHexStrForData(obj.Data) + ",ID:" + obj.ID.ToString("X2"));
                        if (tvm.StepNum == 1 || tvm.StepNum == 2||tvm.StepNum==7||tvm.StepNum==8||tvm.StepNum==16)
                        {
                            //布尔判断
                            //测试，先全部返回true，正式运行时此行需要删除
                            tvm.IsNext = true;
                            if (data[1] == 0x01)
                            {
                                tvm.IsNext = true;
                            }
                            //Console.WriteLine("1");
                        }
                        else if (tvm.StepNum == 3||tvm.StepNum==9)
                        {
                            if (tvm.ReceCount > 1&&data[1]<=tvm.ReceCount)
                            {
                                tvm.CalDates[tvm.Indexs[data[1] - 1]] = data;
                                saveDataCount++;
                                if (saveDataCount == tvm.ReceCount)
                                {
                                    saveDataCount = 0;
                                    tvm.IsNext = true;
                                }
                            }
                           // Console.WriteLine("3,9");
                        }
                        else if (tvm.StepNum == 4|| tvm.StepNum == 5||tvm.StepNum == 10||tvm.StepNum==11 || tvm.StepNum == 14 || tvm.StepNum == 15)
                        {
                            tvm.CalDates[tvm.Indexs[0]] = data;
                            tvm.IsNext = true;
                            Console.WriteLine("4,5,10,11,14,15");
                        }
                        //else if (tvm.StepNum == 5) {
                        //    tvm.CalDates[tvm.Indexs[0]] = data;
                        //    tvm.IsNext = true;
                        //}
                        break;
                }
            }
            else if ((new Regex(BOOTLOADER).IsMatch(id)))
            {
                //Console.WriteLine("bootloader发送的报文");
                string hexdata = DataConverter.byteToHexStrForDataWithoutSpace(obj.Data);
                string h = hexdata.Substring(0, 2);//取出第一个字节
                if (new Regex(BOOTLOADERF3).IsMatch(h))
                {
                    Console.WriteLine("F3");
                    if (tvm.IsWaitting.ContainsKey("bootHS") && tvm.IsWaitting["bootHS"] == 1)
                    {
                        tvm.IsWaitting["bootHS"] = 0;
                        tvm.RetryTimes = 0;
                        tvm.DataCacheIndex = 0;
                        tvm.IsGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        tvm.CanBootNext = 1;
                    }
                    else if (tvm.IsWaitting.ContainsKey("bmasterHS") && tvm.IsWaitting["bmasterHS"] == 1)
                    {
                        tvm.IsWaitting["bmasterHS"] = 0;
                        tvm.RetryTimes = 0;
                        tvm.DataCacheIndex = 0;
                        tvm.IsGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        tvm.CanBootNext = 1;
                    }
                    else if (tvm.IsWaitting.ContainsKey("bootloaderFE") && (tvm.IsWaitting["bootloaderFE"] == 1 || tvm.IsWaitting["bootloaderFE"] == 2))
                    {
                        tvm.IsWaitting["bootloaderFE"] = 0;
                        tvm.RetryTimes = 0;
                        tvm.DataCacheIndex = 0;
                        tvm.IsGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        tvm.CanBootNext = 1;
                        tvm.DataCache = new CANSDK.VCI_CAN_OBJ[17];
                    }

                }
                else if (new Regex(BOOTLOADERF2).IsMatch(h))
                {
                    Console.WriteLine("F2");
                }
                else if (new Regex(BOOTLOADERF1).IsMatch(h))
                {
                    Console.WriteLine("F1");
                    if (tvm.IsWaitting.ContainsKey("bootloaderFE") && tvm.IsWaitting["bootloaderFE"] == 1)
                    {
                        tvm.IsWaitting["bootloaderFE"] = 2;//2表示需要重传数据
                    }
                }
                else if (new Regex(BOOTLOADERF0).IsMatch(h))
                {
                    Console.WriteLine("F0");
                    if (tvm.IsWaitting.ContainsKey("over") && tvm.IsWaitting["over"] == 1)
                    {
                        tvm.IsWaitting["over"] = 0;
                        tvm.RetryTimes = 0;

                        tvm.DataCacheIndex = 0;
                        tvm.IsGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        tvm.CanBootNext = 0;//bootloader刷写完成不用继续进行
                        tvm.IsNext = true;
                    }
                }
            }
        }

        //一直发送，直到收到肯定响应为止
        public void sendDataUntilSuc(Object o) {
            tvm.IsNext = false;
            byte[] data = (byte[])o;
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse(tvm.SendId, System.Globalization.NumberStyles.HexNumber);
            obj.Data = data;
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            while (!tvm.IsNext)
            {
                Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data) + ",ID:" + obj.ID.ToString("X2"));
                waitForResponse();
            }
        }

        public void sendData(Object o)
        {
            tvm.IsNext = false;
            byte[] data = (byte[])o;
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse(tvm.SendId, System.Globalization.NumberStyles.HexNumber);
            obj.Data = data;
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data) + ",ID:" + obj.ID.ToString("X2"));
            waitForResponse();
        }

        private bool waitForResponse()
        {
            long starttime = DateTime.Now.Ticks;

            while ((DateTime.Now.Ticks - starttime) / 10000 < WAITTIME)
            {
                if (tvm.IsNext)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
