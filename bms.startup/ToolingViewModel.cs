using bms.startup.i18n;
using bms.startup.Model;
using bms.startup.MyStyle;
using bms.startup.SDK;
using bms.startup.service;
using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using visual_sap_control;

namespace bms.startup
{
    class ToolingViewModel : INotifyPropertyChanged
    {
        private ToolingMainWindow page;
        private bool connectstate = false;//can连接状态
        private uint cancode = 0;
        private Thread receiveThread; //数据接受线程
        private Thread sendThread;//发送工装步骤线程
        private bool isReceive = true;//是否循环接收线程
        private const uint RECEIVELEN = 100;//一次性可接收数据帧数 
        private ToolingService tService;
        private DataGrid dg;
        private Dictionary<string, ToolingStepsAttr> toolingStepAttr = new Dictionary<string, ToolingStepsAttr>();//用以标记字典类型
        private String ToolingReportPath = "./ToolingReport/";
        private FileStream fileStream;
        private StreamWriter streamWriter;
        private List<byte[]> calDates;//记录用于计算的参数
        private int[] indexs = new int[20];//保存记录存储位于calDates中的索引位置
        private int portCount = 0;//port数
        private int wenganCount = 0;//温感数
        private int cellCount = 0;//单体数
        private int boardTempSensorCount = 0;//板载温感数
        private int receCount = 0;//标志此次需要连续接收多少帧（需要记录下数据），0表示不需要记录数据,直接在接收后做布尔判断
        private string[] cellCountArray = new string[3];//保存各个模块的单体数量排序,从少到多
        private int[] cellCountPerPort;//记录每个port的单体数，按照由小到大排序
        private byte receFrame = 0;//标志正在等待的帧的第一个字节
        private int stepNum = 0;//标志连续动作的动作号
        private int canBootNext = 0;//标志boot是否可以进行
        private int dataCount = 0;//记录一轮发送了多少数据
        int sendNum = 0;//记录当次已经发送了多少次（[0,0F]）
        private int isGetReceived = 1;//判断是否已经收到回送，可以结束发送。0否1是
        private const int RETRYTIMES = 5;//超时重传次数
        private int retryTimes = 0;//重发次数
        private Dictionary<string, int> isWaitting;//判断是否正在等待接收回送数据包,0否1是，2数据重传
        private const int OVERTIME = 1;//超时等待时间，单位秒
        private int reSendTimes = 0;//下位机回复F1时，记录重传数据的次数
        private int dataCacheIndex = 0;//记录bootloader缓存的游标
        private int bootloaderSum = 0;//bootloader每次传输的校验和
        CANSDK.VCI_CAN_OBJ[] dataCache = new CANSDK.VCI_CAN_OBJ[17];//缓存当次发送的数据以防重发
        private string[] resultData1 = new string[200];//保存分析结果数据，用于同一排版打印
        private string[] resultData2 = new string[200];
        private string[] resultData3 = new string[200];
        private string[] resultData4 = new string[200];
        private List<byte[]> DUTBoardTemp = new List<byte[]>();//保存CAL004的DUT板载温度值，用于CAL005中计算均衡前后温度差

        

        //界面绑定方法
        public DelegateCommand<Button> ConnectCanCommand { get; set; }
        public DelegateCommand<ComboBox> CbI18nClickCommand { get; set; }//语言栏按钮
        public DelegateCommand<Object> RunFun { get; set; }// 带参数的绑定方法，以参数标志函数功能
        public DelegateCommand ReadToolingStepsFileCommand { get; set; }//读取需要解密的文件

        //界面绑定变量
        private int? general = 1;//第几代主动均衡
        private bool isStandard = true;//是否是标准串数
        private bool is24chuan;//是否是24串
        private bool is36chuan;//是否是36串
        private int standardChuan = 36;//标准串数
        private int module1_24chuan;//24串模块1
        private int module2_24chuan;//24串模块2
        private int module1_36chuan;//36串模块1
        private int module2_36chuan;//36串模块2
        private int module3_36chuan;//36串模块3
        private string examinerNo = "123";//检验员编号
        private string dutNo = "123";//DUT编号
        private bool isConfigEnable;//标志界面部分控件是否可用
        //发送线程里的对象
        public class sendInfo
        {
            private string flag;

            private CANSDK.VCI_CAN_OBJ obj;

            public CANSDK.VCI_CAN_OBJ Obj
            {
                get { return obj; }
                set { obj = value; }
            }

            public string Flag
            {
                get { return flag; }
                set { flag = value; }
            }
            public sendInfo(string f, CANSDK.VCI_CAN_OBJ o)
            {
                flag = f;
                obj = o;
            }
        }
        private ObservableCollection<int> canIndexList = new ObservableCollection<int>() { 0, 1, 2, 3, 4, 5 }; //can设备索引号
        public ObservableCollection<int> CanIndexList
        {
            get { return canIndexList; }
            set { canIndexList = value; }
        }
        private int selectDeviceIndex;//设备索引值
        public int SelectDeviceIndex
        {
            get { return selectDeviceIndex; }
            set { selectDeviceIndex = value; OnPropertyChanged("SelectDeviceIndex"); }
        }

        private ObservableCollection<string> canChannelList = new ObservableCollection<string>();
        public ObservableCollection<string> CanChannelList
        {
            get { return canChannelList; }
            set { canChannelList = value; OnPropertyChanged("CanChannelList"); }
        }
        private int selectCanChannelIndex;//CAN通道
        public int SelectCanChannelIndex
        {
            get { return selectCanChannelIndex; }
            set { selectCanChannelIndex = value; OnPropertyChanged("SelectCanChannelIndex"); }
        }
        private ObservableCollection<BaudRateModel> canBaudRateList = new ObservableCollection<BaudRateModel>();//波特率列表
        public ObservableCollection<BaudRateModel> CanBaudRateList
        {
            get { return canBaudRateList; }
            set { canBaudRateList = value; OnPropertyChanged("CanBaudRateList"); }
        }
        private int selectRate = 3;//波特率
        public int SelectRate
        {
            get { return selectRate; }
            set { selectRate = value; OnPropertyChanged("SelectRate"); }
        }
        private ObservableCollection<CategoryInfo> categoryI18nList = new ObservableCollection<CategoryInfo>();//语言列表
        public ObservableCollection<CategoryInfo> CategoryI18nList
        {
            get { return categoryI18nList; }
            set { categoryI18nList = value; OnPropertyChanged("CategoryI18nList"); }
        }
        private int selectI18n = 0;
        public int SelectI18n
        {
            get { return selectI18n; }
            set { selectI18n = value; OnPropertyChanged("SelectI18n"); }
        }
        private ObservableCollection<ToolingStep> steps = null;
        public ObservableCollection<ToolingStep> Steps
        {
            get
            {
                return steps;
            }

            set
            {
                steps = value;
                OnPropertyChanged("Steps");
            }
        }
        private int maxpb = 100;//进度条最大值
        public int Maxpb
        {
            get { return maxpb; }
            set { maxpb = value; OnPropertyChanged("Maxpb"); }
        }

        private int pBValue = -1;//进度条当前值
        public int PBValue
        {
            get { return pBValue; }
            set { pBValue = value; OnPropertyChanged("PBValue"); }
        }
        private string toolingStepsFilePath = "";
        public string ToolingStepsFilePath
        {
            get
            {
                return toolingStepsFilePath;
            }

            set
            {
                toolingStepsFilePath = value;
                OnPropertyChanged("ToolingStepsFilePath");
            }
        }
        private bool isNext = false;
        private bool isFCT = true;
        private bool isEOL = false;
        public bool IsFCT
        {
            get
            {
                return isFCT;
            }

            set
            {
                isFCT = value;
                OnPropertyChanged("IsFCT");
            }
        }

        public bool IsEOL
        {
            get
            {
                return isEOL;
            }

            set
            {
                isEOL = value;
                OnPropertyChanged("IsEOL");
            }
        }
        private string sendId = "00000000";
        private string receId = "00000000";
        public string SendId
        {
            get
            {
                return sendId;
            }

            set
            {
                sendId = value;
            }
        }

        public string ReceId
        {
            get
            {
                return receId;
            }

            set
            {
                receId = value;
            }
        }
        //private byte firstByte = 0;//发送的数据帧首字节（同时也应该是回复的数据帧首字节）
        //private int stepType = 0;//工装步骤type
        //public int StepType
        //{
        //    get
        //    {
        //        return stepType;
        //    }

        //    set
        //    {
        //        stepType = value;
        //    }
        //}

        //public byte FirstByte
        //{
        //    get
        //    {
        //        return firstByte;
        //    }

        //    set
        //    {
        //        firstByte = value;
        //    }
        //}

        public bool IsNext
        {
            get
            {
                return isNext;
            }

            set
            {
                isNext = value;
            }
        }

        public bool IsConfigEnable
        {
            get
            {
                return isConfigEnable;
            }

            set
            {
                isConfigEnable = value;
                OnPropertyChanged("IsConfigEnable");
            }
        }

        internal ToolingStepsAttr NowToolingStepAttr
        {
            get
            {
                return nowToolingStepAttr;
            }

            set
            {
                nowToolingStepAttr = value;
            }
        }
        private string result = "                                   ";//log结果
        private string uIResult = "                                   ";//UI显示结果（大部分情况下与log结果相同，如果log太长则特殊处理）
        public string Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        public int? General
        {
            get
            {
                return general;
            }

            set
            {
                general = value;
                OnPropertyChanged("General");
            }
        }

        public bool Is24chuan
        {
            get
            {
                return is24chuan;
            }

            set
            {
                is24chuan = value;
                OnPropertyChanged("Is24chuan");
            }
        }

        public bool Is36chuan
        {
            get
            {
                return is36chuan;
            }

            set
            {
                is36chuan = value;
                OnPropertyChanged("Is36chuan");
            }
        }

        public int StandardChuan
        {
            get
            {
                return standardChuan;
            }

            set
            {
                standardChuan = value;
                OnPropertyChanged("StandardChuan");
            }
        }

        public int Module1_24chuan
        {
            get
            {
                return module1_24chuan;
            }

            set
            {
                module1_24chuan = value;
                OnPropertyChanged("Module1_24chuan");
            }
        }

        public int Module2_24chuan
        {
            get
            {
                return module2_24chuan;
            }

            set
            {
                module2_24chuan = value;
                OnPropertyChanged("Module2_24chuan");
            }
        }

        public int Module1_36chuan
        {
            get
            {
                return module1_36chuan;
            }

            set
            {
                module1_36chuan = value;
                OnPropertyChanged("Module1_36chuan");
            }
        }

        public int Module2_36chuan
        {
            get
            {
                return module2_36chuan;
            }

            set
            {
                module2_36chuan = value;
                OnPropertyChanged("Module2_36chuan");
            }
        }

        public int Module3_36chuan
        {
            get
            {
                return module3_36chuan;
            }

            set
            {
                module3_36chuan = value;
                OnPropertyChanged("Module3_36chuan");
            }
        }

        public string ExaminerNo
        {
            get
            {
                return examinerNo;
            }

            set
            {
                examinerNo = value;
                OnPropertyChanged("ExaminerNo");
            }
        }

        public string DutNo
        {
            get
            {
                return dutNo;
            }

            set
            {
                dutNo = value;
                OnPropertyChanged("DutNo");
            }
        }

        public bool IsStandard
        {
            get
            {
                return isStandard;
            }

            set
            {
                isStandard = value;
                OnPropertyChanged("IsStandard");
            }
        }

        public List<byte[]> CalDates
        {
            get
            {
                return calDates;
            }

            set
            {
                calDates = value;
            }
        }

        public int[] Indexs
        {
            get
            {
                return indexs;
            }

            set
            {
                indexs = value;
            }
        }

        public int ReceCount
        {
            get
            {
                return receCount;
            }

            set
            {
                receCount = value;
            }
        }

        public byte ReceFrame
        {
            get
            {
                return receFrame;
            }

            set
            {
                receFrame = value;
            }
        }

        public int StepNum
        {
            get
            {
                return stepNum;
            }

            set
            {
                stepNum = value;
            }
        }

        public Dictionary<string, int> IsWaitting
        {
            get
            {
                return IsWaitting1;
            }

            set
            {
                IsWaitting1 = value;
            }
        }

        public int RetryTimes
        {
            get
            {
                return retryTimes;
            }

            set
            {
                retryTimes = value;
            }
        }

        public int DataCacheIndex
        {
            get
            {
                return dataCacheIndex;
            }

            set
            {
                dataCacheIndex = value;
            }
        }

        public int IsGetReceived
        {
            get
            {
                return isGetReceived;
            }

            set
            {
                isGetReceived = value;
            }
        }

        public int CanBootNext
        {
            get
            {
                return canBootNext;
            }

            set
            {
                canBootNext = value;
            }
        }

        public Dictionary<string, int> IsWaitting1
        {
            get
            {
                return isWaitting;
            }

            set
            {
                isWaitting = value;
            }
        }

        public CANSDK.VCI_CAN_OBJ[] DataCache
        {
            get
            {
                return dataCache;
            }

            set
            {
                dataCache = value;
            }
        }

        private ToolingStepsAttr nowToolingStepAttr;//标志当前发送的工装步骤类型属性（也是等待下位机响应的属性）

        public ToolingViewModel(ToolingMainWindow toolingMainWindow)
        {
            this.page = toolingMainWindow;
            init();
        }

        private void init()
        {
            tService = new ToolingService(this);
            dg = page.FindName("showStep") as DataGrid;
            isConfigEnable = true;
            ConnectCanCommand = new DelegateCommand<Button>(RunConnectCanCommand);
            CbI18nClickCommand = new DelegateCommand<ComboBox>(RunCbI18nClickCommand);//国际化选框
            ReadToolingStepsFileCommand = new DelegateCommand(runReadToolingStepsFileCommand);//读取工作步骤文件
            RunFun = new DelegateCommand<Object>(RunSendFrame);
            categoryI18nList.Add(new CategoryInfo() { Name = "English", Value = "en_US", sourceName = "English" });
            categoryI18nList.Add(new CategoryInfo() { Name = "中文", Value = "zh_CN", sourceName = "Chinese" });
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;//忽略文档里面的注释
            //XmlReader reader = XmlReader.Create("../../Config/Config.xml", settings);
            XmlReader reader = XmlReader.Create("Config/Config.xml", settings);
            xmlDoc.Load(reader);
            // 得到根节点bookstore
            XmlNode xn = xmlDoc.SelectSingleNode("appSettings");
            // 得到根节点的所有子节点
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                if (x.Name.Equals("i18n"))
                {
                    selectI18n = int.Parse(x.InnerText);
                    break;
                }

            }
            reader.Close();
            ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            page.Resources.MergedDictionaries.Add(langRd);
            //初始CAN通道
            canChannelList.Add("CAN0");
            canChannelList.Add("CAN1");
            //初始化波特率
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "100Kbps", Time0 = 0x04, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "125Kbps", Time0 = 0x03, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "200Kbps", Time0 = 0x81, Time1 = 0xFA });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "250Kbps", Time0 = 0x01, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "500Kbps", Time0 = 0x00, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "1Mbps", Time0 = 0x00, Time1 = 0x14 });

            isWaitting = new Dictionary<string, int>();

            //初始化计算参数List
            calDates = new List<byte[]>();
            for (int i = 0; i < 24; i++)
            {
                calDates.Add(new byte[8]);
            }

            for (int i = 0; i < 3; i++)
            {
                DUTBoardTemp.Add(new byte[8]);
            }

            cellCountPerPort = new int[3];
        }

        //private Thread sendToolingStepsThread = null;
        private void RunSendFrame(Object i)
        {
            //if (!connectstate)
            //{
            //    Application.Current.Dispatcher.Invoke((Action)delegate
            //    {
            //        ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
            //    });
            //    return;
            //}
            switch (Convert.ToInt32(i))
            {

                case 0:
                    //自检
                    break;
                case 1:
                    //开始
                    //开始前先检查配置，如果检查不通过则报错，并把详细错误信息写到自检按钮中。如果检查通过，直接开始运行检测流程
                    if (General == null || ExaminerNo == null || DutNo == null)
                    {
                        //配置信息填写不完全
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["completeConfig"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    if (sendThread != null)
                    {
                        sendThread.Abort();
                        sendThread = null;
                    }
                    //再判断二维码信息和填写的配置信息是否一致
                    //else if () { }
                    sendThread = new Thread(runStep);
                    sendThread.Start();
                    break;
                case 2:
                    //复位
                    if (receiveThread != null)
                    {
                        receiveThread.Abort();
                        receiveThread = null;
                        receiveThread = new Thread(new ThreadStart(ReceiveDataThread));
                        isReceive = true;
                        receiveThread.IsBackground = true;
                        receiveThread.Start();
                    }
                    if (sendThread != null)
                    {
                        sendThread.Abort();
                        sendThread = null;
                    }
                    if (streamWriter != null)
                    {
                        streamWriter.WriteLine("RESET!");
                        streamWriter.Close();
                        streamWriter.Close();
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream = null;
                    }
                    tService = new ToolingService(this);
                    IsConfigEnable = true;
                    steps = null;
                    break;
                case 3:
                    //停止
                    break;
                case 4:
                    //选中FCT
                    steps = null;
                    break;
                case 5:


                    break;

            }
        }

        //PE刷写
        private bool runPE(int imageID)
        {
            // UInt32 connection_type = cyclone_control_api.CyclonePortType_USB;
            UInt32 handle1 = cyclone_control_api.connectToCyclone("Universal_PEME63B6A");
            if (handle1 == 0)
            {
                return false;
            }
            else
            {
                if (cyclone_control_api.startImageExecution(handle1, Convert.ToByte(1)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private string[] sendIDArray = new string[10];//用于保存g1、g3、g5...g19的发送ID值
        private string[] receIDArray = new string[10];//用于保存g2、g4、g6...g20的接收ID值
        //读取工装步骤文件
        private void readToolingStepsFile()
        {
            if (isFCT)
            {
                ToolingStepsFilePath = "./File/FCT1.gy";
            }
            else if (isEOL)
            {
                ToolingStepsFilePath = "./File/FCT2.gy";
            }
            if (steps == null)
            {
                //读取工装步骤参数并存到内存
                toolingStepAttr = new Dictionary<string, ToolingStepsAttr>();
                XmlDocument xmlDoc = new XmlDocument();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;//忽略文档里面的注释
                XmlReader reader = XmlReader.Create("Config/ToolingAttr.xml", settings);
                xmlDoc.Load(reader);
                XmlNode xn = xmlDoc.SelectSingleNode("toolingsteps");
                if (isFCT)
                {
                    xn = xn.SelectSingleNode("FCT1");
                }
                else if (isEOL)
                {
                    xn = xn.SelectSingleNode("FCT2");

                }

                XmlNodeList xnl = xn.ChildNodes;
                foreach (XmlNode xn1 in xnl)
                {
                    ToolingStepsAttr t = new ToolingStepsAttr();
                    XmlElement xe = (XmlElement)xn1;
                    XmlNodeList xnl0 = xe.ChildNodes;
                    t.Id = xnl0.Item(0).InnerText;
                    t.Type = Convert.ToInt16(xnl0.Item(1).InnerText);
                    t.Frame = xnl0.Item(2).InnerText;
                    //t.Frame = Convert.ToByte((xnl0.Item(2).InnerText), 16);
                    //t.Resolution = Convert.ToDouble(xnl0.Item(3).InnerText);
                    //t.Offset = Convert.ToDouble(xnl0.Item(4).InnerText);
                    t.ResolutionS = xnl0.Item(3).InnerText;
                    t.OffsetS = xnl0.Item(4).InnerText;
                    t.Description = xnl0.Item(5).InnerText;
                    toolingStepAttr.Add(t.Id, t);
                }
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    FileStream fs = new FileStream(ToolingStepsFilePath, FileMode.Open, FileAccess.Read);
                    StreamReader read = new StreamReader(fs, Encoding.Default);
                    String s = null;
                    steps = new ObservableCollection<ToolingStep>();
                    while ((s = read.ReadLine()) != null && s != "")
                    {
                        //int itemp = s.IndexOf(':');
                        int mark = int.Parse(s.Substring(1, s.IndexOf(':') - 1));
                        if (mark < 21)
                        {
                            //该行为ID行
                            if (mark % 2 == 1)
                            {
                                //sendID行
                                sendIDArray[mark / 2] = s.Substring(s.IndexOf(':')+1);
                                //sendIDArray[mark / 2] = s.Substring(3);
                            }
                            else
                            {
                                //receID行
                                receIDArray[mark / 2 - 1] = s.Substring(s.IndexOf(':') + 1);
                                //receIDArray[mark / 2 - 1] = s.Substring(3);
                            }
                        }
                        else if (mark == 21)
                        {
                            //工装步骤行
                            //工装步骤
                            String[] sArray = s.Split(',');
                            ToolingStep t = new ToolingStep();
                            int toolMark = int.Parse(sArray[0].Substring(sArray[0].IndexOf(':') + 1));
                            if (toolMark != 0)
                            {
                                //不等于0表示是需要发送ID的步骤
                                t.SendID = sendIDArray[toolMark / 2];
                                t.ReceID = receIDArray[toolMark / 2];
                            }
                            t.Id = sArray[1];
                            t.IsCheck = true;
                            //t.UpperLimit = Convert.ToDouble(sArray[2]);
                            t.UpperLimit = sArray[2];
                            //t.LowerLimit = Convert.ToDouble(sArray[3]);
                            t.LowerLimit = sArray[3];
                            t.Description = toolingStepAttr[t.Id].Description;
                            steps.Add(t);
                            Maxpb += 1;
                        }
                        else if (mark == 31)
                        {
                            //工装计算行，可能需要多组ID和阈值
                            String[] sArray = s.Split(',');
                            String[] idArray = sArray[0].Substring(sArray[0].IndexOf(':') + 1).Split('&');
                            ToolingStep t = new ToolingStep();
                            String send = "";
                            String rece = "";
                            for (int i = 0; i < idArray.Length; i++)
                            {
                                send += (sendIDArray[int.Parse(idArray[i]) / 2] + "&");
                                rece += (receIDArray[int.Parse(idArray[i]) / 2] + "&");
                            }
                            t.SendID = send;
                            t.ReceID = rece;
                            t.Id = sArray[1];
                            t.IsCheck = true;
                            t.Description= toolingStepAttr[t.Id].Description;
                            //t.UpperLimit = Convert.ToDouble(sArray[2]);
                            t.UpperLimit = sArray[2];
                            //t.LowerLimit = Convert.ToDouble(sArray[3]);
                            t.LowerLimit = sArray[3];
                            steps.Add(t);
                            if (t.Id.Equals("CAL001")|| t.Id.Equals("CAL002")|| t.Id.Equals("CAL003") || t.Id.Equals("CAL004"))
                            {
                                Maxpb += 3;
                            }
                            else if (t.Id.Equals("CAL004"))
                            {
                                Maxpb += 4;
                            }
                            else if (t.Id.Equals("CAL005")){
                                Maxpb += portCount;
                            }
                        }
                    }
                    read.Close();
                    fs.Close();
                    (page.FindName("showStep") as DataGrid).ItemsSource = Steps;


                    reader.Close();
                });
            }
            else if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    steps[i].Result = "                                   ";
                }
            }
            //Maxpb = steps.Count;
            PBValue = -1;
        }
        //发送数据
        public void sendData(Object o)
        {
            IsGetReceived = 0;
            CanBootNext = 0;
            // int i = 1;
            sendInfo send = (sendInfo)o;
            CANSDK.VCI_CAN_OBJ obj = send.Obj;
            int coefficient = 10;
            int waitTimes = 0;
            if (send.Flag.Equals("bootHS"))
            {
                waitTimes = 5000;
                coefficient = 10;//系数，以下乘过以后单位为毫秒
            }
            else if (send.Flag.Equals("bmasterHS"))
            {
                waitTimes = 500;
                coefficient = 50;//系数，以下乘过以后单位为毫秒
            }
            else
            {
                coefficient = 10000;//系数，以下乘过以后单位为秒
                waitTimes = RETRYTIMES;
            }
            int time = 0;
            int bmasterhstime = 0;
            while (true)
            {

                //Thread.Sleep(5);
                if (RetryTimes < waitTimes && IsGetReceived == 0)
                {
                    //如果未收到回应且还有重发次数
                    Console.WriteLine("发送：" + DataConverter.byteToHexStrForDataWithoutSpace(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2") + "," + (time++));
                    uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    IsWaitting[send.Flag] = 1;//开始等待接收回送数据包                   
                    RetryTimes++;
                    long sendTicks = DateTime.Now.Ticks; //记录数据发出的时间

                    while (true)
                    {
                        //如果正在等待接收回送数据包
                        if (IsWaitting.ContainsKey(send.Flag) && IsWaitting[send.Flag] == 1 && IsGetReceived == 0)
                        {
                            // if ((DateTime.Now.Ticks - sendTicks) / 10000 < (OVERTIME * 1000))//没有超时
                            if ((DateTime.Now.Ticks - sendTicks) / 10000 < (OVERTIME * coefficient))//没有超时
                            {
                                continue;
                            }
                            else
                            { //超时
                                Console.WriteLine("超时");
                                IsWaitting[send.Flag] = 0;
                                break;
                            }
                        }
                        else if (IsWaitting.ContainsKey("bootloaderFE") && IsWaitting["bootloaderFE"] == 2 && IsGetReceived == 0)
                        {
                            //重传数据
                            if (reSendTimes >= RETRYTIMES)
                            {
                                Console.WriteLine("重传数据五次，接收超时退出！");
                                return;
                            }
                            else
                            {
                                Console.WriteLine("重传数据！！！");
                                reSendTimes++;
                                for (int i = 0; i < DataCacheIndex + 1; i++)
                                {
                                    Console.WriteLine(i + ":" + DataConverter.byteToHexStrForDataWithoutSpace(DataCache[i].Data));
                                    if (DataCache[i].Data != null)
                                    {
                                        reSend(DataCache[i]);
                                        // sendBootData(dataCache[i]);
                                    }

                                }
                                RetryTimes = 0;
                                Thread.Sleep(50);
                            }
                        }
                        else { break; }
                    }
                }
                else
                {
                    //复位
                    reSendTimes = 0;
                    RetryTimes = 0;
                    IsWaitting[send.Flag] = 0;
                    IsGetReceived = 1;
                    //saveBmuConfigList.Clear();
                    return;
                }
            }
        }
        //数据重发
        private void reSend(CANSDK.VCI_CAN_OBJ obj)
        {
            Console.WriteLine(DataConverter.byteToHexStrForDataWithoutSpace(obj.Data).Substring(0, 2 * obj.DataLen));
            if (IsWaitting.ContainsKey("bootloaderFE") && IsWaitting["bootloaderFE"] == 2 && IsGetReceived == 0)
            {
                //这次发送是重发缓存里的数据，所以不用再次修改缓存，直接发送就好
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
        }
        //发送FE帧表示bootloader本次传输结束
        private void sendFE(CANSDK.VCI_CAN_OBJ obj, int addLen, byte[] addData)
        {
            Console.WriteLine("发送FE帧");
            CanBootNext = 0;
            sendNum = 0;
            obj.DataLen = (byte)(3 + addLen);
            obj.Data[0] = 0xFE;
            bootloaderSum += 0xFE;
            for (int i = 0; i < addLen; i++)
            {
                obj.Data[1 + i] = addData[i];
                bootloaderSum += addData[i];
            }
            obj.Data[1 + addLen] = (byte)bootloaderSum;
            obj.Data[2 + addLen] = (byte)dataCount;
            dataCount = 0;
            CANSDK.VCI_CAN_OBJ tempData = (CANSDK.VCI_CAN_OBJ)DataConverter.CloneObject(obj);
            // dataCache[15] = tempData;
            DataCache[DataCacheIndex] = tempData;

            bootloaderSum = 0;//发送完一轮数据后，校验和归0


            sendData(new sendInfo("bootloaderFE", obj));
            reSendTimes = 0;//重发次数归零
        }
        private void runStartBootLoader()
        {
            //int[] serial = (int[])serialObj;
            //int group = serial[0];//组号,从机从0开始，主机为0
            //int t = serial[1];//id号，表示第几个bmu,取值[1,16]
            IsNext = false;
            dataCount = 0;
            //String bmuId = t.ToString("X2");
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            string s = "18A02601";
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            // obj.ID = 0x18A02601;
            byte[] data = new byte[8];
            //FileStream fs = new FileStream("./File/S32K144_ActiveBalance64Pin_4.16_4.srec", FileMode.Open, FileAccess.Read);
            FileStream fs = new FileStream("./File/S32K144_Bootloader_v2_0.srec", FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            int lineSum = 0;
            //统计总行数
            while (read.ReadLine() != null)
            {
                lineSum++;
            }

            //重新定位到开头
            read.BaseStream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("总行数：" + lineSum);

            string strReadline;
            int i = 0;//用户标记发送次数。i=0时读S0数据，第一次发送时i=1
            // double add = 0;
            int addLen = 0;//地址长度，单位字节
            int pos = 0;//下一次发送数据的其实位置（单位字节）
            int lineLen = 0;//当前剩余数据长度
            byte[] temp = null;//记录当前行不完整的data数据
            string dataTemp;
            byte[] byteTemp;
            byte[] addData = null;//记录当次地址
            byte[] address = null;//记录当行地址
            int? nextAdd = null;//记录当次地址加上数据长度（连续情况时下一次的地址）
            //  int sendNum = 0;//记录当次已经发送了多少次（[0,0F]）
            // CANSDK.VCI_CAN_OBJ[] dataCache = new CANSDK.VCI_CAN_OBJ[16];//缓存当次发送的数据以防重发
            int isBreak = 0;//标志此行地址是否不连续，0否1是
            DataCache = new CANSDK.VCI_CAN_OBJ[17];
            CanBootNext = 1;
            while ((strReadline = read.ReadLine()) != null && CanBootNext == 1)
            {
                //测试用
                //if (i >= 40) { return; }
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if ((strReadline.Substring(0, 2).Equals("S8") | strReadline.Substring(0, 2).Equals("S9")) && CanBootNext == 1)
                {
                    //结束行
                    //最后一轮发送的数据可能不满16次，不是8的倍数

                    if (temp != null)
                    {
                        int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                        data[0] = (byte)sendNum;
                        if (7 - temp.Length < fillCount)
                        {

                            //如果当次帧剩余可填充位置少于所需
                            for (int j = 0; j < 7; j++)
                            {
                                if (j < temp.Length)
                                {
                                    data[j + 1] = temp[j];
                                }
                                else
                                {
                                    data[j + 1] = 0xFF;//用0xFF补充
                                }
                            }

                            obj.DataLen = 8;//+
                            sendBootData(obj);
                            //temp = null;
                            sendNum++;
                            if (sendNum < 16)
                            {
                                data[0] = (byte)sendNum;
                                for (int kk = 0; kk < fillCount - (7 - temp.Length); kk++)
                                {
                                    data[kk + 1] = 0xFF;
                                }
                                obj.DataLen = (byte)(fillCount - (7 - temp.Length) + 1);

                                sendBootData(obj);
                                temp = null;
                            }

                        }
                        else
                        {
                            //如果当次帧剩余可填充位置大于等于所需
                            int fillTemp = 0;
                            for (int j = 0; j < 7; j++)
                            {
                                if (j < temp.Length)
                                {
                                    data[j + 1] = temp[j];
                                }
                                else if (fillTemp < fillCount)
                                {
                                    data[j + 1] = 0xFF;//用0xFF补充
                                    fillTemp++;
                                }
                            }
                            obj.Data = data;
                            obj.DataLen = (byte)(temp.Length + 1 + fillCount);

                            sendBootData(obj);
                            temp = null;
                            sendNum++;
                        }
                        //发送FE帧
                        sendFE(obj, addLen, addData);
                    }
                    else
                    {
                        int lastFillCount = (8 - sendNum * 7 % 8) % 8;//需要补充几个0xFF
                        if (lastFillCount != 0)
                        {
                            data[0] = (byte)sendNum;
                            for (int pp = 0; pp < lastFillCount; pp++)
                            {
                                data[pp + 1] = 0xFF;
                            }
                            obj.Data = data;
                            obj.DataLen = (byte)(1 + lastFillCount);
                            sendBootData(obj);
                            sendNum++;
                        }
                        if (sendNum <= 16)
                        {
                            sendFE(obj, addLen, addData);

                        }
                    }
                    //中止程序
                    data[0] = 0xFD;
                    data[1] = 0xFD;
                    obj.DataLen = (byte)2;
                    sendData(new sendInfo("over", obj));
                }

                pos = 0;

                if (i == 1)
                {
                    data[0] = 0xFF;
                    data[1] = (byte)'S';
                    switch (strReadline.Substring(0, 2))
                    {
                        case "S1":
                            data[2] = (byte)'2';
                            addLen = 2;
                            break;
                        case "S2":
                            data[2] = (byte)'3';
                            addLen = 3;
                            break;
                        case "S3":
                            data[2] = (byte)'4';
                            addLen = 4;
                            break;
                    }

                    data[3] = data[4] = data[5] = data[6] = data[7] = 0x00;
                    obj.Data = data;
                    obj.DataLen = 3;
                    CanBootNext = 0;//如果握手成功再赋值为1
                    sendBootHSData(new sendInfo("bootHS", obj));//发送握手指令
                }

                if (i != 0 && CanBootNext == 1)
                {
                    //去掉地址位和校验和
                    lineLen = int.Parse(strReadline.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - addLen - 1;
                    address = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));//当行的存储地址
                    if (nextAdd != null)
                    {
                        if (nextAdd != DataConverter.byteArrayToInt(address))
                        {
                            i = 17;
                            isBreak = 1;
                            if (sendNum != 0 & temp == null)
                            {
                                //int fillCount = 8 -temp.Length- 7 * sendNum % 8;//需要补充几个0xFF
                                int fillCount = (8 - 7 * sendNum % 8) % 8;
                                if (fillCount != 0)
                                {
                                    data[0] = (byte)sendNum;
                                    for (int pp = 0; pp < fillCount; pp++)
                                    {
                                        data[pp + 1] = 0xFF;
                                    }
                                    obj.Data = data;
                                    obj.DataLen = (byte)(1 + fillCount);
                                    sendBootData(obj);
                                }
                                //发送FE帧
                                sendFE(obj, addLen, addData);
                            }
                        }
                        else
                        {
                            isBreak = 0;
                        }

                    }
                    //连续情况下，下一行的地址
                    nextAdd = DataConverter.byteArrayToInt(address) + lineLen;
                }

                if (temp != null & CanBootNext == 1)
                {
                    if (isBreak == 1)
                    {
                        //地址断了，此时将temp单独发出去而不与这一行拼接
                        if (sendNum != 0)
                        {
                            int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                            data[0] = (byte)sendNum;
                            if (7 - temp.Length < fillCount)
                            {

                                //如果当次帧剩余可填充位置少于所需
                                for (int j = 0; j < 7; j++)
                                {
                                    if (j < temp.Length)
                                    {
                                        data[j + 1] = temp[j];
                                    }
                                    else
                                    {
                                        data[j + 1] = 0xFF;//用0xFF补充
                                    }
                                }

                                obj.DataLen = 8;//+
                                sendBootData(obj);
                                //temp = null;
                                sendNum++;
                                if (sendNum < 16)
                                {
                                    data[0] = (byte)sendNum;
                                    for (int kk = 0; kk < fillCount - (7 - temp.Length); kk++)
                                    {
                                        data[kk + 1] = 0xFF;
                                    }
                                    obj.DataLen = (byte)(fillCount - (7 - temp.Length) + 1);

                                    sendBootData(obj);
                                    temp = null;
                                }

                            }
                            else
                            {
                                //如果当次帧剩余可填充位置大于等于所需
                                int fillTemp = 0;
                                for (int j = 0; j < 7; j++)
                                {
                                    if (j < temp.Length)
                                    {
                                        data[j + 1] = temp[j];
                                    }
                                    else if (fillTemp < fillCount)
                                    {
                                        data[j + 1] = 0xFF;//用0xFF补充
                                        fillTemp++;
                                    }
                                }
                                obj.Data = data;
                                obj.DataLen = (byte)(temp.Length + 1 + fillCount);

                                sendBootData(obj);
                                temp = null;
                                sendNum++;
                            }
                        }
                        else
                        {
                            //如果上一次发送的是0x0F,temp从0开始并且单独一帧发送
                            data[0] = 0x00;
                            for (int j = 0; j < 7; j++)
                            {
                                if (j < temp.Length)
                                {
                                    data[j + 1] = temp[j];
                                }
                                else
                                {
                                    data[j + 1] = 0xFF;//用0xFF补充
                                }
                            }
                            obj.Data = data;

                            obj.DataLen = 8;//+
                            sendBootData(obj);

                            data[0] = 0x01;
                            data[1] = 0xFF;
                            obj.Data = data;
                            obj.DataLen = 2;
                            sendBootData(obj);
                        }
                        //发送FE帧
                        sendFE(obj, addLen, addData);

                    }
                    else
                    {
                        data[0] = (byte)sendNum;//注释了这里******
                        {
                            int L = 7 - temp.Length;

                            if (strReadline.Length < 4 + addLen * 2 + L * 2 + 1)
                            {
                                //次行数据长度不足以与之拼接成一个完整帧,则将次行加入temp
                                dataTemp = strReadline.Substring(4 + addLen * 2, strReadline.Length - 4 - addLen * 2 - 2);

                                lineLen = 0;
                                byteTemp = DataConverter.strToHexByte(dataTemp);
                                byte[] bb = new byte[temp.Length + byteTemp.Length];
                                for (int j = 0; j < byteTemp.Length + temp.Length; j++)
                                {
                                    if (j < temp.Length)
                                    {
                                        bb[j] = temp[j];
                                    }
                                    else
                                    {
                                        // bb[j + temp.Length - 1] = byteTemp[j - temp.Length];
                                        bb[j] = byteTemp[j - temp.Length];
                                    }
                                }
                                temp = bb;
                                i--;//弥补底下的++
                                lineLen = 0;//剩余长度置0，因为都放入了temp
                                Console.WriteLine();
                            }
                            else
                            {
                                dataTemp = strReadline.Substring(4 + addLen * 2, L * 2);
                                pos += L;
                                lineLen -= L;
                                byteTemp = DataConverter.strToHexByte(dataTemp);
                                for (int j = 0; j < 7; j++)
                                {
                                    if (j < temp.Length)
                                    {
                                        data[j + 1] = temp[j];
                                    }
                                    else
                                    {
                                        data[j + 1] = byteTemp[j - temp.Length];
                                    }
                                }

                                obj.Data = data;

                                obj.DataLen = 8;//+
                                sendBootData(obj);
                                sendNum++;
                                temp = null;

                            }
                            //如果当前发送的是第15帧，发送FE，偏移地址

                            if (data[0] == 0x0F)
                            {
                                sendFE(obj, addLen, addData);
                                //偏移
                                addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                                byte[] bb = DataConverter.strToHexByte((DataConverter.byteArrayToInt(addData) + L).ToString("X6"));
                                addData = bb;
                            }

                        }
                        i++;
                    }
                }
                else if (sendNum != 0 & isBreak == 1)
                {
                    //没有temp，且地址断了，且上一次没有发送FE，则这次补上一个FE
                    sendFE(obj, addLen, addData);
                }

                if ((i - 1) % 16 == 0 & lineLen == int.Parse(strReadline.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - addLen - 1)
                {

                    //当前是00号帧，且完整（没有与上一行拼接），则获取地址
                    addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                }

                while (i != 0 & lineLen >= 7 & CanBootNext == 1)
                {
                    // data[0] = (byte)((i - 1) % 16);
                    data[0] = (byte)sendNum;//注释了这里，打开了上面

                    if (lineLen >= 7)
                    {
                        dataTemp = strReadline.Substring(4 + addLen * 2 + pos * 2, 14);
                        pos += 7;
                        lineLen -= 7;
                        byteTemp = DataConverter.strToHexByte(dataTemp);
                        for (int j = 1; j < 8; j++)
                        {
                            data[j] = byteTemp[j - 1];
                        }
                        obj.Data = data;
                        //发送数据包（仅发送不用判断返回值）

                        obj.DataLen = 8;//+
                        sendBootData(obj);
                        sendNum++;

                        if (data[0] == 0x0F)
                        {
                            //发送本次传输结束命令
                            //注释用于测试
                            sendFE(obj, addLen, addData);

                            //当最后一帧已发完，但是当前行还有数据没发，在FE帧后发第一帧，同时地址需要偏移
                            if (lineLen > 0)
                            {
                                data[0] = 0x00;
                                int tt = int.Parse(strReadline.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - addLen - 1 - lineLen;
                                // int tt = DataConverter.string2Hex(strReadline.Substring(4, addLen * 2)) - lineLen;

                                if (tt != 0)
                                {
                                    addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                                    byte[] bb = DataConverter.strToHexByte((DataConverter.byteArrayToInt(addData) + tt).ToString("X6"));
                                    addData = bb;
                                }
                            }


                        }
                        i++;
                    }
                }
                if (lineLen != 0 && CanBootNext == 1)
                {
                    dataTemp = strReadline.Substring(4 + addLen * 2 + pos * 2, lineLen * 2);
                    temp = DataConverter.strToHexByte(dataTemp);
                }
            }
            fs.Close();
            read.Close();
        }
        //发送bootloader数据
        private void sendBootData(CANSDK.VCI_CAN_OBJ obj)
        {
            dataCount += (obj.DataLen - 1);
            byte[] b = obj.Data;
            Console.WriteLine(DataConverter.byteToHexStrForDataWithoutSpace(obj.Data).Substring(0, 2 * obj.DataLen));
            if (IsWaitting.ContainsKey("bootloaderFE") && IsWaitting["bootloaderFE"] == 2 && IsGetReceived == 0)
            {
                //这次发送是重发缓存里的数据，所以不用再次修改缓存，直接发送就好
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                dataCount = 0;//重传数据不需要再计算数据字节数，因为上一帧为0所以维持0就好
            }
            else
            {
                CANSDK.VCI_CAN_OBJ tempData = (CANSDK.VCI_CAN_OBJ)DataConverter.CloneObject(obj);

                // dataCache[b[0]] = tempData;//加入缓存防止需要重发
                DataCache[DataCacheIndex++] = tempData;
                for (int i = 0; i < obj.DataLen; i++)
                {
                    bootloaderSum += b[i];
                }
                // Console.WriteLine("校验和：" + bootloaderSum);
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
        }
        private void sendBootHSData(Object o)
        {
            sendData(o);
            Console.WriteLine("发送了握手信息");
        }

        private delegate void runBootLoaderDelegate();
        private void runStep()
        {
            //delegate void runBoot()
            
            Maxpb = 0;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ImageButton im = (ImageButton)page.FindName("light");
                im.Icon = new BitmapImage(new Uri(@"..\images\greenButton.png", UriKind.Relative));
                
            });
            //计算port值和温感数（每6串电芯公用一个温感）
            if (IsStandard)
            {
                if (standardChuan == 24)
                {
                    portCount = 2;
                    boardTempSensorCount = 5;
                    cellCountArray[0] = "B";
                    cellCountArray[1] = "A";
                    cellCountPerPort[0] = 10;
                    cellCountPerPort[1] = 14;
                }
                else if (standardChuan == 36)
                {
                    portCount = 3;
                    boardTempSensorCount = 7;
                    cellCountArray[0] = "B";
                    cellCountArray[1] = "C";
                    cellCountArray[2] = "A";
                    cellCountPerPort[0] = 10;
                    cellCountPerPort[1] = 12;
                    cellCountPerPort[2] = 14;
                }
                else
                {
                    //数据有误
                    return;
                }
                //portCount = standardChuan > 24 ? 3 : 2;
                cellCount = standardChuan;
                wenganCount = standardChuan % 6 == 0 ? standardChuan / 6 : (standardChuan / 6 + 1);
                // boardTempSensorCount = standardChuan > 24 ? 7 : 5;

                //if (portCount == 2)
                //{
                //    cellCountArray[0] = "B";
                //    cellCountArray[1] = "A";
                //}
                //else {
                //    cellCountArray[0] = "B";
                //    cellCountArray[1] = "C";
                //    cellCountArray[2] = "A";
                //}
            }
            else if (Is24chuan)
            {
                portCount = 2;
                cellCount = module1_24chuan + module2_24chuan;
                wenganCount = (int)(cellCount % 6 == 0 ? cellCount / 6 : (cellCount / 6 + 1));
                boardTempSensorCount = 5;
                if (module1_24chuan >= module2_24chuan)
                {
                    //A>B->BA
                    cellCountArray[0] = "B";
                    cellCountArray[1] = "A";
                    cellCountPerPort[0] = module2_24chuan;
                    cellCountPerPort[1] = module1_24chuan;
                }
                else
                {
                    //A<=B->AB
                    cellCountArray[0] = "A";
                    cellCountArray[1] = "B";
                    cellCountPerPort[0] = module1_24chuan;
                    cellCountPerPort[1] = module2_24chuan;
                }
            }
            else
            {
                portCount = 3;
                cellCount = module1_36chuan + module2_36chuan + module3_36chuan;
                wenganCount = (int)(cellCount % 6 == 0 ? cellCount / 6 : (cellCount / 6 + 1));
                boardTempSensorCount = 7;
                if (module1_36chuan <= module2_36chuan && module2_36chuan <= module3_36chuan)
                {
                    cellCountArray[0] = "A";
                    cellCountArray[1] = "B";
                    cellCountArray[2] = "C";
                    cellCountPerPort[0] = module1_36chuan;
                    cellCountPerPort[1] = module2_36chuan;
                    cellCountPerPort[2] = module3_36chuan;
                }
                else if (module1_36chuan <= module3_36chuan && module3_36chuan <= module2_36chuan)
                {
                    cellCountArray[0] = "A";
                    cellCountArray[1] = "C";
                    cellCountArray[2] = "B";
                    cellCountPerPort[0] = module1_36chuan;
                    cellCountPerPort[1] = module3_36chuan;
                    cellCountPerPort[2] = module2_36chuan;
                }
                else if (module2_36chuan <= module1_36chuan && module1_36chuan <= module3_36chuan)
                {
                    cellCountArray[0] = "B";
                    cellCountArray[1] = "A";
                    cellCountArray[2] = "C";
                    cellCountPerPort[0] = module2_36chuan;
                    cellCountPerPort[1] = module1_36chuan;
                    cellCountPerPort[2] = module3_36chuan;
                }
                else if (module2_36chuan <= module3_36chuan && module3_36chuan <= module1_36chuan)
                {
                    cellCountArray[0] = "B";
                    cellCountArray[1] = "C";
                    cellCountArray[2] = "A";
                    cellCountPerPort[0] = module2_36chuan;
                    cellCountPerPort[1] = module3_36chuan;
                    cellCountPerPort[2] = module1_36chuan;
                }
                else if (module3_36chuan <= module1_36chuan && module1_36chuan <= module2_36chuan)
                {
                    cellCountArray[0] = "C";
                    cellCountArray[1] = "A";
                    cellCountArray[2] = "B";
                    cellCountPerPort[0] = module3_36chuan;
                    cellCountPerPort[1] = module1_36chuan;
                    cellCountPerPort[2] = module2_36chuan;
                }
                else if (module3_36chuan <= module2_36chuan && module2_36chuan <= module1_36chuan)
                {
                    cellCountArray[0] = "C";
                    cellCountArray[1] = "B";
                    cellCountArray[2] = "A";
                    cellCountPerPort[0] = module3_36chuan;
                    cellCountPerPort[1] = module2_36chuan;
                    cellCountPerPort[2] = module1_36chuan;
                }
            }
            //string activeDir = @"C:\myDir";
            //string newPath = System.IO.Path.Combine(activeDir, "mySubDirOne");
            System.IO.Directory.CreateDirectory(ToolingReportPath);
            fileStream = new FileStream(ToolingReportPath + DateTime.Now.Ticks + ".txt", FileMode.Create);
            streamWriter = new StreamWriter(fileStream, Encoding.Default);
            streamWriter.WriteLine("Project:" + (IsFCT ? "FCT1" : "FCT2"));
            streamWriter.WriteLine("Tester:" + ExaminerNo);
            streamWriter.WriteLine("StringNum:" + (IsStandard ? StandardChuan + "" : is24chuan ? (Module1_24chuan + "+" + Module2_24chuan) : (Module1_36chuan + "+" + Module2_36chuan + "+" + Module3_36chuan)));
            streamWriter.WriteLine("DUT No:" + DutNo);
            streamWriter.WriteLine("Date&Time:" + DateTime.Now.ToString());
            streamWriter.WriteLine();

            IsConfigEnable = false;
            readToolingStepsFile();
            //先把表格背景色初始为白色
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                for (int i = 0; i < dg.Items.Count; i++)
                {
                    dg.UpdateLayout();
                    dg.ScrollIntoView(dg.Items[i]);
                    DataGridRow dataGridRow = (DataGridRow)dg.ItemContainerGenerator.ContainerFromItem(dg.Items[i]);
                    dataGridRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF"));
                }
                //(page.FindName("showStep") as DataGrid).ItemsSource = Steps;
            });
            Thread.Sleep(500);
            IsNext = true;
            PBValue = 0;
            for (int j = 0; j < steps.Count && IsNext; j++)
            {
                ToolingStep step = steps[j];
                ReceId = step.ReceID;
                SendId = step.SendID;
                NowToolingStepAttr = toolingStepAttr[step.Id];
                //ToolingStepsAttr tsa = toolingStepAttr[step.Id];
                switch (nowToolingStepAttr.Type)
                {
                    case 0:
                        //设置读取工装软件版本号及设置工装参数配置
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data8 = new byte[] { (byte)General, (byte)(IsStandard ? 1 : 2),(byte)StandardChuan, IsStandard ? (byte)cellCount:(byte)0xFF ,
                            (byte)(IsStandard ? 0xFF : (Is24chuan ? Module1_24chuan : Module1_36chuan)), (byte)(IsStandard ? 0xFF : (Is24chuan ? Module2_24chuan : Module2_36chuan)),
                        (byte)(IsStandard ? 0xFF : (Is24chuan ? 0xFF : Module3_36chuan)),0xFF};
                        tService.sendData(data8);
                        Indexs[0] = 0;
                        Indexs[1] = 1;
                        ReceCount = 2;
                        Console.WriteLine(CalDates);
                        if (IsNext)
                        {
                            streamWriter.WriteLine("MB版本号：" + DataConverter.bytestoString(calDates[0].Skip(1).Take(calDates[0].Length).ToArray()) + DataConverter.bytestoString(calDates[1].Skip(1).Take(calDates[1].Length).ToArray()));
                            streamWriter.WriteLine("ID" + "\t" + "LOWERLIMIT" + "\t" + "UPPERLIMIT" + "\t" + "VALUE" + "\t" + "TEST RESULT");
                        }
                        else {
                            streamWriter.WriteLine("MB版本号获取失败！");
                        }
                        break;
                    case 1:
                        //布尔类型
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        //firstByte = NowToolingStepAttr.Frame;                       
                        tService.sendData(data);
                        streamWriter.WriteLine(nowToolingStepAttr.Id + "\t" + "\\" + "\t" + "\\" + "\t" + Result + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        PBValue++;
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 10:
                        //byte[] data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data2 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        //firstByte = NowToolingStepAttr.Frame;
                        tService.sendData(data2);
                        streamWriter.WriteLine(nowToolingStepAttr.Id + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + Result + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        PBValue++;
                        break;
                    case 8:
                        //PE刷写
                        if (runPE(Convert.ToByte(nowToolingStepAttr.Frame, 16)))
                        {
                            IsNext = true;
                            Result = "OK";
                        }
                        else
                        {
                            IsNext = false;
                            Result = "NG";
                        }
                        streamWriter.WriteLine(nowToolingStepAttr.Id + "\t\\\t\\" + Result + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        PBValue++;
                        break;
                    case 9:
                        //一直发送直到返回肯定响应为止
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data3 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        tService.sendDataUntilSuc(data3);
                        PBValue++;
                        break;
                    case 11:
                        //BootLoader刷写APP
                        //runBootLoaderDelegate boot = runStartBootLoader;
                        for (int i = 0; i < 3; i++) {
                            //boot();
                            runStartBootLoader();
                            if (IsNext) {
                                streamWriter.WriteLine(nowToolingStepAttr.Id + "APP刷写成功！");
                                Result = "PASS";
                                break;
                            }
                        }
                        Result = "PASS";
                        streamWriter.WriteLine(nowToolingStepAttr.Id + "APP刷写失败！");
                        PBValue++;
                        break;
                    case 31:
                        String[] sendIdsTemp = step.SendID.Split('&');
                        String[] receIdsTemp = step.ReceID.Split('&');
                        SendId = sendIdsTemp[0];
                        ReceId = receIdsTemp[0];
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data4 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        Indexs[0] = 0;
                        ReceCount = 1;
                        tService.sendData(data4);
                        if (!IsNext)
                        {
                            break;
                        }
                        PBValue++;
                        SendId = sendIdsTemp[1];
                        ReceId = receIdsTemp[1];
                        // data4 = new byte[] { Convert.ToByte(nowToolingStepAttr.Frame, 16), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        Indexs[0] = 1;
                        tService.sendData(data4);
                        if (!isNext)
                        {
                            break;
                        }
                        PBValue++;
                        Console.WriteLine(CalDates);
                        //做差计算
                        byte[] calData1 = calDates[0];
                        byte[] calData2 = calDates[1];
                        Result = "";
                        uIResult = "OK";
                        isNext = true;
                        for (int i = 0; i < portCount && IsNext; i++)
                        {
                            double dvalue = ((calData1[i * 2 + 2] << 8 | calData1[i * 2 + 1]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset) -
                        ((calData2[i * 2 + 2] << 8 | calData2[i * 2 + 1]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset);
                            //Result += dvalue;
                            //if (i < portCount - 1)
                            //{
                            //    Result += ",";
                            //}
                            if (dvalue > Convert.ToDouble(NowToolingStepAttr.UpperLimit) || dvalue < Convert.ToDouble(NowToolingStepAttr.LowerLimit))
                            {
                                //此次结果不符合
                                //if (uIResult.Equals("OK"))
                                //{
                                uIResult = "port" + (i == 0 ? "A" : (i == 1 ? "B" : "C")) + ":" + dvalue + " ";
                                //}
                                //else
                                //{
                                //    uIResult += "port" + (i == 0 ? "A" : (i == 1 ? "B" : "C")) + ":" + dvalue + " ";
                                //}
                                isNext = false;
                            }

                            streamWriter.WriteLine(nowToolingStepAttr.Id + "_port" + (i == 0 ? "A" : (i == 1 ? "B" : "C")) + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + dvalue + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        }
                        PBValue++;
                        break;
                    case 32:
                        String[] sendIdsTemp2 = step.SendID.Split('&');
                        String[] receIdsTemp2 = step.ReceID.Split('&');
                        SendId = sendIdsTemp2[0];
                        ReceId = receIdsTemp2[0];
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data5 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        Indexs[0] = 0;
                        //Indexs[1] = 1;
                        ReceCount = 1;//此次需要连续接收2帧
                        tService.sendData(data5);
                        if (!IsNext)
                        {
                            break;
                        }
                        PBValue++;
                        SendId = sendIdsTemp2[1];
                        ReceId = receIdsTemp2[1];
                        //data5= new byte[] { nowToolingStepAttr.Frame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        Indexs[0] = 1;
                        //Indexs[0] = 2;
                        //Indexs[1] = 3;
                        ReceCount = 1;//此次需要连续接收2帧
                        tService.sendData(data5);
                        if (!isNext)
                        {
                            break;
                        }
                        PBValue++;

                        //做差计算
                        Result = "";
                        uIResult = "OK";
                        isNext = true;
                        for (int i = 0; i < wenganCount && IsNext; i++)
                        {
                            double dvalue = (calDates[0][i + 1] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset) - (CalDates[1][i + 1] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset);
                            //double dvalue =( (calDates[i / 3][i % 3 * 2 + 3] << 8 | calDates[i / 3][i % 3 * 2 + 2]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset) -
                            //    ((calDates[i / 3 + 2][i % 3 * 2 + 3] << 8 | calDates[i / 3 + 2][i % 3 * 2 + 2]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset);
                            //Result += dvalue;
                            //if (i < wenganCount - 1)
                            //{
                            //    Result += ",";
                            //}
                            if (dvalue > Convert.ToDouble(NowToolingStepAttr.UpperLimit) || dvalue < Convert.ToDouble(NowToolingStepAttr.LowerLimit))
                            {
                                //此次结果不符合
                                //if (uIResult.Equals("OK"))
                                //{
                                uIResult = "temp_sensor" + (i + 1) + ":" + dvalue + " ";
                                //}
                                //else
                                //{
                                //    uIResult += "temp_sensor" + (i + 1) + ":" + dvalue + " ";
                                //}
                                isNext = false;

                            }
                            streamWriter.WriteLine(nowToolingStepAttr.Id + "_tv" + (i + 1) + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + dvalue + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        }
                        PBValue++;
                        break;
                    case 33:
                        String[] sendIdsTemp3 = step.SendID.Split('&');
                        String[] receIdsTemp3 = step.ReceID.Split('&');
                        SendId = sendIdsTemp3[0];
                        ReceId = receIdsTemp3[0];
                        ReceFrame = Convert.ToByte(nowToolingStepAttr.Frame, 16);
                        byte[] data6 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        ReceCount = cellCount % 3 == 0 ? cellCount / 3 : (cellCount / 3 + 1);
                        for (int i = 0; i < receCount; i++)
                        {
                            Indexs[i] = i;
                        }
                        //Indexs[0] = 0;
                        //Indexs[1] = 1;
                        tService.sendData(data6);
                        if (!IsNext)
                        {
                            break;
                        }
                        PBValue++;
                        SendId = sendIdsTemp3[1];
                        ReceId = receIdsTemp3[1];
                        for (int i = 0; i < receCount; i++)
                        {
                            Indexs[i] = receCount + i;
                        }
                        tService.sendData(data6);
                        if (!isNext)
                        {
                            break;
                        }
                        PBValue++;
                        //做差计算
                        Result = "";
                        uIResult = "OK";
                        isNext = true;
                        for (int i = 0; i < cellCount && IsNext; i++)
                        {
                            //double dvalue = ((calDates[i / 3][i % 3 * 2 + 3] << 8 | calDates[i / 3][i % 3 * 2 + 2]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset) -
                            //   ( (calDates[i / 3 + 2][i % 3 * 2 + 3] << 8 | calDates[i / 3 + 2][i % 3 * 2 + 2]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset);
                            double dvalue = ((calDates[i / 3][i % 3 * 2 + 3] << 8 | calDates[i / 3][i % 3 * 2 + 2]) - (calDates[i / 3 + ReceCount][i % 3 * 2 + 3] << 8 | calDates[i / 3 + ReceCount][i % 3 * 2 + 2]))
                                * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                            //Result += dvalue;
                            //if (i < cellCount - 1)
                            //{
                            //    Result += ",";
                            //}
                            if (dvalue > Convert.ToDouble(NowToolingStepAttr.UpperLimit) || dvalue < Convert.ToDouble(NowToolingStepAttr.LowerLimit))
                            {
                                //此次结果不符合
                                //if (uIResult.Equals("OK"))
                                //{
                                uIResult = "cell" + (i + 1) + ":" + dvalue + " ";
                                //}
                                //else {
                                //    uIResult += "cell" + (i + 1) + ":" + dvalue + " ";
                                //}
                                isNext = false;
                            }
                            streamWriter.WriteLine(nowToolingStepAttr.Id + "_Vcell" + (i + 1) + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + dvalue + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        }
                        PBValue++;
                        break;
                    case 34:
                        String[] sendIdsTemp4 = step.SendID.Split('&');
                        String[] receIdsTemp4 = step.ReceID.Split('&');
                        SendId = sendIdsTemp4[0];
                        ReceId = receIdsTemp4[0];
                        //此类型的frame有两个，用逗号隔开
                        string[] framesS = nowToolingStepAttr.Frame.Split(',');
                        ReceFrame = Convert.ToByte(framesS[0], 16);
                        byte[] data7 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        nowToolingStepAttr.UpperLimit = Convert.ToDouble(step.UpperLimit);
                        nowToolingStepAttr.LowerLimit = Convert.ToDouble(step.LowerLimit);
                        nowToolingStepAttr.Resolution = Convert.ToDouble(nowToolingStepAttr.ResolutionS.Split('&')[0]);
                        nowToolingStepAttr.Offset = Convert.ToDouble(nowToolingStepAttr.OffsetS.Split('&')[0]);
                        ReceCount = boardTempSensorCount == 5 ? 2 : 3;
                        //ReceCount = cellCount % 3 == 0 ? cellCount / 3 : (cellCount / 3 + 1);

                        Indexs[0] = 0;
                        StepNum = 1;
                        //for (int i = 0; i < receCount; i++)
                        //{
                        //    Indexs[i] = i;
                        //}
                        //Indexs[0] = 0;
                        //Indexs[1] = 1;
                        tService.sendData(data7);
                        if (!IsNext)
                        {
                            break;
                        }
                        PBValue++;
                        DUTBoardTemp[0] = CalDates[0];
                        //for (int i = 0; i < receCount; i++) {
                        //    DUTBoardTemp[i] = calDates[i];
                        //}
                        //第二步，计算金板板载温感值
                        StepNum = 2;
                        SendId = sendIdsTemp4[1];
                        ReceId = receIdsTemp4[1];
                        Indexs[0] = 1;
                        tService.sendData(data7);
                        if (!IsNext)
                        {
                            break;
                        }
                        PBValue++;
                        DUTBoardTemp[1] = CalDates[1];

                        //第三步
                        StepNum = 3;
                        SendId = sendIdsTemp4[2];
                        ReceId = receIdsTemp4[2];
                        Indexs[0] = 2;
                        //Indexs[0] = ReceCount;
                        ReceCount = 1;//采集自MB板，仅有一帧回复
                        ReceFrame = Convert.ToByte(framesS[1], 16);
                        data7 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        tService.sendData(data7);
                        if (!isNext)
                        {
                            break;
                        }
                        PBValue++;
                        //做差计算
                        Result = "";
                        uIResult = "OK";
                        isNext = true;
                        double surTemp = (CalDates[2][2] << 8 | CalDates[2][1]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                        //计算ABC口的温度
                        for (int i = 0; i < boardTempSensorCount - 1 && IsNext; i++)
                        {
                            double dvalue = calDates[0][i + 1] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset - surTemp;
                            //double dvalue = (calDates[i / 3][i % 3 * 2 + 3] << 8 | calDates[i / 3][i % 3 * 2 + 2]) * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset - surTemp;
                            Result += dvalue;
                            if (i < boardTempSensorCount - 1)
                            {
                                Result += ",";
                            }
                            if (dvalue > Convert.ToDouble(NowToolingStepAttr.UpperLimit) || dvalue < Convert.ToDouble(NowToolingStepAttr.LowerLimit))
                            {
                                //此次结果不符合
                                //if (uIResult.Equals("OK"))
                                //{
                                uIResult = "OnobardTempSensor" + (i + 1) + ":" + dvalue + " ";
                                //}
                                //else {
                                //    uIResult += "onobardTempSensor" + (i + 1) + ":" + dvalue + " ";
                                //}
                                isNext = false;
                            }
                            streamWriter.WriteLine(nowToolingStepAttr.Id + "_BoardTempSensorCount" + (i < 1 ? "A" : i > 3 ? "C" : "B") + (i % 2 + 1) + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + dvalue + "\t" + (IsNext ? "PASSED" : "FAILED"));
                        }
                        //计算MCU温度
                        if (IsNext)
                        {
                            double dvalue2 = calDates[0][7] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset - surTemp;
                            Result += dvalue2;
                            //Result += ",";
                            if (dvalue2 > Convert.ToDouble(NowToolingStepAttr.UpperLimit) || dvalue2 < Convert.ToDouble(NowToolingStepAttr.LowerLimit))
                            {
                                //此次结果不符合
                                //if (uIResult.Equals("OK"))
                                //{
                                uIResult = "MCUTempSensor" + boardTempSensorCount + ":" + dvalue2 + " ";
                                //}
                                //else
                                //{
                                //    uIResult += "onobardTempSensor" + boardTempSensorCount + ":" + dvalue2 + " ";
                                //}
                                isNext = false;
                            }
                            streamWriter.WriteLine(nowToolingStepAttr.Id + "_MCUTempSensorCount" + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t" + dvalue2 + "\t" + (IsNext ? "PASSED" : "FAILED"));
                            //streamWriter.Write(nowToolingStepAttr.Id + "\t" + nowToolingStepAttr.LowerLimit + "\t" + nowToolingStepAttr.UpperLimit + "\t");
                        }
                        PBValue++;
                        break;
                    case 35:
                        int calCellCount = 0;
                        for (int i = 0; i < portCount && IsNext; i++)
                        {
                            for (; cellCountPerPort[i] - calCellCount != 0 && IsNext; calCellCount++)
                            {
                                //balanceEfficiency(step, k, cellCountArray[i]);
                                balanceEfficiency(step, calCellCount, cellCountArray[i]);
                                //calCellCount++;

                            }

                            if (IsNext)
                            {
                                //Console.WriteLine(CalDates);
                                boardTem(step, cellCountArray[i]);
                            }
                            PBValue++;
                        }
                        //打印数据
                        Console.WriteLine(resultData1);
                        Console.WriteLine(resultData2);
                        Console.WriteLine(resultData3);

                        break;
                }
                //结果记录和显示
                //if (nowToolingStepAttr.Type != 8 && nowToolingStepAttr.Type != 9) {
                //    //写运行结果
                //    streamWriter.WriteLine(Result + "\t" + (IsNext ? "PASSED" : "FAILED"));
                //}
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (nowToolingStepAttr.Type == 31 || nowToolingStepAttr.Type == 32 || nowToolingStepAttr.Type == 33 || nowToolingStepAttr.Type == 34)
                    {
                        step.Result = uIResult;
                    }
                    else
                    {
                        step.Result = Result;
                    }
                    Result = "                                   ";
                    uIResult = "                                   ";
                    DataGrid dg = page.FindName("showStep") as DataGrid;
                    dg.UpdateLayout();
                    dg.ScrollIntoView(dg.Items[j]);
                    DataGridRow dataGridRow = (DataGridRow)dg.ItemContainerGenerator.ContainerFromItem(dg.Items[j]);
                    if (IsNext)
                    {
                        //成功则将当前行标绿
                        dataGridRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66CC99"));
                    }
                    else if (!IsNext)
                    {
                        //失败则将当前行标红
                        dataGridRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
                        ImageButton im = (ImageButton)page.FindName("light");
                        im.Icon = new BitmapImage(new Uri(@"..\images\redButton.png", UriKind.Relative));
                    }


                    // (page.FindName("showStep") as DataGrid).ItemsSource = Steps;
                });

                //PBValue = j + 1;
            }

            IsConfigEnable = true;
            streamWriter.Close();
            fileStream.Close();
        }

        //CAL006
        private void boardTem(ToolingStep step, String port)
        {
            string[] sendIdsTemp = step.SendID.Split('&');
            string[] receIdsTemp = step.ReceID.Split('&');
            string[] upperLimits = step.UpperLimit.Split('&');
            string[] lowerLimits = step.LowerLimit.Split('&');
            //第一步(PC-DUT)
            SendId = sendIdsTemp[0];
            ReceId = receIdsTemp[0];
            //此类型的frame有5个，用逗号隔开
            string[] framesS2 = nowToolingStepAttr.Frame.Split(',');
            ReceFrame = Convert.ToByte(framesS2[4], 16);
            byte[] data = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            nowToolingStepAttr.UpperLimit = Convert.ToDouble(upperLimits[3]);
            nowToolingStepAttr.LowerLimit = Convert.ToDouble(lowerLimits[3]);
            StepNum = 14;
            Indexs[0] = 0;
            //ReceCount = boardTempSensorCount == 5 ? 2 : 3;
            //for (int i = 0; i < receCount; i++)
            //{
            //    Indexs[i] = i;
            //}
            tService.sendData(data);
            if (!IsNext)
            {
                return;
            }

            SendId = sendIdsTemp[1];
            ReceId = receIdsTemp[1];
            StepNum = 15;
            Indexs[0] = 1;
            //Indexs[0] = ReceCount;
            //for (int i = 0; i < receCount; i++)
            //{
            //    Indexs[i] = receCount + i;
            //}
            data = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            tService.sendData(data);
            if (!isNext)
            {
                return;
            }

            //计算
            uIResult = "OK";
            double calResult1 = 0;
            double calResult2 = 0;
            double calResult3 = 0;
            switch (port)
            {
                case "A":
                    calResult1 = CalDates[0][1] - DUTBoardTemp[0][1];
                    calResult2 = CalDates[0][2] - DUTBoardTemp[0][2];

                    //calResult1 = CalDates[0][1] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][1] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult2 = CalDates[0][2] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][2] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult1 = CalDates[0][3] << 8 | CalDates[0][2] - (DUTBoardTemp[0][3] << 8 | DUTBoardTemp[0][2]);
                    //calResult2 = CalDates[0][5] << 8 | CalDates[0][4] - (DUTBoardTemp[0][5] << 8 | DUTBoardTemp[0][4]);

                    break;
                case "B":
                    calResult1 = CalDates[0][3] - DUTBoardTemp[0][3];
                    calResult2 = CalDates[0][4] - DUTBoardTemp[0][4];

                    //calResult1 = CalDates[0][3] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][3] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult2 = CalDates[0][4] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][4] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult1 = CalDates[0][7] << 8 | CalDates[0][6] - (DUTBoardTemp[0][7] << 8 | DUTBoardTemp[0][6]);
                    //calResult2 = CalDates[1][3] << 8 | CalDates[1][2] - (DUTBoardTemp[1][3] << 8 | DUTBoardTemp[1][3]);
                    break;
                case "C":
                    calResult1 = CalDates[0][5] - DUTBoardTemp[0][5];
                    calResult2 = CalDates[0][6] - DUTBoardTemp[0][6];

                    //calResult1 = CalDates[0][5] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][5] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult2 = CalDates[0][6] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
                    //    DUTBoardTemp[0][6] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
                    //calResult1 = CalDates[1][5] << 8 | CalDates[1][4] - (DUTBoardTemp[1][5] << 8 | DUTBoardTemp[1][5]);
                    //calResult2 = CalDates[1][7] << 8 | CalDates[1][6] - (DUTBoardTemp[1][7] << 8 | DUTBoardTemp[1][6]);
                    break;
            }
            calResult3 = CalDates[0][7] - DUTBoardTemp[0][7];
            //calResult3 = CalDates[0][7] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset -
            //            DUTBoardTemp[0][7] * NowToolingStepAttr.Resolution + NowToolingStepAttr.Offset;
            //calResult3 = CalDates[2][3] << 8 | CalDates[2][2] - (DUTBoardTemp[2][3] << 8 | DUTBoardTemp[2][2]);

            if (calResult1 > Convert.ToDouble(upperLimits[3]) || calResult1 < Convert.ToDouble(lowerLimits[3]) ||
                calResult2 > Convert.ToDouble(upperLimits[3]) || calResult2 < Convert.ToDouble(lowerLimits[3]) ||
                calResult3 > Convert.ToDouble(upperLimits[3]) || calResult3 < Convert.ToDouble(lowerLimits[3]))
            {
                //结果不通过
                uIResult = "NG";
                IsNext = false;
                //   return;
            }
            streamWriter.WriteLine(nowToolingStepAttr.Id + "_DUT温升" + port + "1" + "\t" + lowerLimits[3] + "\t" + upperLimits[3] + "\t" + calResult1 + "\t" + (IsNext ? "PASSED" : "FAILED"));
            streamWriter.WriteLine(nowToolingStepAttr.Id + "_DUT温升" + port + "2" + "\t" + lowerLimits[3] + "\t" + upperLimits[3] + "\t" + calResult2 + "\t" + (IsNext ? "PASSED" : "FAILED"));
            streamWriter.WriteLine(nowToolingStepAttr.Id + "_DUT温升MCU" + "\t" + lowerLimits[3] + "\t" + upperLimits[3] + "\t" + calResult3 + "\t" + (IsNext ? "PASSED" : "FAILED"));
        }

        //CAL005计算均衡效率
        private void balanceEfficiency(ToolingStep step, int cellNum, string portNum)
        {
            int delay = 1;
            string[] sendIdsTemp5 = step.SendID.Split('&');
            string[] receIdsTemp5 = step.ReceID.Split('&');
            string[] upperLimits = step.UpperLimit.Split('&');
            string[] lowerLimits = step.LowerLimit.Split('&');
            string[] res = nowToolingStepAttr.ResolutionS.Split('&');
            string[] off = nowToolingStepAttr.OffsetS.Split('&');
            //第一步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            //此类型的frame有5个，用逗号隔开
            string[] framesS2 = nowToolingStepAttr.Frame.Split(',');
            ReceFrame = Convert.ToByte(framesS2[0], 16);
            byte[] data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            nowToolingStepAttr.UpperLimit = Convert.ToDouble(upperLimits[0]);
            nowToolingStepAttr.LowerLimit = Convert.ToDouble(lowerLimits[0]);
            nowToolingStepAttr.Resolution = Convert.ToDouble(res[0]);
            nowToolingStepAttr.Offset = Convert.ToDouble(off[0]);
            StepNum = 1;//第一步：DUT以2A充电
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            Thread.Sleep(delay);//PC静默1s

            //第二步(PC-GS)
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            ReceFrame = Convert.ToByte(framesS2[1], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 2;//第二步：GS放电
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            Thread.Sleep(delay);//PC静默2s

            //第三步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            ReceCount = portCount;
            for (int i = 0; i < portCount; i++)
            {
                Indexs[i] = i;
            }
            ReceFrame = Convert.ToByte(framesS2[2], 16);
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 3;//第三步：反馈DUT各port均衡状态下的信息
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            Console.WriteLine(CalDates);

            //第四步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            ReceCount = 1;
            Indexs[0] = portCount;
            ReceFrame = Convert.ToByte(framesS2[3], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 4;
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            Console.WriteLine(CalDates);

            //第五步(PC-GS)
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            ReceCount = 1;
            Indexs[0] = portCount + 1;
            ReceFrame = Convert.ToByte(framesS2[3], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 5;//第三步：反馈DUT各port均衡状态下的信息
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            //Console.WriteLine(CalDates);

            //关闭均衡
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            ReceFrame = Convert.ToByte(framesS2[5], 16);
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 16;
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }

            //第六步，计算DUT充电均衡效率
            uIResult = "OK";
            for (int i = 0; i < portCount && IsNext; i++)
            {
                String portName = i == 0 ? "A" : i == 1 ? "B" : "C";
                int cellCountThisPort = -1;
                for (int j = 0; j < cellCountArray.Length; j++)
                {
                    if (cellCountArray[j] != null && cellCountArray[j].Equals(portName))
                    {
                        cellCountThisPort = cellCountPerPort[j];
                    }
                }
                if (!(cellCountThisPort > cellNum))
                {
                    continue;
                }
                double temp = ((calDates[i][3] << 8 | calDates[i][2]) * ((calDates[i][5] << 8 | calDates[i][4]) - 8000));
                if (temp == 0)
                {
                    uIResult = "NG";
                    IsNext = false;
                    streamWriter.WriteLine(nowToolingStepAttr.Id + "Cell" + (cellNum + 1) + "_DUT充电均衡效率_port" + portName + "\t" + lowerLimits[0] + "\t" + upperLimits[0] + "\t除数为零\t" + "FAILED");
                }
                else
                {
                    double calResult = ((calDates[portCount][2 * i + 2] << 8 | calDates[portCount][2 * i + 1]) *
                   ((calDates[i][7] << 8 | calDates[i][6]) - 8000)) / temp;
                    resultData1[cellNum * portCount + i] = Math.Round(calResult, 2).ToString();
                    //暂时屏蔽，有真数据时打开
                    if (calResult > Convert.ToDouble(upperLimits[0]) || calResult < Convert.ToDouble(lowerLimits[0]))
                    {
                        //结果不通过
                        uIResult = "NG";
                        IsNext = false;
                        //return;
                    }
                    streamWriter.WriteLine(nowToolingStepAttr.Id +"Cell"+ (cellNum + 1) + "_DUT充电均衡效率_port" + portName + "\t" + lowerLimits[0] + "\t" + upperLimits[0] + "\t" + calResult + "\t" + (IsNext ? "PASSED" : "FAILED"));
                }

            }
            if (!IsNext)
            {
                return;
            }
            //第七步(PC-GS)
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            //此类型的frame有5个，用逗号隔开
            ReceFrame = Convert.ToByte(framesS2[0], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            nowToolingStepAttr.UpperLimit = Convert.ToDouble(upperLimits[1]);
            nowToolingStepAttr.LowerLimit = Convert.ToDouble(lowerLimits[1]);
            StepNum = 7;//第一步：DUT以2A充电
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            Thread.Sleep(delay);//PC静默1s

            //第八步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            ReceFrame = Convert.ToByte(framesS2[1], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 8;//第二步：GS放电
            tService.sendData(data8);
            if (!isNext)
            {
                return;
            }
            Thread.Sleep(delay);//PC静默2s

            //第九步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            ReceCount = portCount;
            for (int i = 0; i < portCount; i++)
            {
                Indexs[i] = portCount + 2 + i;
            }
            ReceFrame = Convert.ToByte(framesS2[2], 16);
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 9;
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            Console.WriteLine(CalDates);

            //第十步(PC-DUT)
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            ReceCount = 1;
            Indexs[0] = portCount * 2 + 2;
            ReceFrame = Convert.ToByte(framesS2[3], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 10;
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            Console.WriteLine(CalDates);

            //第十一步(PC-GS)
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            ReceCount = 1;
            Indexs[0] = portCount * 2 + 3;
            ReceFrame = Convert.ToByte(framesS2[3], 16);
            data8 = new byte[] { ReceFrame, (byte)(cellNum + 1), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 11;//第三步：反馈DUT各port均衡状态下的信息
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            //test
            // Console.WriteLine(CalDates);

            //关闭均衡
            SendId = sendIdsTemp5[1];
            ReceId = receIdsTemp5[1];
            ReceFrame = Convert.ToByte(framesS2[5], 16);
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            StepNum = 16;
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }
            SendId = sendIdsTemp5[0];
            ReceId = receIdsTemp5[0];
            data8 = new byte[] { ReceFrame, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            tService.sendData(data8);
            if (!IsNext)
            {
                return;
            }

            //第十二步，计算DUT放电均衡效率
            for (int i = 0; i < portCount && IsNext; i++)
            {
                String portName = i == 0 ? "A" : i == 1 ? "B" : "C";
                int cellCountThisPort = -1;
                for (int j = 0; j < cellCountArray.Length; j++)
                {
                    if (cellCountArray[j]!=null&&cellCountArray[j].Equals(portName)) {
                        cellCountThisPort = cellCountPerPort[j];
                    }
                }
                if (!(cellCountThisPort > cellNum)) {
                    continue;
                }
                //if (cellCount[i] > cellNum) {
                //    break;
                //}
                double temp= (((calDates[portCount + 2 + i][3] << 8 | calDates[portCount + 2 + i][2]) * ((calDates[portCount + 2 + i][5] << 8 | calDates[portCount + 2 + i][4]) - 8000)));
                if (temp == 0)
                {
                    uIResult = "NG";
                    IsNext = false;
                    streamWriter.WriteLine(nowToolingStepAttr.Id + "Cell" + (cellNum + 1) + "_DUT放电均衡效率_portCount" +
                       portName + "\t" + lowerLimits[1] + "\t" + upperLimits[1] + "\t除数为零\t" + "FAILED");

                }
                else {
                    double calResult = ((calDates[2 * portCount + 2][2 * i + 2] << 8 | calDates[2 * portCount + 2][2 * i + 1]) *
                                       ((calDates[portCount + 2 + i][7] << 8 | calDates[portCount + 2 + i][6]) - 8000)) / temp;                                      
                    resultData2[cellNum * portCount + i] = Math.Round(calResult, 2).ToString();
                    if (calResult > Convert.ToDouble(upperLimits[1]) || calResult < Convert.ToDouble(lowerLimits[1]))
                    {
                        //暂时屏蔽，接上真实环境后打开
                        //结果不通过
                        uIResult = "NG";
                        IsNext = false;
                        //return;
                    }
                    streamWriter.WriteLine(nowToolingStepAttr.Id + "Cell" + (cellNum + 1) + "_DUT放电均衡效率_portCount" +
                       portName + "\t" + lowerLimits[1] + "\t" + upperLimits[1] + "\t" + calResult + "\t" + (IsNext ? "PASSED" : "FAILED"));
                }
                
            }
            if (!IsNext)
            {
                return;
            }
            //第十三步
            for (int i = 0; i < portCount && IsNext; i++)
            {
                String portName = i == 0 ? "A" : i == 1 ? "B" : "C";
                int cellCountThisPort = -1;
                for (int j = 0; j < cellCountArray.Length; j++)
                {
                    if (cellCountArray[j] != null && cellCountArray[j].Equals(portName))
                    {
                        cellCountThisPort = cellCountPerPort[j];
                    }
                }
                if (!(cellCountThisPort > cellNum))
                {
                    continue;
                }
                double calResult1 = (calDates[portCount][2 * i + 2] << 8 | calDates[portCount][2 * i + 1]) * nowToolingStepAttr.Resolution + nowToolingStepAttr.Offset -
                    ((calDates[2 * portCount + 3][2 * i + 2] << 8 | calDates[2 * portCount + 3][2 * i + 1]) * nowToolingStepAttr.Resolution + nowToolingStepAttr.Offset);
                double calResult2 = (calDates[portCount + 1][2 * i + 2] << 8 | calDates[portCount + 1][2 * i + 1]) * nowToolingStepAttr.Resolution + nowToolingStepAttr.Offset -
                    ((calDates[2 * portCount + 2][2 * i + 2] << 8 | calDates[2 * portCount + 2][2 * i + 1]) * nowToolingStepAttr.Resolution + nowToolingStepAttr.Offset);
                if (calResult1 > Convert.ToDouble(upperLimits[2]) || calResult1 < Convert.ToDouble(lowerLimits[2]) ||
                    calResult2 > Convert.ToDouble(upperLimits[2]) || calResult2 < Convert.ToDouble(lowerLimits[2]))
                {
                    //结果不通过
                    uIResult = "NG";
                    IsNext = false;
                    //return;
                }
                streamWriter.WriteLine(nowToolingStepAttr.Id + "Cell" + (cellNum + 1) + "_压差_DUT充电port" + portName + "\t" + lowerLimits[2] + "\t" + upperLimits[2] + "\t" + calResult1 + "\t" + (IsNext ? "PASSED" : "FAILED"));
                streamWriter.WriteLine(nowToolingStepAttr.Id + "Cell" + (cellNum + 1) + "_压差_DUT放电port" + portName + "\t" + lowerLimits[2] + "\t" + upperLimits[2] + "\t" + calResult2 + "\t" + (IsNext ? "PASSED" : "FAILED"));
                //保存数据
                resultData3[cellNum * portCount] = calResult1.ToString();
                resultData3[cellNum * portCount + 1] = calResult2.ToString();
            }
            //测试用
            //isNext = false;
            //return;
        }

        //读取工装步骤文件
        private void runReadToolingStepsFileCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.gy)|*.gy";
            dialog.ShowDialog();
            String path = dialog.FileName;
            ToolingStepsFilePath = path;
            if (path == "")
            {
                return;
            }

            FileStream fs = new FileStream(ToolingStepsFilePath, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            String s = null;
            steps = new ObservableCollection<ToolingStep>();
            while ((s = read.ReadLine()) != null)
            {
                String[] sArray = s.Split(',');
                ToolingStep t = new ToolingStep();
               // t.Id = sArray[0];
                t.IsCheck = true;
                t.UpperLimit = sArray[1];
                t.LowerLimit = sArray[2];
                steps.Add(t);
            }
            read.Close();
            fs.Close();
            (page.FindName("showStep") as DataGrid).ItemsSource = Steps;

            //读取工装步骤参数并存到内存
            toolingStepAttr = new Dictionary<string, ToolingStepsAttr>();
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;//忽略文档里面的注释
            XmlReader reader = XmlReader.Create("Config/ToolingAttr.xml", settings);
            xmlDoc.Load(reader);
            XmlNode xn = xmlDoc.SelectSingleNode("toolingsteps");
            xn = xn.SelectSingleNode("FCT1");

            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                ToolingStepsAttr t = new ToolingStepsAttr();
                XmlElement xe = (XmlElement)xn1;
                XmlNodeList xnl0 = xe.ChildNodes;
                t.Id = xnl0.Item(0).InnerText;
                t.Type = Convert.ToInt16(xnl0.Item(1).InnerText);
                t.Frame = xnl0.Item(2).InnerText;
                //t.Frame = Convert.ToByte((xnl0.Item(2).InnerText), 16);
                toolingStepAttr.Add(t.Id, t);
            }
            reader.Close();
        }

        private void RunCbI18nClickCommand(System.Windows.Controls.ComboBox comboBox)
        {
            ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            page.Resources.MergedDictionaries.Add(langRd);
            //改变属性的值
            XmlDocument doc = new XmlDocument();
            //doc.Load("../../Config/Config.xml");
            doc.Load("Config/Config.xml");
            XmlNode xn = doc.SelectSingleNode("/appSettings/i18n");
            xn.InnerText = selectI18n.ToString();

            //doc.Save("../../Config/Config.xml");
            doc.Save("Config/Config.xml");
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                //ModernDialog.ShowMessage("只能连接一个从机", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["effectiveafterrestart"], (string)page.Resources["tips"], MessageBoxButton.OK);

            });
        }

        private void RunConnectCanCommand(Button button)
        {
            try
            {
                if (!connectstate)
                {
                    BaudRateModel item = canBaudRateList[SelectRate];
                    CANSDK.m_devind = (uint)canIndexList[SelectDeviceIndex];//设备索引号;
                    CANSDK.m_canind = (uint)SelectCanChannelIndex;//can通道
                    if (cancode == 1) CANSDK.VCI_CloseDevice(CANSDK.m_devtype, CANSDK.m_devind);
                    cancode = CANSDK.VCI_OpenDevice(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_reserved);
                    if (cancode == 0)
                    {
                        //ModernDialog.ShowMessage("打开设备失败，请连接CAN卡！", "提示", MessageBoxButton.OK);
                        ModernDialog.ShowMessage((string)page.Resources["opencanwrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        return;
                    }
                    //获取选择的波特率
                    CANSDK.Caninit(item.Time0, item.Time1);
                    uint result = CANSDK.VCI_InitCAN(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref CANSDK.config);
                    CANSDK.VCI_ERR_INFO error = new CANSDK.VCI_ERR_INFO();
                    result = CANSDK.VCI_ReadErrInfo(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref error);
                    uint code = error.ErrCode;

                    result = CANSDK.VCI_StartCAN(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind);
                    if (result == 1)
                    {
                        receiveThread = new Thread(new ThreadStart(ReceiveDataThread));
                        isReceive = true;
                        receiveThread.IsBackground = true;
                        receiveThread.Start();

                        // button.Content = "断开";
                        button.SetResourceReference(ContentControl.ContentProperty, "disconnect");
                        connectstate = true;
                    }
                }
                else
                {
                    //复位CAN
                    CANSDK.VCI_ResetCAN(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind);
                    //断开CAN
                    CANSDK.VCI_CloseDevice(CANSDK.m_devtype, CANSDK.m_devind);
                    cancode = 0;
                    //button.Content = "连接";
                    button.SetResourceReference(ContentControl.ContentProperty, "connect");
                    connectstate = false;
                    isReceive = false;
                }

            }
            catch (Exception ex)
            {
                //ModernDialog.ShowMessage("打开设备异常！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["opendevicewrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
        }


        //接收线程
        private void ReceiveDataThread()
        {
            while (isReceive)
            {
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CANSDK.VCI_CAN_OBJ)) * (Int32)RECEIVELEN);
                uint receiveRealLen = CANSDK.VCI_Receive(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, pt, RECEIVELEN, 500);
                Thread.Sleep(10);
                if (receiveRealLen <= 0) continue;
                for (int i = 0; i < receiveRealLen; i++)
                {
                    CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(CANSDK.VCI_CAN_OBJ))), typeof(CANSDK.VCI_CAN_OBJ));
                    ThreadPool.QueueUserWorkItem(new WaitCallback(tService.parseDataThread), obj);
                }
                Marshal.FreeHGlobal(pt);
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
