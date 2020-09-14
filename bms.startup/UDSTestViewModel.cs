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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using bms.startup.i18n;
using bms.startup.util;

namespace bms.startup
{
    class UDSTestViewModel : INotifyPropertyChanged
    {
        private UDSTestMainWindow page;
        private bool connectstate = false;//can连接状态
        private uint cancode = 0;
        private Thread receiveThread; //数据接受线程
        private bool isReceive = true;//是否循环接收线程
        private const uint RECEIVELEN = 100;//一次性可接收数据帧数 
        private UDSTestService uDSTestService = new UDSTestService();
        private Dictionary<string, string[]> sidAndDidDic = null;//sid和did二级联动下拉框数据
        private Dictionary<string, string[]> sidAndNRCDic = null;//sid和对应的NRC
        private ComboBox sidCb = null;
        private ComboBox didCb = null;
        private int MAXSHOWUDSLOG = 100;//UDSLog最大显示行数
        private int udsLongCount = 0;//当前UDSLog显示行数
        private bool isSendHeart = false;//是否发送心跳
        private Thread sendHeartThread = null;//发送心跳的线程
        List<string> dataPerBlock = new List<string>();//保存每个块的数据
        List<int> addPerBlock = new List<int>();//保存每一个块的起始地址
        private const string flashDriver = "./File/MPC5746rFlashDriver.srec";//flashDriver刷写地址
        //private const string flashDriver = "E:\\国轩资料\\UDS标准\\tataBootloader\\Tata_Flash_Driver_Corrected.hex";
        private string info = "";//界面显示信息

        public string Info
        {
            get { return info; }
            set { info = value; OnPropertyChanged("Info"); }
        }
        //界面绑定变量
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

        private int selectRate = 4;//波特率
        public int SelectRate
        {
            get { return selectRate; }
            set { selectRate = value; OnPropertyChanged("SelectRate"); }
        }

        private Dictionary<string, List<string>> sidProcess = new Dictionary<string, List<string>>();//List中保存在string服务发出前需要发出的服务列表

        private ObservableCollection<CategoryInfo> categoryI18nList = new ObservableCollection<CategoryInfo>();//语言列表
        public ObservableCollection<CategoryInfo> CategoryI18nList
        {
            get { return categoryI18nList; }
            set { categoryI18nList = value; OnPropertyChanged("CategoryI18nList"); }
        }
        private int selectI18n = 0;//语言
        public int SelectI18n
        {
            get { return selectI18n; }
            set { selectI18n = value; OnPropertyChanged("SelectI18n"); }
        }

        private string sendID = "7E1";//UDS发送ID
        //private string sendID = "785";//UDS发送ID
        public string SendID
        {
            get { return sendID; }
            set { sendID = value; OnPropertyChanged("SendID"); uDSTestService.SendID = value.ToUpper(); }
        }

        //private string receiveID = "78D";//UDS接收ID
        private string receiveID = "7E9";//UDS接收ID
        public string ReceiveID
        {
            get { return receiveID; }
            set { receiveID = value; OnPropertyChanged("ReceiveID"); uDSTestService.ReceiveID = value.ToUpper(); }
        }

        private string appFile;//bootloader刷写功能中app文件地址

        public string AppFile
        {
            get { return appFile; }
            set { appFile = value; OnPropertyChanged("AppFile"); }
        }
        private string fillingFile;//app的s19填充FF文件路径
        public string FillingFile
        {
            get
            {
                return fillingFile;
            }

            set
            {
                fillingFile = value;
                OnPropertyChanged("FillingFile");
            }
        }
        private string combin;//组合帧
        public string Combin
        {
            get
            {
                return combin;
            }

            set
            {
                combin = value;
                OnPropertyChanged("Combin");
            }
        }
        private string leftFrame = "";//人工输入的数据部分

        public string LeftFrame
        {
            get { return leftFrame; }
            set { leftFrame = value; OnPropertyChanged("LeftFrame"); }
        }
        private string udsLog;//打印数据信息

        public string UdsLog
        {
            get { return udsLog; }
            set { udsLog = value; OnPropertyChanged("UdsLog"); }
        }

        private int maxpb = 100;//进度条最大值
        public int Maxpb
        {
            get { return maxpb; }
            set { maxpb = value; OnPropertyChanged("Maxpb"); }
        }

        private int pBValue = 0;//进度条当前值
        public int PBValue
        {
            get { return pBValue; }
            set { pBValue = value; OnPropertyChanged("PBValue"); }
        }

        private string data22 = "010203040506070809";//测试用22填写数值

        public string Data22
        {
            get { return data22; }
            set { data22 = value; OnPropertyChanged("Data22"); }
        }

        public UDSTestViewModel(UDSTestMainWindow p)
        {
            page = p;
            Init();
        }
        private string did;
        public string Did
        {
            get
            {
                return did;
            }

            set
            {
                did = value;
                OnPropertyChanged("Did");
            }
        }

        private bool hexOrSrec=true;
        public bool HexOrSrec
        {
            get
            {
                return hexOrSrec;
            }

            set
            {
                hexOrSrec = value;
                OnPropertyChanged("HexOrSrec");
            }
        }
        //界面绑定方法
        public DelegateCommand<Button> ConnectCanCommand { get; set; }
        public DelegateCommand<ComboBox> CbI18nClickCommand { get; set; }//语言栏按钮
        public DelegateCommand<Object> RunFun { get; set; }// 带参数的绑定方法，以参数标志函数功能
        public DelegateCommand<RichTextBox> RtbLog_TextChanged { get; set; }// udsLog显示框文字变动时
        //初始化
        private void Init()
        {
            ConsoleManager.Show();//打开信息打印窗口
            //开启心跳线程
            sendHeartThread = new Thread(sendHeart);
            sendHeartThread.Start();
            InitSidAndDidCB();

            sendHeartThread = new Thread(sendHeart);

            categoryI18nList.Add(new CategoryInfo() { Name = "English", Value = "en_US", sourceName = "English" });
            categoryI18nList.Add(new CategoryInfo() { Name = "中文", Value = "zh_CN", sourceName = "Chinese" });

            sidProcess.Add("27", new List<string> { "1003" });
            sidProcess.Add("31", new List<string> { "1003", "2701" });
            sidProcess.Add("11", new List<string> { "1003", "2701" });
            sidProcess.Add("28", new List<string> { "1003", "2701" });


            uDSTestService.changeViewForGen2VM += changeView;
            ConnectCanCommand = new DelegateCommand<Button>(RunConnectCanCommand);
            CbI18nClickCommand = new DelegateCommand<ComboBox>(RunCbI18nClickCommand);//国际化选框
            RunFun = new DelegateCommand<Object>(RunSendFrame);
            RtbLog_TextChanged = new DelegateCommand<RichTextBox>(RunRtbLog_TextChanged);
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

        }
        private void RunRtbLog_TextChanged(RichTextBox richTextBox)
        {
            // RichTextBox rtbLog = page.FindName("");
            //richTextBox.SelectionStart = richTextBox.Text.Length; //Set the current caret position at the end
            //richTextBox.ScrollToCaret(); //Now scroll it automatically
        }

        public void send(Object o)
        {
            //Thread.Sleep(10000);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Button sendBtn = page.FindName("send") as Button;
                sendBtn.IsEnabled = false;
            });
            List<Object[]> process = (List<Object[]>)o;
            String frameS;
            byte sid;
            byte did;
            uDSTestService.IsNext = true;
            //bool isNext = true;
            for (int i = 0; i < process.Count && uDSTestService.IsNext; i++)
            {
                uDSTestService.IsNext = false;
                Object[] os = process[i];
                frameS = (string)os[0];
                sid = (byte)os[1];
                // did = (byte)os[2];//xg1
                //did==0xAA为了适配在没有子功能码的情况下
                //if (frameS.Length <= 5 * 2 || (did == 0xAA && frameS.Length <= 6 * 2))//xg2
                if (frameS.Length <= 6 * 2)
                {
                    //单帧发送
                    byte[] b = new byte[8];
                    for (int j = 0; j < b.Length; j++)
                    {
                        b[j] = 0xAA;//用55填充，有的用AA，根据需要改
                    }
                    b[0] = (byte)(frameS.Length / 2 + 1);
                    //b[0] = (did == 0xAA ? (byte)(frameS.Length / 2 + 1) : (byte)(frameS.Length / 2 + 2));//xg3
                    b[1] = sid;
                    //b[2] = did;
                    //byte[] temp = DataConverter.strToHexByte(frameS);
                    //byte[] temp = did==0xAA? DataConverter.strToHexByte(frameS): DataConverter.strToHexByte(did+frameS);//xg3
                    byte[] temp = DataConverter.strToHexByte(frameS);
                    //Array.Copy(temp, 0, b, 3, temp.Length);
                    Array.Copy(temp, 0, b, 2, temp.Length);
                    //o[0] = b;
                    //o[1] = SendID;
                    if (b[1] == 0x10 && b[2] == 0x03)
                    {
                        //1003进入扩展会话，打开心跳
                        //if (!isDoBootLoader)
                        //{
                        // isSendHeart = true;
                        //}
                        //if (sendHeartThread == null)
                        //{
                        //    sendHeartThread = new Thread(sendHeart);
                        //}
                        //sendHeartThread.Start();
                    }
                    uDSTestService.sendFrame(b);
                    //new Thread(new ParameterizedThreadStart(uDSTestService.sendFrame)).Start(b);
                    // new Thread( sendFrame(b)).Start();
                }
                else
                {
                    //多帧发送
                    Object[] os2 = new Object[3];
                    os2[0] = frameS;
                    os2[1] = sid;
                    //os2[2] = did;
                    uDSTestService.sendMultiFrame(os2);
                    //new Thread(new ParameterizedThreadStart(uDSTestService.sendMultiFrame)).Start(os);
                    //sendMultiFrame(frameS, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text));
                }
            }

            //isSendHeart = false;
            //if (sendHeartThread != null)
            //{
            //    sendHeartThread.Abort();
            //}
            //sendHeartThread = null;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Button sendBtn = page.FindName("send") as Button;
                sendBtn.IsEnabled = true;
            });
        }

        //测试专用
        private int ter;

        public int Ter
        {
            get { return ter; }
            set { ter = value; OnPropertyChanged("Ter"); }
        }

        

        private void send2(Object o)
        {
            isSendHeart = false;
            //Thread.Sleep(10000);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Button sendBtn = page.FindName("send") as Button;
                sendBtn.IsEnabled = false;
            });
            List<Object[]> process = (List<Object[]>)o;
            String frameS;
            byte sid;
            byte did;
            uDSTestService.IsNext = true;
            //bool isNext = true;
            for (int i = 0; i < process.Count && uDSTestService.IsNext; i++)
            {

                uDSTestService.IsNext = false;
                Object[] os = process[i];
                frameS = (string)os[0];
                sid = (byte)os[1];
                did = (byte)os[2];
                //did==0xAA为了适配在没有子功能码的情况下
                if (frameS.Length <= 5 * 2 || (did == 0xAA && frameS.Length <= 6 * 2))
                {
                    //单帧发送
                    byte[] b = new byte[8];
                    for (int j = 0; j < b.Length; j++)
                    {
                        b[j] = 0xAA;//用55填充，有的用AA，根据需要改
                    }
                    b[0] = (did == 0xAA ? (byte)(frameS.Length / 2 + 1) : (byte)(frameS.Length / 2 + 2));
                    b[1] = sid;
                    b[2] = did;
                    byte[] temp = DataConverter.strToHexByte(frameS);
                    Array.Copy(temp, 0, b, 3, temp.Length);
                    //o[0] = b;
                    //o[1] = SendID;
                    if (b[1] == 0x10 && b[2] == 0x03)
                    {
                        //1003进入扩展会话，打开心跳
                        //if (!isDoBootLoader)
                        //{
                        // isSendHeart = true;
                        //}
                        //if (sendHeartThread == null)
                        //{
                        //    sendHeartThread = new Thread(sendHeart);
                        //}
                        //sendHeartThread.Start();
                    }
                    uDSTestService.sendFrame(b);
                    //new Thread(new ParameterizedThreadStart(uDSTestService.sendFrame)).Start(b);
                    // new Thread( sendFrame(b)).Start();
                }
                else
                {
                    //多帧发送
                    Object[] os2 = new Object[3];
                    os2[0] = frameS;
                    os2[1] = sid;
                    os2[2] = did;
                    uDSTestService.sendMultiFrame(os2);
                    //new Thread(new ParameterizedThreadStart(uDSTestService.sendMultiFrame)).Start(os);
                    //sendMultiFrame(frameS, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text));
                }
                Console.WriteLine(DataConverter.hex2String(sid) + "," + DataConverter.hex2String(did) + ":" + uDSTestService.IsNext);
                Thread.Sleep(Ter);
            }

            //isSendHeart = false;
            //if (sendHeartThread != null)
            //{
            //    sendHeartThread.Abort();
            //}
            //sendHeartThread = null;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Button sendBtn = page.FindName("send") as Button;
                sendBtn.IsEnabled = true;
            });
        }
        private int blockNum = 1;//块号，变化顺序为1,2,3,...,255,0,1,2,...
        //界面绑定方法
        private void RunSendFrame(Object i)
        {
            if (!connectstate)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });
                return;
            }
            switch (Convert.ToInt32(i))
            {

                case 0:
                    if (Combin != null)
                    {
                        String[] combins = combin.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<Object[]> combinProcess = new List<Object[]>();
                        //最后会有一个多出来的空行
                        for (int j = 0; j < combins.Length - 1; j++)
                        {
                            String[] s = combins[j].Split(',');

                            //if (s[1].Equals("null")&&s[])
                            combinProcess.Add(new object[] { (s[1].Equals("null") ? "" : s[1]) + (s[2].Equals("null") ? "" : s[2]) + s[3], DataConverter.strToHexByte(s[0])[0] });
                        }
                        new Thread(new ParameterizedThreadStart(send)).Start(combinProcess);

                    }
                    else
                    {
                        //String frameS = LeftFrame.Trim();
                        String frameS = (Did == null || DataConverter.hexStringToInt(Did) > 65535 || DataConverter.hexStringToInt(Did) <= 0) ? Regex.Replace(LeftFrame, @"\s", "") : (Did + Regex.Replace(LeftFrame, @"\s", ""));
                        if (frameS == null || frameS.Length % 2 == 1)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                            return;
                        }
                        if (!(new Regex("^[A-Fa-f0-9]+$").IsMatch(SendID) && new Regex("^[A-Fa-f0-9]+$").IsMatch(ReceiveID)) || SendID.Length > 4 || ReceiveID.Length > 4)
                        {
                            //发送ID或者接收ID不符合格式
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage((string)page.Resources["wrongID"], (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                            return;
                        }
                        if (!(frameS == "" || new Regex("^[A-Fa-f0-9]+$").IsMatch(frameS)))
                        {
                            //输入的数据不符合要求
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                            return;
                        }
                        List<Object[]> process = new List<Object[]>();
                        if (sidProcess.ContainsKey(sidCb.Text))
                        {
                            //在此服务发送前需要先发送其他服务
                            foreach (string s in sidProcess[sidCb.Text])
                            {
                                Object[] o = new Object[3];
                                o[0] = s.Substring(2);
                                o[1] = (byte)DataConverter.string2Hex(s.Substring(0, 2));
                                // o[2] = (byte)DataConverter.string2Hex(s.Substring(2, 2));
                                process.Add(o);
                                // send(s.Substring(4), (byte)DataConverter.string2Hex(s.Substring(0,2)), (byte)DataConverter.string2Hex(s.Substring(2,2)));
                            }
                        }
                        //if (didCb.Text.Equals("null"))
                        //{
                        //AA表示没有子功能码
                        //process.Add(new object[] { frameS, (byte)DataConverter.string2Hex(sidCb.Text), (byte)0xAA });
                        process.Add(new object[] { didCb.Text.Equals("null") ? frameS : (didCb.Text + frameS), (byte)DataConverter.string2Hex(sidCb.Text) });
                        //}
                        //else
                        //{
                        //process.Add(new object[] { frameS, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                        //}

                        new Thread(new ParameterizedThreadStart(send)).Start(process);
                    }
                    // send(frameS, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text));

                    break;
                case 1:
                    //清屏
                    Paragraph p = page.FindName("showUdsLog") as Paragraph;
                    Inline inline = null;

                    while ((inline = p.Inlines.FirstInline) != null)
                    {
                        p.Inlines.Remove(inline);
                    }
                    udsLongCount = 0;
                    break;
                case 2:
                    //发送心跳
                    if (isSendHeart)
                    {
                        //关闭心跳
                        isSendHeart = false;
                        Button sendHeartBtn = page.FindName("sendHeart") as Button;
                        sendHeartBtn.Content = (string)page.Resources["sendHeart"];
                    }
                    else
                    {
                        //发送心跳
                        isSendHeart = true;
                        Button sendHeartBtn = page.FindName("sendHeart") as Button;
                        sendHeartBtn.Content = (string)page.Resources["stopHeart"];
                        new Thread(sendHeart).Start();
                    }
                    break;
                case 3:
                    //读取bootloader app文件
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Multiselect = false;//该值确定是否可以选择多个文件
                    dialog.Title = "请选择文件夹";
                    dialog.Filter = "所有文件(*.*)|*.*";
                    dialog.ShowDialog();
                    String path = dialog.FileName;
                    AppFile = path;
                    if (path == "")
                    {
                        return;
                    }
                    break;
                case 4:
                    //bootloader刷写
                    new Thread(runBootLoader).Start();
                    break;
                case 5:
                    //测试——去掉部分服务前缀的1003发送
                    //String frameS = LeftFrame.Trim();
                    String frameS2 = Regex.Replace(LeftFrame, @"\s", "");
                    if (frameS2 == null || frameS2.Length % 2 == 1)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    if (!(new Regex("^[A-Fa-f0-9]+$").IsMatch(SendID) && new Regex("^[A-Fa-f0-9]+$").IsMatch(ReceiveID)) || SendID.Length > 4 || ReceiveID.Length > 4)
                    {
                        //发送ID或者接收ID不符合格式
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongID"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    if (!(frameS2 == "" || new Regex("^[A-Fa-f0-9]+$").IsMatch(frameS2)))
                    {
                        //输入的数据不符合要求
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    List<Object[]> process2 = new List<Object[]>();
                    if (sidProcess.ContainsKey(sidCb.Text))
                    {
                        //在此服务发送前需要先发送其他服务
                        foreach (string s in sidProcess[sidCb.Text])
                        {
                            if (s.Equals("1003"))
                            {
                                continue;
                            }
                            Object[] o = new Object[3];
                            o[0] = s.Substring(4);
                            o[1] = (byte)DataConverter.string2Hex(s.Substring(0, 2));
                            o[2] = (byte)DataConverter.string2Hex(s.Substring(2, 2));
                            process2.Add(o);
                            // send(s.Substring(4), (byte)DataConverter.string2Hex(s.Substring(0,2)), (byte)DataConverter.string2Hex(s.Substring(2,2)));
                        }
                    }
                    process2.Add(new object[] { frameS2, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                    new Thread(new ParameterizedThreadStart(send)).Start(process2);
                    break;
                case 6:
                    //测试——去掉部分服务前缀的1003发送,并重复多次发送
                    //String frameS = LeftFrame.Trim();
                    String frameS3 = Regex.Replace(LeftFrame, @"\s", "");
                    if (frameS3 == null || frameS3.Length % 2 == 1)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    if (!(new Regex("^[A-Fa-f0-9]+$").IsMatch(SendID) && new Regex("^[A-Fa-f0-9]+$").IsMatch(ReceiveID)) || SendID.Length > 4 || ReceiveID.Length > 4)
                    {
                        //发送ID或者接收ID不符合格式
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongID"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    if (!(frameS3 == "" || new Regex("^[A-Fa-f0-9]+$").IsMatch(frameS3)))
                    {
                        //输入的数据不符合要求
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["wrongdata"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    List<Object[]> process3 = new List<Object[]>();
                    if (sidProcess.ContainsKey(sidCb.Text))
                    {
                        //在此服务发送前需要先发送其他服务
                        foreach (string s in sidProcess[sidCb.Text])
                        {
                            Object[] o = new Object[3];
                            o[0] = s.Substring(4);
                            o[1] = (byte)DataConverter.string2Hex(s.Substring(0, 2));
                            o[2] = (byte)DataConverter.string2Hex(s.Substring(2, 2));
                            process3.Add(o);
                            // send(s.Substring(4), (byte)DataConverter.string2Hex(s.Substring(0,2)), (byte)DataConverter.string2Hex(s.Substring(2,2)));
                        }
                    }
                    process3.Add(new object[] { frameS3, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                    process3.Add(new object[] { frameS3, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                    process3.Add(new object[] { frameS3, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                    process3.Add(new object[] { frameS3, (byte)DataConverter.string2Hex(sidCb.Text), (byte)DataConverter.string2Hex(didCb.Text) });
                    new Thread(new ParameterizedThreadStart(send2)).Start(process3);
                    break;
                case 7:
                    //changeFileCRC(AppFile);
                    changeFileS19(AppFile);
                    break;
                case 8:
                    //添加组合帧
                    Combin += sidCb.Text + "," + didCb.Text + "," + ((Did == null || Did.Equals("") || DataConverter.hexStringToInt(Did) > 65535 || DataConverter.hexStringToInt(Did) <= 0) ? "null" : Did) + "," + Regex.Replace(LeftFrame, @"\s", "") + "\r\n";
                    break;
                case 9:
                    //组合框清屏
                    Combin = null;
                    break;
                case 10:
                    //读取需要填充FF的s19格式的app文件
                    OpenFileDialog dialog2 = new OpenFileDialog();
                    dialog2.Multiselect = false;//该值确定是否可以选择多个文件
                    dialog2.Title = "请选择文件夹";
                    dialog2.Filter = "所有文件(*.*)|*.*";
                    dialog2.ShowDialog();
                    String path2 = dialog2.FileName;
                    FillingFile = path2;
                    if (path2 == "")
                    {
                        return;
                    }
                    break;
                case 11:
                    fillFile(FillingFile);
                    break;
            }
        }

        public void fillFile(String path) {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            List<String> lastLines = new List<string>();
            String line1=null;
            String line2=null;
            String s = null;
            for(int i = 0;(s= read.ReadLine()) != null; i++)
            {
                if (i == 0)
                {
                    line1 = s;
                }
                else if (i == 1)
                {
                    line2 = s;
                }
                else {
                    String head_s = s.Substring(1, 1);
                    if (head_s != "0" && head_s != "1" && head_s != "2" && head_s != "3"&&head_s!="5") {
                        lastLines.Add(s);
                    }
                }
            }
            //String line1 = read.ReadLine();
            //String line2 = read.ReadLine();
            int addCount = 0;
            switch (line2.Substring(1, 1)) {
                case "1":
                    addCount = 2;
                    break;
                case "2":
                    addCount = 3;
                    break;
                case "3":
                    addCount = 4;
                    break;
            }
            String head = line2.Substring(0, 4);
            int countPerLine = DataConverter.hexStringToInt(line2.Substring(2, 2))- addCount - 1;//数据行每行多少个数据
            int add = DataConverter.hexStringToInt(line2.Substring(4, addCount * 2));
            FileStream fsWrite = new FileStream("./File/FillingFile.s19", FileMode.Create);
            StreamWriter write = new StreamWriter(fsWrite, Encoding.Default);
            
            write.WriteLine(line1);
            bool isIntegerRow = false;//标志最后一行是否是countPerLine个数据
            //int position = 0;
            int lineNum = 0;//数据行的行号
            preHexFileForS19ToOneBlock(path);
            int allLineCount = dataPerBlock[0].Length / 2 / countPerLine;//数据行有多少行
            if (dataPerBlock[0].Length / 2 % countPerLine != 0) {
                //allLineCount++;
                isIntegerRow = true;
            }
            while (lineNum<allLineCount) {
                String thisLine = head + DataConverter.intToHexString2(add, addCount*2)+dataPerBlock[0].Substring(countPerLine * (lineNum++)*2, countPerLine*2);
                //String sss = DataConverter.getAllBytesSum3(DataConverter.strToHexByte(thisLine.Substring(2))).ToString("X2");
                write.WriteLine(thisLine + DataConverter.getAllBytesSum3(DataConverter.strToHexByte(thisLine.Substring(2))).ToString("X2"));
                add += countPerLine;
            }
            if (isIntegerRow) {
                //如果最后一行不是整的countPerLine个数据
                String thisLine = dataPerBlock[0].Substring(countPerLine * (lineNum++) * 2);
                thisLine = head.Substring(0, 2) + DataConverter.intToHexString2(thisLine.Length / 2+1+ addCount, 2)+
                    DataConverter.intToHexString2(add, addCount * 2)+ thisLine;
                write.WriteLine(thisLine + DataConverter.getAllBytesSum3(DataConverter.strToHexByte(thisLine.Substring(2))).ToString("X2"));
            }
            for (int i = 0; i < lastLines.Count; i++) {
                write.WriteLine(lastLines[i]);
            }
            write.Close();
            fsWrite.Close();
            read.Close();
            fs.Close();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ModernDialog.ShowMessage((string)page.Resources["finish"], (string)page.Resources["tips"], MessageBoxButton.OK);
            });
        }

        private void changeFileCRC(String path) {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            FileStream fsWrite = new FileStream("./File/MPC5746rFlashDriver.hex", FileMode.Create);

            StreamWriter write = new StreamWriter(fsWrite, Encoding.Default);
            String s;
            while ((s = read.ReadLine()) != null) {
                String data = s.Substring(1, s.Length - 3);
                String s2 = ":" + DataConverter.getAllBytesSum2(DataConverter.strToHexByte(data));
                write.WriteLine(":" + data + (DataConverter.getAllBytesSum2(DataConverter.strToHexByte(data))).ToString("X2"));
            }
            write.Close();
            fsWrite.Close();
            read.Close();
            fs.Close();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ModernDialog.ShowMessage((string)page.Resources["finish"], (string)page.Resources["tips"], MessageBoxButton.OK);
            });
        }
        private void changeFileS19(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            FileStream fsWrite = new FileStream("./File/MPC5746rFlashDriver.s19", FileMode.Create);

            StreamWriter write = new StreamWriter(fsWrite, Encoding.Default);
            String s;
            int add = 0;
            while ((s = read.ReadLine()) != null)
            {
                String data;
                String temp;
                if (s.Substring(0, 2).Equals("S3"))
                {
                    if (add == 0)
                    {
                        add = DataConverter.hexStringToInt(s.Substring(4, 8));
                    }
                    else
                    {
                        add = add + 0x18;
                    }
                    temp = s.Substring(2, 2) + DataConverter.intToHexString(add) + s.Substring(12, s.Length - 14);


                }
                else {
                    temp = s.Substring(2, s.Length - 4);
                }
 
                    data = s.Substring(0, 2) + temp + ( DataConverter.getAllBytesSum3(DataConverter.strToHexByte(temp))).ToString("X2");

                write.WriteLine(data);
            }
            write.Close();
            fsWrite.Close();
            read.Close();
            fs.Close();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ModernDialog.ShowMessage((string)page.Resources["finish"], (string)page.Resources["tips"], MessageBoxButton.OK);
            });
        }

        //private void test() {
        //    List<Object[]> process = new List<Object[]>();
        //    process.Add(new object[] { "", (byte)0x10, (byte)0x01 });
        //    for (int i = 0; i < 100; i++) {
        //        process.Add(new object[] { "91", (byte)0x22, (byte)0xf1 });
        //    }
        //    send(process);
        //}
        
        //bootloader刷写
        private void runBootLoader()
        {

            //preHexFile(AppFile);
            uDSTestService.IsDoBootLoader = true;
            List<Object[]> process2 = new List<Object[]>();

            ////测试
            //process2.Add(new object[] { "721A", (byte)0x22 });
            //send(process2);
            //process2 = new List<object[]>();

            //process2.Add(new object[] { "", (byte)0x10, (byte)0x03 });
            //是否区分应用层ID
           uDSTestService.IsECUFun = true;
            process2.Add(new object[] { "03", (byte)0x10 });
            send(process2);
            if (uDSTestService.IsNext)
            {
                uDSTestService.IsECUFun = true;
                //有应用程序
                process2 = new List<Object[]>();
                process2.Add(new Object[] { "02", (byte)0x85 });
                process2.Add(new Object[] { "0301", (byte)0x28 });
                send(process2);
                process2 = new List<Object[]>();
                uDSTestService.IsECUFun = false;

                //暂时不要
                //process2.Add(new Object[] { "91", (byte)0x22, (byte)0xf1 });
                //process2.Add(new Object[] { "95", (byte)0x22, (byte)0xf1 });
                // process2.Add(new Object[] { "84" + data22, (byte)0x2E, (byte)0xf1 });
                //process2.Add(new Object[] { "99", (byte)0x22, (byte)0xf1 });
                process2.Add(new Object[] { "010203", (byte)0x31 });
                process2.Add(new Object[] { "02", (byte)0x10 });
                //process2.Add(new object[] { "721A", (byte)0x22 });//读取是第几次下载，从0开始
                process2.Add(new Object[] { "01", (byte)0x27 });
               // process2.Add(new Object[] { "F1830101010101010101010101010101010101010101010101010101010101010101", (byte)0x2E });
                //process2.Add(new Object[] { "F184" + data22, (byte)0x2E });
                send(process2);
            }
            else
            {
                uDSTestService.IsECUFun = true;
                //没有应用程序
                process2 = new List<Object[]>();
                process2.Add(new Object[] { "02", (byte)0x85 });
                process2.Add(new Object[] { "0301", (byte)0x28 });
                send(process2);
                process2 = new List<Object[]>();
                uDSTestService.IsECUFun = false;

                //暂时不要
                //process2.Add(new Object[] { "91", (byte)0x22, (byte)0xf1 });
                //process2.Add(new Object[] { "95", (byte)0x22, (byte)0xf1 });
                //process2.Add(new Object[] { "84" + data22, (byte)0x2E, (byte)0xf1 });
                //process2.Add(new Object[] { "99", (byte)0x22, (byte)0xf1 });
                process2.Add(new Object[] { "010203", (byte)0x31 });
                process2.Add(new Object[] { "02", (byte)0x10 });
                //process2.Add(new object[] { "721A", (byte)0x22 });
                process2.Add(new Object[] { "01", (byte)0x27 });
                //process2.Add(new Object[] { "F1830101010101010101010101010101010101010101010101010101010101010101", (byte)0x2E });

                //process2.Add(new Object[] { "F184" + data22, (byte)0x2E });
                send(process2);
            };
            Console.WriteLine();
            //开始发送文件
            //初始化文件，将文件数据保存到内存
            Info = (string)page.Resources["preFlashDriver"];
            if (!uDSTestService.IsNext)
            {
                Console.WriteLine("结束程序");
                isSendHeart = false;//关闭心跳
                uDSTestService.IsDoBootLoader = false;
                return;
            }
            //preHexFileForS19ToOneBlock(flashDriver);
            //preHexFile(flashDriver);
            preHexFileForS19ToOneBlock(flashDriver);

            //刷写flashdriver文件
            Info = (string)page.Resources["downFlashDriver"];
            startBootLoader();
            //Erase Flash
            if (uDSTestService.IsNext)
            {
                Info = (string)page.Resources["preAPP"];
                if (HexOrSrec)
                {
                    preHexFileForS19ToOneBlock(AppFile);
                }
                else {
                    preHexFile(AppFile);
                }



                process2 = new List<Object[]>();
                //计算擦除大小
                Info = (string)page.Resources["eraseFlash"];
                string size = DataConverter.intToHexString(addPerBlock[addPerBlock.Count - 1] + dataPerBlock[dataPerBlock.Count - 1].Length / 2 - addPerBlock[0]);
                string add = DataConverter.intToHexString(addPerBlock[0]);
                int addNum = 8 - size.Length;
                for (int i = 0; i < addNum; i++)
                {
                    size = "0" + size;
                }
                addNum = 8 - add.Length;
                for (int i = 0; i < addNum; i++)
                {
                    add = "0" + add;
                }
                process2.Add(new Object[] { "01FF0044" + add + size, (byte)0x31});
                //process2.Add(new Object[] { "FF00440100000000141D00", (byte)0x31, (byte)0x01 });
               // process2.Add(new Object[] { "01FF00440100000000141D00", (byte)0x31 });
                //擦除
                send(process2);
            }
            Info = (string)page.Resources["downAPP"];
            if (!uDSTestService.IsNext)
            {
                Console.WriteLine("结束程序！");
                isSendHeart = false;//关闭心跳
                uDSTestService.IsDoBootLoader = false;
                return;
            }
            String s = "";
            for (int i = 0; i < dataPerBlock.Count; i++)
            {
                s += dataPerBlock[i];
            }
            //CRCTest
            //s += "0A";
            s = DataConverter.byteToHexStrForData(DataConverter.Crc_CalcateCRC32(DataConverter.strToHexByte(s)));
            Console.WriteLine(s);
            startBootLoader();
            if (uDSTestService.IsNext)
            {
                process2 = new List<Object[]>();
                process2.Add(new Object[] { "01FF01", (byte)0x31 });
                process2.Add(new Object[] { "01", (byte)0x11 });
                process2.Add(new Object[] { "03", (byte)0x10 });
                process2.Add(new Object[] { "0001", (byte)0x28 });
                process2.Add(new Object[] { "01", (byte)0x85 });
                process2.Add(new Object[] { "01", (byte)0x10 });
                send(process2);
            }

            isSendHeart = false;//关闭心跳
            uDSTestService.IsDoBootLoader = false;
        }

        //预处理S19文件，并填充所有的断地址组合成一个整的块
        private void preHexFileForS19ToOneBlock(string path) {
            Maxpb = 0;
            PBValue = 0;
            addPerBlock.Clear();
            dataPerBlock.Clear();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            StringBuilder data = new StringBuilder("");//记录本次Block的数据
            string s = null;
            int add = 0;
            int addThisLine = 0;
            int nextadd = 0;
            int dataLen = 0;
            while ((s = read.ReadLine()) != null) {
                String head = s.Substring(0,2);
                if (head.Equals("S1") || head.Equals("S2")|| head.Equals("S3")) {
                    int addLen = 0;
                    switch (int.Parse(head.Substring(1, 1))) {
                        case 1:
                            addLen = 2;
                            break;
                        case 2:
                            addLen = 3;
                            break;
                        case 3:
                            addLen = 4;
                            break;
                    }
                    addThisLine = DataConverter.hexStringToInt(s.Substring(4, addLen * 2));
                    dataLen = DataConverter.hexStringToInt(s.Substring(2, 2)) - 1 - addLen;
                    if (add == 0)
                    {
                        add = addThisLine;                        
                        //data.Append(s.Substring(4 + addLen * 2, dataLen*2));
                       // nextadd = add + dataLen;
                    }
                    else {
                        if (addThisLine != nextadd)
                        {
                            //断地址
                            for (int i = 0; i < addThisLine - nextadd; i++) {
                                data.Append("FF");
                            } 
                        }
                        
                        //else {
                        //    data.Append(s.Substring(4 + addLen * 2, dataLen * 2));
                        //}
                    }
                    nextadd = addThisLine + dataLen;
                    data.Append(s.Substring(4 + addLen * 2, dataLen * 2));
                }
                
            }
            int additionSize = 256 - data.Length / 2 % 256;
            if (additionSize == 256)
            {
                additionSize = 0;
            }
            else {
                for (int i = 0; i < additionSize; i++) {
                    data.Append("FF");
                }
            }
            Maxpb = data.Length;
            addPerBlock.Add(add);
            dataPerBlock.Add(data.ToString());
        }

        //hex文件预处理，将文件块保存，并记录每块的地址
        private void preHexFile(string path)
        {
            Maxpb = 0;
            PBValue = 0;
            addPerBlock.Clear();
            dataPerBlock.Clear();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            int baseAdd = -1;//基地址
            int nextAdd = -1;//连续情况下下一行的地址
            // int blockNum = 0;//记录块号索引
            //string dataThisBlock = "";//记录本次Block的数据
            StringBuilder dataThisBlock = new StringBuilder("");//记录本次Block的数据

            string s = null;
            while ((s = read.ReadLine()) != null)
            {
                int lineCount = DataConverter.hexStringToInt(s.Substring(1, 2));
                if (nextAdd == -1)
                {
                    if (baseAdd == -1)
                    {
                        if (s.Substring(0, 9) != ":02000004")
                        {
                            continue;
                        }
                        else
                        {
                            //获取基地址
                            baseAdd = DataConverter.hexStringToInt(s.Substring(9, 4));
                        }
                    }
                    else
                    {
                        if (s.Substring(7, 2).Equals("00"))
                        {
                            //第一次读取到数据行
                            //计算这一块的地址
                            addPerBlock.Add(baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4)));
                            nextAdd = (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4)) + lineCount);
                            dataThisBlock.Append(s.Substring(9, lineCount * 2));
                            //dataThisBlock += s.Substring(9, lineCount * 2);
                        }
                    }
                }
                else
                {
                    string fun = s.Substring(7, 2);
                    if (fun.Equals("04"))
                    {
                        //更换基地址
                        baseAdd = DataConverter.hexStringToInt(s.Substring(9, 4));
                    }
                    else if (fun.Equals("00"))
                    {
                        //数据部分
                        if (nextAdd == (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4))))
                        {
                            //地址是连续的
                            dataThisBlock.Append(s.Substring(9, lineCount * 2));
                            //dataThisBlock += s.Substring(9, lineCount * 2);
                            //nextAdd = (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4))) + lineCount;
                        }
                        else
                        {
                            //地址断了
                            int additionSize = 256 - dataThisBlock.Length / 2 % 256;
                            if (additionSize == 256)
                            {
                                additionSize = 0;
                            }
                            int thisAdd = baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4));
                            if (thisAdd - addPerBlock[addPerBlock.Count - 1] <= dataThisBlock.Length / 2 + additionSize)
                            {
                                //把中间断的地址用00补齐，这次不计入断地址
                                additionSize = thisAdd - addPerBlock[addPerBlock.Count - 1] - dataThisBlock.Length / 2;
                                for (int i = 0; i < additionSize; i++)
                                {
                                    //dataThisBlock += "00";
                                    dataThisBlock.Append("00");
                                }
                                dataThisBlock.Append(s.Substring(9, lineCount * 2));
                                //dataThisBlock += s.Substring(9, lineCount * 2);
                            }
                            else
                            {
                                //添加addtionSize个00，并计入断地址
                                for (int i = 0; i < additionSize; i++)
                                {
                                    //dataThisBlock += "00";
                                    dataThisBlock.Append("00");
                                }
                                dataPerBlock.Add(dataThisBlock.ToString());

                                //塞进下一块的地址
                                addPerBlock.Add(baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4)));
                                Maxpb += dataThisBlock.Length;

                                // Console.WriteLine("nextAdd:" + DataConverter.intToHexString(nextAdd));
                                //dataThisBlock = s.Substring(9, lineCount * 2);
                                dataThisBlock = new StringBuilder(s.Substring(9, lineCount * 2));
                            }

                        }
                        nextAdd = (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4))) + lineCount;
                    }
                    else if (fun.Equals("01"))
                    {
                        //读取到了末尾
                        int additionSize = 256 - dataThisBlock.Length / 2 % 256;
                        if (additionSize != 256)
                        {
                            for (int i = 0; i < additionSize; i++)
                            {
                                //dataThisBlock += "00";
                                dataThisBlock.Append("00");
                            }
                        }

                        dataPerBlock.Add(dataThisBlock.ToString());
                        Maxpb += dataThisBlock.Length;
                        Console.WriteLine("读取文件结束" + addPerBlock.ToString() + dataPerBlock.ToString());
                    }
                }

            }//while
            read.Close();
            fs.Close();
        }

        private void preHexFileForS19(string path)
        {
            Maxpb = 0;
            PBValue = 0;
            addPerBlock.Clear();
            dataPerBlock.Clear();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader read = new StreamReader(fs, Encoding.Default);
        int baseAdd = -1;//基地址
        int nextAdd = -1;//连续情况下下一行的地址
                         // int blockNum = 0;//记录块号索引
                         //string dataThisBlock = "";//记录本次Block的数据
        StringBuilder dataThisBlock = new StringBuilder("");//记录本次Block的数据

        string s = null;
            while ((s = read.ReadLine()) != null)
            {
                //int lineCount = DataConverter.hexStringToInt(s.Substring(1, 2));
                if (nextAdd == -1)
                {
                    //if (baseAdd == -1)
                    //{
                    //    if (s.Substring(0, 9) != ":02000004")
                    //    {
                    //        continue;
                    //    }
                    //    else
                    //    {
                    //        //获取基地址
                    //        baseAdd = DataConverter.hexStringToInt(s.Substring(9, 4));
                    //    }
                    //}
                    //else
                    //{
                        if (s.Substring(0, 2).Equals("S1")|| s.Substring(0, 2).Equals("S2")|| s.Substring(0, 2).Equals("S3"))
                        {
                            //第一次读取到数据行
                            //计算这一块的地址
                            int addCount = DataConverter.hexStringToInt(s.Substring(1, 1)) + 1;
                            int dataCount = DataConverter.hexStringToInt(s.Substring(2, 2))-1;
                            addPerBlock.Add(DataConverter.hexStringToInt(s.Substring(4, addCount * 2)));
                            nextAdd = (DataConverter.hexStringToInt(s.Substring(4, addCount * 2)) + dataCount);
                            dataThisBlock.Append(s.Substring(4+addCount*2, dataCount));
                        }
                    //}
                }
                else
                {
                    string fun = s.Substring(0, 2);
                    int addCount=-1;
                    int dataCount=-1;
                    //if (fun.Equals("04"))
                    //{
                    //    //更换基地址
                    //    baseAdd = DataConverter.hexStringToInt(s.Substring(9, 4));
                    //}
                    if (fun.Equals("S1")|| fun.Equals("S2") || fun.Equals("S3"))
                    {
                        addCount = DataConverter.hexStringToInt(s.Substring(1, 1)) + 1;
                        dataCount = DataConverter.hexStringToInt(s.Substring(2, 2)) - 1;
                        //数据部分
                        //if (nextAdd == (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4))))
                        if (nextAdd == DataConverter.hexStringToInt((s.Substring(4,addCount*2))))
                        {
                            //地址是连续的
                            dataThisBlock.Append(s.Substring(4 + addCount * 2, dataCount));
                            //dataThisBlock += s.Substring(9, lineCount * 2);
                            //nextAdd = (baseAdd << 16 | DataConverter.hexStringToInt(s.Substring(3, 4))) + lineCount;
                        }
                        else
                        {
                            //地址断了
                            addCount = DataConverter.hexStringToInt(s.Substring(1, 1)) + 1;
                            dataCount = DataConverter.hexStringToInt(s.Substring(2, 2)) - 1;
                            int additionSize = 256 - dataThisBlock.Length / 2 % 256;
                            if (additionSize == 256)
                            {
                                additionSize = 0;
                            }
                            int thisAdd = DataConverter.hexStringToInt(s.Substring(4, addCount * 2));
                            if (thisAdd - addPerBlock[addPerBlock.Count - 1] <= dataThisBlock.Length / 2 + additionSize)
                            {
                                //把中间断的地址用00补齐，这次不计入断地址
                                additionSize = thisAdd - addPerBlock[addPerBlock.Count - 1] - dataThisBlock.Length / 2;
                                for (int i = 0; i<additionSize; i++)
                                {
                                    //dataThisBlock += "00";
                                    dataThisBlock.Append("00");
                                }
                                dataThisBlock.Append(s.Substring(4 + addCount * 2, dataCount));
                                //dataThisBlock += s.Substring(9, lineCount * 2);
                            }
                            else
                            {
                                //添加addtionSize个00，并计入断地址
                                for (int i = 0; i<additionSize; i++)
                                {
                                    //dataThisBlock += "00";
                                    dataThisBlock.Append("00");
                                }
                                dataPerBlock.Add(dataThisBlock.ToString());

                                //塞进下一块的地址
                                addPerBlock.Add(DataConverter.hexStringToInt(s.Substring(4, addCount * 2)));
                                Maxpb += dataThisBlock.Length;

                                // Console.WriteLine("nextAdd:" + DataConverter.intToHexString(nextAdd));
                                //dataThisBlock = s.Substring(9, lineCount * 2);
                                dataThisBlock = new StringBuilder(s.Substring(4 + addCount * 2, dataCount));
                            }

                        }
                        nextAdd = (DataConverter.hexStringToInt(s.Substring(4, addCount * 2))) + dataCount;
                    }
                    else if (fun.Equals("S5")|| fun.Equals("S7") || fun.Equals("S8") || fun.Equals("S9"))
                    {
                        //读取到了末尾
                        int additionSize = 256 - dataThisBlock.Length / 2 % 256;
                        if (additionSize != 256)
                        {
                            for (int i = 0; i<additionSize; i++)
                            {
                                //dataThisBlock += "00";
                                dataThisBlock.Append("00");
                            }
                        }

                        dataPerBlock.Add(dataThisBlock.ToString());
                        Maxpb += dataThisBlock.Length;
                        Console.WriteLine("读取文件结束" + addPerBlock.ToString() + dataPerBlock.ToString());
                    }
                }

            }//while
            read.Close();
            fs.Close();
        }

        //hex文件预处理，将文件块保存，并记录每块的地址
        //private void preHexFile2(string path)
        //{
        //    Maxpb = 0;
        //    PBValue = 0;
        //    addPerBlock.Clear();
        //    dataPerBlock.Clear();
        //    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    StreamReader read = new StreamReader(fs, Encoding.Default);
        //    int baseAdd = -1;//基地址
        //    int nextAdd = -1;//连续情况下下一行的地址
        //    // int blockNum = 0;//记录块号索引
        //    //string dataThisBlock = "";//记录本次Block的数据
        //    StringBuilder dataThisBlock = new StringBuilder("");//记录本次Block的数据


        //    int address = -1;
        //    string s = null;
        //    while ((s = read.ReadLine()) != null)
        //    {
        //        if (s.Substring(0, 2).Equals("S0"))
        //        {
        //            //首行
        //            continue;
        //        }
        //        else if (s.Substring(0, 2).Equals("S5") || s.Substring(0, 2).Equals("S7") ||
        //          s.Substring(0, 2).Equals("S7") || s.Substring(0, 2).Equals("S8")) {
        //            //结束

        //            break;
        //        }
        //        else
        //        {
        //            int addCount = DataConverter.hexStringToInt(s.Substring(1, 1)) + 1;
        //            if (address == -1)
        //            {
        //                //第一次
        //                addPerBlock.Add(DataConverter.hexStringToInt(s.Substring(4, addCount * 2)));
        //                dataThisBlock.Append(s.Substring(4 + addCount * 2, DataConverter.hexStringToInt(s.Substring(2, 2)) - addCount * 2));
        //                //连续情况下下一次的地址
        //                // address = DataConverter.hexStringToInt(s.Substring(4, addCount * 2)) + DataConverter.hexStringToInt(s.Substring(1, 2));
        //            }
        //            else {
        //                if (address == DataConverter.hexStringToInt(s.Substring(4, addCount * 2)))
        //                {
        //                    //地址连续
        //                    //连续情况下下一次的地址
        //                    address = DataConverter.hexStringToInt(s.Substring(4, addCount * 2)) + DataConverter.hexStringToInt(s.Substring(1, 2));
        //                    //  dataThisBlock.Append(s.Substring(4 + addCount * 2, DataConverter.hexStringToInt(s.Substring(2, 2)) - addCount * 2));
        //                }
        //                else {
        //                    //地址断
        //                    addPerBlock.Add(DataConverter.hexStringToInt(s.Substring(4, addCount * 2)));
        //                    // dataPerBlock.Add(dataThisBlock.ToString());
        //                    // dataThisBlock.Append(s.Substring(4 + addCount * 2, DataConverter.hexStringToInt(s.Substring(2, 2)) - addCount * 2));
        //                }
        //            }
        //            //连续情况下下一次的地址
        //            address = DataConverter.hexStringToInt(s.Substring(4, addCount * 2)) + DataConverter.hexStringToInt(s.Substring(1, 2))
        //        } }
        //    }//while


        //    read.Close();
        //    fs.Close();
        //}
        //刷写文件
        //private bool isflashdriver = true;
        private void startBootLoader()
        {
            
            PBValue = 0;
            List<Object[]> process = new List<Object[]>();
            //开始发送
            for (int i = 0; uDSTestService.IsNext && i < addPerBlock.Count; i++)
            {
                String data = dataPerBlock[i];
                string dataSize = DataConverter.intToHexString(data.Length / 2);
                string add = DataConverter.intToHexString(addPerBlock[i]);
                process = new List<Object[]>();
                process.Add(new Object[] { "00"+dataSize.Length / 2  + add.Length / 2 + add + dataSize, (byte)0x34 });
                send(process);//发送34
                blockNum = 1;
                if (!uDSTestService.IsNext)
                {
                    //未收到34帧的回复，可以增加重发机制
                    return;
                }
                String sTemp = "";
                for (int j = 0; uDSTestService.IsNext && j < data.Length; j += uDSTestService.BlockSize * 2 - 4)
                {
                    if (j + uDSTestService.BlockSize * 2 - 4 > data.Length)
                    {
                        sTemp = data.Substring(j);
                    }
                    else
                    {
                        sTemp = data.Substring(j, uDSTestService.BlockSize * 2 - 4);
                    }
                    process = new List<Object[]>();
                    process.Add(new Object[] { DataConverter.intToHexString(blockNum) +sTemp, (byte)0x36 });
                    blockNum = (blockNum + 1) > 255 ? 0 : (blockNum + 1);
                    send(process);//发送36
                    PBValue += sTemp.Length;
                }
                //发送37
                if (uDSTestService.IsNext)
                {
                    process = new List<Object[]>();
                    process.Add(new Object[] { "", (byte)0x37});
                    send(process);
                }
            }//for
            ////发送37
            //if (uDSTestService.IsNext)
            //{
            //    process = new List<Object[]>();
            //    process.Add(new Object[] { "", (byte)0x37, (byte)0xAA });
            //    send(process);
            //}
            //开启心跳，发送CRC校验
            if (uDSTestService.IsNext)
            {
                Info = (string)page.Resources["crc"];
                process = new List<Object[]>();
                String s = "";
                for (int i = 0; i < dataPerBlock.Count; i++)
                {
                    s += dataPerBlock[i];
                }
                //if (isflashdriver)
                //{
                //    process.Add(new Object[] { "010202" + Regex.Replace(DataConverter.byteToHexStrForData(DataConverter.Crc_CalcateCRC32(DataConverter.strToHexByte(s))), @"\s", ""), (byte)0x31 });
                //    isflashdriver = false;
                //}
                //else
                //{
                //    s += "0A";
                    process.Add(new Object[] { "010202" + Regex.Replace(DataConverter.byteToHexStrForData(DataConverter.Crc_CalcateCRC32(DataConverter.strToHexByte(s))), @"\s", ""), (byte)0x31 });
                //    isflashdriver = true;
                //}


                //process.Add(new Object[] { "", (byte)0x37 });
                send(process);
            }
        }
        private long heartBaseTime = -1;//下一次心跳发送时间以此为基准
        //发送心跳
        private void sendHeart()
        {
            while (true)
            {
                if (isSendHeart && (DateTime.Now.Ticks - heartBaseTime) / 10000 > 3000)
                {
                    uDSTestService.sendFrame(new byte[] { 0x02, 0x3E, 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
                    heartBaseTime = DateTime.Now.Ticks;
                    // Thread.Sleep(3000);
                    // Console.WriteLine("发送心跳");
                }
                //new Thread(new ParameterizedThreadStart(uDSTestService.sendMultiFrame)).Start(new byte[] { 0x02, 0x3E, 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            }
        }

        //初始化Sid和Did级联下拉框
        private void InitSidAndDidCB()
        {
            //sid和did映射表
            sidAndDidDic = new Dictionary<string, string[]>{
                { "10",new string[]{"00","01","02","03","04"}},
                {"11",new string[]{"00","01","02","03","04","05"}},
                {"27",new string[]{"01","02","03","04","11","12"}},
                {"28",new string[]{"00","01","02","03","04","05"}},
                {"83",new string[]{"01","02","03","04"}},
                {"84",new string[]{"null"}},
                {"85",new string[]{"01","02"}},
                {"87",new string[]{"01","02","03"}},
                {"22",new string[]{"null"}},
                {"23",new string[]{"null"}},
                {"24",new string[]{"null"}},
                {"2A",new string[]{"null"}},
                {"2C",new string[]{"01","02","03"}},
                {"2E",new string[]{"null"}},
                {"3D",new string[]{"null"}},
                {"14",new string[]{"null"}},

                {"19",new string[]{"01","02","03","04","06","0A"}},
                {"31",new string[]{"01","02","03"}}
                
            };
            //sid和NRC映射表
            sidAndNRCDic = new Dictionary<string, string[]>{
                {"10",new string[]{"12","13","22"}},
                {"11",new string[]{"12","13","22","33"}},
                {"27",new string[]{"12","13","22","24","31","35","36","37"}} ,
                 {"28",new string[]{"12","13","22","31"}},
                 {"83",new string[]{"12","13","22","31"}},
                 {"84",new string[]{"13"}},
                 {"85",new string[]{"12","13","22","31"}},
                 {"87",new string[]{"12","13","22","24","31"}},
                 {"22",new string[]{"13","14","22","31","33"}},
                 {"23",new string[]{"13","22","31","33"}},
                 {"24",new string[]{"13","22","31","33"}},
                 {"2A",new string[]{"13","22","31","33"}},
                 {"2C",new string[]{"12","13","22","31","33"}},
                {"2E",new string[]{"13","22","31","33","72"}},
                {"3D",new string[]{"13","22","31","33","72"}},
                {"14",new string[]{"13","22","31","72"}},

                {"19",new string[]{"12","13","31"}},
                {"31",new string[]{"12","13","22","24","31","33","72"}}

             };

            sidCb = page.FindName("sidComboxBox") as ComboBox;
            didCb = page.FindName("didComboxBox") as ComboBox;
            ItemCollection sidColl = sidCb.Items;
            ItemCollection didColl = didCb.Items;
            foreach (KeyValuePair<string, string[]> kvp in sidAndDidDic)
            {
                ComboBoxItem boxItem = new ComboBoxItem() { Content = kvp.Key };
                sidColl.Add(boxItem);
            }
            foreach (string s in sidAndDidDic["10"])
            {
                ComboBoxItem boxItem = new ComboBoxItem() { Content = s };
                didColl.Add(boxItem);
            }
            sidCb.SelectionChanged += new SelectionChangedEventHandler(sidAndDidCB_SelectionChanged);
        }
        //sid和did联动下拉框状态改变
        private void sidAndDidCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemCollection coll = didCb.Items;
            coll.Clear();
            foreach (KeyValuePair<string, string[]> kvp in sidAndDidDic)
            {
                String selectedSid = (sidCb.SelectedItem as ComboBoxItem).Content.ToString();
                if (selectedSid.Equals(kvp.Key))
                {
                    foreach (var item in kvp.Value)
                    {
                        ComboBoxItem boxItem = new ComboBoxItem() { Content = item };
                        coll.Add(boxItem);
                    }
                }
            }
        }

        //uDSTestService类中接收函数的回调函数，用来返回接收结果
        public void changeView(UDSTestService.ViewEventArgs args)
        {
            Object[] o = args.Obj;
            //o[0]表示功能，其余表示参数
            switch (Convert.ToInt32(o[0]))
            {
                case FunCode.SHOWINFOFORUDS:
                    //给出提示
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);
                        ModernDialog.ShowMessage((string)page.Resources[Convert.ToString(o[1])], (string)page.Resources["tips"], MessageBoxButton.OK);
                    });
                    break;
                case FunCode.SHOWUDSLOG:
                    //显示UDSLog
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Paragraph p = page.FindName("showUdsLog") as Paragraph;
                        //TextBlock tb = page.FindName("showUdsLog") as TextBlock;
                        Run r = new Run();
                        r.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Convert.ToString(o[2])));
                        r.Text = Convert.ToString(o[1]) + "\r\n";
                        Cursor c = p.Cursor;

                        p.Inlines.Add(r);
                        udsLongCount++;
                        if (udsLongCount > MAXSHOWUDSLOG)
                        {
                            p.Inlines.Remove(p.Inlines.FirstInline);
                        }
                        RichTextBox rtb = page.FindName("rtb") as RichTextBox;

                        rtb.ScrollToEnd();

                    });

                    // UdsLog += (Convert.ToString(o[1]) + "\r\n");
                    break;
                case FunCode.SENDFC:
                    byte[] data = new byte[] { 0x30, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    new Thread(new ParameterizedThreadStart(uDSTestService.sendFrame)).Start(data);
                    // sendFrame(data);
                    break;
                case FunCode.ERROR:
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Paragraph p = page.FindName("showUdsLog") as Paragraph;
                        //TextBlock tb = page.FindName("showUdsLog") as TextBlock;
                        Run r = new Run();
                        r.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
                        r.Text = (string)page.Resources[Convert.ToString(o[1])] + "\r\n";
                        p.Inlines.Add(r);
                        udsLongCount++;
                        if (udsLongCount > MAXSHOWUDSLOG)
                        {
                            p.Inlines.Remove(p.Inlines.FirstInline);
                        }
                    });
                    // UdsLog += (string)page.Resources[Convert.ToString(o[1])];
                    break;
                case FunCode.CHANGEHEARTTIME:
                    heartBaseTime = DateTime.Now.Ticks;
                    break;
            }
        }
        ////发送帧
        private void sendFrame(Object o)
        {
            byte[] data = (byte[])o;
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 0;//标准帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            string s = "000007E1";
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            obj.Data = data;
            // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            UdsLog += "sendUDSData：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff") + " " + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2") + "\r\n";

            //发送的是流控帧，判断数据是否接收完全，根据需要启用
            if ((data[0] >> 4 & 0x0F) == 3)
            {
                bool isEnd = false;
                long starttime = DateTime.Now.Ticks;
                // long end = (new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks - starttime) / 10000;
                long e = (DateTime.Now.Ticks - starttime) / 10000;//单位毫秒
                long e2 = uDSTestService.Ff_dl / 7 * 50;//根据需要接收的帧数计算超时时间
                while (e < e2)
                {
                    if (uDSTestService.IsWaitting.ContainsKey("waitCF") && uDSTestService.IsWaitting["waitCF"] == 0xFF)
                    {
                        isEnd = true;
                        break;
                    }
                }
                if (!isEnd)
                {
                    //没有收到下位机完整的数据回复
                    UdsLog += "receive overtime\r\n";
                }
            }
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
                    isSendHeart = false;
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
                    //Console.WriteLine("接收："+DataConverter.byteToHexStrForData( obj.Data));
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(uDSTestService.parseDataThread), obj);
                    uDSTestService.parseDataThread(obj);//串行
                    //收到5003或5002打开心跳
                    if ((obj.Data[1] == 0x50 && obj.Data[2] == 0x03)|| (obj.Data[1] == 0x50 && obj.Data[2] == 0x02)) { isSendHeart = true; }
                }
                Marshal.FreeHGlobal(pt);
            }

        }
        private void RunCbI18nClickCommand(ComboBox comboBox)
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
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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

