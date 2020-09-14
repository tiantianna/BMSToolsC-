using bms.startup.i18n;
using bms.startup.SDK;
using bms.startup.Model;
using bms.startup.service;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Converters;
using Microsoft.Practices.Prism.Commands;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Media;
using System.Windows.Data;
using bms.startup.util;
using System.Collections;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;

namespace bms.startup
{
    class Gen2ViewModel : INotifyPropertyChanged
    {
        private Gen2MainWindow page;
        private const uint RECEIVELEN = 100;//一次性可接收数据帧数 
        private bool connectstate = false;//can连接状态
        private uint cancode = 0;
        private Thread receiveThread; //数据接受线程
        private bool isReceive = true;//是否循环接收线程
        private String slaveVersion1 = null;//保存从机版本号前半部分
        private String slaveVersion2 = null;//保存从机版本号后半部分
        private string TraceCode1 = null;//追溯码
        private string TraceCode2 = null;//追溯码
        private string TraceCode3 = null;//追溯码

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
        //从机编号
        private string slaveId;
        public string SlaveId
        {
            get { return slaveId; }
            set { slaveId = value; OnPropertyChanged("SlaveId"); }
        }
        private Gen2SlaveInfo gen2SlaveInfo;

        public Gen2SlaveInfo Gen2SlaveInfo
        {
            get { return gen2SlaveInfo; }
            set { gen2SlaveInfo = value; OnPropertyChanged("Gen2SlaveInfo"); }
        }

        private string[] itemlist;//标志每个item显示与否

        public string[] Itemlist
        {
            get { return itemlist; }
            set { itemlist = value; OnPropertyChanged("Itemlist"); }
        }
        private Gen2SlaveConfig gen2SlaveConfig;

        public Gen2SlaveConfig Gen2SlaveConfig
        {
            get { return gen2SlaveConfig; }
            set { gen2SlaveConfig = value; OnPropertyChanged("Gen2SlaveConfig"); }
        }

        public ICommand Command1 { get; set; }
        public ICommand BalanceAllSelectCommand { get; set; }
        public bool CanExecuteCommand1(object parameter)
        {
            return true;
        }

        //界面绑定方法
        public DelegateCommand<Object> RunFun { get; set; }// 带参数的绑定方法，以参数标志函数功能
        public DelegateCommand<Button> ConnectCanCommand { get; set; }
        public DelegateCommand<ComboBox> CbI18nClickCommand { get; set; }//语言栏按钮

        Gen2Service gen2Service = new Gen2Service();
        private bool isShishi = true;//用于标志按钮“开启监测”状态，true表示“开启监测”，false为“关闭监测”
        private bool isReadingTraceCode = true;//用户标志开启追溯码读取循环按钮的状态，true表示“开启”，false表示“关闭”
        private Thread startShishiThread = null;//循环发送实时信息请求帧
        private Thread startReadTraceCodeThread = null;//循环发送实时信息请求帧
        private int selectI18n = 0;//语言
        public int SelectI18n
        {
            get { return selectI18n; }
            set { selectI18n = value; OnPropertyChanged("SelectI18n"); }
        }

        public Gen2ViewModel(Gen2MainWindow p)
        {
            page = p;
            Init();
        }

        public Gen2ViewModel(Gen2MainWindow p, string[] il)
        {
            Itemlist = il;
            page = p;
            Init();
        }

        private void Init()
        {
            ConsoleManager.Show();//打开信息打印窗口

            gen2Service.changeViewForGen2VM += changeView;

            gen2SlaveInfo = new Gen2SlaveInfo();
            gen2SlaveConfig = new Gen2SlaveConfig();

            ConnectCanCommand = new DelegateCommand<Button>(RunConnectCanCommand);
            RunFun = new DelegateCommand<Object>(RunSendFrame);
            CbI18nClickCommand = new DelegateCommand<ComboBox>(RunCbI18nClickCommand);//国际化选框

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

            InitView();


        }

        //界面控件初始化
        private void InitView()
        {
            //初始化单体电压界面
            StackPanel sp1 = page.FindName("cellVol") as StackPanel;
            sp1.Children.Clear();
            for (int i = 0; i < 10; i++)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.VerticalAlignment = VerticalAlignment.Center;
                //sp.IsEnabled = false;
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                for (int j = 0; j < 6; j++)
                {
                    //if (i * 8 + j > 59)
                    //{
                    //    break;
                    //}
                    Label l = new Label();
                    l.Content = "V" + (i * 6 + j + 1) + "：";
                    l.VerticalAlignment = VerticalAlignment.Center;
                    l.Margin = new Thickness(1);
                    TextBox tb = new TextBox();
                    tb.Width = 70;
                    tb.FontSize = 14;
                    tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.Margin = new Thickness(3);
                    tb.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#acacac"));
                    tb.IsEnabled = false;
                    Binding bindChinese = new Binding("Gen2SlaveInfo.Vol[" + (i * 6 + j) + "]");
                    // bindChinese.StringFormat = stringF4;
                    //bindChinese.Converter = FindResource("nanConvert") as IValueConverter;
                    tb.SetBinding(TextBox.TextProperty, bindChinese);
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
                    sp.Children.Add(l);
                    sp.Children.Add(tb);
                }
                sp1.Children.Add(sp);
            }
            //初始化信号采集线状态界面
            sp1 = page.FindName("signalConnect") as StackPanel;
            sp1.Children.Clear();
            for (int i = 0; i < 11; i++)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.VerticalAlignment = VerticalAlignment.Center;
                //sp.IsEnabled = false;
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                for (int j = 0; j < 6; j++)
                {
                    if (i * 6 + j > 60)
                    {
                        break;
                    }
                    Label l = new Label();
                    l.Content = "C" + (i * 6 + j) + "：";
                    l.VerticalAlignment = VerticalAlignment.Center;
                    l.Width = 100;
                    l.Margin = new Thickness(1);
                    TextBox tb = new TextBox();
                    tb.Width = 100;
                    tb.FontSize = 14;
                    tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.Margin = new Thickness(3);
                    tb.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#acacac"));
                    tb.IsEnabled = false;
                    Binding bindChinese = new Binding("Gen2SlaveInfo.Signal[" + (i * 6 + j) + "]");
                    // bindChinese.StringFormat = stringF4;
                    //bindChinese.Converter = FindResource("nanConvert") as IValueConverter;
                    tb.SetBinding(TextBox.TextProperty, bindChinese);
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
                    sp.Children.Add(l);
                    sp.Children.Add(tb);
                }
                sp1.Children.Add(sp);
            }
            //初始化均衡控制界面
            sp1 = page.FindName("balance") as StackPanel;
            sp1.Children.Clear();
            for (int i = 0; i < 10; i++)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.VerticalAlignment = VerticalAlignment.Center;
                //sp.IsEnabled = false;
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                for (int j = 0; j < 6; j++)
                {
                    //if (i * 6 + j > 60)
                    //{
                    //    break;
                    //}
                    Label l = new Label();
                    l.Content = "cell" + (i * 6 + j+1) + "：";
                    l.VerticalAlignment = VerticalAlignment.Center;
                    l.Margin = new Thickness(1);
                    CheckBox cb = new CheckBox();
                    cb.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("Gen2SlaveConfig.Balance[" + (i * 6 + j) + "]") });

                    sp.Children.Add(l);
                    sp.Children.Add(cb);
                }
                sp1.Children.Add(sp);
                    
            }
            StackPanel sp2 = new StackPanel();
            sp2.Orientation = Orientation.Horizontal;
            sp2.HorizontalAlignment = HorizontalAlignment.Left;

            Label alllb = new Label();
            alllb.Content = (string)page.Resources["allselect"];
            alllb.VerticalContentAlignment = VerticalAlignment.Center;
            alllb.HorizontalContentAlignment = HorizontalAlignment.Right;
            alllb.Width = 70;
            alllb.Height = 32;
            alllb.Margin = new Thickness(20, 0, 0, 0);
            CheckBox allcb = new CheckBox();
            page.RegisterName("balanceAll", allcb);
           // allcb.Name = "balanceAll";
            BalanceAllSelectCommand = new bms.startup.command.gen2BalanceCommand(RunSendFrame, CanExecuteCommand1, 9);
            allcb.Command = BalanceAllSelectCommand;
            sp2.Children.Add(alllb);
            sp2.Children.Add(allcb);
            Button balanceBtn = new Button();
            Command1 = new bms.startup.command.gen2BalanceCommand(RunSendFrame, CanExecuteCommand1, 8);
            balanceBtn.Command = Command1;
            balanceBtn.Content = (string)page.Resources["send"];
            balanceBtn.Width = 80;
            balanceBtn.Margin = new Thickness(20, 0, 0, 0);
            sp2.Children.Add(balanceBtn);
            sp1.Children.Add(sp2);

            ////初始化均衡状态界面
            //sp1 = page.FindName("balance") as StackPanel;
            //sp1.Children.Clear();
            //for (int i = 0; i < 10; i++)
            //{
            //    StackPanel sp = new StackPanel();
            //    sp.Orientation = Orientation.Horizontal;
            //    sp.VerticalAlignment = VerticalAlignment.Center;
            //    //sp.IsEnabled = false;
            //    sp.HorizontalAlignment = HorizontalAlignment.Left;
            //    for (int j = 0; j < 6; j++)
            //    {
            //        Label l = new Label();
            //        l.Content = "电池" + (i * 6 + j+1) + "：";
            //        l.VerticalAlignment = VerticalAlignment.Center;
            //        l.Margin = new Thickness(1);
            //        TextBox tb = new TextBox();
            //        tb.Width = 70;
            //        tb.FontSize = 14;
            //        tb.VerticalContentAlignment = VerticalAlignment.Center;
            //        tb.Margin = new Thickness(3);
            //        tb.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#acacac"));
            //        tb.IsEnabled = false;
            //        Binding bindChinese = new Binding("Gen2SlaveInfo.Balance[" + (i * 6 + j) + "]");
            //        // bindChinese.StringFormat = stringF4;
            //        //bindChinese.Converter = FindResource("nanConvert") as IValueConverter;
            //        tb.SetBinding(TextBox.TextProperty, bindChinese);
            //        tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            //        sp.Children.Add(l);
            //        sp.Children.Add(tb);
            //    }
            //    sp1.Children.Add(sp);
            //}
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
        //获取从机编号
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
                    //获取从机编号
                    new Thread(new ThreadStart(gen2Service.getSalveId)).Start();
                    break;
                case 1:
                    Button btn = page.FindName("shishi") as Button;
                    if (!isShishi)
                    {
                        if (startShishiThread != null)
                        {
                            startShishiThread.Abort();
                            startShishiThread = null;
                        }
                        isShishi = true;
                        btn.SetResourceReference(ContentControl.ContentProperty, "startshishi");
                    }
                    else
                    {
                        if (SlaveId == null)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                            return;
                        }
                        startShishiThread = new Thread(new ParameterizedThreadStart(gen2Service.startShishi));
                        startShishiThread.Start(SlaveId);
                        isShishi = false;
                        btn.SetResourceReference(ContentControl.ContentProperty, "closeshishi");
                    }
                    break;
                case 2:
                    //读取配置信息
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    new Thread(new ParameterizedThreadStart(gen2Service.getConfig)).Start(SlaveId);
                    //startShishiThread.Start(SlaveId);
                    break;
                case 3:
                    //发送配置信息1
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    ArrayList al = new ArrayList();
                    al.Add(SlaveId);
                    al.Add(Gen2SlaveConfig);
                    al.Add(Gen2SlaveInfo);
                    new Thread(new ParameterizedThreadStart(gen2Service.sendConfig1Pack)).Start(al);

                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ModernDialog.ShowMessage((string)page.Resources["sendSuc"], (string)page.Resources["tips"], MessageBoxButton.OK);
                    });
                    //startShishiThread.Start(al);
                    break;

                case 4:
                    //发送配置信息2
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    ArrayList al2 = new ArrayList();
                    al2.Add(SlaveId);
                    al2.Add(Gen2SlaveConfig);
                    al2.Add(Gen2SlaveInfo);
                    new Thread(new ParameterizedThreadStart(gen2Service.sendConfig2Pack)).Start(al2);
                    //startShishiThread.Start(al2);
                    break;

                case 5:
                    //发送追溯码
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    ArrayList al3 = new ArrayList();
                    al3.Add(SlaveId);
                    al3.Add(Gen2SlaveConfig);
                    new Thread(new ParameterizedThreadStart(gen2Service.sendTraceCodePack)).Start(al3);
                    //startShishiThread.Start(al3);

                    break;

                case 6:
                    //发送Relay2，can_life及时间控制信息
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    ArrayList al4 = new ArrayList();
                    al4.Add(SlaveId);
                    al4.Add(Gen2SlaveConfig);
                    new Thread(new ParameterizedThreadStart(gen2Service.sendRelay2Pack)).Start(al4);
                    break;

                case 7:
                    //读取追溯码
                    Button btn2 = page.FindName("readTraceCode") as Button;
                    if (!isReadingTraceCode)
                    {
                        if (startReadTraceCodeThread != null)
                        {
                            startReadTraceCodeThread.Abort();
                            startReadTraceCodeThread = null;
                        }
                        isReadingTraceCode = true;
                        btn2.SetResourceReference(ContentControl.ContentProperty, "read2");
                    }

                    else
                    {
                        if (SlaveId == null)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                            return;
                        }
                        startReadTraceCodeThread = new Thread(new ParameterizedThreadStart(gen2Service.getTraceCode));
                        startReadTraceCodeThread.Start(SlaveId);
                        isReadingTraceCode = false;
                        btn2.SetResourceReference(ContentControl.ContentProperty, "stop");
                    }
                    break;
                case 8:
                    //发送均衡控制指令
                    if (SlaveId == null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage((string)page.Resources["plsGetIdFirst"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });
                        return;
                    }
                    ArrayList al5 = new ArrayList();
                    al5.Add(SlaveId);
                    al5.Add(Gen2SlaveConfig);
                    new Thread(new ParameterizedThreadStart(gen2Service.sendBalancePack)).Start(al5);
                    break;

                case 9:
                    //全选或者全不选
                    CheckBox cb = page.FindName("balanceAll") as CheckBox;
                    if (cb.IsChecked==true) {
                        Gen2SlaveConfig.setAll(true);
                    }else{
                        Gen2SlaveConfig.setAll(false);
                    }
                    break;
                case 10:
                    //读取配置信息
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Multiselect = false;//该值确定是否可以选择多个文件
                    dialog.Title = "请选择文件夹";
                    dialog.Filter = "所有文件(*.*)|*.*";
                    dialog.ShowDialog();
                    String readPath = dialog.FileName;
                    if (readPath == "")
                    {
                        return;
                    }
                    Gen2SlaveConfig = Serializer.DeserializeFromFileByXml<Gen2SlaveConfig>(readPath);
                    break;
                case 11:
                    //存储配置信息
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "";
                    sfd.InitialDirectory = @"C:\";
                    sfd.Filter = "文本文件| *.txt";
                    sfd.ShowDialog();
                    string savePath = sfd.FileName;
                    // Console.WriteLine(path);
                    if (savePath == "")
                    {
                        return;
                    }
                    //List<Gen2SlaveConfig> l = new List<Gen2SlaveConfig>();
                    //l.Add(Gen2SlaveConfig);
                    //序列化
                    Serializer.SerializeToFileByXml<Gen2SlaveConfig>(gen2SlaveConfig, Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
                    break;

            }


        }
        
        //Gen2Service类中接收函数的回调函数，用来返回接收结果
        public void changeView(Gen2Service.ViewEventArgs args)
        {
            Object[] o = args.Obj;
            //o[0]表示功能，其余表示参数
            switch (Convert.ToInt32(o[0]))
            {
                case FunCode.SHOWINFO:
                    //给出提示
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);
                        ModernDialog.ShowMessage((string)page.Resources[Convert.ToString(o[1])], (string)page.Resources["tips"], MessageBoxButton.OK);
                    });
                    break;
                case FunCode.SLAVEID:
                    //收到从机编号
                    SlaveId = Convert.ToString(o[1]);
                    break;
                case FunCode.REALINFO:
                    //接收实时信息0C0041XX
                    Gen2SlaveInfo.BCNT = Convert.ToString(o[1]);
                    Gen2SlaveInfo.SOC = Convert.ToString(o[2]);
                    Gen2SlaveInfo.VTotal = Convert.ToString(o[3]);
                    Gen2SlaveInfo.CVMax = Convert.ToString(o[4]);
                    switch (Convert.ToString(o[5])) { 
                        case "0":
                            Gen2SlaveInfo.CellInfo = (string)page.Resources["guoya"];
                            break;
                        case "1":
                            Gen2SlaveInfo.CellInfo = (string)page.Resources["qianya"];
                            break;
                        case "2":
                            Gen2SlaveInfo.CellInfo = (string)page.Resources["volnormal"];
                            break;
                        case "3":
                            Gen2SlaveInfo.CellInfo = (string)page.Resources["yzxcha"];
                            break;
                        default:
                             Gen2SlaveInfo.CellInfo = (string)page.Resources["volnormal"];
                            break;
                    }
                    //Gen2SlaveInfo.CellInfo = Convert.ToString(o[5]);
                    Gen2SlaveInfo.TemInfo = Convert.ToString(o[6]).Equals("0") ? (string)page.Resources["normal"] : (string)page.Resources["abnormal"];
                    gen2SlaveInfo.CVmin = Convert.ToString(o[7]);
                    Gen2SlaveInfo.BmuHardwareFault = Convert.ToString(o[8]).Equals("0") ? (string)page.Resources["normal"] : (string)page.Resources["abnormal"];
                    Gen2SlaveInfo.SignalInfo = Convert.ToString(o[9]).Equals("0") ? (string)page.Resources["normal"] : (string)page.Resources["abnormal"];
                    Gen2SlaveInfo.BalanceInfo = Convert.ToString(o[10]).Equals("0") ? (string)page.Resources["open"] : (string)page.Resources["close"];
                    break;

                case FunCode.VOLINFO:
                    int start = Convert.ToInt32(o[1]) - 1;
                    Gen2SlaveInfo.Vol[start++] = Convert.ToString(o[2]);
                    Gen2SlaveInfo.Vol[start++] = Convert.ToString(o[3]);
                    Gen2SlaveInfo.Vol[start++] = Convert.ToString(o[4]);
                    Gen2SlaveInfo.Vol[start++] = Convert.ToString(o[5]);
                    break;

                case FunCode.TEMPINFO:
                    Gen2SlaveInfo.TemSensor[0] = Convert.ToString(o[1]);
                    Gen2SlaveInfo.TemSensor[1] = Convert.ToString(o[2]);
                    Gen2SlaveInfo.TemSensor[2] = Convert.ToString(o[3]);
                    Gen2SlaveInfo.TemSensor[3] = Convert.ToString(o[4]);
                    Gen2SlaveInfo.TemSensor[4] = Convert.ToString(o[5]);
                    Gen2SlaveInfo.TemSensor[5] = Convert.ToString(o[6]);
                    Gen2SlaveInfo.Tb = Convert.ToString(o[7]);
                    Gen2SlaveInfo.Can_life = Convert.ToString(o[8]);
                    break;
                case FunCode.SIGNALINFO:
                    for (int i = 0; i < 61; i++)
                    {
                        Gen2SlaveInfo.Signal[i] = Convert.ToString(o[i + 1]).Equals("0") ? (string)page.Resources["normal"] : (string)page.Resources["openCircuit"];
                    }
                    break;

                //case FunCode.BALANCESTATUS:
                //    for (int i = 0; i < 60; i++)
                //    {
                //        Gen2SlaveInfo.Balance[i] = Convert.ToString(o[i + 1]);
                //    }
                //    break;
                case FunCode.REALINFO2:
                    Gen2SlaveInfo.PacMaxTemp = Convert.ToString(o[1]);
                    Gen2SlaveInfo.PacMinTemp = Convert.ToString(o[2]);
                    slaveVersion2 = Convert.ToString(o[3]) + Convert.ToString(o[4]) + Convert.ToString(o[5]);
                    if (slaveVersion1 != null && !slaveVersion1.Equals("")) {
                        Gen2SlaveInfo.SlaveVersion = slaveVersion1 + slaveVersion2;
                        slaveVersion1 = null;
                        slaveVersion2 = null;
                    }
                    break;

                case FunCode.REALINFO3:
                    slaveVersion1 = Convert.ToString(o[1]) + Convert.ToString(o[2]) + Convert.ToString(o[3]) + Convert.ToString(o[4]) +
                        Convert.ToString(o[5]) + Convert.ToString(o[6]) + Convert.ToString(o[7]) + Convert.ToString(o[8]);
                    if (slaveVersion2 != null && !slaveVersion2.Equals(""))
                    {
                        Gen2SlaveInfo.SlaveVersion = slaveVersion1 + slaveVersion2;
                        slaveVersion1 = null;
                        slaveVersion2 = null;
                    }
                    break;
                case FunCode.TEMPINFO2:
                    Gen2SlaveInfo.TemSensor[6] = Convert.ToString(o[1]);
                    Gen2SlaveInfo.TemSensor[7] = Convert.ToString(o[2]);
                    Gen2SlaveInfo.TemSensor[8] = Convert.ToString(o[3]);
                    Gen2SlaveInfo.TemSensor[9] = Convert.ToString(o[4]);
                    break;
                case FunCode.CONFIG1:
                    Gen2SlaveInfo.Bcnt_2 = Convert.ToString(o[1]);
                    Gen2SlaveInfo.Sid = Convert.ToString(o[2]);
                    Gen2SlaveInfo.Covth = Convert.ToString(o[3]);
                    Gen2SlaveInfo.Cuvth = Convert.ToString(o[4]);
                    Gen2SlaveInfo.Foth = Convert.ToString(o[5]);
                    Gen2SlaveInfo.Fcth = Convert.ToString(o[6]);
                    break;
                case FunCode.CONFIG2:
                    Gen2SlaveInfo.Bcnt_A = Convert.ToString(o[1]);
                    Gen2SlaveInfo.Bcnt_B = Convert.ToString(o[2]);
                    Gen2SlaveInfo.Bcnt_C = Convert.ToString(o[3]);
                    Gen2SlaveInfo.Bcnt_D = Convert.ToString(o[4]);
                    Gen2SlaveInfo.Bcnt_E = Convert.ToString(o[5]);
                    Gen2SlaveInfo.Bcnt_F = Convert.ToString(o[6]);
                    Gen2SlaveInfo.MaxChargeCur=Convert.ToString(o[7]);
                    break;

                case FunCode.TRACECODE1:
                    TraceCode1 = Convert.ToString(o[1]) + Convert.ToString(o[2]) + Convert.ToString(o[3]) + Convert.ToString(o[4]) +
                        Convert.ToString(o[5]) + Convert.ToString(o[6]) + Convert.ToString(o[7]) + Convert.ToString(o[8]);
                    if (TraceCode2 != null && !TraceCode2.Equals("") && TraceCode3 != null && !TraceCode3.Equals(""))
                    {
                        Gen2SlaveInfo.TraceCode = TraceCode1 + TraceCode2 + TraceCode3;
                        TraceCode1 = null;
                        TraceCode2 = null;
                        TraceCode3 = null;
                    }
                    break;
                case FunCode.TRACECODE2:
                    TraceCode2 = Convert.ToString(o[1]) + Convert.ToString(o[2]) + Convert.ToString(o[3]) + Convert.ToString(o[4]) +
                        Convert.ToString(o[5]) + Convert.ToString(o[6]) + Convert.ToString(o[7]) + Convert.ToString(o[8]);
                    if (TraceCode1 != null && !TraceCode1.Equals("") && TraceCode3 != null && !TraceCode3.Equals(""))
                    {
                        Gen2SlaveInfo.TraceCode = TraceCode1 + TraceCode2 + TraceCode3;
                        TraceCode1 = null;
                        TraceCode2 = null;
                        TraceCode3 = null;
                    }
                    break;
                case FunCode.TRACECODE3:
                    TraceCode3 = Convert.ToString(o[1]) + Convert.ToString(o[2]) + Convert.ToString(o[3]) + Convert.ToString(o[4]) +
                        Convert.ToString(o[5]) + Convert.ToString(o[6]) + Convert.ToString(o[7]) + Convert.ToString(o[8]);
                    if (TraceCode1 != null && !TraceCode1.Equals("") && TraceCode2 != null && !TraceCode2.Equals(""))
                    {
                        Gen2SlaveInfo.TraceCode = TraceCode1 + TraceCode2 + TraceCode3;
                        TraceCode1 = null;
                        TraceCode2 = null;
                        TraceCode3 = null;
                    }
                    break;
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
                    ThreadPool.QueueUserWorkItem(new WaitCallback(gen2Service.parseDataThread), obj);
                }

                Marshal.FreeHGlobal(pt);
            }

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
