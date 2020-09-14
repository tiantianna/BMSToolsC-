using bms.startup.i18n;
using bms.startup.Model;
using bms.startup.SDK;
using bms.startup.userControl;
using bms.startup.util;
using bms.startup.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Converters;
using log4net;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
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

namespace bms.startup
{

    public class ReadCfgArgs : EventArgs
    {
        private CANSDK.VCI_CAN_OBJ args;
        public CANSDK.VCI_CAN_OBJ Args
        {
            get { return args; }
            set { args = value; }
        }
    }



    public class SlaveViewModel : INotifyPropertyChanged
    {
        public void check(object sender, RoutedEventArgs e) { }

        private static ILog logger = LogManager.GetLogger(typeof(SlaveViewModel));
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private static object locker = new object();
        private static object locker2 = new object();
        private static object locker3 = new object();
        private static object debuglocker = new object();
        private static object lockercell = new object();
        private static object _lockQueue = new object();
        private static object masterFElocker = new object();
        private static object slaveLocker = new object();

        private bool isReadCfgSendReapeat = true;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ReadCfgArgs> ReadCfgEvent;
        public event EventHandler<ReadCfgArgs> DiagnoseEvent;
        private ObservableCollection<string> canChannelList = new ObservableCollection<string>();
        private ObservableCollection<int> canIndexList = new ObservableCollection<int>() { 0, 1, 2, 3, 4, 5 }; //can设备索引号
        private ObservableCollection<string> balanceModeList = new ObservableCollection<string>(); //均衡模式
        private ObservableCollection<SlaveBalanceModel> slaveBalanceModelList = new ObservableCollection<SlaveBalanceModel>();//从机均衡控制（主机界面）
        Thread sendBalanceThread = null;

        public ObservableCollection<SlaveBalanceModel> SlaveBalanceModelList
        {
            get { return slaveBalanceModelList; }
            set { slaveBalanceModelList = value; OnPropertyChanged("SlaveBalanceModelList"); }
        }

        private ObservableCollection<BaudRateModel> canBaudRateList = new ObservableCollection<BaudRateModel>();//波特率列表
        private ObservableCollection<CategoryInfo> categoryI18nList = new ObservableCollection<CategoryInfo>();//语言列表
        
        private string[] dataRecordPerLine = null;//保存需要记录到硬盘文件上的数据，保存的为一行的数据量
        //用于保存单体温度和单体电压的dataRecordPerLine保存index，比如<"w1",10>表示一号从机温度从dataRecordPerLine[10]开始(1号从机第1单体温度)
        private Dictionary<string, int> dataRecordIndex = new Dictionary<string, int>();
        private int dataRecordPerLineNum = 0;//记录dataRecordPerLine数组里面已有数据的数量，理论上当所有的数据都填满时再保存
        //从机软件版本号1
        private Dictionary<int, string> slaveSoftwareVersion1 = new Dictionary<int, string>();       
        //从机软件版本号2
        private Dictionary<int, string> slaveSoftwareVersion2 = new Dictionary<int, string>();

        public Dictionary<int, string> SlaveSoftwareVersion1
        {
            get { return slaveSoftwareVersion1; }
            set { slaveSoftwareVersion1 = value; }
        }
        public Dictionary<int, string> SlaveSoftwareVersion2
        {
            get { return slaveSoftwareVersion2; }
            set { slaveSoftwareVersion2 = value; }
        }
        public ObservableCollection<CategoryInfo> CategoryI18nList
        {
            get { return categoryI18nList; }
            set { categoryI18nList = value; OnPropertyChanged("CategoryI18nList"); }
        }

        //private List<BMUConfigViewModel> saveBmuConfigList = new List<BMUConfigViewModel>();//保存所有页的配置

        private List<BMUConfigModel_gy> saveBmuConfigList = new List<BMUConfigModel_gy>();//保存所有页的配置
        private string info;//运行信息显示

        private List<byte> alarmInfo = new List<byte>();//保存系统故障报警代码（来自主机）
        private List<byte> alarmInfo2 = new List<byte>();//保存充电故障报警代码（来自主机）

        #region CAN数据帧ID
        private string bmuinfo = "0C0041[01][0123456789ABCDEF]";//每箱BMU总信息报文
        private string bmuinfo2 = "0C0141[01][0123456789ABCDEF]";//每箱BMU信息
        private string bmubalanceactiveinfo = "0C0[345]41[01][0123456789ABCDEF]";//BMS主动均衡 从机发送数据帧，上位机解析
        private string bmubalanceinfo = "0C1041[01][0123456789ABCDEF]";//从机BMU主动均衡动作控制报文
        private string bmubalancehandinfo = "0C11[01][0123456789ABCDEF]41";//从机BMU手动均衡动作
        private string cellinfo = "180[123456789ABCDEF]41[01][0123456789ABCDEF]"; //每个BMU单体个数
        private string tmpinfo = "181[012]41[01][0123456789ABCDEF]"; //电池模块温度BMU_TEMP
        private string tmosinfo = "181441[01][0123456789ABCDEF]";//TMOS温度值
        private string signalvolstatus = "181641[01][0123456789ABCDEF]";//电压信号采集连接状态
        private string signaltmpstatus = "181541[01][0123456789ABCDEF]";//温度信号采集连接状态
        private string bmucfginfo = "0C217F[01][0123456789ABCDEF]";//BMU配置信息报文
        private string bmuversioninfo = "0C0241[0123456789ABCDEF][0123456789ABCDEF]";//BMU版本号信息
        private string TIEDIANHS = "1806C1B1";//铁电握手
        private string TIEDIANDATA = "1801C1B1";//铁电数据
        private string SLAVEPE = "1C1041[0123456789ABCDEF][0123456789ABCDEF]";//接收从机被动均衡报文
        #endregion

        #region 界面元素信息

        #endregion

        private string versionNum1 = "";//主机版本号1
        private string versionNum2 = "";//主机版本号2
        private string versionNum3 = "";//主机版本号3
        private string versionNum4 = "";//主机版本号4
        private string version = "";//主机版本号=主机版本号1+主机版本号2
        private int isGetMasterInfo = 0;//是否需要获取主机信息，0否1是


        public string Version
        {
            get { return version; }
            set { version = value; OnPropertyChanged("Version"); }
        }

        //主机时钟
        public string Clock
        {
            get { return version; }
            set { version = value; OnPropertyChanged("Clock"); }
        }
       //private double[][] BMU_Vol = new double[16][];//存储从机单体电压最大值,第一个索引表示从机号，第二个表示该从机的单体号，都从1开始
        private string slaveNumTarget;//原从机编号 
        private string slaveNum;//从机编号
        private string cellBalanMode;//均衡模式
        private string childModuleMonCellNumber;//从机监控单体总数
        private string childMonModuleTemperatureNumber;//从机监控温感总数
        private string moduleAMonCellNum;//子模块A监控电池数目
        private string moduleAMonTemperatureNum;//子模块A监控温感数目
        private string slaveNum_rec;//从机编号
        private string cellBalanMode_rec;//均衡模式
        private string childModuleMonCellNumber_rec;//从机监控单体总数
        private string childMonModuleTemperatureNumber_rec;//从机监控温感总数
        private string moduleAMonCellNum_rec;//子模块A监控电池数目
        private string moduleAMonTemperatureNum_rec;//子模块A监控温感数目
        //出厂配置对象
        private BMUConfigModel_gy factoryConfig;
        //出厂配置同步
        private static AutoResetEvent factoryEvent = new AutoResetEvent(false);
        //定义读写锁用于多线程同步处理进行写锁
        private ReaderWriterLock rwlock = new ReaderWriterLock();

        private List<BMUConfigModel> receiveBmuList = new List<BMUConfigModel>();//实时页面用于保存BMU列表信息
        private bool isCreateBmuFlag = false;//界面是否创建BMU列表
        private DateTime start;//开始时间
        private DateTime end; //结束时间

        private int bMUCNT;
        private bool isBMUCNTExist = false;

        private bool isSendBmuInfo;
        private bool isSendBmuCellInfo;
        private bool isSendBmuTempInfo;
        private bool isSendBmuSLSInfo;
        private bool isSendBmuSLSTMPInfo;
        private bool isSendBmuTMOSInfo;
        private bool isSendBalanceInfo;
        private bool isReadVersionInfo;//读取从机版本号 
        private bool isCheckAllInfo;
        private ObservableCollection<string> versionVisibility = new ObservableCollection<string>();

        //private int[] congjiID = new int[24];//从机ID号的集合，从机ID的取值范围为1-24；
        private int[] chuanshu = new int[24];//接收主机0x43帧，保存各从机串数。chuanshu[0]表示一号从机有多少串
        private int[] wenganshu = new int[24];//接收主机0x44帧，保存各从机温感数。wenganshu[0]表示一号从机有多少个温感
        #region 单体电压变化用颜色区分
        /// <summary>
        /// add by zhengzhonghua 2018/09/29
        /// 用于保存单体电压值，
        /// 当前值与上一次值发送变化时用颜色变化显示数值发生变化
        /// </summary>
        private double lastcellshow1 = 0;
        private double lastcellshow2 = 0;
        private double lastcellshow3 = 0;
        private double lastcellshow4 = 0;
        private int lasttempshow = 0;
        private int lasttmosshow = 0;
        private int lasttcshow = 0;
        #endregion


        /// <summary>
        /// 等待状态
        /// </summary>
        private EnumStatus waitStatus;

        public EnumStatus WaitStatus
        {
            get { return waitStatus; }
            set { waitStatus = value; OnPropertyChanged("WaitStatus"); }
        }


        public BMUConfigModel_gy FactoryConfig
        {
            get { return factoryConfig; }
            set { factoryConfig = value; OnPropertyChanged("FactoryConfig"); }
        }

        public string CellBalanMode
        {
            get
            {
                switch (selectBalanceMode)
                {
                    case 0:
                        return ((int)'N').ToString();
                    case 1:
                        return ((int)'A').ToString();
                    case 2:
                        return ((int)'P').ToString();
                };
                return cellBalanMode;
            }
            set
            {
                switch (selectBalanceMode)
                {
                    case 'N':
                        cellBalanMode = "不均衡";
                        break;
                    case 'A':
                        cellBalanMode = "主动均衡";
                        break;
                    case 'P':
                        cellBalanMode = "被动均衡";
                        break;
                }
                //cellBalanMode = value;
                OnPropertyChanged("CellBalanMode");
            }
        }
        public ObservableCollection<string> BalanceModeList
        {
            get { return balanceModeList; }
            set { balanceModeList = value; }
        }

        public string SlaveNumTarget
        {
            get { return slaveNumTarget; }
            set { slaveNumTarget = value; OnPropertyChanged("SlaveNumTarget"); }
        }
        public string SlaveNum
        {
            get { return slaveNum; }
            set { slaveNum = value; OnPropertyChanged("SlaveNum"); }
        }

        public string ChildModuleMonCellNumber
        {
            get { return childModuleMonCellNumber; }
            set { childModuleMonCellNumber = value; OnPropertyChanged("ChildModuleMonCellNumber"); }
        }

        public string ChildMonModuleTemperatureNumber
        {
            get { return childMonModuleTemperatureNumber; }
            set { childMonModuleTemperatureNumber = value; OnPropertyChanged("ChildMonModuleTemperatureNumber"); }
        }

        public string ModuleAMonCellNum
        {
            get { return moduleAMonCellNum; }
            set { moduleAMonCellNum = value; OnPropertyChanged("ModuleAMonCellNum"); }
        }

        public string ModuleAMonTemperatureNum
        {
            get { return moduleAMonTemperatureNum; }
            set { moduleAMonTemperatureNum = value; OnPropertyChanged("ModuleAMonTemperatureNum"); }
        }

        public string SlaveNum_rec
        {
            get { return slaveNum_rec; }
            set { slaveNum_rec = value; OnPropertyChanged("SlaveNum_rec"); }
        }

        private string cellBalanModeTemp = "";
        public string CellBalanMode_rec
        {
            get
            {
                int temp = 0;
                if (!cellBalanModeTemp.Equals(""))
                {
                    temp = Convert.ToInt32(cellBalanModeTemp);
                    cellBalanModeTemp = "";
                }
                else
                {
                    temp = Convert.ToInt32(cellBalanMode_rec);
                }
                //int temp = Convert.ToInt32(cellBalanMode_rec);
                switch (temp)
                {
                    case (int)'N':
                        return "不均衡";
                    case (int)'A':
                        return "主动均衡";
                    case (int)'P':
                        return "被动均衡";
                    default:
                        return "";
                }
                // return cellBalanMode_rec;
            }
            set { cellBalanMode_rec = value; cellBalanModeTemp = value; OnPropertyChanged("CellBalanMode_rec"); }
        }

        public string ChildModuleMonCellNumber_rec
        {
            get { return childModuleMonCellNumber_rec; }
            set { childModuleMonCellNumber_rec = value; OnPropertyChanged("ChildModuleMonCellNumber_rec"); }
        }

        public string ChildMonModuleTemperatureNumber_rec
        {
            get { return childMonModuleTemperatureNumber_rec; }
            set { childMonModuleTemperatureNumber_rec = value; OnPropertyChanged("ChildMonModuleTemperatureNumber_rec"); }
        }

        public string ModuleAMonCellNum_rec
        {
            get { return moduleAMonCellNum_rec; }
            set { moduleAMonCellNum_rec = value; OnPropertyChanged("ModuleAMonCellNum_rec"); }
        }

        public string ModuleAMonTemperatureNum_rec
        {
            get { return moduleAMonTemperatureNum_rec; }
            set { moduleAMonTemperatureNum_rec = value; OnPropertyChanged("ModuleAMonTemperatureNum_rec"); }
        }

        public bool IsCheckAllInfo
        {
            get { return isCheckAllInfo; }
            set { isCheckAllInfo = value; OnPropertyChanged("IsCheckAllInfo"); }
        }

        public bool IsSendBalanceInfo
        {
            get { return isSendBalanceInfo; }
            set { isSendBalanceInfo = value; OnPropertyChanged("IsSendBalanceInfo"); }
        }

        //读取从机版本号
        public bool IsReadVersionInfo
        {
            get { return isReadVersionInfo; }
            set { isReadVersionInfo = value; OnPropertyChanged("IsReadVersionInfo"); }
        }

        public bool IsSendBmuTMOSInfo
        {
            get { return isSendBmuTMOSInfo; }
            set { isSendBmuTMOSInfo = value; OnPropertyChanged("IsSendBmuTMOSInfo"); }
        }

        public bool IsSendBmuSLSTMPInfo
        {
            get { return isSendBmuSLSTMPInfo; }
            set { isSendBmuSLSTMPInfo = value; OnPropertyChanged("IsSendBmuSLSTMPInfo"); }
        }

        public bool IsSendBmuSLSInfo
        {
            get { return isSendBmuSLSInfo; }
            set { isSendBmuSLSInfo = value; OnPropertyChanged("IsSendBmuSLSInfo"); }
        }
        public bool IsSendBmuTempInfo
        {
            get { return isSendBmuTempInfo; }
            set { isSendBmuTempInfo = value; OnPropertyChanged("IsSendBmuTempInfo"); }
        }
        public bool IsSendBmuCellInfo
        {
            get { return isSendBmuCellInfo; }
            set { isSendBmuCellInfo = value; OnPropertyChanged("IsSendBmuCellInfo"); }
        }
        public bool IsSendBmuInfo
        {
            get { return isSendBmuInfo; }
            set { isSendBmuInfo = value; OnPropertyChanged("IsSendBmuInfo"); }
        }

        private uint cancode = 0;
        private bool connectstate = false;//can连接状态
        private ObservableCollection<TabItem> bmuConfigList = new ObservableCollection<TabItem>();//BMU配置列表
        private int bmuConfigListNum = 0;
        private MainWindow page;

        #region Command命令事件
        public DelegateCommand All_Check { get; set; }//全选选中
        public DelegateCommand All_UnCheck { get; set; }//全选取消
        public DelegateCommand AddBMUConfigCommand { get; set; }//添加BMU配置
        public DelegateCommand SaveConfigCommand { get; set; }//保存配置文件
        public DelegateCommand ReadConfigCommand { get; set; }//读取配置文件
        public DelegateCommand ClearBMUConfigCommand { get; set; }//清空配置页
        public DelegateCommand<Grid> DeleteBMUCommand { get; set; }//删除BMU配置
        public DelegateCommand SendConfigCommand { get; set; }//发送配置文件数据包
        public DelegateCommand<Grid> TestBMUCommand { get; set; }//测试配置
        public DelegateCommand<Button> DelBMUCommand { get; set; }//删除BMU
        public DelegateCommand<Button> ConnectCanCommand { get; set; }
        public DelegateCommand<ComboBox> CbI18nClickCommand { get; set; }//语言栏按钮
        public DelegateCommand InitConfigCommand { get; set; }//发送初始化配置文件
        public DelegateCommand GetID { get; set; }//配置从机ID
        public DelegateCommand ReadFactoryConfigCommand { get; set; }//读取出厂配置文件
        public DelegateCommand SaveFactoryConfigCommand { get; set; }//保存出厂配置文件

        public DelegateCommand ReadBootLoaderUrlCommand { get; set; }//读取bootloader文件
        public DelegateCommand ReadBootLoaderUrlCommand2 { get; set; }//读取bootloader文件
        public DelegateCommand ReadBootLoaderUrlCommand3 { get; set; }//读取bootloader文件
        public DelegateCommand<TabControl> StartBootLoaderCommand { get; set; }//发送第一组bootloader文件
        public DelegateCommand StartMasterBootLoaderCommand { get; set; }//发送第三组bootloader文件
        public DelegateCommand StartBootLoaderCommand2 { get; set; }//发送第二组bootloader文件
        public DelegateCommand StartBootLoaderCommand3 { get; set; }//发送第三组bootloader文件
        public DelegateCommand ReadMasterBootLoaderUrlCommand { get; set; }//发送主机bootloader文件
        public DelegateCommand ReadDecryptFileCommand { get; set; }//读取需要解密的文件

        public DelegateCommand DecryptCommand { get; set; }//解密
        public DelegateCommand ReadBootLoaderFileOneCommand { get; set; }//读取需要合并的文件1
        public DelegateCommand ReadBootLoaderFileTwoCommand { get; set; }//读取需要合并的文件2
        public DelegateCommand StartMergeCommand { get; set; }//合并文件

        public DelegateCommand HandShakeWithMaster { get; set; }//主机握手

        public DelegateCommand MasterClockCommand { get; set; }//主机时钟实时配置
        public DelegateCommand MasterCapacityCommand { get; set; }//主机剩余容量配置
        public DelegateCommand MasterBCCapacityCommand { get; set; }//主机标称容量配置
        public DelegateCommand MasterZCapacityCommand { get; set; }//主机总容量配置
        public DelegateCommand MasterHardwareVersionCommand { get; set; }//主机硬件版本号配置
        public DelegateCommand SlaveHardwareVersionCommand { get; set; }//从机硬件版本号配置
        public DelegateCommand VINConfigCommand { get; set; }//VIN配置
        public DelegateCommand MasterDeviationCommand { get; set; }//霍尔1电流偏移配置
        public DelegateCommand MasterDeviationCommand2 { get; set; }//霍尔2电流偏移配置
        public DelegateCommand MasterDeviationCommand3 { get; set; }//霍尔3电流偏移配置
        public DelegateCommand MasterDeviationCommand4 { get; set; }//霍尔4电流偏移配置
        public DelegateCommand Shunt1Command { get; set; }//Shunt_1电流偏移配置
        public DelegateCommand Shunt2Command { get; set; }//Shunt_2电流偏移配置
        public DelegateCommand FangdznlCommand { get; set; }//放电总能量配置
        public DelegateCommand MasterRelayCommand { get; set; }//主机配置
        public DelegateCommand MasterShiShiInfo { get; set; }//主机实时信息监测
        public DelegateCommand Tiedian { get; set; }//铁电
        public DelegateCommand OpenMasterRecord { get; set; }//开启主机信息记录
        public DelegateCommand CloseMasterRecord { get; set; }//关闭主机信息记录
        public DelegateCommand ClearBMUCommand { get; set; }//清空界面所有控件
        public DelegateCommand ReadBMUCommand { get; set; }//根据配置信息读取BMU信息add by zhengzhonghua 2018-10-11
        public DelegateCommand ReadCfgCommand { get; set; }//配置页面读取BMU配置信息add by zhengzhonghua1
        public DelegateCommand ReadDiaCommand { get; set; }//配置页面读取add by gaoya

        //public DelegateCommand ReadVersion { get; set; }//读取从机版本号

        public DelegateCommand All_Check_Factory { get; set; }//全选出厂配置
        public DelegateCommand All_UnCheck_Factory { get; set; }//全不选出厂配置
        //  public DelegateCommand ClickFacInfo17 { get; set; }//点击info17的checkbox出厂配置

        public DelegateCommand UserCommand { get; set; }//用户管理
        public DelegateCommand LogoutCommand { get; set; }//注销

        public DelegateCommand<TextBox> BmuTextChangedCommand { get; set; }//BMU个数验证事件

        public DelegateCommand SendSPECommand { get; set; }//发送从机被动均衡控制帧

        #endregion
        public ICommand AllInfoCheckCommand
        {

            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    //判断当前can是否连接
                    if (!connectstate)
                    {
                        //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                        ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        IsCheckAllInfo = false;
                        return;
                    }

                    IsSendBmuInfo = true;
                    IsSendBmuCellInfo = true;
                    IsSendBmuTMOSInfo = true;
                    IsSendBmuSLSTMPInfo = true;
                    IsSendBmuSLSInfo = true;
                    IsSendBmuTempInfo = true;
                    IsSendBalanceInfo = true;

                }
                );
            }

        }

        public ICommand AllInfoUnCheckCommand
        {

            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {


                    IsSendBmuInfo = false;
                    IsSendBmuCellInfo = false;
                    IsSendBmuTMOSInfo = false;
                    IsSendBmuSLSTMPInfo = false;
                    IsSendBmuSLSInfo = false;
                    IsSendBmuTempInfo = false;
                    IsSendBalanceInfo = false;

                }
                );
            }

        }

        /// <summary>
        /// bmu勾选信息
        /// </summary>
        public ICommand BmuInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendBmuInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }


        /// <summary>
        /// bmu不勾选信息
        /// </summary>
        public ICommand BmuInfoUnCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    try
                    {
                        //if (page.grid.Children.Count > 0)
                        //    page.grid.Children.Clear();
                        //isCreateBmuFlag = false;
                        //isInitTime = false;
                    }
                    catch
                    {

                    }
                }
                );
            }
        }

        /// <summary>
        /// 单体信息勾选
        /// </summary>
        public ICommand BmuCellInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendCellInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }


        /// <summary>
        /// 温感信息
        /// </summary>
        public ICommand BmuTempInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendTmpInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }

        /// <summary>
        /// 勾选电压信号线状态
        /// </summary>
        public ICommand BmuSLSInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendSLSInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand BmuSLSTMPInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendSLSTMPInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }

        /// <summary>
        /// TMOS温度信息
        /// </summary>
        public ICommand BmuTMOSInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    try
                    {
                        Thread blackName = new Thread(sendTMOSInfo);
                        blackName.IsBackground = true;
                        blackName.Start();
                    }
                    catch
                    {

                    }
                }
                );
            }
        }

        /// <summary>
        /// 均衡信息
        /// </summary>
        public ICommand BmuBalanceInfoCheckCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendBalanceInfo);
                    blackName.IsBackground = true;
                    blackName.Start();
                }
                );
            }
        }

        public ICommand ReadVersionCommand
        {
            get
            {
                return new DelegateCommand<RoutedEventArgs>(e =>
                {
                    Thread blackName = new Thread(sendVersionInfo);
                    blackName.IsBackground = true;
                    blackName.Start();

                }
                );
            }
        }


        

        private int selectDeviceIndex;//设备索引值
        private int selectBalanceMode;//均衡模式索引    


        private int selectCanChannelIndex;//CAN通道
        private int selectRate = 3;//波特率
        private Thread receiveThread; //数据接受线程
        private Thread sendBMSRemoteThread;//发送BMS远程帧线程
        private Thread sendUpSystemHeart;//上位机心跳发送线程（与主机握手时开启）
        public DelegateCommand addBMUCommand { get; set; }//添加BMU配置
        private int addBMUConfigNum = 1;
        private int index;//itemlist的索引

        private int selectI18n = 0;//语言

        public int SelectI18n
        {
            get { return selectI18n; }
            set { selectI18n = value; OnPropertyChanged("SelectI18n"); }
        }

        public int Index
        {
            get { return index; }
            set { index = value; OnPropertyChanged("Index"); }
        }

        //标志位
        private int isGetReceived = 1;//判断是否已经收到回送，可以结束发送。0否1是
        private Dictionary<string, int> isWaitting;//判断是否正在等待接收回送数据包,0否1是，2数据重传
        private int retryTimes = 0;//重发次数
        private Thread sendThread;//数据发送线程
        private const int OVERTIME = 1;//超时等待时间，单位秒
        private const int RETRYTIMES = 5;//超时重传次数
        private const uint RECEIVELEN = 100;//一次性可接收数据帧数 
        private const string SECRETKEY = "GotionGY";//文件加密秘钥
        private int isGaosProfram = 0;//是否是回送给配置功能的配置信息，0否1是,2表示回送给出厂配置
        private int userpower = -1;//当前登陆的用户权限
        private string buttonVisible;//标志用户

        public string ButtonVisible
        {
            get { if (Userpower == 0) { return "Visible"; } else { return "Hidden"; } }
            set { buttonVisible = value; }
        }

        public int Userpower
        {
            get { return userpower; }
            set { userpower = value; }
        }
        private string[] itemlist;//标志每个item显示与否

        public string[] Itemlist
        {
            get { return itemlist; }
            set { itemlist = value; OnPropertyChanged("Itemlist"); }
        }
        private bool[] cBArray = new bool[48];//标志bootloader下载到哪几个从机（17+16+16=49）
        public bool[] CBArray
        {
            get { return cBArray; }
            set { cBArray = value; OnPropertyChanged("CBArray"); }
        }

        private bool[] cBArray2 = new bool[17];//标志bootloader下载到第二组哪几个从机
        public bool[] CBArray2
        {
            get { return cBArray2; }
            set { cBArray2 = value; OnPropertyChanged("CBArray2"); }
        }

        private bool[] cBArray3 = new bool[17];//标志bootloader下载到第三组哪几个从机
        public bool[] CBArray3
        {
            get { return cBArray3; }
            set { cBArray3 = value; OnPropertyChanged("CBArray3"); }
        }


        #region 委托
        public delegate void receiveHandler(CANSDK.VCI_CAN_OBJ obj);
        #endregion

        //正则
        private const string CONFIG_REG = "0C21[0123456789ABCDEF][0123456789ABCDEF]7F";//BMS返回配置信息的id
        private const string INFO1_REG = "41";//BMS返回info1的byte1位
        private const string INFO2_REG = "42";//info2
        private const string INFO3_REG = "43";//info3
        private const string INFO4_REG = "44";//info4
        private const string INFO5_REG = "45";//info5
        private const string INFO6_REG = "46";//info6
        private const string INFO7_REG = "47";//info7
        private const string INFO8_REG = "48";//info8
        private const string INFO9_REG = "49";//info9
        private const string INFO10_REG = "4A";//info10
        private const string INFO11_REG = "4B";//info11
        private const string INFO12_REG = "4C";//info12
        private const string INFO13_REG = "4D";//info13
        private const string INFO14_REG = "4E";//info13
        private const string INFO15_REG = "4F";//info14
        private const string INFO16_REG = "50";//info15
        private const string INFO17_REG = "51";//info16
        private const string INFO18_REG = "52";//info16
        private const string INFO19_REG = "53";//info16
        private const string INFO20_REG = "54";//info16
        private const string DIA = "182[123456789ABCDF]41";//诊断电压
        private const string BOOTLOADER = "18A[0123][0123456789ABCDEF][0123456789ABCDEF]26";//bootloader
        private const string MASTERBOOTLOADER = "18A00126";//主机bootloader发送来的的id
        private const string BOOTLOADERF3 = "F3";//下位机回复F3表示准备就绪
        private const string BOOTLOADERF2 = "F2";//F2表示忙碌
        private const string BOOTLOADERF1 = "F1";//F1表示重传
        //private const string BOOTLOADERFD = "FD";//FD表示代码刷写结束
        private const string BOOTLOADERF0 = "F0";//F0表示全部代码刷写结束

        private const string BOOTLOADERERRF5 = "F5";// 

        //private const string BOOTLOADERF6 = "F6";//F6表示地址超出底层刷写范围
        //private const string BOOTLOADERF7 = "F7";//F7表示地址不是8的倍数
        //private const string BOOTLOADERF1 = "F9";//F9表示

        private const string MASTERHS = "18F1F003";//主机握手
        private const string MASTERINFO = "18F3F003";//主机实时信息
        private const string MASTERHEART = "18F0F003";//主机心跳


        public int SelectBalanceMode
        {
            get { return selectBalanceMode; }
            set { selectBalanceMode = value; OnPropertyChanged("SelectBalanceMode"); }
        }

        public string Info
        {
            get { return info; }
            set { info = value; OnPropertyChanged("Info"); }

        }

        public bool IsBMUCNTExist
        {
            get { return isBMUCNTExist; }
            set { isBMUCNTExist = value; OnPropertyChanged("IsBMUCNTExist"); }
        }

        public int BMUCNT
        {
            get { return bMUCNT; }
            set
            {
                bMUCNT = value; OnPropertyChanged("BMUCNT");
            }
        }
        public List<BMUConfigModel_gy> SaveBmuConfigList
        {
            get { return saveBmuConfigList; }
            set { saveBmuConfigList = value; OnPropertyChanged("SaveBmuConfigList"); }
        }
        public int AddBMUConfigNum
        {
            get { return addBMUConfigNum; }
            set { addBMUConfigNum = value; OnPropertyChanged("AddBMUConfigNum"); }
        }

        public int SelectRate
        {
            get { return selectRate; }
            set { selectRate = value; OnPropertyChanged("SelectRate"); }
        }

        public int SelectCanChannelIndex
        {
            get { return selectCanChannelIndex; }
            set { selectCanChannelIndex = value; OnPropertyChanged("SelectCanChannelIndex"); }
        }

        public int SelectDeviceIndex
        {
            get { return selectDeviceIndex; }
            set { selectDeviceIndex = value; OnPropertyChanged("SelectDeviceIndex"); }
        }

        public SlaveViewModel(MainWindow p, string[] itemlist, int userpower)
        // public SlaveViewModel(MainWindow p)
        {
            for (int i = 0; i < itemlist.Length; i++)
            {

                if (itemlist[i] == "Visible")
                {
                    Index = i;
                    break;
                }
            }
            Console.WriteLine("Index:" + Index);
            Userpower = userpower;
            Itemlist = itemlist;
            //读取配置文件bmu个数
            bMUCNT = int.Parse(ConfigurationManager.AppSettings["BMUCNT"]);
            page = p;
            Init();
        }

        public SlaveViewModel(MainWindow p)
        {

            page = p;
            Init();
        }


        private void RunCbI18nClickCommand(ComboBox comboBox) {
            //ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            ResourceDictionary langRd = Application.LoadComponent(new Uri("/i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            page.Resources.MergedDictionaries.Add(langRd);
            //改变属性的值
            XmlDocument doc = new XmlDocument();
            //doc.Load("../../Config/Config.xml");
            doc.Load("Config/Config.xml");
            XmlNode xn = doc.SelectSingleNode("/appSettings/i18n");
            xn.InnerText = selectI18n.ToString();

            doc.Save("Config/Config.xml");
            //doc.Save("../../Config/Config.xml");
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                //ModernDialog.ShowMessage("只能连接一个从机", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["effectiveafterrestart"], (string)page.Resources["tips"], MessageBoxButton.OK);

            });
            //categoryI18nList.Clear();
            //canChannelList.Clear();
            //Init();
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
                        receiveThread.IsBackground = true;
                        receiveThread.Start();

                        button.Content = "断开";
                        button.SetResourceReference(ContentControl.ContentProperty, "disconnect");
                        connectstate = true;
                    }

                }
                else
                {

                    //复位CAN
                    CANSDK.VCI_ResetCAN(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind);
                    // saveBmuConfigList.Clear();

                    //断开CAN
                    CANSDK.VCI_CloseDevice(CANSDK.m_devtype, CANSDK.m_devind);
                    cancode = 0;
                    button.Content = "连接";
                    button.SetResourceReference(ContentControl.ContentProperty, "connect");
                    //if (sendBMSRemoteThread != null)
                    //    sendBMSRemoteThread.Abort();
                    connectstate = false;
                    //复位
                    retryTimes = 0;
                    isWaitting.Clear();
                    isGetReceived = 1;
                    isGaosProfram = 0;
                    saveBmuConfigList.Clear();

                    if (receiveThread != null)
                    {
                        receiveThread.Abort();
                        receiveThread = null;
                        //主机状态按钮重置为未连接
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Label btn = page.FindName("isOnlineBtn") as Label;
                            btn.Content = "主机离线";
                            btn.SetResourceReference(ContentControl.ContentProperty, "masteroffline");
                            isMasterOnline = false;
                            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
                        });
                        // isWaitting.Clear();
                    }
                    //关闭发送心跳线程
                    Button btn2 = page.FindName("shishiBtn") as Button;
                    if (!isMasterShishi)
                    {
                        isMasterShishi = true;
                        isGetMasterInfo = 0;
                        if (sendUpSystemHeart != null)
                        {
                            //关闭上位机发送心跳
                            sendUpSystemHeart.Abort();
                            sendUpSystemHeart = null;
                        }

                        btn2.Content = "开启实时监测";
                    }
                    //关闭主机数据文件记录 717
                   // closeMasterDataFile();


                    //if (masterDebug != null && masterSW != null)
                    //{
                    //    masterSW.Close();
                    //    masterDebug.Close();                     
                    //    masterDebug = null;
                    //    masterSW = null;
                    //}

                    //清空主机信息显示页
                    clearMasterInfo();
                }

            }
            catch (Exception ex)
            {
                //ModernDialog.ShowMessage("打开设备异常！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["opendevicewrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
        }

        //求累加校验和
        private byte checkSum(byte[] b)
        {
            byte sum = b[0];
            for (int i = 1; i < b.Length - 1; i++)
            {
                sum += b[i];
            }
            return sum;
        }

        //上位机发送心跳
        private void sendHeartThread()
        {
            int period = 2;//周期单位秒

            byte[] data = new byte[8];
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F003F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x00;
            data[1] = 0x00;
            data[2] = 0x00;
            data[3] = 0x00;
            data[4] = 0x00;
            data[5] = 0x00;
            data[6] = 0x00;
            data[7] = 0x00;
            obj.DataLen = 8;
            obj.Data = data;
            while (true)
            {
                Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                Thread.Sleep(period * 1000);
            }

        }

        private bool isReceive = true;
        /// <summary>
        /// CAN接收数据线程
        /// </summary>
        private void ReceiveDataThread()
        {

            while (isReceive)
            {
                if (DateTime.Now.Ticks / 10000 - masterHeartTime > 6000)
                {
                    //超过6s未收到心跳，视为主机离线
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Label btn = page.FindName("isOnlineBtn") as Label;
                        btn.Content = "主机离线";
                        btn.SetResourceReference(ContentControl.ContentProperty, "masteroffline");
                        isMasterOnline = false;
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
                    });
                    //runHandShakeWithMaster();//重新握手
                }
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CANSDK.VCI_CAN_OBJ)) * (Int32)RECEIVELEN);
                uint receiveRealLen = CANSDK.VCI_Receive(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, pt, RECEIVELEN, 500);
                Thread.Sleep(10);
                if (receiveRealLen <= 0) continue;
                for (int i = 0; i < receiveRealLen; i++)
                {
                    CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(CANSDK.VCI_CAN_OBJ))), typeof(CANSDK.VCI_CAN_OBJ));
                    //lock (debuglocker)
                    //{
                    //    debugSW.WriteLine("接收：" + DataConverter.byteToHexStrForData(obj.Data) + " ID:" + obj.ID.ToString("X2") + "    " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));
                    //}
                    analysisPackage(obj);
                    analysisID(obj);
                    //分析主机下位机发送的主机信息报文（ID=0x18F3F003）
                    ThreadPool.QueueUserWorkItem(new WaitCallback(receiveMasterInfoThread), obj);
                    //执行异步调用 解析BMU总体信息包
                    receiveHandler handler1 = new receiveHandler(receiveBMUInfo);
                    handler1.BeginInvoke(obj, null, null);
                    receiveHandler handler2 = new receiveHandler(receiveCellInfo);
                    handler2.BeginInvoke(obj, null, null);
                    receiveHandler handler3 = new receiveHandler(receiveBmuInfo2);
                    handler3.BeginInvoke(obj, null, null);
                    receiveHandler handler4 = new receiveHandler(receiveSignalVolInfo);
                    handler4.BeginInvoke(obj, null, null);
                    receiveHandler handler5 = new receiveHandler(receiveTmpInfo);
                    handler5.BeginInvoke(obj, null, null);
                    receiveHandler handler6 = new receiveHandler(receiveSignalTmpInfo);
                    handler6.BeginInvoke(obj, null, null);
                    //handler = new receiveHandler(receiveSignalTMOSInfo);
                    //handler.BeginInvoke(obj, null, null);
                    receiveHandler handler7 = new receiveHandler(receiveTMOSInfo);
                    handler7.BeginInvoke(obj, null, null);
                    //异步接受从机配置信息
                    receiveHandler handler8 = new receiveHandler(receiveBmucfgInfo);
                    handler8.BeginInvoke(obj, null, null);
                    receiveHandler handler9 = new receiveHandler(receiveBalanceInfo);
                    handler9.BeginInvoke(obj, null, null);
                    receiveHandler handler10 = new receiveHandler(receiveBMUBalanceInfo);
                    handler10.BeginInvoke(obj, null, null);
                    //异步接收从机版本号信息
                    receiveHandler handler11 = new receiveHandler(receiveBMUVersionInfo);
                    handler11.BeginInvoke(obj, null, null);

                    receiveDiaInfo(obj);//by GaoYa
                    receiveTiedianInfo(obj);//2019.7.26 by GaoYa 铁电数据
                    receiveSlavePassiveEqu(obj);//2019.7.29 by GaoYa 接收
                }

                Marshal.FreeHGlobal(pt);
            }
        }


        //关闭主机数据保存文件
        private void closeMasterDataFile(bool isWriteXml)
        {

            if (masterDebug != null && masterSW != null)
            {
                masterSW.Close();
                masterDebug.Close();
                masterDebug = null;
                masterSW = null;
            }
            if (isWriteXml && isOpenMasterRecord)
            {
                //将信息写入Config.xml
                XmlDocument doc = new XmlDocument();
                //doc.Load("../../Config/Config.xml");
                doc.Load("Config/Config.xml");
                XmlNode xn = doc.SelectSingleNode("/appSettings/masterdatafile");
                xn.InnerText = masterFile;
                xn = doc.SelectSingleNode("/appSettings/masterdataline2");
                xn.InnerText = masterFileLineNum.ToString();

                //doc.Save("../../Config/Config.xml");
                doc.Save("Config/Config.xml");

            }
        }

        //新建主机数据保存文件
        private void createNewMasterDataFile()
        {
            masterFile = "debuginfo/masterDebug" + (DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000 + ".csv";
            closeMasterDataFile(false);

            masterDebug = new FileStream(masterFile, FileMode.Create);
            masterSW = new StreamWriter(masterDebug, Encoding.Default);
            //masterSW.WriteLine("编号,接收时间,信息项,细分,值");
            String dataLine = "序号,接收时间,SOC,总压,总流,温差,压差";
            //String dataLine = "No.,Receiving time,SOC,Total voltage,Total current,Temperature difference,Differential pressure";
            //masterSW.Write("序号,接收时间,SOC,总压,总流,温差,压差");
            for (int i = 0; i < wenganshu.Length; i++)
            {
                for (int j = 0; j < wenganshu[i]; j++)
                {
                    dataLine += ",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度";
                    //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell temperature";
                    //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度");
                }
            }
            for (int i = 0; i < chuanshu.Length; i++)
            {
                for (int j = 0; j < chuanshu[i]; j++)
                {
                    dataLine += ",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压";
                    //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell voltage";
                    //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压");
                }
            }
            //masterSW.WriteLine();
            masterSW.WriteLine(DataConverter.AESEncrypt(dataLine, SECRETKEY));
            //masterSW.Write(DataConverter.bytestoString(DataConverter.TextEncrypt(dataLine+"\r\n", SECRETKEY)));
            //masterSW.WriteLine(dataLine);
            masterFileLineNum = 0;

            //将信息写入Config.xml
            XmlDocument doc = new XmlDocument();
            //doc.Load("../../Config/Config.xml");
            doc.Load("Config/Config.xml");
            XmlNode xn = doc.SelectSingleNode("/appSettings/masterdatafile");
            xn.InnerText = masterFile;
            xn = doc.SelectSingleNode("/appSettings/masterdataline2");
            xn.InnerText = masterFileLineNum.ToString();

            //doc.Save("../../Config/Config.xml");
            doc.Save("Config/Config.xml");
        }

        //解析主机心跳包和信息
        private void receiveMasterInfoThread(Object o)
        {
            CANSDK.VCI_CAN_OBJ obj = (CANSDK.VCI_CAN_OBJ)o;
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string id = DataConverter.byteToHexStrForId(intBuff);
            if (new Regex(MASTERHEART).IsMatch(id))
            {
                //收到主机心跳包
                masterHeartTime = DateTime.Now.Ticks / 10000;//单位毫秒

                if (!isMasterOnline)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Label btn = page.FindName("isOnlineBtn") as Label;
                        btn.Content = "主机在线";
                        btn.SetResourceReference(ContentControl.ContentProperty, "masteronline");
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                    });
                }
                isMasterOnline = true;
            }
            else if (new Regex(MASTERINFO).IsMatch(id) && isGetMasterInfo == 1)
            {

                //收到主机信息
                Console.WriteLine("receiveMasterInfo：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                if (new Regex(MASTERINFO).IsMatch(id))
                {
                    byte[] data = obj.Data;
                    if (data[0] == 0x41)
                    {

                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = (int)data[5];
                        int temp4 = data[7] << 8 | data[6];
                        Application.Current.Dispatcher.Invoke((Action)delegate
                       {
                           StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                           if (spf.Children.Count == 0)
                           {
                               return;
                           }
                           TextBlock tbtitle = spf.Children[0] as TextBlock;
                           tbtitle.Visibility = Visibility.Visible;
                           int q = data[0] - 0x41 + 1;
                           StackPanel sp = spf.Children[q] as StackPanel;

                           StackPanel sp1 = sp.Children[0] as StackPanel;
                           sp1.Children.Clear();
                           sp1.Visibility = Visibility.Visible;
                           TextBlock tb0 = new TextBlock();
                           //tb0.Width = 150;
                           tb0.Width = 350;
                           tb0.VerticalAlignment = VerticalAlignment.Center;
                           tb0.TextAlignment = TextAlignment.Left;
                           //tb0.Visibility = Visibility.Visible;
                           tb0.Text = "电池组总电压：" + temp * 0.1 + "V";
                           tb0.Text = (string)page.Resources["packagevolsum"] + temp * 0.1 + "V";
                           if (isOpenMasterRecord)
                           {
                               dataRecordPerLine[1] = temp * 0.1 + "V";
                               lock (debuglocker) { dataRecordPerLineNum += 1; }
                           }

                           sp1.Children.Add(tb0);

                           StackPanel sp2 = sp.Children[1] as StackPanel;
                           sp2.Visibility = Visibility.Visible;
                           sp2.Children.Clear();
                           TextBlock tb1 = new TextBlock();
                           tb1.Width = 350;
                           tb1.VerticalAlignment = VerticalAlignment.Center;
                           tb1.TextAlignment = TextAlignment.Left;
                           //tb1.Visibility = Visibility.Visible;

                           double b = temp2 * 0.1 - 600;
                           String str = b.ToString();
                           int pos = b.ToString().IndexOf('.');
                           if (pos != -1)
                           {
                               //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                               int py = (pos + 2) > str.Length ? str.Length : (pos + 2);
                               str = str.Substring(0, py);
                           }

                           tb1.Text = "总电流：" + str + "A";
                           tb1.Text = (string)page.Resources["currentsum"] + str + "A";
                           //if (masterSW != null)
                           //{
                           //    lock (debuglocker) { masterSW.WriteLine("接收时间：" + DateTime.Now.ToLocalTime().ToString() + "，" + tb1.Text); }

                           //}

                           //717
                           //if (masterSW != null & isOpenMasterRecord)
                           //{
                           //    lock (debuglocker)
                           //    {

                           //        if (masterFileLineNum < 0 | masterFileLineNum >= masterDataRecordPerFileNum)
                           //        {
                           //            createNewMasterDataFile();
                           //        }

                           //        masterSW.WriteLine((masterFileLineNum++) + "," + DateTime.Now.ToLocalTime().ToString() + "," + "总流," + "电池组总电流," + str + "A");
                           //    }

                           //}
                           sp2.Children.Add(tb1);

                           StackPanel sp3 = sp.Children[2] as StackPanel;
                           sp3.Visibility = Visibility.Visible;
                           sp3.Children.Clear();
                           TextBlock tb2 = new TextBlock();
                           tb2.Width = 350;
                           tb2.VerticalAlignment = VerticalAlignment.Center;
                           tb2.TextAlignment = TextAlignment.Left;
                           //tb2.Visibility = Visibility.Visible;
                           tb2.Text = "SOC：" + temp3 + "%";
                           if (isOpenMasterRecord)
                           {
                               dataRecordPerLine[0] = temp3 + "%";
                               lock (debuglocker) { dataRecordPerLineNum += 1; }

                           }
                           sp3.Children.Add(tb2);

                           StackPanel sp4 = sp.Children[3] as StackPanel;
                           sp4.Visibility = Visibility.Visible;
                           sp4.Children.Clear();
                           TextBlock tb3 = new TextBlock();
                           tb3.Width = 350;
                           tb3.VerticalAlignment = VerticalAlignment.Center;
                           tb3.TextAlignment = TextAlignment.Left;
                           //tb3.Visibility = Visibility.Visible;
                           tb3.Text = "总容量：" + temp4 * 0.1 + "Ah";
                           tb3.Text = (string)page.Resources["capacitysum"] + temp4 * 0.1 + "Ah";
                           sp4.Children.Add(tb3);

                           spf = page.FindName("masterShiShiInfo12") as StackPanel;
                           if (spf.Children.Count == 0)
                           {
                               return;
                           }
                           tbtitle = spf.Children[0] as TextBlock;
                           tbtitle.Visibility = Visibility.Visible;
                           q = 8;
                           sp = spf.Children[q] as StackPanel;

                           sp1 = sp.Children[0] as StackPanel;
                           sp1.Visibility = Visibility.Visible;
                           sp1.Children.Clear();
                           tb0 = new TextBlock();
                           tb0.Width = 350;
                           tb0.VerticalAlignment = VerticalAlignment.Center;
                           tb0.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           b = temp2 * 0.1 - 600;
                           str = b.ToString();
                           pos = b.ToString().IndexOf('.');
                           if (pos != -1)
                           {
                               //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                               //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                               int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                               str = str.Substring(0, py);
                           }
                           tb0.Text = "霍尔1电流：" + str + "A";
                           tb0.Text = (string)page.Resources["currentsum"] + str + "A";
                           if (isOpenMasterRecord)
                           {
                               dataRecordPerLine[2] = str + "A";
                               lock (debuglocker) { dataRecordPerLineNum += 1; }
                           }
                           sp1.Children.Add(tb0);
                       });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：41");
                            Info = "接收到41帧数据";
                            Info = (string)page.Resources["recframe41"];
                        });
                    }
                    else if (data[0] == 0x42)
                    {

                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = data[0] - 0x41 + 1;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "电池组余量：" + temp * 0.1 + "Ah";
                            tb0.Text = (string)page.Resources["packcapacity"] + temp * 0.1 + "Ah";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            tb1.Text = "单体最高电压：" + temp2 * 0.001 + "V";
                            tb1.Text = (string)page.Resources["cellmaxhighvol"] + temp2 * 0.001 + "V";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "单体最低电压：" + temp3 * 0.001 + "V";
                            tb2.Text = (string)page.Resources["cellmaxlowvol"] + temp3 * 0.001 + "V";
                            sp3.Children.Add(tb2);

                            StackPanel sp4 = sp.Children[3] as StackPanel;
                            sp4.Visibility = Visibility.Visible;
                            sp4.Children.Clear();
                            tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "SOH：" + data[7] + "%";
                            //tb2.Text = (string)page.Resources["SOH"] + data[7] + "%";
                            sp4.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：42");
                            Info = "接收到42帧数据";
                            Info = (string)page.Resources["recframe42"];
                        });
                    }
                    else if (data[0] == 0x43)
                    {


                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = data[0] - 0x41 + 1;//3
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp0 = sp.Children[0] as StackPanel;
                            sp0.Visibility = Visibility.Visible;
                            sp0.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            //tb0.Visibility = Visibility.Visible;
                            tb0.Text = "单体最高电压箱号：" + (int)data[1];
                            tb0.Text = (string)page.Resources["cellmaxhighboxnum"] + (int)data[1];
                            sp0.Children.Add(tb0);

                            StackPanel sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Text = "节号：" + (int)data[2];
                            tb1.Text = (string)page.Resources["jienum"] + (int)data[2];
                            sp1.Children.Add(tb1);

                            StackPanel sp2 = sp.Children[2] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            //tb2.Visibility = Visibility.Visible;
                            tb2.Text = "单体最低电压箱号：" + (int)data[3];
                            tb2.Text = (string)page.Resources["cellmaxlowboxnum"] + (int)data[3];
                            sp2.Children.Add(tb2);

                            StackPanel sp3 = sp.Children[3] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb3 = new TextBlock();
                            tb3.Width = 350;
                            tb3.VerticalAlignment = VerticalAlignment.Center;
                            tb3.TextAlignment = TextAlignment.Left;
                            //tb3.Visibility = Visibility.Visible;
                            tb3.Text = "节号：" + (int)data[4];
                            tb3.Text = (string)page.Resources["jienum"] + (int)data[4];
                            sp3.Children.Add(tb3);

                            q += 2;
                            sp = spf.Children[q] as StackPanel;

                            //StackPanel sp4 = sp.Children[4] as StackPanel;
                            StackPanel sp4 = sp.Children[0] as StackPanel;
                            sp4.Visibility = Visibility.Visible;
                            sp4.Children.Clear();
                            TextBlock tb4 = new TextBlock();
                            tb4.Width = 350;
                            tb4.VerticalAlignment = VerticalAlignment.Center;
                            tb4.TextAlignment = TextAlignment.Left;
                            //tb4.Visibility = Visibility.Visible;
                            tb4.Text = "单体最高温度：" + ((int)data[5] - 40) + "℃";
                            tb4.Text = (string)page.Resources["cellmaxhightem"] + ((int)data[5] - 40) + "℃";
                            sp4.Children.Add(tb4);

                            //StackPanel sp5 = sp.Children[5] as StackPanel;
                            StackPanel sp5 = sp.Children[1] as StackPanel;
                            sp5.Visibility = Visibility.Visible;
                            sp5.Children.Clear();
                            TextBlock tb5 = new TextBlock();
                            tb5.Width = 350;
                            tb5.VerticalAlignment = VerticalAlignment.Center;
                            tb5.TextAlignment = TextAlignment.Left;
                            //tb5.Visibility = Visibility.Visible;
                            tb5.Text = "单体最低温度：" + ((int)data[6] - 40) + "℃";
                            tb5.Text = (string)page.Resources["cellmaxlowtem"] + ((int)data[6] - 40) + "℃";
                            sp5.Children.Add(tb5);

                            sp5 = sp.Children[2] as StackPanel;
                            sp5.Visibility = Visibility.Visible;
                            sp5.Children.Clear();
                            tb5 = new TextBlock();
                            tb5.Width = 350;
                            tb5.VerticalAlignment = VerticalAlignment.Center;
                            tb5.TextAlignment = TextAlignment.Left;
                            //tb5.Visibility = Visibility.Visible;
                            tb5.Text = "BMU个数：" + data[7];
                            tb5.Text = (string)page.Resources["bmugeshu"] + data[7];
                            sp5.Children.Add(tb5);
                            createMasterBalance(data[7]);


                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：43");
                            Info = "接收到43帧数据";
                            Info = (string)page.Resources["recframe43"];
                        });
                    }
                    else if (data[0] == 0x44)
                    {
                        int temp = data[6] << 8 | data[5];


                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = data[0] - 0x41 + 1;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp0 = sp.Children[0] as StackPanel;
                            sp0.Visibility = Visibility.Visible;
                            sp0.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            //tb0.Visibility = Visibility.Visible;
                            tb0.Text = "单体最高温度箱号：" + (int)data[1];
                            tb0.Text = (string)page.Resources["cellmaxhightemboxnum"] + (int)data[1];
                            sp0.Children.Add(tb0);

                            StackPanel sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Text = "内部位置：" + (int)data[2];
                            tb1.Text = (string)page.Resources["innerposition"] + (int)data[2];
                            sp1.Children.Add(tb1);

                            StackPanel sp2 = sp.Children[2] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            //tb2.Visibility = Visibility.Visible;
                            tb2.Text = "单体最低温度箱号：" + (int)data[3];
                            tb2.Text = (string)page.Resources["cellmaxlowtemboxnum"] + (int)data[3];
                            sp2.Children.Add(tb2);

                            StackPanel sp3 = sp.Children[3] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb3 = new TextBlock();
                            tb3.Width = 350;
                            tb3.VerticalAlignment = VerticalAlignment.Center;
                            tb3.TextAlignment = TextAlignment.Left;
                            //tb3.Visibility = Visibility.Visible;
                            tb3.Text = "内部位置：" + (int)data[4];
                            tb3.Text = (string)page.Resources["innerposition"] + (int)data[4];
                            sp3.Children.Add(tb3);

                            q++;
                            sp = spf.Children[q] as StackPanel;
                            //StackPanel sp4 = sp.Children[4] as StackPanel;
                            StackPanel sp4 = sp.Children[3] as StackPanel;
                            sp4.Visibility = Visibility.Visible;
                            sp4.Children.Clear();
                            TextBlock tb4 = new TextBlock();
                            tb4.Width = 350;
                            tb4.VerticalAlignment = VerticalAlignment.Center;
                            tb4.TextAlignment = TextAlignment.Left;
                            //tb4.Visibility = Visibility.Visible;
                            tb4.Text = "绝缘阻值：" + temp + "KΩ";
                            tb4.Text = (string)page.Resources["jyzz"] + temp + "KΩ";
                            sp4.Children.Add(tb4);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：44");
                            Info = "接收到44帧数据";
                            Info = (string)page.Resources["recframe44"];
                        });
                    }
                    else if (data[0] == 0x45)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            WrapPanel wp = page.FindName("masterShiShiInfo10") as WrapPanel;

                            for (int i = 0; i < 6; i++)
                            {
                                if (alarmInfo.Contains(data[i + 1]))
                                {
                                    break;
                                }
                                else
                                {
                                    alarmInfo.Add(data[i + 1]);
                                }
                                //for (int j = 0; j < alarmInfo.Count; j++)
                                //{
                                //    //如果已经显示过该报警信息则不再重复显示
                                //    if (data[i + 1].Equals(alarmInfo[j]))
                                //    {
                                //        break;
                                //    }
                                //    else {
                                //        alarmInfo.Add(data[i + 1]);

                                //    }
                                //}
                                String s_alarm = "";
                                switch (data[i + 1])
                                {
                                    case 1:
                                        s_alarm = "电池组总压过高1级";
                                        s_alarm = (string)page.Resources["packvol1"];
                                        break;
                                    case 51:
                                        s_alarm = "电池组总压过高2级";
                                        s_alarm = (string)page.Resources["packvol2"];
                                        break;
                                    case 101:
                                        s_alarm = "电池组总压过高3级";
                                        s_alarm = (string)page.Resources["packvol3"];
                                        break;
                                    case 151:
                                        s_alarm = "电池组总压过高4级";
                                        s_alarm = (string)page.Resources["packvol4"];
                                        break;
                                    case 3:
                                        s_alarm = "单体电压过高1级";
                                        s_alarm = (string)page.Resources["cellvol1"];
                                        break;
                                    case 53:
                                        s_alarm = "单体电压过高2级";
                                        s_alarm = (string)page.Resources["cellvol2"];
                                        break;
                                    case 103:
                                        s_alarm = "单体电压过高3级";
                                        s_alarm = (string)page.Resources["cellvol3"];
                                        break;
                                    case 153:
                                        s_alarm = "单体电压过高4级";
                                        s_alarm = (string)page.Resources["cellvol4"];
                                        break;
                                    case 5:
                                        s_alarm = "电池组温度过高1级";
                                        s_alarm = (string)page.Resources["packtem1"];
                                        break;
                                    case 55:
                                        s_alarm = "电池组温度过高2级";
                                        s_alarm = (string)page.Resources["packtem2"];
                                        break;
                                    case 105:
                                        s_alarm = "电池组温度过高3级";
                                        s_alarm = (string)page.Resources["packtem3"];
                                        break;
                                    case 155:
                                        s_alarm = "电池组温度过高4级";
                                        s_alarm = (string)page.Resources["packtem4"];
                                        break;
                                    case 7:
                                        s_alarm = "电池组防电过流1级";
                                        s_alarm = (string)page.Resources["packcur1"];
                                        break;
                                    case 57:
                                        s_alarm = "电池组防电过流2级";
                                        s_alarm = (string)page.Resources["packcur2"];
                                        break;
                                    case 107:
                                        s_alarm = "电池组防电过流3级";
                                        s_alarm = (string)page.Resources["packcur3"];
                                        break;
                                    case 157:
                                        s_alarm = "电池组防电过流4级";
                                        s_alarm = (string)page.Resources["packcur4"];
                                        break;
                                    case 9:
                                        s_alarm = "电池组压差过大1级";
                                        s_alarm = (string)page.Resources["packyacha1"];
                                        break;
                                    case 59:
                                        s_alarm = "电池组压差过大2级";
                                        s_alarm = (string)page.Resources["packyacha2"];
                                        break;
                                    case 109:
                                        s_alarm = "电池组压差过大3级";
                                        s_alarm = (string)page.Resources["packyacha3"];
                                        break;
                                    case 159:
                                        s_alarm = "电池组压差过大4级";
                                        s_alarm = (string)page.Resources["packyacha4"];
                                        break;
                                    case 11:
                                        s_alarm = "SOC过低1级";
                                        s_alarm = (string)page.Resources["soclow1"];
                                        break;
                                    case 61:
                                        s_alarm = "SOC过低2级";
                                        s_alarm = (string)page.Resources["soclow2"];
                                        break;
                                    case 111:
                                        s_alarm = "SOC过低3级";
                                        s_alarm = (string)page.Resources["soclow3"];
                                        break;
                                    case 13:
                                        s_alarm = "充电座过温故障1级";
                                        s_alarm = (string)page.Resources["cdzhightem1"];
                                        break;
                                    case 63:
                                        s_alarm = "充电座过温故障2级";
                                        s_alarm = (string)page.Resources["cdzhightem2"];
                                        break;
                                    case 113:
                                        s_alarm = "充电座过温故障3级";
                                        s_alarm = (string)page.Resources["cdzhightem3"];
                                        break;
                                    case 21:
                                        s_alarm = "内部通讯故障";
                                        s_alarm = (string)page.Resources["innercommfault"];
                                        break;
                                    case 22:
                                        s_alarm = "从机硬件故障";
                                        s_alarm = (string)page.Resources["bmuhardwarefault"];
                                        break;
                                    case 23:
                                        s_alarm = "信号采集线故障";
                                        s_alarm = (string)page.Resources["signalcollectfault"];
                                        break;
                                    case 2:
                                        s_alarm = "电池组总压过低1级";
                                        s_alarm = (string)page.Resources["packlowvol1"];
                                        break;
                                    case 52:
                                        s_alarm = "电池组总压过低2级";
                                        s_alarm = (string)page.Resources["packlowvol2"];
                                        break;
                                    case 102:
                                        s_alarm = "电池组总压过低3级";
                                        s_alarm = (string)page.Resources["packlowvol3"];
                                        break;
                                    case 152:
                                        s_alarm = "电池组总压过低4级";
                                        s_alarm = (string)page.Resources["packlowvol4"];
                                        break;
                                    case 4:
                                        s_alarm = "单体电压过低1级";
                                        s_alarm = (string)page.Resources["celllowvol1"];
                                        break;
                                    case 54:
                                        s_alarm = "单体电压过低2级";
                                        s_alarm = (string)page.Resources["celllowvol2"];
                                        break;
                                    case 104:
                                        s_alarm = "单体电压过低3级";
                                        s_alarm = (string)page.Resources["celllowvol3"];
                                        break;
                                    case 154:
                                        s_alarm = "单体电压过低4级";
                                        s_alarm = (string)page.Resources["celllowvol4"];
                                        break;
                                    case 6:
                                        s_alarm = "电池组温度过低1级";
                                        s_alarm = (string)page.Resources["packlowtem1"];
                                        break;
                                    case 56:
                                        s_alarm = "电池组温度过低2级";
                                        s_alarm = (string)page.Resources["packlowtem2"];
                                        break;
                                    case 106:
                                        s_alarm = "电池组温度过低3级";
                                        s_alarm = (string)page.Resources["packlowtem3"];
                                        break;
                                    case 156:
                                        s_alarm = "电池组温度过低4级";
                                        s_alarm = (string)page.Resources["packlowtem4"];
                                        break;
                                    case 8:
                                        s_alarm = "电池组充电过流1级";
                                        s_alarm = (string)page.Resources["packchargecurr1"];
                                        break;
                                    case 58:
                                        s_alarm = "电池组充电过流2级";
                                        s_alarm = (string)page.Resources["packchargecurr2"];
                                        break;
                                    case 108:
                                        s_alarm = "电池组充电过流3级";
                                        s_alarm = (string)page.Resources["packchargecurr3"];
                                        break;
                                    case 158:
                                        s_alarm = "电池组充电过流4级";
                                        s_alarm = (string)page.Resources["packchargecurr4"];
                                        break;
                                    case 10:
                                        s_alarm = "电池组温差过大1级";
                                        s_alarm = (string)page.Resources["packwenchahigh1"];
                                        break;
                                    case 60:
                                        s_alarm = "电池组温差过大2级";
                                        s_alarm = (string)page.Resources["packwenchahigh2"];
                                        break;
                                    case 110:
                                        s_alarm = "电池组温差过大3级";
                                        s_alarm = (string)page.Resources["packwenchahigh3"];
                                        break;
                                    case 160:
                                        s_alarm = "电池组温差过大4级";
                                        s_alarm = (string)page.Resources["packwenchahigh4"];
                                        break;
                                    case 12:
                                        s_alarm = "绝缘过低1级";
                                        s_alarm = (string)page.Resources["jueyuanlow1"];
                                        break;
                                    case 62:
                                        s_alarm = "绝缘过低2级";
                                        s_alarm = (string)page.Resources["jueyuanlow2"];
                                        break;
                                    case 112:
                                        s_alarm = "绝缘过低3级";
                                        s_alarm = (string)page.Resources["jueyuanlow3"];
                                        break;
                                    case 31:
                                        s_alarm = "MSD故障";
                                        s_alarm = (string)page.Resources["msd"];
                                        break;
                                    case 32:
                                        s_alarm = "高压互锁故障";
                                        s_alarm = (string)page.Resources["gyhs"];
                                        break;
                                    case 33:
                                        s_alarm = "放电回路继电器故障";
                                        s_alarm = (string)page.Resources["fangdianjdqfault"];
                                        break;
                                    case 34:
                                        s_alarm = "充电回路继电器故障";
                                        s_alarm = (string)page.Resources["chongfangdianjdqfault"];
                                        break;
                                    case 35:
                                        s_alarm = "加热回路继电器故障";
                                        s_alarm = (string)page.Resources["jiarejdqfault"];
                                        break;
                                    default:
                                        break;
                                }
                                if (!s_alarm.Equals(""))
                                {
                                    Border border = new Border();


                                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(91, 192, 222));
                                    border.BorderThickness = new Thickness(2);
                                    border.Margin = new Thickness(5);
                                    border.Padding = new Thickness(5);
                                    TextBlock tb = new TextBlock();
                                    tb.Width = 120;

                                    tb.Text = s_alarm;
                                    tb.TextWrapping = TextWrapping.Wrap;
                                    border.Child = tb;
                                    wp.Children.Add(border);

                                }

                            }//for
                            if (!alarmInfo2.Contains(data[7]))
                            {
                                alarmInfo2.Add(data[7]);
                                String s_alarm2 = "";
                                switch (data[7])
                                {
                                    case 1:
                                        s_alarm2 = "充电握手启动阶段超时";
                                        s_alarm2 = (string)page.Resources["chargehsstartovertime"];
                                        break;
                                    case 2:
                                        s_alarm2 = "充电握手阶段通讯超时";
                                        s_alarm2 = (string)page.Resources["chargehscommovertime"];
                                        break;
                                    case 3:
                                        s_alarm2 = "参数配置阶段1超时";
                                        s_alarm2 = (string)page.Resources["paraconfig1overtime"];
                                        break;
                                    case 4:
                                        s_alarm2 = "参数配置阶段2超时";
                                        s_alarm2 = (string)page.Resources["paraconfig2overtime"];
                                        break;
                                    case 20:
                                        s_alarm2 = "单体电压过高，异常停机";
                                        s_alarm2 = (string)page.Resources["cellvolhighshutdown"];
                                        break;
                                    case 21:
                                        s_alarm2 = "SOC达到100%，充电完成";
                                        s_alarm2 = (string)page.Resources["chargefinish"];
                                        break;
                                    case 22:
                                        s_alarm2 = "电池组温度过高，异常停机";
                                        s_alarm2 = (string)page.Resources["packhightemshutdown"];
                                        break;
                                    case 23:
                                        s_alarm2 = "充电电流过大，异常停机";
                                        s_alarm2 = (string)page.Resources["chargehighcurshutdown"];
                                        break;
                                    case 24:
                                        s_alarm2 = "内部CAN故障，异常停机";
                                        s_alarm2 = (string)page.Resources["innercanshutdown"];
                                        break;
                                    case 25:
                                        s_alarm2 = "绝缘报警，异常停机";
                                        s_alarm2 = (string)page.Resources["jueyuanshutdown"];
                                        break;
                                    case 26:
                                        s_alarm2 = "充电座过温，异常停机";
                                        s_alarm2 = (string)page.Resources["cdzhightemshutdown"];
                                        break;
                                    case 27:
                                        s_alarm2 = "充电机先发CST，异常停机";
                                        s_alarm2 = (string)page.Resources["cdjcstshutdown"];
                                        break;
                                    case 28:
                                        s_alarm2 = "充电阶段超时，异常停机";
                                        s_alarm2 = (string)page.Resources["chargeovertimeshutdown"];
                                        break;
                                    case 29:
                                        s_alarm2 = "电池组温度过低，异常停机";
                                        s_alarm2 = (string)page.Resources["packlowtemshutdown"];
                                        break;
                                    case 30:
                                        s_alarm2 = "SPN3513电流超过BCL需求";
                                        s_alarm2 = (string)page.Resources["spn3513bcl"];
                                        break;
                                    case 31:
                                        s_alarm2 = "SPN3512继电器黏连";
                                        s_alarm2 = (string)page.Resources["spn3513jdqnl"];
                                        break;
                                    case 32:
                                        s_alarm2 = "其他故障";
                                        s_alarm2 = (string)page.Resources["otherfault"];
                                        break;
                                    case 41:
                                        s_alarm2 = "加热继电器故障";
                                        s_alarm2 = (string)page.Resources["jrjdqfault"];
                                        break;
                                    case 50:
                                        s_alarm2 = "接收CST报文超时，异常停机";
                                        s_alarm2 = (string)page.Resources["reccstshutdown"];
                                        break;
                                    case 51:
                                        s_alarm2 = "接收CSD报文超时，异常停机";
                                        s_alarm2 = (string)page.Resources["reccsdshutdown"];
                                        break;
                                    default:
                                        break;
                                }
                                if (!s_alarm2.Equals(""))
                                {
                                    Border border2 = new Border();
                                    border2.BorderBrush = new SolidColorBrush(Color.FromRgb(91, 192, 222));
                                    border2.BorderThickness = new Thickness(2);
                                    border2.Margin = new Thickness(5);
                                    border2.Padding = new Thickness(5);
                                    TextBlock tb2 = new TextBlock();
                                    tb2.Width = 120;

                                    tb2.Text = s_alarm2;
                                    tb2.TextWrapping = TextWrapping.Wrap;
                                    border2.Child = tb2;
                                    wp.Children.Add(border2);
                                }
                            }


                            //StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                            //int q = data[0] - 0x41;
                            //StackPanel sp = spf.Children[q] as StackPanel;
                            //TextBox tb0 = sp.Children[0] as TextBox;
                            //tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "故障代码1：" + (int)data[1];

                            //TextBox tb1 = sp.Children[1] as TextBox;
                            //tb1.Visibility = Visibility.Visible;
                            //tb1.Text = "故障代码2：" + (int)data[2];

                            //TextBox tb2 = sp.Children[2] as TextBox;
                            //tb2.Visibility = Visibility.Visible;
                            //tb2.Text = "故障代码3：" + (int)data[3];

                            //TextBox tb3 = sp.Children[3] as TextBox;
                            //tb3.Visibility = Visibility.Visible;
                            //tb3.Text = "故障代码4：" + (int)data[4];

                            //TextBox tb4 = sp.Children[4] as TextBox;
                            //tb4.Visibility = Visibility.Visible;
                            //tb4.Text = "故障代码5：" + (int)data[5];

                            //TextBox tb5 = sp.Children[5] as TextBox;
                            //tb5.Visibility = Visibility.Visible;
                            //tb5.Text = "故障代码6：" + (int)data[4];

                            //TextBox tb6 = sp.Children[6] as TextBox;
                            //tb6.Visibility = Visibility.Visible;
                            //tb6.Text = "故障代码7：" + (int)data[5];

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：45");
                            Info = "接收到45帧数据";
                            Info = (string)page.Resources["recframe45"];
                        });
                    }
                    else if (data[0] == 0x46)
                    {

                        //单体电压和温感（0x47）在一块显示
                        if (data[1] > 250 || data[1] <= 0) { return; }
                        int temp = data[3] << 8 | data[2];
                        int temp2 = data[5] << 8 | data[4];
                        int temp3 = data[7] << 8 | data[6];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo2") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = data[1];
                            int i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 3 + 1 <= s + chuanshu[i] && (data[1] - 1) * 3 + 1 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += chuanshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            int titlePos = i + 1 + i;//标题地址索引
                            int dataPos = (data[1] - 1) * 3 + 1;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : (chuanshu[j] / 3 + 1));
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : (wenganshu[j] / 6 + 1));
                                //if (dataPos - chuanshu[j] != 0)
                                //{
                                dataPos -= chuanshu[j];
                                //}
                            }
                            //二级标题表示几号从机

                            StackPanel spt = spf.Children[titlePos] as StackPanel;
                            StackPanel spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                //spt1_tb1.Width = 170;
                                spt1_tb1.Width = 250;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体电压：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["cellvol"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5bc0de"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            int hPos = (dataPos % 3 == 0 ? dataPos / 3 : 1 + dataPos / 3);

                            //int hPos = 1+dataPos / 3;
                            int zPos = (dataPos - 1) % 3;
                            int q1 = titlePos + hPos;
                            StackPanel sp1 = spf.Children[q1] as StackPanel;
                            StackPanel sp1_1 = sp1.Children[zPos] as StackPanel;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 250;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb0.Text = "第" + dataPos + "单体电压：" + temp * 0.001 + "V";
                            tb0.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["cellvoltage"] + temp * 0.001 + "V";
                            if (isOpenMasterRecord)
                            {
                                dataRecordPerLine[dataRecordIndex["y" + (i + 1)] + (dataPos - 1)] = temp * 0.001 + "V";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                                //dataRecordPerLine[dataindex] = temp * 0.001 + "V";
                            }

                            sp1_1.Children.Clear();
                            sp1_1.Children.Add(tb0);
                            sp1_1.Visibility = Visibility.Visible;


                            i = 0;
                            for (int s = 0; i < 24; i++)
                            {
                                if ((data[1] - 1) * 3 + 2 <= s + chuanshu[i] && (data[1] - 1) * 3 + 2 > s)
                                {
                                    //数据位于第i个从机
                                    break;
                                }
                                s += chuanshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 1 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 3 + 2;//数据地址索引
                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : (chuanshu[j] / 3 + 1));
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : (wenganshu[j] / 6 + 1));
                                //if (dataPos - chuanshu[j] != 0)
                                //{
                                dataPos -= chuanshu[j];
                                //}
                            }
                            //二级标题表示几号从机
                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 250;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体电压：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["cellvol"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5bc0de"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 3 == 0 ? dataPos / 3 : 1 + dataPos / 3);
                            //hPos = 1 + dataPos / 3;
                            zPos = (dataPos - 1) % 3;
                            q1 = titlePos + hPos;
                            StackPanel sp2 = spf.Children[q1] as StackPanel;
                            StackPanel sp2_1 = sp2.Children[zPos] as StackPanel;
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 250;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb1.Text = "第" + ((data[1] - 1) * 3 + 2 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb1.Text = "第" + dataPos + "单体电压：" + temp2 * 0.001 + "V";
                            tb1.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["cellvoltage"] + temp2 * 0.001 + "V";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["y" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["y" + (i + 1)] + (dataPos - 1)] = temp2 * 0.001 + "V";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp2_1.Children.Clear();
                            sp2_1.Children.Add(tb1);
                            sp2_1.Visibility = Visibility.Visible;

                            i = 0;
                            for (int s = 0; i < 24; i++)
                            {
                                if ((data[1] - 1) * 3 + 3 <= s + chuanshu[i] && (data[1] - 1) * 3 + 3 > s)
                                {
                                    //数据位于第i个从机
                                    break;
                                }
                                s += chuanshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 1 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 3 + 3;//数据地址索引
                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : (chuanshu[j] / 3 + 1));
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : (wenganshu[j] / 6 + 1));
                                //if (dataPos - chuanshu[j] != 0)
                                //{
                                dataPos -= chuanshu[j];
                                //}
                            }
                            //二级标题表示几号从机
                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 250;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体电压：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["cellvol"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5bc0de"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 3 == 0 ? dataPos / 3 : 1 + dataPos / 3);
                            //hPos = hPos = 1 + dataPos / 3;
                            zPos = (dataPos - 1) % 3;
                            q1 = titlePos + hPos;
                            StackPanel sp3 = spf.Children[q1] as StackPanel;
                            StackPanel sp3_1 = sp3.Children[zPos] as StackPanel;
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 250;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb2.Text = "第" + ((data[1] - 1) * 3 + 3 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb2.Text = "第" + dataPos + "单体电压：" + temp3 * 0.001 + "V";
                            tb2.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["cellvoltage"] + temp3 * 0.001 + "V";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["y" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["y" + (i + 1)] + (dataPos - 1)] = temp3 * 0.001 + "V";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp3_1.Children.Clear();
                            sp3_1.Children.Add(tb2);
                            sp3_1.Visibility = Visibility.Visible;



                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：46");
                            Info = "接收到46帧数据";
                            Info = (string)page.Resources["recframe46"];
                        });
                    }
                    else if (data[0] == 0x47)
                    {
                        if (data[1] > 20 || data[1] <= 0) { return; }
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo2") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitile = spf.Children[0] as TextBlock;
                            tbtitile.Visibility = Visibility.Visible;

                            int q = data[1];
                            int i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 1 <= s + wenganshu[i] && (data[1] - 1) * 6 + 1 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            int titlePos = i + 2 + i;//标题地址索引
                            int dataPos = (data[1] - 1) * 6 + 1;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            StackPanel spt = spf.Children[titlePos] as StackPanel;
                            StackPanel spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltemperature"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            int hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //int hPos = 1 + dataPos / 6;
                            int zPos = (dataPos - 1) % 6;
                            int q1 = titlePos + hPos;
                            StackPanel sp1 = spf.Children[q1] as StackPanel;
                            StackPanel sp1_1 = sp1.Children[zPos] as StackPanel;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 170;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb0.Text = "第" + dataPos + "单体温度：" + ((int)data[2] - 40) + "℃";
                            tb0.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[2] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[2] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp1_1.Children.Clear();
                            sp1_1.Children.Add(tb0);
                            sp1_1.Visibility = Visibility.Visible;

                            //data[3]
                            q = data[1];
                            i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 2 <= s + wenganshu[i] && (data[1] - 1) * 6 + 2 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 2 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 6 + 2;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltem"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //hPos = 1 + dataPos / 6;
                            zPos = (dataPos - 1) % 6;
                            q1 = titlePos + hPos;
                            StackPanel sp2 = spf.Children[q1] as StackPanel;
                            StackPanel sp2_1 = sp2.Children[zPos] as StackPanel;
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb1.Text = "第" + dataPos + "单体温度：" + ((int)data[3] - 40) + "℃";
                            tb1.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[3] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["w" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[3] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp2_1.Children.Clear();
                            sp2_1.Children.Add(tb1);
                            sp2_1.Visibility = Visibility.Visible;

                            //data[4]
                            q = data[1];
                            i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 3 <= s + wenganshu[i] && (data[1] - 1) * 6 + 3 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 2 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 6 + 3;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltem"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //hPos = 1 + dataPos / 6;
                            zPos = (dataPos - 1) % 6;
                            q1 = titlePos + hPos;
                            StackPanel sp3 = spf.Children[q1] as StackPanel;
                            StackPanel sp3_1 = sp3.Children[zPos] as StackPanel;
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 170;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb2.Text = "第" + dataPos + "单体温度：" + ((int)data[4] - 40) + "℃";
                            tb2.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[4] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["w" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[4] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp3_1.Children.Clear();
                            sp3_1.Children.Add(tb2);
                            sp3_1.Visibility = Visibility.Visible;
                            //------------------------------------------------------------
                            //data[5]
                            q = data[1];
                            i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 4 <= s + wenganshu[i] && (data[1] - 1) * 6 + 4 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 2 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 6 + 4;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltem"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //hPos = 1 + dataPos / 6;
                            zPos = (dataPos - 1) % 6;
                            q1 = titlePos + hPos;
                            StackPanel sp4 = spf.Children[q1] as StackPanel;
                            StackPanel sp4_1 = sp4.Children[zPos] as StackPanel;
                            TextBlock tb3 = new TextBlock();
                            tb3.Width = 170;
                            tb3.VerticalAlignment = VerticalAlignment.Center;
                            tb3.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb3.Text = "第" + dataPos + "单体温度：" + ((int)data[5] - 40) + "℃";
                            tb3.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[5] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["w" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[5] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }
                            sp4_1.Children.Clear();
                            sp4_1.Children.Add(tb3);
                            sp4_1.Visibility = Visibility.Visible;

                            //data[6]
                            q = data[1];
                            i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 5 <= s + wenganshu[i] && (data[1] - 1) * 6 + 5 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 2 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 6 + 5;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltem"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //hPos = 1 + dataPos / 6;
                            zPos = (dataPos - 1) % 6;
                            q1 = titlePos + hPos;
                            StackPanel sp5 = spf.Children[q1] as StackPanel;
                            StackPanel sp5_1 = sp5.Children[zPos] as StackPanel;
                            TextBlock tb4 = new TextBlock();
                            tb4.Width = 170;
                            tb4.VerticalAlignment = VerticalAlignment.Center;
                            tb4.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb4.Text = "第" + dataPos + "单体温度：" + ((int)data[6] - 40) + "℃";
                            tb4.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[6] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["w" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[6] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }
                            sp5_1.Children.Clear();
                            sp5_1.Children.Add(tb4);
                            sp5_1.Visibility = Visibility.Visible;

                            //data[7]
                            q = data[1];
                            i = 0;

                            for (int s = 0; i < 24; i++)
                            {

                                if ((data[1] - 1) * 6 + 6 <= s + wenganshu[i] && (data[1] - 1) * 6 + 6 > s)
                                {

                                    //数据位于第i个从机
                                    break;
                                }
                                s += wenganshu[i];
                            }
                            if (i < 0 || i > 23) { return; }
                            titlePos = i + 2 + i;//标题地址索引
                            dataPos = (data[1] - 1) * 6 + 6;//数据地址索引

                            for (int j = 0; j < i; j++)
                            {
                                titlePos += (wenganshu[j] % 6 == 0 ? wenganshu[j] / 6 : wenganshu[j] / 6 + 1);
                                titlePos += (chuanshu[j] % 3 == 0 ? chuanshu[j] / 3 : chuanshu[j] / 3 + 1);
                                //if (dataPos - wenganshu[j] != 0)
                                //{
                                dataPos -= wenganshu[j];
                                //}
                            }
                            titlePos += (chuanshu[i] % 3 == 0 ? chuanshu[i] / 3 : chuanshu[i] / 3 + 1);
                            //二级标题表示几号从机

                            spt = spf.Children[titlePos] as StackPanel;
                            spt1 = spt.Children[0] as StackPanel;
                            if (spt1.Children.Count == 0)
                            {
                                spt1.Visibility = Visibility.Visible;
                                TextBlock spt1_tb1 = new TextBlock();
                                spt1_tb1.Width = 170;
                                spt1_tb1.VerticalAlignment = VerticalAlignment.Center;
                                spt1_tb1.TextAlignment = TextAlignment.Left;
                                spt1_tb1.Visibility = Visibility.Visible;
                                // tb0.Visibility = Visibility.Visible;
                                spt1_tb1.Text = "第" + (i + 1) + "号从机单体温度：";
                                spt1_tb1.Text = (string)page.Resources["no"] + (i + 1) + (string)page.Resources["celltem"];
                                spt1_tb1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99CC66"));
                                spt1.Children.Add(spt1_tb1);
                            }
                            hPos = (dataPos % 6 == 0 ? dataPos / 6 : 1 + dataPos / 6);
                            //hPos = 1 + dataPos / 6;
                            zPos = (dataPos - 1) % 6;
                            q1 = titlePos + hPos;
                            StackPanel sp6 = spf.Children[q1] as StackPanel;
                            StackPanel sp6_1 = sp6.Children[zPos] as StackPanel;
                            TextBlock tb5 = new TextBlock();
                            tb5.Width = 170;
                            tb5.VerticalAlignment = VerticalAlignment.Center;
                            tb5.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "第" + ((data[1] - 1) * 3 + 1 - dataPos) + "单体电压：" + temp * 0.001 + "V";
                            tb5.Text = "第" + dataPos + "单体温度：" + ((int)data[7] - 40) + "℃";
                            tb5.Text = (string)page.Resources["no"] + dataPos + (string)page.Resources["celltem2"] + ((int)data[7] - 40) + "℃";
                            if (isOpenMasterRecord)
                            {
                                //dataindex = dataRecordIndex["w" + (i + 1)] + (dataPos - 1);
                                dataRecordPerLine[dataRecordIndex["w" + (i + 1)] + (dataPos - 1)] = ((int)data[7] - 40) + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }

                            sp6_1.Children.Clear();
                            sp6_1.Children.Add(tb5);
                            sp6_1.Visibility = Visibility.Visible;


                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：47");
                            Info = "接收到47帧数据";
                            Info = (string)page.Resources["recframe47"];
                        });
                    }
                    else if (data[0] == 0x48)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo3") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            int q = 1;

                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Children.Clear();
                            sp1.Visibility = Visibility.Visible;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 170;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "BAT_B1-：" + temp * 0.001 + "V";
                            //tb0.Text = (string)page.Resources["volsumv1"] + temp * 0.1 + "V";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Children.Clear();
                            sp2.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.Text = "BAT_B2-：" + temp2 * 0.001 + "V";
                            //tb1.Text = (string)page.Resources["volsumv2"] + temp2 * 0.1 + "V";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            TextBlock tb2 = new TextBlock();
                            sp3.Children.Clear();
                            sp3.Visibility = Visibility.Visible;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Width = 170;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            tb2.Text = "BAT_B3-：" + temp3 * 0.001 + "V";
                            //tb2.Text = (string)page.Resources["volsumv3"] + temp3 * 0.1 + "V";
                            sp3.Children.Add(tb2);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine("接收到主机数据：48");
                            Info = "接收到48帧数据";
                            Info = (string)page.Resources["recframe48"];
                        });
                    }
                    else if (data[0] == 0x49)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo3") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            int q = 2;

                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Children.Clear();
                            sp1.Visibility = Visibility.Visible;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 170;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "BAT_B4-：" + temp * 0.001 + "V";
                            //tb0.Text = (string)page.Resources["volsumv4"] + temp * 0.1 + "V";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Children.Clear();
                            sp2.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.Text = "BAT+：" + temp2 * 0.1 + "V";
                            //tb1.Text = (string)page.Resources["volsumv5"] + temp2 * 0.1 + "V";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            TextBlock tb2 = new TextBlock();
                            sp3.Children.Clear();
                            sp3.Visibility = Visibility.Visible;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Width = 170;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            tb2.Text = "BAT_A1+：" + temp3 * 0.1 + "V";
                            //tb2.Text = (string)page.Resources["volsumv6"] + temp3 * 0.1 + "V";
                            sp3.Children.Add(tb2);
                        });

                        Application.Current.Dispatcher.Invoke((Action)delegate
                         {
                             Console.WriteLine("接收到主机数据：49");
                             //Info = "接收到49帧数据";
                             Info = (string)page.Resources["recframe49"];
                         });
                    }
                    else if (data[0] == 0x4A)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo3") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            int q = 3;

                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Children.Clear();
                            sp1.Visibility = Visibility.Visible;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 170;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "BAT_A2+：" + temp * 0.1 + "V";
                            //tb0.Text = (string)page.Resources["volsumv7"] + temp * 0.1 + "V";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Children.Clear();
                            sp2.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.Text = "BAT_A3+：" + temp2 * 0.1 + "V";
                            //tb1.Text = (string)page.Resources["volsumv8"] + temp2 * 0.1 + "V";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            TextBlock tb2 = new TextBlock();
                            sp3.Children.Clear();
                            sp3.Visibility = Visibility.Visible;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Width = 170;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            tb2.Text = "BAT_A4+：" + temp3 * 0.1 + "V";
                            //tb2.Text = (string)page.Resources["volsumv9"] + temp3 * 0.1 + "V";
                            sp3.Children.Add(tb2);
                        });

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：4A");
                            //Info = "接收到4A帧数据";
                            Info = (string)page.Resources["recframe4A"];
                        });
                    }
                    else if (data[0] == 0x4B)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[7] << 8 | data[6];
                        //int temp3 = data[6] << 8 | data[5];

                        //int temp3 = data[5] - 40;

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo3") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            int q = 4;

                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Children.Clear();
                            sp1.Visibility = Visibility.Visible;
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 170;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "BAT_A5+：" + temp * 0.1 + "V";
                            //tb0.Text = (string)page.Resources["volsumv10"] + temp * 0.1 + "V";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Children.Clear();
                            sp2.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.Text = "BAT_A6+：" + temp2 * 0.1 + "V";
                            //tb1.Text = (string)page.Resources["volsumv11"] + temp2 * 0.1 + "V";
                            sp2.Children.Add(tb1);

                            sp2 = sp.Children[2] as StackPanel;
                            sp2.Children.Clear();
                            sp2.Visibility = Visibility.Visible;
                            tb1 = new TextBlock();
                            //tb1.Visibility = Visibility.Visible;
                            tb1.Width = 170;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.Text = "BAT_A7+：" + temp3 * 0.1 + "V";
                            //tb1.Text = (string)page.Resources["volsumv11"] + temp2 * 0.1 + "V";
                            sp2.Children.Add(tb1);

                            //StackPanel sp3 = sp.Children[2] as StackPanel;
                            //TextBlock tb2 = new TextBlock();
                            //sp3.Children.Clear();
                            //sp3.Visibility = Visibility.Visible;
                            //// tb2.Visibility = Visibility.Visible;
                            //tb2.Width = 170;
                            //tb2.VerticalAlignment = VerticalAlignment.Center;
                            //tb2.TextAlignment = TextAlignment.Left;
                            //tb2.Text = "总电压V12：" + temp3 * 0.1 + "V";
                            //tb2.Text = (string)page.Resources["volsumv12"] + temp3 * 0.1 + "V";
                            //sp3.Children.Add(tb2);

                            //温度T8和4C帧合并显示，显示在4C帧的最后一行
                            StackPanel spf2 = page.FindName("masterShiShiInfo3_1") as StackPanel;
                            //TextBlock tbtitle2 = spf2.Children[0] as TextBlock;
                            //tbtitle2.Visibility = Visibility.Visible;
                            //q = 1;
                            //StackPanel sp_2 = spf2.Children[q] as StackPanel;
                            //StackPanel sp7 = sp_2.Children[7] as StackPanel;
                            //sp7.Children.Clear();
                            //sp7.Visibility = Visibility.Visible;
                            //TextBlock tb6 = new TextBlock();
                            //tb6.Width = 180;
                            ////tb6.Width = 170;
                            //tb6.VerticalAlignment = VerticalAlignment.Center;
                            //tb6.TextAlignment = TextAlignment.Left;
                            ////tb6.Visibility = Visibility.Visible;
                            //tb6.Text = "温度T8：" + ((int)data[7] - 40) + "℃";
                            //tb6.Text = (string)page.Resources["temt8"] + ((int)data[7] - 40) + "℃";
                            //sp7.Children.Add(tb6);

                            q = 2;
                            StackPanel sp_2 = spf2.Children[q] as StackPanel;
                            StackPanel sp7 = sp_2.Children[0] as StackPanel;
                            sp7.Children.Clear();
                            sp7.Visibility = Visibility.Visible;
                            TextBlock tb6 = new TextBlock();
                            tb6.Width = 180;
                            //tb6.Width = 170;
                            tb6.VerticalAlignment = VerticalAlignment.Center;
                            tb6.TextAlignment = TextAlignment.Left;
                            //tb6.Visibility = Visibility.Visible;
                            tb6.Text = "Shunt温度：" + ((int)data[7] - 40) + "℃";
                            tb6.Text = (string)page.Resources["shuntTemp"] + ((int)data[5] - 40) + "℃";
                            sp7.Children.Add(tb6);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：4B");
                            //Info = "接收到4B帧数据";
                            Info = (string)page.Resources["recframe4B"];
                        });
                    }
                    else if (data[0] == 0x4C)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                       {
                           StackPanel spf = page.FindName("masterShiShiInfo3_1") as StackPanel;
                           if (spf.Children.Count == 0)
                           {
                               return;
                           }
                           TextBlock tbtitle = spf.Children[0] as TextBlock;
                           tbtitle.Visibility = Visibility.Visible;
                           int q = 1;
                           StackPanel sp = spf.Children[q] as StackPanel;

                           StackPanel sp1 = sp.Children[0] as StackPanel;
                           sp1.Children.Clear();
                           sp1.Visibility = Visibility.Visible;
                           TextBlock tb0 = new TextBlock();
                           tb0.Width = 200;
                           tb0.VerticalAlignment = VerticalAlignment.Center;
                           tb0.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           tb0.Text = "温度T1：" + ((int)data[1] - 40) + "℃";
                           tb0.Text = (string)page.Resources["temt1"] + ((int)data[1] - 40) + "℃";
                           sp1.Children.Add(tb0);

                           StackPanel sp2 = sp.Children[1] as StackPanel;
                           sp2.Visibility = Visibility.Visible;
                           sp2.Children.Clear();
                           TextBlock tb1 = new TextBlock();
                           tb1.Width = 200;
                           tb1.VerticalAlignment = VerticalAlignment.Center;
                           tb1.TextAlignment = TextAlignment.Left;
                           // tb1.Visibility = Visibility.Visible;
                           tb1.Text = "温度T2：" + ((int)data[2] - 40) + "℃";
                           tb1.Text = (string)page.Resources["temt2"] + ((int)data[2] - 40) + "℃";
                           sp2.Children.Add(tb1);

                           StackPanel sp3 = sp.Children[2] as StackPanel;
                           sp3.Visibility = Visibility.Visible;
                           sp3.Children.Clear();
                           TextBlock tb2 = new TextBlock();
                           tb2.Width = 200;
                           tb2.VerticalAlignment = VerticalAlignment.Center;
                           tb2.TextAlignment = TextAlignment.Left;
                           //tb2.Visibility = Visibility.Visible;
                           tb2.Text = "温度T3：" + ((int)data[3] - 40) + "℃";
                           tb2.Text = (string)page.Resources["temt3"] + ((int)data[3] - 40) + "℃";
                           sp3.Children.Add(tb2);

                           StackPanel sp4 = sp.Children[3] as StackPanel;
                           sp4.Children.Clear();
                           sp4.Visibility = Visibility.Visible;
                           TextBlock tb3 = new TextBlock();
                           tb3.Width = 200;
                           tb3.VerticalAlignment = VerticalAlignment.Center;
                           tb3.TextAlignment = TextAlignment.Left;
                           //tb3.Visibility = Visibility.Visible;
                           tb3.Text = "温度T4：" + ((int)data[4] - 40) + "℃";
                           tb3.Text = (string)page.Resources["temt4"] + ((int)data[4] - 40) + "℃";
                           sp4.Children.Add(tb3);

                           StackPanel sp5 = sp.Children[4] as StackPanel;
                           sp5.Visibility = Visibility.Visible;
                           sp5.Children.Clear();
                           TextBlock tb4 = new TextBlock();
                           tb4.Width = 200;
                           tb4.VerticalAlignment = VerticalAlignment.Center;
                           tb4.TextAlignment = TextAlignment.Left;
                           //tb4.Visibility = Visibility.Visible;
                           tb4.Text = "温度T5：" + ((int)data[5] - 40) + "℃";
                           tb4.Text = (string)page.Resources["temt5"] + ((int)data[5] - 40) + "℃";
                           sp5.Children.Add(tb4);

                           StackPanel sp6 = sp.Children[5] as StackPanel;
                           sp6.Visibility = Visibility.Visible;
                           sp6.Children.Clear();
                           TextBlock tb5 = new TextBlock();
                           tb5.Width = 200;
                           tb5.VerticalAlignment = VerticalAlignment.Center;
                           tb5.TextAlignment = TextAlignment.Left;
                           // tb5.Visibility = Visibility.Visible;
                           tb5.Text = "温度T6：" + ((int)data[6] - 40) + "℃";
                           tb5.Text = (string)page.Resources["temt6"] + ((int)data[6] - 40) + "℃";
                           sp6.Children.Add(tb5);

                           StackPanel sp7 = sp.Children[6] as StackPanel;
                           sp7.Visibility = Visibility.Visible;
                           sp7.Children.Clear();
                           TextBlock tb6 = new TextBlock();
                           tb6.Width = 200;
                           tb6.VerticalAlignment = VerticalAlignment.Center;
                           tb6.TextAlignment = TextAlignment.Left;
                           //tb6.Visibility = Visibility.Visible;
                           tb6.Text = "温度T7：" + ((int)data[7] - 40) + "℃";
                           tb6.Text = (string)page.Resources["temt7"] + ((int)data[7] - 40) + "℃";
                           sp7.Children.Add(tb6);
                       });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：4C");
                            //Info = "接收到4C帧数据";
                            Info = (string)page.Resources["recframe4C"];
                        });
                    }
                    else if (data[0] == 0x4D)
                    {
                        //这一帧信息显示占两行
                        // if (data[1] > 24) { return; }//从机数设置在[1,24]
                        int temp = data[4] << 8 | data[3];
                        int temp2 = data[6] << 8 | data[5];

                        int temp3 = data[1] - 40;//进水口温度
                        int temp4 = data[2] - 40;//出水口温度
                        //进出水水口温度
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo11") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 1;//第0行是标题
                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp_0 = sp.Children[0] as StackPanel;
                            sp_0.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.TextAlignment = TextAlignment.Left;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.Visibility = Visibility.Visible;
                            tb0.Text = "进水水口温度：" + (data[1] - 40) + "℃";
                            tb0.Text = (string)page.Resources["inwatertem"] + (data[1] - 40) + "℃";
                            tb0.Width = 250;
                            sp_0.Visibility = Visibility.Visible;
                            sp_0.Children.Add(tb0);

                            StackPanel sp_1 = sp.Children[1] as StackPanel;
                            sp_1.Children.Clear();
                            sp_1.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 250;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.Visibility = Visibility.Visible;
                            tb1.Text = "出水水口温度：" + (data[2] - 40) + "℃";
                            tb1.Text = (string)page.Resources["outwatertem"] + (data[2] - 40) + "℃";
                            sp_1.Children.Add(tb1);
                        });

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo4") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 1;//第0行是标题

                            StackPanel sp = spf.Children[q] as StackPanel;
                            StackPanel sp_0 = sp.Children[0] as StackPanel;
                            sp_0.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.TextAlignment = TextAlignment.Left;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.Visibility = Visibility.Visible;

                            //  tb0.Text = "CC信号电压：" + temp * 0.001 + "V";
                            // tb0.Text = (string)page.Resources["ccvol"] + temp * 0.001 + "V";

                            tb0.Text = (string)page.Resources["pzzkb"] + data[3] + "%";
                            tb0.Width = 170;
                            sp_0.Visibility = Visibility.Visible;
                            sp_0.Children.Add(tb0);

                            StackPanel sp_1 = sp.Children[1] as StackPanel;
                            sp_1.Children.Clear();
                            sp_1.Visibility = Visibility.Visible;
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 170;
                            tb1.TextAlignment = TextAlignment.Left;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.Visibility = Visibility.Visible;
                            //tb1.Text = "CP信号占空比：" + temp2 + "%";
                            //tb1.Text = (string)page.Resources["cpzkb"] + temp2 + "%";

                            tb1.Text = (string)page.Resources["cpzkb"] + data[5] + "%";
                            sp_1.Children.Add(tb1);

                            sp_1 = sp.Children[2] as StackPanel;
                            sp_1.Children.Clear();
                            sp_1.Visibility = Visibility.Visible;
                            TextBlock tb = new TextBlock();
                            tb.Width = 90;
                            tb.TextAlignment = TextAlignment.Left;
                            tb.VerticalAlignment = VerticalAlignment.Center;
                            tb.Visibility = Visibility.Visible;
                            //tb.Text = "CC：";
                            tb.Text = (string)page.Resources["pzxhyxztw"]; 
                            // tb2_1.Text = (data[7] >> 4 & 0x1) == 0 ? "CC:无信号" : "CC:有信号";
                            sp_1.Children.Add(tb);

                            Button btn = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            //btn2.Margin = new Thickness(2, 0, 10, 0);
                            btn.ToolTip = (string)page.Resources["pzxhyxztw"]; 
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 1 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_1.Children.Add(btn);


                            q = 2;
                            StackPanel sp2 = spf.Children[q] as StackPanel;
                            StackPanel sp_2 = sp2.Children[0] as StackPanel;
                            sp_2.Visibility = Visibility.Visible;
                            sp_2.Children.Clear();
                            TextBlock tb2_0 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb2_0.Width = 50;
                            tb2_0.TextAlignment = TextAlignment.Left;
                            tb2_0.VerticalAlignment = VerticalAlignment.Center;
                            tb2_0.Text = "S2：";
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_2.Children.Add(tb2_0);

                            Button btn11 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn11.ToolTip = "S2";
                            btn11.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                            sp_2.Children.Add(btn11);

                            //StackPanel sp_3 = sp2.Children[1] as StackPanel;
                            //sp_3.Visibility = Visibility.Visible;
                            //sp_3.Children.Clear();
                            //TextBlock tb2_1 = new TextBlock();
                            //tb2_1.Width = 50;
                            //tb2_1.TextAlignment = TextAlignment.Left;
                            //tb2_1.VerticalAlignment = VerticalAlignment.Center;
                            //tb2_1.Visibility = Visibility.Visible;
                            //tb2_1.Text = "S2：";
                            //// tb2_1.Text = (data[7] >> 4 & 0x1) == 0 ? "CC:无信号" : "CC:有信号";
                            //sp_3.Children.Add(tb2_1);

                            //Button btn2 = new Button();
                            //// btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            ////btn2.Margin = new Thickness(2, 0, 10, 0);
                            //btn2.ToolTip = "S2";
                            //btn2.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7] >> 2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                            //sp_3.Children.Add(btn2);

                            StackPanel sp_3 = sp2.Children[1] as StackPanel;
                            sp_3.Visibility = Visibility.Visible;
                            sp_3.Children.Clear();
                            TextBlock tb2_1 = new TextBlock();
                            tb2_1.Width = 50;
                            tb2_1.TextAlignment = TextAlignment.Left;
                            tb2_1.VerticalAlignment = VerticalAlignment.Center;
                            tb2_1.Visibility = Visibility.Visible;
                            tb2_1.Text = "CC：";
                            // tb2_1.Text = (data[7] >> 4 & 0x1) == 0 ? "CC:无信号" : "CC:有信号";
                            sp_3.Children.Add(tb2_1);

                            Button btn2 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            //btn2.Margin = new Thickness(2, 0, 10, 0);
                            btn2.ToolTip = "CC";
                            btn2.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 3 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_3.Children.Add(btn2);

                            StackPanel sp_4 = sp2.Children[2] as StackPanel;
                            sp_4.Visibility = Visibility.Visible;
                            sp_4.Children.Clear();
                            TextBlock tb2_2 = new TextBlock();
                            tb2_2.Width = 50;
                            tb2_2.TextAlignment = TextAlignment.Left;
                            tb2_2.VerticalAlignment = VerticalAlignment.Center;
                            tb2_2.Visibility = Visibility.Visible;
                            tb2_2.Text = "ON：";
                            // tb2_2.Text = (data[7] >> 3 & 0x1) == 0 ? "ON:无信号" : "ON:有信号";
                            sp_4.Children.Add(tb2_2);

                            Button btn3 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn3.Margin = new Thickness(2, 0, 10, 0);
                            btn3.ToolTip = "ON";
                            btn3.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 4 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            //btn3.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 4 & 0x1) == 0 ?   "NormalButton":"ErrorButton"]);
                            sp_4.Children.Add(btn3);

                            StackPanel sp_5 = sp2.Children[3] as StackPanel;
                            sp_5.Children.Clear();
                            sp_5.Visibility = Visibility.Visible;
                            TextBlock tb2_3 = new TextBlock();
                            tb2_3.Width = 50;
                            tb2_3.TextAlignment = TextAlignment.Left;
                            tb2_3.VerticalAlignment = VerticalAlignment.Center;
                            tb2_3.Visibility = Visibility.Visible;
                            tb2_3.Text = "CC2：";
                            //tb2_3.Text = (data[7] >> 2 & 0x1) == 0 ? "CC2:无信号" : "CC2:有信号";
                            sp_5.Children.Add(tb2_3);

                            Button btn4 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn4.Margin = new Thickness(2, 0, 10, 0);
                            btn4.ToolTip = "CC2";
                            btn4.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 5 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_5.Children.Add(btn4);

                            StackPanel sp_6 = sp2.Children[4] as StackPanel;
                            sp_6.Children.Clear();
                            sp_6.Visibility = Visibility.Visible;
                            TextBlock tb2_4 = new TextBlock();
                            tb2_4.Width = 50;
                            tb2_4.TextAlignment = TextAlignment.Left;
                            tb2_4.VerticalAlignment = VerticalAlignment.Center;
                            tb2_4.Visibility = Visibility.Visible;
                            tb2_4.Text = "Schg：";
                            // tb2_4.Text = (data[7] >> 1 & 0x1) == 0 ? "Schg:无信号" : "Schg:有信号";
                            sp_6.Children.Add(tb2_4);

                            Button btn5 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            //btn5.Margin = new Thickness(2, 0, 10, 0);
                            btn5.ToolTip = "Schg";
                            btn5.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 6 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_6.Children.Add(btn5);

                            StackPanel sp_7 = sp2.Children[5] as StackPanel;
                            sp_7.Children.Clear();
                            sp_7.Visibility = Visibility.Visible;
                            TextBlock tb2_5 = new TextBlock();
                            tb2_5.Width = 50;
                            tb2_5.TextAlignment = TextAlignment.Left;
                            tb2_5.VerticalAlignment = VerticalAlignment.Center;
                            tb2_5.Visibility = Visibility.Visible;
                            tb2_5.Text = "Fchg：";
                            //tb2_5.Text = (data[7] & 0x1) == 0 ? "Fchg:无信号" : "Fchg:有信号";
                            sp_7.Children.Add(tb2_5);

                            Button btn6 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();ErrorButton
                            // btn6.Margin = new Thickness(2, 0, 10, 0);
                            btn6.ToolTip = "Fchg";
                            btn6.SetValue(Button.StyleProperty, Application.Current.Resources[(data[6] >> 7 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_7.Children.Add(btn6);


                            q = 3;
                            StackPanel sp3 = spf.Children[q] as StackPanel;
                            StackPanel sp_8 = sp3.Children[0] as StackPanel;
                            sp_8.Visibility = Visibility.Visible;
                            sp_8.Children.Clear();
                            TextBlock tb3_0 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb3_0.Width = 90;
                            tb3_0.TextAlignment = TextAlignment.Left;
                            tb3_0.VerticalAlignment = VerticalAlignment.Center;
                            tb3_0.Text = "高压互锁1:";
                            tb3_0.Text = (string)page.Resources["gaoyahusuo1"];
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_8.Children.Add(tb3_0);

                            Button btn7 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn7.ToolTip = "高压互锁1";
                            btn7.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7] & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_8.Children.Add(btn7);

                            StackPanel sp_9 = sp3.Children[1] as StackPanel;
                            sp_9.Visibility = Visibility.Visible;
                            sp_9.Children.Clear();
                            TextBlock tb3_1 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb3_1.Width = 90;
                            tb3_1.TextAlignment = TextAlignment.Left;
                            tb3_1.VerticalAlignment = VerticalAlignment.Center;
                            tb3_1.Text = "高压互锁2:";
                            tb3_1.Text = (string)page.Resources["gaoyahusuo2"];
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_9.Children.Add(tb3_1);

                            Button btn8 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn8.ToolTip = "高压互锁2";
                            btn8.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7] >> 1 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_9.Children.Add(btn8);

                            StackPanel sp_10 = sp3.Children[2] as StackPanel;
                            sp_10.Visibility = Visibility.Visible;
                            sp_10.Children.Clear();
                            TextBlock tb3_2 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb3_2.Width = 90;
                            tb3_2.TextAlignment = TextAlignment.Left;
                            tb3_2.VerticalAlignment = VerticalAlignment.Center;
                            tb3_2.Text = "充电枪2_CC2:";
                            tb3_2.Text = (string)page.Resources["chongdianqiang2cc2"];
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_10.Children.Add(tb3_2);

                            Button btn9 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn9.ToolTip = "充电枪2_CC2";
                            btn9.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7] >> 2 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_10.Children.Add(btn9);

                            sp_10 = sp3.Children[3] as StackPanel;
                            sp_10.Visibility = Visibility.Visible;
                            sp_10.Children.Clear();
                            tb3_2 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb3_2.Width = 90;
                            tb3_2.TextAlignment = TextAlignment.Left;
                            tb3_2.VerticalAlignment = VerticalAlignment.Center;
                            tb3_2.Text = "开关量信号1:";
                            tb3_2.Text = (string)page.Resources["kaiguanliangsig1"];
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_10.Children.Add(tb3_2);

                            btn9 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn9.ToolTip = "开关量信号1";
                            btn9.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7] >> 3 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_10.Children.Add(btn9);

                            sp_10 = sp3.Children[4] as StackPanel;
                            sp_10.Visibility = Visibility.Visible;
                            sp_10.Children.Clear();
                            tb3_2 = new TextBlock();
                            // tb2_0.Visibility = Visibility.Visible;
                            tb3_2.Width = 90;
                            tb3_2.TextAlignment = TextAlignment.Left;
                            tb3_2.VerticalAlignment = VerticalAlignment.Center;
                            tb3_2.Text = "开关量信号2:";
                            tb3_2.Text = (string)page.Resources["kaiguanliangsig2"];
                            //tb2_0.Text = (data[7] >> 5 & 0x1) == 0 ? "S2:闭合" : "S2:断开";
                            sp_10.Children.Add(tb3_2);

                            btn9 = new Button();
                            // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            // btn.Margin = new Thickness(2, 0, 10, 0);
                            btn9.ToolTip = "开关量信号2";
                            btn9.SetValue(Button.StyleProperty, Application.Current.Resources[(data[7]>>4 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                            sp_10.Children.Add(btn9);
                        });

                        Application.Current.Dispatcher.Invoke((Action)delegate
                         {
                             //Console.WriteLine("接收到主机数据：4D");
                             //Info = "接收到4D帧数据";
                             Info = (string)page.Resources["recframe4D"];
                         });
                    }
                    else if (data[0] == 0x4E)
                    {
                        //这一帧信息显示占七行
                        if (data[1] > 24 || data[1] <= 0) { return; }//从机数设置在[1,24]
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo4_2") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // int q = 2+data[1]-1;
                            int q = ((int)data[1] - 1) * 9 + 1;
                            StackPanel spp = spf.Children[q] as StackPanel;
                            StackPanel spp1 = spp.Children[0] as StackPanel;
                            spp1.Children.Clear();
                            spp1.Visibility = Visibility.Visible;
                            TextBlock tbb = new TextBlock();
                            tbb.Width = 170;
                            tbb.VerticalAlignment = VerticalAlignment.Center;
                            tbb.TextAlignment = TextAlignment.Left;
                            tbb.Visibility = Visibility.Visible;
                            tbb.Text = "从机编号：" + (int)data[1];
                            tbb.Text = (string)page.Resources["bmunum"] + (int)data[1];
                            spp1.Children.Add(tbb);

                            for (int n = 0; n < 6; n++)
                            {
                                //六行
                                int sTemp = q + n + 1;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Width = 50;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        int iTemp = 7 - 2 * i - j;
                                        int iTemp2 = 2 * i + j;
                                        tb.Text = "C" + (n * 8 + 7 - iTemp) + "：";
                                        sp1.Children.Add(tb);

                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[2 + n] >> iTemp2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                                        sp1.Children.Add(btn1);

                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }

                                }
                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：4E");
                            //Info = "接收到4E帧数据";
                            Info = (string)page.Resources["recframe4E"];
                        });
                    }
                    else if (data[0] == 0x4F)
                    {
                        //这一帧信息显示占两行，与上一帧显示位置（4E）帧交织
                        if (data[1] > 24 || data[1] <= 0) { return; }
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo4_2") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            //int q = 2+168 + data[1] - 1;
                            int q = ((int)data[1] - 1) * 9 + 7 + 1;
                            int q2 = q - 7;
                            // int q = 2 + ((int)data[1] - 1) * 9;
                            StackPanel spptitle = spf.Children[q2] as StackPanel;
                            StackPanel spptitle1 = spptitle.Children[0] as StackPanel;
                            spptitle1.Children.Clear();
                            spptitle1.Visibility = Visibility.Visible;
                            TextBlock tbbtitle = new TextBlock();
                            tbbtitle.Width = 170;
                            tbbtitle.VerticalAlignment = VerticalAlignment.Center;
                            tbbtitle.TextAlignment = TextAlignment.Left;
                            tbbtitle.Visibility = Visibility.Visible;
                            tbbtitle.Text = "从机编号：" + (int)data[1];
                            tbbtitle.Text = (string)page.Resources["bmunum"] + (int)data[1];
                            spptitle1.Children.Add(tbbtitle);

                            //StackPanel spp = spf.Children[q] as StackPanel;
                            //TextBox tbb = spp.Children[0] as TextBox;
                            //tbb.Visibility = Visibility.Visible;
                            //tbb.Text="从机编号："

                            StackPanel sp = spf.Children[q] as StackPanel;
                            for (int i = 0; i < 4; i++)
                            {
                                //四列
                                StackPanel spp = sp.Children[i] as StackPanel;
                                spp.Visibility = Visibility.Visible;
                                spp.Children.Clear();



                                for (int j = 0; j < 2; j++)
                                {
                                    //每列排两个数据
                                    int iTemp = 7 - 2 * i - j;
                                    int iTemp2 = 2 * i + j;
                                    TextBlock tb = new TextBlock();
                                    tb.Width = 50;
                                    tb.VerticalAlignment = VerticalAlignment.Center;
                                    tb.TextAlignment = TextAlignment.Left;
                                    // tb.Visibility = Visibility.Visible;
                                    // tb.Text = "";
                                    Button btn1 = new Button();
                                    btn1.Margin = new Thickness(2, 0, -3, 0);

                                    tb.Text = "C" + (48 + 2 * i + j) + ":";
                                    btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[2] >> iTemp2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                                    //tb.Text = (data[2] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (48 + 2 * i + j) + "：正常 " : tb.Text + "C" + (48 + 2 * i + j) + "：开路";
                                    spp.Children.Add(tb);
                                    spp.Children.Add(btn1);
                                }


                            }
                            int qt = q + 1;

                            StackPanel sp2 = spf.Children[qt] as StackPanel;
                            for (int i = 0; i < 2; i++)
                            {
                                //两列
                                StackPanel spp2 = sp2.Children[i] as StackPanel;
                                spp2.Visibility = Visibility.Visible;
                                spp2.Children.Clear();

                                //tb.Text = "";
                                for (int j = 0; j < 2; j++)
                                {
                                    //每列排两个数据
                                    int iTemp = 7 - 2 * i - j;
                                    TextBlock tb = new TextBlock();
                                    //tb.Visibility = Visibility.Visible;
                                    tb.Width = 50;
                                    tb.VerticalAlignment = VerticalAlignment.Center;
                                    tb.TextAlignment = TextAlignment.Left;
                                    Button btn2 = new Button();
                                    btn2.Margin = new Thickness(2, 0, -3, 0);
                                    tb.Text = "C" + (56 + 2 * i + j) + ":";
                                    btn2.SetValue(Button.StyleProperty, Application.Current.Resources[(data[3] >> iTemp & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                                    //tb.Text = (data[2] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (56 + 2 * i + j) + "：正常 " : tb.Text + "C" + (48 + 2 * i + j) + "：开路 ";
                                    spp2.Children.Add(tb);
                                    spp2.Children.Add(btn2);
                                }


                            }
                            // qt = q + 2;
                            //StackPanel sp3 = spf.Children[qt] as StackPanel;
                            StackPanel spp3 = sp2.Children[2] as StackPanel;
                            spp3.Children.Clear();
                            spp3.Visibility = Visibility.Visible;
                            TextBlock tbb = new TextBlock();
                            // tbb.Visibility = Visibility.Visible;
                            tbb.VerticalAlignment = VerticalAlignment.Center;
                            tbb.TextAlignment = TextAlignment.Left;
                            // tbb.Text = "";
                            tbb.Width = 50;
                            tbb.Text = "C60:";
                            Button btn3 = new Button();
                            btn3.Margin = new Thickness(2, 0, -3, 0);
                            btn3.SetValue(Button.StyleProperty, Application.Current.Resources[(data[3] >> 3 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                            spp3.Children.Add(tbb);
                            spp3.Children.Add(btn3);
                        });

                        Application.Current.Dispatcher.Invoke((Action)delegate
                         {
                             //Console.WriteLine("接收到主机数据：4F");
                             //Info = "接收到4F帧数据";
                             Info = (string)page.Resources["recframe4F"];
                         });
                    }
                    else if (data[0] == 0x50)
                    {
                        //这一帧信息显示占七行
                        if (data[1] > 24 || data[1] <= 0) { return; }
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo5") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // int q = 2+data[1]-1;
                            //第0行是标题行
                            int q = ((int)data[1] - 1) * 9 + 1;
                            StackPanel spp = spf.Children[q] as StackPanel;

                            StackPanel spp1 = spp.Children[0] as StackPanel;
                            spp1.Children.Clear();
                            spp1.Visibility = Visibility.Visible;
                            TextBlock tbb = new TextBlock();
                            tbb.Width = 220;
                            tbb.VerticalAlignment = VerticalAlignment.Center;
                            tbb.TextAlignment = TextAlignment.Left;
                            tbb.Visibility = Visibility.Visible;
                            tbb.Text = "从机编号：" + (int)data[1];
                            tbb.Text = (string)page.Resources["bmunum"] + (int)data[1];
                            spp1.Children.Add(tbb);

                            for (int n = 0; n < 6; n++)
                            {
                                //六行
                                int sTemp = q + n + 1;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        //tb.Width = 50;
                                        tb.Width = 80;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        int iTemp = 7 - 2 * i - j;
                                        int iTemp2 = 2 * i + j;
                                        //tb.Text = "均衡" + (n * 8 + 7 - iTemp + 1) + "：";
                                        tb.Text = (string)page.Resources["balance"] + (n * 8 + 7 - iTemp + 1) + "：";
                                        sp1.Children.Add(tb);

                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[2 + n] >> iTemp2 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                        sp1.Children.Add(btn1);

                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }

                                }
                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：50");
                            //Info = "接收到50帧数据";
                            Info = (string)page.Resources["recframe50"];
                        });
                    }
                    else if (data[0] == 0x51)
                    {
                        if (data[1] > 24 || data[1] <= 0) { return; }
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo5") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;

                            //int q = 2+168 + data[1] - 1;
                            // int q = 2 + ((int)data[1] - 1) * 9 + 7;
                            int q = 1 + ((int)data[1] - 1) * 9 + 7;
                            int q2 = q - 7;
                            // int q = 2 + ((int)data[1] - 1) * 9;
                            StackPanel spptitle = spf.Children[q2] as StackPanel;
                            StackPanel spptitle1 = spptitle.Children[0] as StackPanel;
                            spptitle1.Visibility = Visibility.Visible;
                            if (spptitle1.Children.Count == 0)
                            {
                                TextBlock tbbtitle = new TextBlock();
                                tbbtitle.Width = 220;
                                tbbtitle.VerticalAlignment = VerticalAlignment.Center;
                                tbbtitle.TextAlignment = TextAlignment.Left;
                                tbbtitle.Visibility = Visibility.Visible;
                                tbbtitle.Text = "从机编号：" + (int)data[1];
                                tbbtitle.Text = (string)page.Resources["bmunum"] + (int)data[1];
                                spptitle1.Children.Add(tbbtitle);
                            }

                            //StackPanel spp = spf.Children[q] as StackPanel;
                            //TextBox tbb = spp.Children[0] as TextBox;
                            //tbb.Visibility = Visibility.Visible;
                            //tbb.Text="从机编号："

                            StackPanel sp = spf.Children[q] as StackPanel;
                            for (int i = 0; i < 4; i++)
                            {
                                //四列
                                StackPanel spp = sp.Children[i] as StackPanel;
                                spp.Visibility = Visibility.Visible;
                                spp.Children.Clear();



                                for (int j = 0; j < 2; j++)
                                {
                                    //每列排两个数据
                                    int iTemp = 7 - 2 * i - j;
                                    int iTemp2 = 2 * i + j;
                                    TextBlock tb = new TextBlock();
                                    //tb.Width = 50;
                                    tb.Width = 80;
                                    tb.VerticalAlignment = VerticalAlignment.Center;
                                    tb.TextAlignment = TextAlignment.Left;
                                    // tb.Visibility = Visibility.Visible;
                                    // tb.Text = "";
                                    Button btn1 = new Button();
                                    btn1.Margin = new Thickness(2, 0, -3, 0);

                                    //tb.Text = "均衡" + (48 + 2 * i + j + 1) + "：";
                                    tb.Text = (string)page.Resources["balance"] + (48 + 2 * i + j + 1) + "：";
                                    btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[2] >> iTemp2 & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                    //tb.Text = (data[2] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (48 + 2 * i + j) + "：正常 " : tb.Text + "C" + (48 + 2 * i + j) + "：开路";
                                    spp.Children.Add(tb);
                                    spp.Children.Add(btn1);
                                }


                            }
                            int qt = q + 1;

                            StackPanel sp2 = spf.Children[qt] as StackPanel;
                            for (int i = 0; i < 2; i++)
                            {
                                //两列
                                StackPanel spp2 = sp2.Children[i] as StackPanel;
                                spp2.Visibility = Visibility.Visible;
                                spp2.Children.Clear();

                                //tb.Text = "";
                                for (int j = 0; j < 2; j++)
                                {
                                    //每列排两个数据
                                    //int iTemp = 7 - 2 * i - j;
                                    int iTemp = 2 * i + j;
                                    TextBlock tb = new TextBlock();
                                    //tb.Visibility = Visibility.Visible;
                                    tb.Width = 80;
                                    //tb.Width = 50;
                                    tb.VerticalAlignment = VerticalAlignment.Center;
                                    tb.TextAlignment = TextAlignment.Left;
                                    Button btn2 = new Button();
                                    btn2.Margin = new Thickness(2, 0, -3, 0);
                                    tb.Text = "均衡" + (56 + 2 * i + j + 1) + ":";
                                    tb.Text = (string)page.Resources["balance"] + (56 + 2 * i + j + 1) + ":";
                                    btn2.SetValue(Button.StyleProperty, Application.Current.Resources[(data[3] >> iTemp & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                    //tb.Text = (data[2] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (56 + 2 * i + j) + "：正常 " : tb.Text + "C" + (48 + 2 * i + j) + "：开路 ";
                                    spp2.Children.Add(tb);
                                    spp2.Children.Add(btn2);
                                }

                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：51");
                            //Info = "接收到51帧数据";
                            Info = (string)page.Resources["recframe51"];
                        });
                    }
                    else if (data[0] == 0x52)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo6") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // 第0行是标题
                            int q = 1;
                            StackPanel spp = spf.Children[q] as StackPanel;

                            for (int n = 0; n < 3; n++)
                            {
                                //三行
                                int sTemp = q + n;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Width = 50;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        int iTemp = 7 - 2 * i - j;
                                        int iTemp2 = 2 * i + j;
                                        tb.Text = "从机" + (n * 8 + 7 - iTemp + 1) + ":";
                                        tb.Text = (string)page.Resources["bmu"] + (n * 8 + 7 - iTemp + 1) + ":";
                                        sp1.Children.Add(tb);


                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        if (chuanshu[n * 8 + 7 - iTemp] > 0)
                                        {
                                            btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[1 + n] >> iTemp2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                                        }
                                        else
                                        {
                                            btn1.SetValue(Button.StyleProperty, Application.Current.Resources["NullButton"]);
                                        }
                                        sp1.Children.Add(btn1);


                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }

                                }
                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：52");
                            //Info = "接收到52帧数据";
                            Info = (string)page.Resources["recframe52"];
                        });
                    }
                    else if (data[0] == 0x53)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo7") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // 第0行是标题
                            int q = 1;
                            StackPanel spp = spf.Children[q] as StackPanel;

                            for (int n = 0; n < 2; n++)
                            {
                                //2行
                                int sTemp = q + n;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        //tb.Width = 100;
                                        tb.Width = 120;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        // int iTemp = 7 - 2 * i - j;
                                        int iTemp = 2 * i + j;
                                        if (n == 0)
                                        {
                                            switch (iTemp)
                                            {
                                                case 0:
                                                    tb.Text = "放电正继电器:";
                                                    tb.Text = (string)page.Resources["dischargez"];
                                                    break;
                                                case 1:
                                                    tb.Text = "放电负继电器:";
                                                    tb.Text = (string)page.Resources["dischargef"];
                                                    break;
                                                case 2:
                                                    tb.Text = "充电正继电器:";
                                                    tb.Text = (string)page.Resources["chargez"];
                                                    break;
                                                case 3:
                                                    tb.Text = "充电负继电器:";
                                                    tb.Text = (string)page.Resources["chargef"];
                                                    break;
                                                case 4:
                                                    tb.Text = "加热正继电器:";
                                                    tb.Text = (string)page.Resources["heatz"];
                                                    break;
                                                case 5:
                                                    tb.Text = "加热负继电器:";
                                                    tb.Text = (string)page.Resources["heatf"];
                                                    break;
                                                case 6:
                                                    tb.Text = "预充继电器:";
                                                    tb.Text = (string)page.Resources["precharge"];
                                                    break;
                                                case 7:
                                                    tb.Text = "风扇继电器:";
                                                    tb.Text = (string)page.Resources["fan"];
                                                    break;

                                            }
                                        }
                                        else if (n == 1)
                                        {
                                            switch (iTemp)
                                            {
                                                case 0:
                                                    tb.Text = "辅助正继电器:";
                                                    tb.Text = (string)page.Resources["supportz"];
                                                    break;
                                                case 1:
                                                    tb.Text = "辅助负继电器:";
                                                    tb.Text = (string)page.Resources["supportf"];
                                                    break;
                                                case 2:
                                                    tb.Text = "电子锁1:";
                                                    tb.Text = (string)page.Resources["key1"];
                                                    break;
                                                case 3:
                                                    tb.Text = "电子锁2:";
                                                    tb.Text = (string)page.Resources["key2"];
                                                    break;
                                                case 4:
                                                    tb.Text = "负控1:";
                                                    tb.Text = (string)page.Resources["fukong1"];
                                                    break;
                                                case 5:
                                                    tb.Text = "负控2:";
                                                    tb.Text = (string)page.Resources["fukong2"];
                                                    break;
                                                case 6:
                                                    tb.Text = "负控3:";
                                                    tb.Text = (string)page.Resources["fukong3"];
                                                    break;
                                                case 7:
                                                    tb.Text = "负控4:";
                                                    tb.Text = (string)page.Resources["fukong4"];
                                                    break;
                                            }
                                        }

                                        //tb.Text = "继电器" + (n * 8 + 7 - iTemp + 1) + ":";
                                        sp1.Children.Add(tb);

                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.HorizontalContentAlignment = HorizontalAlignment.Right;
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        //btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[1 + n] >> iTemp & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                        int ttttt = data[iTemp / 4 + 1 + 2 * n] >> (2 * (iTemp % 4)) & 0x03;
                                        switch (data[iTemp / 4 + 1 + 2 * n] >> (2 * (iTemp % 4)) & 0x03)
                                        {
                                            case 0:
                                                //断开
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["ErrorButton"]);
                                                break;
                                            case 1:
                                                //闭合
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["NormalButton"]);
                                                break;
                                            case 2:
                                                //粘连
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["ZhanlianButton"]);
                                                break;
                                            case 3:
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["ShiXiaoButton"]);
                                                break;
                                        }
                                        //btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[iTemp/4+1+2*n] >> (2*(iTemp%4)) & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                        sp1.Children.Add(btn1);

                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }

                                }
                            }

                            //继电器逻辑状态
                            spf = page.FindName("masterShiShiInfo7_2") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // 第0行是标题
                            q = 1;
                            //StackPanel spp = spf.Children[q] as StackPanel;

                            for (int n = 0; n < 2; n++)
                            {
                                //2行
                                int sTemp = q + n;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Width = 120;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        // int iTemp = 7 - 2 * i - j;
                                        int iTemp = 2 * i + j;
                                        if (n == 0)
                                        {
                                            switch (iTemp)
                                            {
                                                case 0:
                                                    tb.Text = "放电正继电器:";
                                                    tb.Text = (string)page.Resources["dischargez"];
                                                    break;
                                                case 1:
                                                    tb.Text = "放电负继电器:";
                                                    tb.Text = (string)page.Resources["dischargef"];
                                                    break;
                                                case 2:
                                                    tb.Text = "充电正继电器:";
                                                    tb.Text = (string)page.Resources["chargez"];
                                                    break;
                                                case 3:
                                                    tb.Text = "充电负继电器:";
                                                    tb.Text = (string)page.Resources["chargef"];
                                                    break;
                                                case 4:
                                                    tb.Text = "加热正继电器:";
                                                    tb.Text = (string)page.Resources["heatz"];
                                                    break;
                                                case 5:
                                                    tb.Text = "加热负继电器:";
                                                    tb.Text = (string)page.Resources["heatf"];
                                                    break;
                                                case 6:
                                                    tb.Text = "预充继电器:";
                                                    tb.Text = (string)page.Resources["precharge"];
                                                    break;
                                                case 7:
                                                    tb.Text = "风扇继电器:";
                                                    tb.Text = (string)page.Resources["fan"];
                                                    break;

                                            }
                                        }
                                        else if (n == 1)
                                        {
                                            switch (iTemp)
                                            {
                                                case 0:
                                                    tb.Text = "辅助正继电器:";
                                                    tb.Text = (string)page.Resources["supportz"];
                                                    break;
                                                case 1:
                                                    tb.Text = "辅助负继电器:";
                                                    tb.Text = (string)page.Resources["supportf"];
                                                    break;
                                                case 2:
                                                    tb.Text = "电子锁1:";
                                                    tb.Text = (string)page.Resources["key1"];
                                                    break;
                                                case 3:
                                                    tb.Text = "电子锁2:";
                                                    tb.Text = (string)page.Resources["key2"];
                                                    break;
                                                case 4:
                                                    tb.Text = "负控1:";
                                                    tb.Text = (string)page.Resources["fukong1"];
                                                    break;
                                                case 5:
                                                    tb.Text = "负控2:";
                                                    tb.Text = (string)page.Resources["fukong2"];
                                                    break;
                                                case 6:
                                                    tb.Text = "负控3:";
                                                    tb.Text = (string)page.Resources["fukong3"];
                                                    break;
                                                case 7:
                                                    tb.Text = "负控4:";
                                                    tb.Text = (string)page.Resources["fukong4"];
                                                    break;
                                            }
                                        }

                                        //tb.Text = "继电器" + (n * 8 + 7 - iTemp + 1) + ":";
                                        sp1.Children.Add(tb);

                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.HorizontalContentAlignment = HorizontalAlignment.Right;
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        //btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[1 + n] >> iTemp & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                        //switch (data[iTemp / 4 + 1 + 2 * n] >> (2 * (iTemp % 4)))
                                        switch (data[5 + n] >> iTemp & 0x01)
                                        {
                                            case 0:
                                                //断开
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["ErrorButton"]);
                                                break;
                                            case 1:
                                                //闭合
                                                btn1.SetValue(Button.StyleProperty, Application.Current.Resources["NormalButton"]);
                                                break;
                                        }
                                        //btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[iTemp/4+1+2*n] >> (2*(iTemp%4)) & 0x1) == 0 ? "ErrorButton" : "NormalButton"]);
                                        sp1.Children.Add(btn1);

                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }
                                }
                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：53");
                            //Info = "接收到53帧数据";
                            Info = (string)page.Resources["recframe53"];
                        });
                    }
                    else if (data[0] == 0x54)
                    {
                        int temp = data[7] << 8 | data[6];
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo8") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 1;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            string strtemp = "";
                            switch (data[1])
                            {
                                case 0:
                                    strtemp = "自检";
                                    strtemp = (string)page.Resources["selfexamine"];
                                    break;
                                case 1:
                                    strtemp = "待命";
                                    strtemp = (string)page.Resources["daiming"];
                                    break;
                                case 2:
                                    strtemp = "放电";
                                    strtemp = (string)page.Resources["fangdian"];
                                    break;
                                case 3:
                                    strtemp = "充电";
                                    strtemp = (string)page.Resources["chongdian"];
                                    break;
                                case 4:
                                    strtemp = "下电";
                                    strtemp = (string)page.Resources["xiadian"];
                                    break;
                                case 5:
                                    strtemp = "预充";
                                    strtemp = (string)page.Resources["yuchong"];
                                    break;
                                case 6:
                                    strtemp = "故障";
                                    strtemp = (string)page.Resources["guzhang"];
                                    break;
                                case 7:
                                    strtemp = "休眠";
                                    strtemp = (string)page.Resources["xiumian"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)data[1] + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)data[1] + ")";
                                    break;
                            }

                            //tb0.Text = "系统运行模式:" + strtemp;
                            tb0.Text = (string)page.Resources["sysrunmode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;

                            switch (data[2] & 0x0F)
                            {
                                case 0:
                                    strtemp = "等待";
                                    strtemp = (string)page.Resources["dengdai"];
                                    break;
                                case 1:
                                    strtemp = "加热中";
                                    strtemp = (string)page.Resources["jiarezhong"];
                                    break;
                                case 2:
                                    strtemp = "加热结束";
                                    strtemp = (string)page.Resources["jiarejieshu"];
                                    break;
                                case 3:
                                    strtemp = "禁止加热";
                                    strtemp = (string)page.Resources["jinzhijiare"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)(data[2] & 0x0F) + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)(data[2] & 0x0F) + ")";
                                    break;
                            }
                            //tb0.Text = "行车加热状态:" + strtemp;
                            tb0.Text = (string)page.Resources["xingchejiaremode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[2] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;

                            switch (data[2] >> 4)
                            {
                                case 0:
                                    strtemp = "等待";
                                    strtemp = (string)page.Resources["dengdai"];
                                    break;
                                case 1:
                                    strtemp = "禁止加热";
                                    strtemp = (string)page.Resources["jinzhijiare"];
                                    break;
                                case 2:
                                    strtemp = "充电20S阶段";
                                    strtemp = (string)page.Resources["chongdian20s"];
                                    break;
                                case 3:
                                    strtemp = "只加热阶段";
                                    strtemp = (string)page.Resources["zhijiere"];
                                    break;
                                case 4:
                                    strtemp = "边充电边加热";
                                    strtemp = (string)page.Resources["bianchongdianbianjiare"];
                                    break;
                                case 5:
                                    strtemp = "加热结束";
                                    strtemp = (string)page.Resources["jiarejieshu"];
                                    break;
                                case 6:
                                    strtemp = "从只加热跳转到快充";
                                    strtemp = (string)page.Resources["congzhijiere"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)(data[2] >> 4) + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)(data[2] >> 4) + ")";
                                    break;
                            }
                            //tb0.Text = "快充电加热状态:" + strtemp;
                            tb0.Text = (string)page.Resources["kuaichongjiaremode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[3] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;

                            switch (data[3] & 0x0F)
                            {
                                case 0:
                                    strtemp = "等待";
                                    strtemp = (string)page.Resources["dengdai"];
                                    break;
                                case 1:
                                    strtemp = "禁止加热";
                                    strtemp = (string)page.Resources["jinzhijiare"];
                                    break;
                                case 2:
                                    strtemp = "充电20S阶段";
                                    strtemp = (string)page.Resources["chongdian20s"];
                                    break;
                                case 3:
                                    strtemp = "只加热阶段";
                                    strtemp = (string)page.Resources["zhijiere"];
                                    break;
                                case 4:
                                    strtemp = "边充电边加热";
                                    strtemp = (string)page.Resources["bianchongdianbianjiare"];
                                    break;
                                case 5:
                                    strtemp = "加热结束";
                                    strtemp = (string)page.Resources["jiarejieshu"];
                                    break;
                                case 6:
                                    strtemp = "从只加热跳转到快充";
                                    strtemp = (string)page.Resources["congzhijiere"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)(data[3] & 0x0F) + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)(data[2] >> 4) + ")";
                                    break;
                            }
                            tb0.Text = "慢充充电状态:" + strtemp;
                            tb0.Text = (string)page.Resources["manchongchongdianmode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[4] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;

                            switch (data[3] >> 4)
                            {
                                case 0:
                                    strtemp = "等待";
                                    strtemp = (string)page.Resources["dengdai"];
                                    break;
                                case 1:
                                    strtemp = "禁止加热";
                                    strtemp = (string)page.Resources["jinzhijiare"];
                                    break;
                                case 2:
                                    strtemp = "充电20S阶段";
                                    strtemp = (string)page.Resources["chongdian20s"];
                                    break;
                                case 3:
                                    strtemp = "只加热阶段";
                                    strtemp = (string)page.Resources["zhijiere"];
                                    break;
                                case 4:
                                    strtemp = "边充电边加热";
                                    strtemp = (string)page.Resources["bianchongdianbianjiare"];
                                    break;
                                case 5:
                                    strtemp = "加热结束";
                                    strtemp = (string)page.Resources["jiarejieshu"];
                                    break;
                                case 6:
                                    strtemp = "从只加热跳转到快充";
                                    strtemp = (string)page.Resources["congzhijiere"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)(data[3] >> 4) + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)(data[2] >> 4) + ")";
                                    break;
                            }
                            tb0.Text = "慢充加热状态:" + strtemp;
                            tb0.Text = (string)page.Resources["manchongjiaremode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[5] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;

                            switch (data[4])
                            {
                                case 0:
                                    strtemp = "未连接";
                                    strtemp = (string)page.Resources["unconnected"];
                                    break;
                                case 1:
                                    strtemp = "物理连接阶段";
                                    strtemp = (string)page.Resources["wuliconnectstage"];
                                    break;
                                case 2:
                                    strtemp = "握手辨识阶段";
                                    strtemp = (string)page.Resources["woshoubianshistage"];
                                    break;
                                case 3:
                                    strtemp = "参数配置1阶段";
                                    strtemp = (string)page.Resources["canshupeizhi1stage"];
                                    break;
                                case 4:
                                    strtemp = "参数配置2阶段";
                                    strtemp = (string)page.Resources["canshupeizhi2stage"];
                                    break;
                                case 5:
                                    strtemp = "充电过程阶段";
                                    strtemp = (string)page.Resources["chongdianstage"];
                                    break;
                                case 6:
                                    strtemp = "小电流充电阶段，需求4.5A";
                                    strtemp = (string)page.Resources["xiaodianliu45stage"];
                                    break;
                                case 7:
                                    strtemp = "充电停止阶段";
                                    strtemp = (string)page.Resources["chongdianstopstage"];
                                    break;
                                case 8:
                                    strtemp = "充电统计阶段";
                                    strtemp = (string)page.Resources["chongdiantongjistage"];
                                    break;
                                case 9:
                                    strtemp = "充电结束阶段";
                                    strtemp = (string)page.Resources["chongdianjieshustage"];
                                    break;
                                case 10:
                                    strtemp = "充电超时阶段";
                                    strtemp = (string)page.Resources["chongdianchaoshistage"];
                                    break;
                                default:
                                    strtemp = "数据有误" + "(" + (int)data[4] + ")";
                                    strtemp = (string)page.Resources["wrongdata"] + "(" + (int)data[4] + ")";
                                    break;
                            }
                            tb0.Text = "慢充加热状态:" + strtemp;
                            tb0.Text = (string)page.Resources["manchongjiaremode"] + strtemp;
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[6] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;

                            spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            //q = 9;
                            q = 11;
                            sp = spf.Children[q] as StackPanel;

                            sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "最高温度和最低温度差：" + (int)data[5] + "℃";
                            tb0.Text = (string)page.Resources["zuigaozuidiwenducha"] + (int)data[5] + "℃";
                            if (isOpenMasterRecord)
                            {
                                dataRecordPerLine[3] = (int)data[5] + "℃";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }
                            sp1.Children.Add(tb0);

                            sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "单体压差：" + temp + "mv";
                            tb0.Text = (string)page.Resources["dantiyacha"] + temp + "mv";
                            if (isOpenMasterRecord)
                            {
                                dataRecordPerLine[4] = temp + "mv";
                                lock (debuglocker) { dataRecordPerLineNum += 1; }
                            }
                            sp1.Children.Add(tb0);


                            //StackPanel spf = page.FindName("masterShiShiInfo8") as StackPanel;
                            //if (spf.Children.Count == 0)
                            //{
                            //    return;
                            //}
                            //TextBlock tbtitle = spf.Children[0] as TextBlock;
                            //tbtitle.Visibility = Visibility.Visible;
                            //// 第0行是标题
                            //int q = 1;
                            //StackPanel spp = spf.Children[q] as StackPanel;
                            //spp.Visibility = Visibility.Visible;
                            //spp.Children.Clear();
                            //TextBlock tb = new TextBlock();
                            //tb.VerticalAlignment = VerticalAlignment.Center;
                            //tb.TextAlignment = TextAlignment.Left;
                            //switch (data[1])
                            //{
                            //    case 0:
                            //        tb.Text = "自检";
                            //        tb.Text = (string)page.Resources["selfexamine"];
                            //        break;
                            //    case 1:
                            //        tb.Text = "上电";
                            //        tb.Text = (string)page.Resources["shangdian"];
                            //        break;
                            //    case 3:
                            //        tb.Text = "防电";
                            //        tb.Text = (string)page.Resources["fangdian"];
                            //        break;
                            //    case 4:
                            //        tb.Text = "下电";
                            //        tb.Text = (string)page.Resources["xiadian"];
                            //        break;
                            //    default:
                            //        tb.Text = "数据有误"+"(" + (int)data[1] + ")";
                            //        tb.Text = (string)page.Resources["wrongdata"] + "(" + (int)data[1] + ")";
                            //        break;
                            //}
                            //spp.Children.Add(tb);


                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：54");
                            //Info = "接收到54帧数据";
                            Info = (string)page.Resources["recframe54"];
                        });
                    }
                    else if (data[0] == 0x55)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo9") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // 第0行是标题
                            int q = 1;
                            StackPanel spp = spf.Children[q] as StackPanel;
                            spp.Visibility = Visibility.Visible;
                            //spp.Children.Clear();
                            TextBlock tb = new TextBlock();
                            tb.VerticalAlignment = VerticalAlignment.Center;
                            tb.TextAlignment = TextAlignment.Left;

                            //tb.Text = (int)data[1] + "年" + (int)data[2] + "月" + (int)data[3] + "日" + (int)data[4] + "时" + (int)data[5] + "分" + (int)data[6] + "秒";
                            //tb.Text = (int)data[1] + (string)page.Resources["year"] + (int)data[2] + (string)page.Resources["month"] + (int)data[3] +
                            //    (string)page.Resources["day"] + (int)data[4] + (string)page.Resources["hour"] + (int)data[5] +
                            //    (string)page.Resources["minute"] + (int)data[6] + (string)page.Resources["second"];
                            tb.Text = "20"+data[1].ToString("X") + "年" + data[2].ToString("X") + "月" + data[3].ToString("X") + "日" + data[4].ToString("X") + "时" +
                                data[5].ToString("X") + "分" + data[6].ToString("X") + "秒";
                            //tb.Text =(int.Parse(DataConverter.hex2String(data[1])) + 2000) + (string)page.Resources["year"] + DataConverter.hex2String(data[2]) + (string)page.Resources["month"] + DataConverter.hex2String(data[3]) +
                            //    (string)page.Resources["day"] + DataConverter.hex2String(data[4]) + (string)page.Resources["hour"] + DataConverter.hex2String(data[5]) +
                            //    (string)page.Resources["minute"] + DataConverter.hex2String(data[6]) + (string)page.Resources["second"];
                            StackPanel sppp = spp.Children[0] as StackPanel;
                            sppp.Visibility = Visibility.Visible;
                            sppp.Children.Clear();
                            sppp.Children.Add(tb);
                            //spp.Children.Add(tb);
                            tb = new TextBlock();
                            tb.VerticalAlignment = VerticalAlignment.Center;
                            tb.TextAlignment = TextAlignment.Left;
                            tb.Text = data[7] == 1 ? (string)page.Resources["clockError"] : (string)page.Resources["clockNormal"];
                            sppp = spp.Children[1] as StackPanel;
                            sppp.Visibility = Visibility.Visible;
                            sppp.Children.Clear();
                            sppp.Children.Add(tb);


                            Clock = "主机时钟：20" + data[1].ToString("X") + "年" + data[2].ToString("X") + "月" + data[3].ToString("X") + "日" + data[4].ToString("X") + "时" +
                                data[5].ToString("X") + "分" + data[6].ToString("X") + "秒";
                            //if (data[7] == 1) {
                            //    WrapPanel wp = page.FindName("masterShiShiInfo10") as WrapPanel;
                            //    Border border = new Border();
                            //    border.BorderBrush = new SolidColorBrush(Color.FromRgb(91, 192, 222));
                            //    border.BorderThickness = new Thickness(2);
                            //    border.Margin = new Thickness(5);
                            //    border.Padding = new Thickness(5);
                            //    TextBlock tb2 = new TextBlock();
                            //    tb.Width = 120;

                            //    tb.Text = "时钟异常";
                            //    tb.TextWrapping = TextWrapping.Wrap;
                            //    border.Child = tb;
                            //    wp.Children.Add(border);
                            //}
                            // Clock = "主机时钟："+DataConverter.byteToHexStrForData(data);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：55");
                            //Info = "接收到55帧数据";
                            Info = (string)page.Resources["recframe55"];
                        });
                    }
                    else if (data[0] == 0x56)
                    {
                        lock (slaveLocker)
                        {
                            if (data[1] > 4 || data[1] < 1)
                            {
                                return;
                            }
                            byte[] b = new byte[6];
                            Array.Copy(data, 2, b, 0, 6);
                            if (slaveSoftwareVersion1.ContainsKey(data[1])) {
                                slaveSoftwareVersion1.Remove(data[1]);
                            }
                            slaveSoftwareVersion1.Add(data[1], DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion2.ContainsKey(data[1]))
                            {
                                SlaveVersionList[data[1]] = "从机" + data[1] + "软件版本号：" + slaveSoftwareVersion1[data[1]] + slaveSoftwareVersion2[data[1]];
                                SlaveSoftwareVersion2.Remove(data[1]);
                                SlaveSoftwareVersion1.Remove(data[1]);
                                VersionVisibility[2 + data[1]] = "Visible";
                            }
                        }
                    }
                    else if (data[0] == 0x57)
                    {
                        lock (slaveLocker)
                        {
                            if (data[1] > 4 || data[1] < 1)
                            {
                                return;
                            }
                            byte[] b = new byte[6];
                            Array.Copy(data, 2, b, 0, 6);
                            if (slaveSoftwareVersion2.ContainsKey(data[1]))
                            {
                                slaveSoftwareVersion2.Remove(data[1]);
                            }
                            slaveSoftwareVersion2.Add(data[1], DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion1.ContainsKey(data[1]))
                            {
                                SlaveVersionList[data[1]] = "从机" + data[1] + "软件版本号：" + slaveSoftwareVersion1[data[1]] + slaveSoftwareVersion2[data[1]];
                                SlaveSoftwareVersion2.Remove(data[1]);
                                SlaveSoftwareVersion1.Remove(data[1]);
                                VersionVisibility[2 + data[1]] = "Visible";
                            }
                        }
                    }
                    else if (data[0] == 0x58)
                    {
                        //INF25
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo4_3") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 1;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "CC2_1电压：" + temp * 0.001 + "V";
                            tb0.Text = (string)page.Resources["CC2_1Vol"] + temp * 0.001 + "V";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            tb1.Text = "CC2_2电压：" + temp2 * 0.001 + "V";
                            tb1.Text = (string)page.Resources["CC2_2Vol"] + temp2 * 0.001 + "V";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "CC电压：" + temp3 * 0.001 + "V";
                            tb2.Text = (string)page.Resources["CCVol"] + temp3 * 0.001 + "V";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：58");
                            //Info = "接收到58帧数据";
                            Info = (string)page.Resources["recframe58"];
                        });
                    }
                    else if (data[0] == 0x59)
                    {
                        //INF26
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 1;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "电池组总串数：" + temp;
                            tb0.Text = (string)page.Resources["packchuanshu"] + temp;
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            tb1.Text = "标称容量：" + temp2 * 0.1 + "AH";
                            tb1.Text = (string)page.Resources["biaocheng"] + temp2 * 0.1 + "AH";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "BMS供电电源电压：" + temp3 * 0.001 + "mV";
                            tb2.Text = (string)page.Resources["BMSVol"] + temp3*0.001 + "V";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：59");
                            //Info = "接收到59帧数据";
                            Info = (string)page.Resources["recframe59"];
                        });
                    }
                    else if (data[0] == 0x5A)
                    {
                        //INF27
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[7] << 16 | data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 2;//在第三行与0x59在一个控件下面
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            double b = temp * 0.1 - 600;
                            String str = b.ToString();
                            int pos = b.ToString().IndexOf('.');
                            if (pos != -1)
                            {
                                //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                                //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                                int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                                str = str.Substring(0, py);
                            }
                            tb0.Text = "Shunt_1电流：" + str + "A";
                            tb0.Text = (string)page.Resources["shuntcur1"] + str + "A";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;

                            b = temp2 * 0.1 - 600;
                            str = b.ToString();
                            pos = b.ToString().IndexOf('.');
                            if (pos != -1)
                            {
                                //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                                //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                                int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                                str = str.Substring(0, py);
                            }
                            tb1.Text = "Shunt_2电流：" + str + "A";
                            tb1.Text = (string)page.Resources["shuntcur2"] + str + "A";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "累计放电总能量：" + temp3 * 0.1 + "KWh";
                            tb2.Text = (string)page.Resources["dischargeSum"] + temp3 * 0.1 + "KWh";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：5A");
                            //Info = "接收到5A帧数据";
                            Info = (string)page.Resources["recframe5A"];
                        });
                    }
                    else if (data[0] == 0x61)
                    {
                        //INF28
                        int temp = data[3] << 16 | data[2] << 8 | data[1];
                        int temp2 = data[5] << 8 | data[4];
                        int temp3 = data[7] << 8 | data[6];
//;                        int temp = data[2] << 8 | data[1];
//                        int temp2 = data[4] << 8 | data[3];
//                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 4;//在第四行与0x5A在一个控件下面
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "累计充电总能量：" + temp * 0.1 + "KWh";
                            tb0.Text = (string)page.Resources["chargeSum"] + temp * 0.1 + "KWh";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            tb1.Text = "最大瞬时可用回馈电流：" + temp2 * 0.1 + "A";
                            tb1.Text = (string)page.Resources["maxsshkcur"] + temp2 * 0.1 + "A";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "最大瞬时可用放电电流：" + temp3 * 0.1 + "A";
                            tb2.Text = (string)page.Resources["maxssky"] + temp3 * 0.1 + "A";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：61");
                            //Info = "接收到61帧数据";
                            Info = (string)page.Resources["recframe61"];
                        });
                    }
                    else if (data[0] == 0x62)
                    {
                        //INF29
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 5;//在第5行与0x61在一个控件下面
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "最大瞬时可用放电功率：" + temp * 0.1 + "KW";
                            tb0.Text = (string)page.Resources["maxssgl"] + temp * 0.1 + "KW";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            tb1.Text = "最大瞬时可用回馈功率：" + temp2 * 0.1 + "KW";
                            tb1.Text = (string)page.Resources["maxsshk"] + temp2 * 0.1 + "KW";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            tb2.Text = "持续充电需求电流：" + temp3 * 0.1 + "A";
                            tb2.Text = (string)page.Resources["cxcd"] + temp3 * 0.1 + "A";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：62");
                            //Info = "接收到62帧数据";
                            Info = (string)page.Resources["recframe62"];
                        });
                    }
                    else if (data[0] == 0x63)
                    {
                        //INF30
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 6;//在第6行与0x61在一个控件下面
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "最大持续充电电流：" + temp * 0.1 + "A";
                            tb0.Text = (string)page.Resources["maxcxcd"] + temp * 0.1 + "A";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            //tb1.Text = "持续可用放电电流：" + temp2 * 0.1 + "A";
                            tb1.Text = (string)page.Resources["maxcxkyhkdl"] + temp2 * 0.1 + "A";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            //tb2.Text = "最大持续可用放电电流：" + temp3 * 0.1 + "KW";
                            tb2.Text = (string)page.Resources["maxcxkyfdcur"] + temp3 * 0.1 + "KW";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：63");
                            //Info = "接收到63帧数据";
                            Info = (string)page.Resources["recframe63"];
                        });
                    }
                    else if (data[0] == 0x64)
                    {
                        //INF31
                        int temp = data[2] << 8 | data[1];
                        int gygz = data[3] & 0x0F;
                        int dygz = data[3] >> 4;
                        int gwgz = data[4] & 0x0F;
                        int dwgz = data[4] >> 4;
                        int hkdl = data[5] & 0x0F;
                        int fddl = data[5] >> 4;
                        int cxcd = data[6] & 0x0F;
                        int dycyx = data[6] >> 4;
                        int dczzygy = data[7] & 0x0F;
                        int dczzydy = data[7] >> 4;
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 6;//在第7行与0x63在一个控件下面
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[3] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb0.Text = "最大持续可用放电功率：" + temp * 0.1 + "KW";
                            tb0.Text = (string)page.Resources["maxcxkyfdgl"] + temp * 0.1 + "KW";
                            sp1.Children.Add(tb0);

                            //故障等级
                            StackPanel spf2 = page.FindName("masterShiShiInfo13") as StackPanel;
                            if (spf2.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle2 = spf2.Children[0] as TextBlock;
                            tbtitle2.Visibility = Visibility.Visible;

                            q = 1;//在第7行与0x63在一个控件下面
                            sp = spf2.Children[q] as StackPanel;

                            sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb1.Text = "高压故障：" + gygz;
                            tb1.Text = (string)page.Resources["gygzlevel"] + gygz;
                            sp1.Children.Add(tb1);

                            sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb2.Text = "低压故障：" + dygz;
                            tb2.Text = (string)page.Resources["dygzlevel"] + dygz;
                            sp1.Children.Add(tb2);

                            sp1 = sp.Children[2] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb3 = new TextBlock();
                            tb3.Width = 350;
                            tb3.VerticalAlignment = VerticalAlignment.Center;
                            tb3.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb3.Text = "高温故障：" + gwgz;
                            tb3.Text = (string)page.Resources["gwgzlevel"] + gwgz;
                            sp1.Children.Add(tb3);

                            sp1 = sp.Children[3] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb4 = new TextBlock();
                            tb4.Width = 350;
                            tb4.VerticalAlignment = VerticalAlignment.Center;
                            tb4.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb4.Text = "低温故障：" + dwgz;
                            tb4.Text = (string)page.Resources["dwgzlevel"] + dwgz;
                            sp1.Children.Add(tb4);

                            sp1 = sp.Children[4] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb5 = new TextBlock();
                            tb5.Width = 350;
                            tb5.VerticalAlignment = VerticalAlignment.Center;
                            tb5.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb5.Text = "回馈电流故障：" + hkdl;
                            tb5.Text = (string)page.Resources["hkdllevel"] + hkdl;
                            sp1.Children.Add(tb5);

                            sp1 = sp.Children[5] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb6 = new TextBlock();
                            tb6.Width = 350;
                            tb6.VerticalAlignment = VerticalAlignment.Center;
                            tb6.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb6.Text = "放电电流故障：" + fddl;
                            tb6.Text = (string)page.Resources["fddllevel"] + fddl;
                            sp1.Children.Add(tb6);

                            q = 2;//在第7行与0x63在一个控件下面
                            sp = spf2.Children[q] as StackPanel;

                            sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb7 = new TextBlock();
                            tb7.Width = 350;
                            tb7.VerticalAlignment = VerticalAlignment.Center;
                            tb7.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb7.Text = "持续充电电流故障：" + cxcd;
                            tb7.Text = (string)page.Resources["cxcdlevel"] + cxcd;
                            sp1.Children.Add(tb7);

                            sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb8 = new TextBlock();
                            tb8.Width = 350;
                            tb8.VerticalAlignment = VerticalAlignment.Center;
                            tb8.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb8.Text = "电压采样线故障：" + dycyx;
                            tb8.Text = (string)page.Resources["dycyxlevel"] + dycyx;
                            sp1.Children.Add(tb8);

                            sp1 = sp.Children[2] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb9 = new TextBlock();
                            tb9.Width = 350;
                            tb9.VerticalAlignment = VerticalAlignment.Center;
                            tb9.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb9.Text = "电池组总电压高压故障：" + dczzygy;
                            tb9.Text = (string)page.Resources["dczzygylevel"] + dczzygy;
                            sp1.Children.Add(tb9);

                            sp1 = sp.Children[3] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb10 = new TextBlock();
                            tb10.Width = 350;
                            tb10.VerticalAlignment = VerticalAlignment.Center;
                            tb10.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            tb10.Text = "电池组总电压低压故障：" + dczzydy;
                            tb10.Text = (string)page.Resources["dczzydylevel"] + dczzydy;
                            sp1.Children.Add(tb10);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：63");
                            //Info = "接收到63帧数据";
                            Info = (string)page.Resources["recframe63"];
                        });
                    }
                    else if (data[0] == 0x65)
                    {
                        int dyyzx = data[1] & 0x0F;
                        int wdyzx = data[1] >> 4;
                        int jygz = data[2] & 0x0F;
                        int cdczwd = data[2] >> 4;
                        int temp = data[4] << 8 | data[3];
                        int temp2 = data[6] << 8 | data[5];
                        //info32
                        Application.Current.Dispatcher.Invoke((Action)delegate
                       {
                           //故障等级
                           StackPanel spf = page.FindName("masterShiShiInfo13") as StackPanel;
                           if (spf.Children.Count == 0)
                           {
                               return;
                           }
                           TextBlock tbtitle2 = spf.Children[0] as TextBlock;
                           tbtitle2.Visibility = Visibility.Visible;
                           int q = 2;//在第7行与0x63在一个控件下面
                           StackPanel sp = spf.Children[q] as StackPanel;

                           StackPanel sp1 = sp.Children[4] as StackPanel;
                           sp1.Visibility = Visibility.Visible;
                           sp1.Children.Clear();
                           TextBlock tb7 = new TextBlock();
                           tb7.Width = 350;
                           tb7.VerticalAlignment = VerticalAlignment.Center;
                           tb7.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           tb7.Text = "电压一致性故障：" + dyyzx;
                           tb7.Text = (string)page.Resources["dyyzxlevel"] + dyyzx;
                           sp1.Children.Add(tb7);

                           sp1 = sp.Children[5] as StackPanel;
                           sp1.Visibility = Visibility.Visible;
                           sp1.Children.Clear();
                           tb7 = new TextBlock();
                           tb7.Width = 350;
                           tb7.VerticalAlignment = VerticalAlignment.Center;
                           tb7.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           tb7.Text = "温度一致性故障：" + wdyzx;
                           tb7.Text = (string)page.Resources["wdyzxlevel"] + wdyzx;
                           sp1.Children.Add(tb7);

                           q = 3;//在第7行与0x63在一个控件下面
                           sp = spf.Children[q] as StackPanel;

                           sp1 = sp.Children[0] as StackPanel;
                           sp1.Visibility = Visibility.Visible;
                           sp1.Children.Clear();
                           tb7 = new TextBlock();
                           tb7.Width = 350;
                           tb7.VerticalAlignment = VerticalAlignment.Center;
                           tb7.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           tb7.Text = "绝缘故障：" + jygz;
                           tb7.Text = (string)page.Resources["jygzlevel"] + jygz;
                           sp1.Children.Add(tb7);

                           sp1 = sp.Children[1] as StackPanel;
                           sp1.Visibility = Visibility.Visible;
                           sp1.Children.Clear();
                           tb7 = new TextBlock();
                           tb7.Width = 350;
                           tb7.VerticalAlignment = VerticalAlignment.Center;
                           tb7.TextAlignment = TextAlignment.Left;
                           // tb0.Visibility = Visibility.Visible;
                           tb7.Text = "充电插座温度故障：" + cdczwd;
                           tb7.Text = (string)page.Resources["cdczwdlevel"] + cdczwd;
                           sp1.Children.Add(tb7);

                           //暂时不用，保留
                           ////Shunt_1电流偏移
                           //spf = page.FindName("masterShiShiInfo12") as StackPanel;
                           //if (spf.Children.Count == 0)
                           //{
                           //    return;
                           //}
                           //TextBlock tbtitle = spf.Children[0] as TextBlock;
                           //tbtitle.Visibility = Visibility.Visible;
                           //q = 7;//在第7行
                           //sp = spf.Children[q] as StackPanel;

                           //sp1 = sp.Children[0] as StackPanel;
                           //sp1.Visibility = Visibility.Visible;
                           //sp1.Children.Clear();
                           //TextBlock tb0 = new TextBlock();
                           //tb0.Width = 350;
                           //tb0.VerticalAlignment = VerticalAlignment.Center;
                           //tb0.TextAlignment = TextAlignment.Left;
                           //// tb0.Visibility = Visibility.Visible;
                           //tb0.Text = "Shunt_1电流偏移值：" + (temp - 1000) * 0.1 + "A";
                           //tb0.Text = (string)page.Resources["shunt1dlpy"] + (temp - 1000) * 0.1 + "A";
                           //sp1.Children.Add(tb0);

                           //sp1 = sp.Children[1] as StackPanel;
                           //sp1.Visibility = Visibility.Visible;
                           //sp1.Children.Clear();
                           //tb0 = new TextBlock();
                           //tb0.Width = 350;
                           //tb0.VerticalAlignment = VerticalAlignment.Center;
                           //tb0.TextAlignment = TextAlignment.Left;
                           //// tb0.Visibility = Visibility.Visible;
                           //tb0.Text = "Shunt_2电流偏移值：" + (temp2 - 1000) * 0.1 + "A";
                           //tb0.Text = (string)page.Resources["shunt2dlpy"] + (temp2 - 1000) * 0.1 + "A";
                           //sp1.Children.Add(tb0);

                       });

                    }
                    else if (data[0] == 0x66)
                    {
                        //info33
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        masterHardwareVersion1 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到主机硬件版本号1：" + masterHardwareVersion1);
                        if (!masterHardwareVersion2.Equals("") && !masterHardwareVersion3.Equals(""))
                        {
                            MasterHardwareVersion_rec = "主机硬件版本号：" + masterHardwareVersion1 + masterHardwareVersion2 + masterHardwareVersion3;
                            masterHardwareVersion1 = "";
                            masterHardwareVersion2 = "";
                            masterHardwareVersion3 = "";
                            VersionVisibility[1] = "Visible";
                        }
                    }
                    else if (data[0] == 0x67)
                    {
                        //info34
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        masterHardwareVersion2 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到主机硬件版本号2：" + masterHardwareVersion2);
                        if (!masterHardwareVersion1.Equals("") && !masterHardwareVersion3.Equals(""))
                        {
                            MasterHardwareVersion_rec = "主机硬件版本号：" + masterHardwareVersion1 + masterHardwareVersion2 + masterHardwareVersion3;
                            masterHardwareVersion1 = "";
                            masterHardwareVersion2 = "";
                            masterHardwareVersion3 = "";
                            VersionVisibility[1] = "Visible";
                        }
                    }
                    else if (data[0] == 0x68)
                    {
                        //info35
                        byte[] b = new byte[4];
                        Array.Copy(data, 1, b, 0, 4);
                        masterHardwareVersion3 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到主机硬件版本号3：" + masterHardwareVersion3);
                        if (!masterHardwareVersion1.Equals("") && !masterHardwareVersion2.Equals(""))
                        {
                            MasterHardwareVersion_rec = "主机硬件版本号：" + masterHardwareVersion1 + masterHardwareVersion2 + masterHardwareVersion3;
                            masterHardwareVersion1 = "";
                            masterHardwareVersion2 = "";
                            masterHardwareVersion3 = "";
                            VersionVisibility[1] = "Visible";
                        }
                    }
                    else if (data[0] == 0x69)
                    {
                        //info36
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        slaveHardwareVersion1 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到从机硬件版本号1：" + slaveHardwareVersion1);
                        if (!slaveHardwareVersion2.Equals("") && !slaveHardwareVersion3.Equals(""))
                        {
                            SlaveHardwareVersion_rec = "从机硬件版本号：" + slaveHardwareVersion1 + slaveHardwareVersion2 + slaveHardwareVersion3;
                            slaveHardwareVersion1 = "";
                            slaveHardwareVersion2 = "";
                            slaveHardwareVersion3 = "";
                            VersionVisibility[2] = "Visible";
                        }
                    }
                    else if (data[0] == 0x6A)
                    {
                        //info37
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        slaveHardwareVersion2 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到从机硬件版本号2：" + slaveHardwareVersion2);
                        if (!slaveHardwareVersion1.Equals("") && !slaveHardwareVersion3.Equals(""))
                        {
                            SlaveHardwareVersion_rec = "从机硬件版本号：" + slaveHardwareVersion1 + slaveHardwareVersion2 + slaveHardwareVersion3;
                            slaveHardwareVersion1 = "";
                            slaveHardwareVersion2 = "";
                            slaveHardwareVersion3 = "";
                            VersionVisibility[2] = "Visible";
                        }
                    }
                    else if (data[0] == 0x6B)
                    {
                        //info38
                        byte[] b = new byte[4];
                        Array.Copy(data, 1, b, 0, 4);
                        slaveHardwareVersion3 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到主机硬件版本号3：" + slaveHardwareVersion3);
                        if (!slaveHardwareVersion1.Equals("") && !slaveHardwareVersion2.Equals(""))
                        {
                            SlaveHardwareVersion_rec = "从机硬件版本号：" + slaveHardwareVersion1 + slaveHardwareVersion2 + slaveHardwareVersion3;
                            slaveHardwareVersion1 = "";
                            slaveHardwareVersion2 = "";
                            slaveHardwareVersion3 = "";
                            VersionVisibility[2] = "Visible";
                        }
                    }
                    //霍尔电流2-4显示，咱不用，保留
                    else if (data[0] == 0x6C)
                    {
                        //INF39
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        //Application.Current.Dispatcher.Invoke((Action)delegate
                        //{
                        //    StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                        //    if (spf.Children.Count == 0)
                        //    {
                        //        return;
                        //    }
                        //    TextBlock tbtitle = spf.Children[0] as TextBlock;
                        //    tbtitle.Visibility = Visibility.Visible;
                        //    int q = 8;
                        //    StackPanel sp = spf.Children[q] as StackPanel;

                        //    StackPanel sp1 = sp.Children[1] as StackPanel;
                        //    sp1.Visibility = Visibility.Visible;
                        //    sp1.Children.Clear();
                        //    TextBlock tb0 = new TextBlock();
                        //    tb0.Width = 350;
                        //    tb0.VerticalAlignment = VerticalAlignment.Center;
                        //    tb0.TextAlignment = TextAlignment.Left;
                        //    // tb0.Visibility = Visibility.Visible;
                        //    double b = temp * 0.1 - 600;
                        //    String str = b.ToString();
                        //    int pos = b.ToString().IndexOf('.');
                        //    if (pos != -1)
                        //    {
                        //        //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                        //        //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                        //        int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                        //        str = str.Substring(0, py);
                        //    }
                        //    tb0.Text = "霍尔2电流：" + str + "A";
                        //    tb0.Text = (string)page.Resources["hall2cur"] + str + "A"; ;
                        //    sp1.Children.Add(tb0);

                        //    StackPanel sp2 = sp.Children[2] as StackPanel;
                        //    sp2.Visibility = Visibility.Visible;
                        //    sp2.Children.Clear();
                        //    TextBlock tb1 = new TextBlock();
                        //    tb1.Width = 350;
                        //    tb1.VerticalAlignment = VerticalAlignment.Center;
                        //    tb1.TextAlignment = TextAlignment.Left;
                        //    // tb1.Visibility = Visibility.Visible;

                        //    b = temp2 * 0.1 - 600;
                        //    str = b.ToString();
                        //    pos = b.ToString().IndexOf('.');
                        //    if (pos != -1)
                        //    {
                        //        //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                        //        //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                        //        int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                        //        str = str.Substring(0, py);
                        //    }
                        //    tb1.Text = "霍尔3电流：" + str + "A";
                        //    tb1.Text = (string)page.Resources["hall3cur"] + str + "A";
                        //    sp2.Children.Add(tb1);

                        //    StackPanel sp3 = sp.Children[3] as StackPanel;
                        //    sp3.Visibility = Visibility.Visible;
                        //    sp3.Children.Clear();
                        //    TextBlock tb2 = new TextBlock();
                        //    tb2.Width = 350;
                        //    tb2.VerticalAlignment = VerticalAlignment.Center;
                        //    tb2.TextAlignment = TextAlignment.Left;
                        //    // tb2.Visibility = Visibility.Visible;

                        //    b = temp3 * 0.1 - 600;
                        //    str = b.ToString();
                        //    pos = b.ToString().IndexOf('.');
                        //    if (pos != -1)
                        //    {
                        //        //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                        //        //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                        //        int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                        //        str = str.Substring(0, py);
                        //    }
                        //    tb2.Text = "霍尔4电流：" + str + "A";
                        //    tb2.Text = (string)page.Resources["hall4cur"] + str + "A";
                        //    sp3.Children.Add(tb2);

                        //});
                        //Application.Current.Dispatcher.Invoke((Action)delegate
                        //{
                        //    Console.WriteLine("接收到主机数据：6C");
                        //    Info = "接收到6C帧数据";
                        //    Info = (string)page.Resources["recframe6C"];
                        //});
                    }
                    else if (data[0] == 0x6D)
                    {
                        //INF40
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 10;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "霍尔1电流偏移：" + (temp - 600) * 0.1 + "A";
                            tb0.Text = (string)page.Resources["hall1"] + (temp * 0.1-600) + "A";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;
                            //tb1.Text = "霍尔2电流偏移：" + (temp2 - 1000) * 0.1 + "A";
                            tb1.Text = (string)page.Resources["hall2"] + (temp2  * 0.1-600) + "A";
                            sp2.Children.Add(tb1);

                            StackPanel sp3 = sp.Children[2] as StackPanel;
                            sp3.Visibility = Visibility.Visible;
                            sp3.Children.Clear();
                            TextBlock tb2 = new TextBlock();
                            tb2.Width = 350;
                            tb2.VerticalAlignment = VerticalAlignment.Center;
                            tb2.TextAlignment = TextAlignment.Left;
                            // tb2.Visibility = Visibility.Visible;
                            //tb2.Text = "霍尔3电流偏移：" + (temp3 - 1000) * 0.1 + "A";
                            tb2.Text = (string)page.Resources["hall3"] + (temp3 * 0.1-600) + "A";
                            sp3.Children.Add(tb2);

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：6D");
                            //Info = "接收到6D帧数据";
                            Info = (string)page.Resources["recframe6D"];
                        });
                    }
                    else if (data[0] == 0x6E)
                    {
                        //INF41
                        int temp = data[2] << 8 | data[1];
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 10;
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[3] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //tb0.Text = "霍尔4电流偏移：" + (temp - 1000) * 0.1 + "A";
                            tb0.Text = (string)page.Resources["hall4"] + (temp * 0.1-600) + "A";
                            sp1.Children.Add(tb0);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：6E");
                            //Info = "接收到6E帧数据";
                            Info = (string)page.Resources["recframe6E"];
                        });
                    }
                    else if (data[0] == 0x6F)
                    {
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        VIN1 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到VIN1：" + VIN1);
                        if (!VIN2.Equals("") && !VIN3.Equals(""))
                        {
                            VIN_rec = "VIN：" + VIN1 + VIN2 + VIN3;
                            VIN1 = "";
                            VIN2 = "";
                            VIN3 = "";
                            VersionVisibility[12] = "Visible";
                        }
                    }
                    else if (data[0] == 0x70)
                    {
                        byte[] b = new byte[7];
                        Array.Copy(data, 1, b, 0, 7);
                        VIN2 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到VIN2：" + VIN2);
                        if (!VIN1.Equals("") && !VIN3.Equals(""))
                        {
                            VIN_rec = "VIN：" + VIN1 + VIN2 + VIN3;
                            VIN1 = "";
                            VIN2 = "";
                            VIN3 = "";
                            VersionVisibility[12] = "Visible";
                        }
                    }
                    else if (data[0] == 0x71)
                    {
                        byte[] b = new byte[3];
                        Array.Copy(data, 1, b, 0, 3);
                        VIN3 = DataConverter.bytetoAscString(b).Trim();
                        Console.WriteLine("收到VIN3：" + VIN3);
                        if (!VIN1.Equals("") && !VIN2.Equals(""))
                        {
                            VIN_rec = "VIN：" + VIN1 + VIN2 + VIN3;
                            VIN1 = "";
                            VIN2 = "";
                            VIN3 = "";
                            VersionVisibility[12] = "Visible";
                        }
                    }
                    else if (data[0] == 0x72)
                    {
                        lock (slaveLocker)
                        {
                            byte[] b = new byte[7];
                            Array.Copy(data, 1, b, 0, 7);
                            if (slaveSoftwareVersion1.ContainsKey(0)) { slaveSoftwareVersion1.Remove(0); }
                            slaveSoftwareVersion1.Add(0, DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion2.ContainsKey(0))
                            {
                                SlaveVersionList[0] = "主机boot版本号：" + slaveSoftwareVersion1[0] + slaveSoftwareVersion2[0];
                                SlaveSoftwareVersion2.Remove(0);
                                SlaveSoftwareVersion1.Remove(0);
                                VersionVisibility[13] = "Visible";
                            }
                        }
                    }
                    else if (data[0] == 0x73)
                    {
                        lock (slaveLocker)
                        {
                            byte[] b = new byte[3];
                            Array.Copy(data, 1, b, 0, 3);
                            if (slaveSoftwareVersion2.ContainsKey(0)) { slaveSoftwareVersion2.Remove(0); }
                            slaveSoftwareVersion2.Add(0, DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion1.ContainsKey(0))
                            {
                                SlaveVersionList[0] = "主机boot版本号：" + slaveSoftwareVersion1[0] + slaveSoftwareVersion2[0];
                                SlaveSoftwareVersion2.Remove(0);
                                SlaveSoftwareVersion1.Remove(0);
                                VersionVisibility[13] = "Visible";
                            }
                        }
                    }
                    else if (data[0] == 0x74)
                    {
                        lock (slaveLocker)
                        {
                            byte[] b = new byte[6];
                            Array.Copy(data, 2, b, 0, 6);
                            if (slaveSoftwareVersion1.ContainsKey(data[1] + 4)) { slaveSoftwareVersion1.Remove(data[1] + 4); }
                            int i = data[1] + 4;
                            slaveSoftwareVersion1.Add(i, DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion2.ContainsKey(data[1] + 4))
                            {
                                SlaveVersionList[data[1] + 4] = "从机" + data[1] + "boot软件版本号：" + slaveSoftwareVersion1[data[1] + 4] + slaveSoftwareVersion2[data[1] + 4];
                                SlaveSoftwareVersion2.Remove(data[1] + 4);
                                SlaveSoftwareVersion1.Remove(data[1] + 4);
                                VersionVisibility[data[1]+6] = "Visible";
                            }
                        }
                        
                    }
                    else if (data[0] == 0x75)
                    {
                        lock (slaveLocker)
                        {
                            byte[] b = new byte[4];
                            Array.Copy(data, 2, b, 0, 4);
                            if (slaveSoftwareVersion2.ContainsKey(data[1] + 4)) { slaveSoftwareVersion2.Remove(data[1] + 4); }
                            int i = data[1] + 4;
                            slaveSoftwareVersion2.Add(i, DataConverter.bytetoAscString(b).Trim());
                            if (slaveSoftwareVersion1.ContainsKey(data[1] + 4))
                            {
                                SlaveVersionList[data[1] + 4] = "从机" + data[1] + "boot软件版本号：" + slaveSoftwareVersion1[data[1] + 4] + slaveSoftwareVersion2[data[1] + 4];
                                SlaveSoftwareVersion2.Remove(data[1] + 4);
                                SlaveSoftwareVersion1.Remove(data[1] + 4);
                                VersionVisibility[data[1] + 6] = "Visible";
                            }
                        }   
                       
                    }
                    else if (data[0] == 0x76)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 3;//放在0x5A
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //double b = temp;
                            //String str = b.ToString();
                            ////int pos = b.ToString().IndexOf('.');
                            //if (pos != -1)
                            //{
                            //    //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                            //    //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                            //    int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                            //    str = str.Substring(0, py);
                            //}
                            tb0.Text = (string)page.Resources["shunt1dycy"] + temp*0.001 + "mv";
                            //tb0.Text = "Shunt_1电流：" + str + "A";
                            //tb0.Text = (string)page.Resources["shuntcur1"] + str + "A";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[1] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;
                            // tb1.Visibility = Visibility.Visible;

                            //b = temp2 * 0.1 - 600;
                            //str = b.ToString();
                            //pos = b.ToString().IndexOf('.');
                            //if (pos != -1)
                            //{
                            //    //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                            //    //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                            //    int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                            //    str = str.Substring(0, py);
                            //}
                            //tb1.Text = "Shunt_2电流：" + str + "A";
                            tb1.Text = (string)page.Resources["shunt2dycy"] + temp2*0.001 + "mv";
                            sp2.Children.Add(tb1);

                            q = 9;
                            sp = spf.Children[q] as StackPanel;

                            sp1 = sp.Children[0] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            // tb0.Visibility = Visibility.Visible;
                            //b = temp3 * 0.1 - 600;
                            //str = b.ToString();
                            //pos = b.ToString().IndexOf('.');
                            //if (pos != -1)
                            //{
                            //    //str = str.Substring(0, b.ToString().IndexOf('.') + 2);
                            //    //str = str.Substring(0, pos + (b < 0 ? 3 : 2));
                            //    int py = (pos + (b < 0 ? 3 : 2)) > str.Length ? str.Length : (pos + (b < 0 ? 3 : 2));
                            //    str = str.Substring(0, py);
                            //}
                            //tb0.Text = "霍尔1电流：" + temp3 + "mv";
                            tb0.Text = (string)page.Resources["hall1dycy"] + temp3 + "mv";
                            sp1.Children.Add(tb0);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：5A");
                            //Info = "接收到76帧数据";
                            Info = (string)page.Resources["recframe76"];
                        });

                    }
                    else if (data[0] == 0x77)
                    {
                        int temp = data[2] << 8 | data[1];
                        int temp2 = data[4] << 8 | data[3];
                        int temp3 = data[6] << 8 | data[5];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo12") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            int q = 9;//放在0x5A
                            StackPanel sp = spf.Children[q] as StackPanel;

                            StackPanel sp1 = sp.Children[1] as StackPanel;
                            sp1.Visibility = Visibility.Visible;
                            sp1.Children.Clear();
                            TextBlock tb0 = new TextBlock();
                            tb0.Width = 350;
                            tb0.VerticalAlignment = VerticalAlignment.Center;
                            tb0.TextAlignment = TextAlignment.Left;
                            tb0.Text = (string)page.Resources["hall2dycy"] + temp + "mv";
                            sp1.Children.Add(tb0);

                            StackPanel sp2 = sp.Children[2] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            TextBlock tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;

                            tb1.Text = (string)page.Resources["hall3dycy"] + temp2 + "mv";
                            sp2.Children.Add(tb1);

                            sp2 = sp.Children[3] as StackPanel;
                            sp2.Visibility = Visibility.Visible;
                            sp2.Children.Clear();
                            tb1 = new TextBlock();
                            tb1.Width = 350;
                            tb1.VerticalAlignment = VerticalAlignment.Center;
                            tb1.TextAlignment = TextAlignment.Left;

                            tb1.Text = (string)page.Resources["hall4dycy"] + temp3 + "mv";
                            sp2.Children.Add(tb1);
                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：5A");
                            //Info = "接收到76帧数据";
                            Info = (string)page.Resources["recframe77"];
                        });
                    }
                    else if (data[0] == 0x78)
                    {
                        //info51
                        int temp = data[2] << 8 | data[1];
                        Application.Current.Dispatcher.Invoke((Action)delegate
                      {
                          StackPanel spf = page.FindName("masterShiShiInfo1") as StackPanel;
                          if (spf.Children.Count == 0)
                          {
                              return;
                          }
                          TextBlock tbtitle = spf.Children[0] as TextBlock;
                          tbtitle.Visibility = Visibility.Visible;
                          //int q = 1;
                          int q=6;
                          StackPanel sp = spf.Children[q] as StackPanel;

                          StackPanel sp1 = sp.Children[0] as StackPanel;
                          sp1.Children.Clear();
                          sp1.Visibility = Visibility.Visible;
                          TextBlock tb0 = new TextBlock();
                          //tb0.Width = 150;
                          tb0.Width = 350;
                          tb0.VerticalAlignment = VerticalAlignment.Center;
                          tb0.TextAlignment = TextAlignment.Left;
                          //tb0.Visibility = Visibility.Visible;
                          //tb0.Text = "电池单体平均电压：" + temp + "mV";
                          tb0.Text = (string)page.Resources["cellAvgVol"] + temp + "mV";
                          sp1.Children.Add(tb0);

                          sp1 = sp.Children[1] as StackPanel;
                          sp1.Children.Clear();
                          sp1.Visibility = Visibility.Visible;
                          tb0 = new TextBlock();
                          //tb0.Width = 150;
                          tb0.Width = 350;
                          tb0.VerticalAlignment = VerticalAlignment.Center;
                          tb0.TextAlignment = TextAlignment.Left;
                          //tb0.Visibility = Visibility.Visible;
                          //tb0.Text = "电池单体平均电压：" + temp + "mV";
                          tb0.Text = (string)page.Resources["cellAvgTem"] + (data[3]-40) + "℃";
                          sp1.Children.Add(tb0);
                      });
                    }
                    else if (data[0] == 0x79)
                    {
                        //这一帧信息显示占七行
                        if (data[1] > 24 || data[1] <= 0) { return; }//从机数设置在[1,24]
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StackPanel spf = page.FindName("masterShiShiInfo4_4") as StackPanel;
                            if (spf.Children.Count == 0)
                            {
                                return;
                            }
                            TextBlock tbtitle = spf.Children[0] as TextBlock;
                            tbtitle.Visibility = Visibility.Visible;
                            // int q = 2+data[1]-1;
                            int q = ((int)data[1] - 1) * 4 + 1;
                            StackPanel spp = spf.Children[q] as StackPanel;
                            StackPanel spp1 = spp.Children[0] as StackPanel;
                            spp1.Children.Clear();
                            spp1.Visibility = Visibility.Visible;
                            TextBlock tbb = new TextBlock();
                            tbb.Width = 170;
                            tbb.VerticalAlignment = VerticalAlignment.Center;
                            tbb.TextAlignment = TextAlignment.Left;
                            tbb.Visibility = Visibility.Visible;
                            //tbb.Text = "从机编号：" + (int)data[1];
                            tbb.Text = (string)page.Resources["bmunum"] + (int)data[1];
                            spp1.Children.Add(tbb);

                            for (int n = 0; n < 3; n++)
                            {
                                //三行
                                int sTemp = q + n + 1;
                                StackPanel sp = spf.Children[sTemp] as StackPanel;
                                for (int i = 0; i < 4; i++)
                                {
                                    //四列
                                    StackPanel sp1 = sp.Children[i] as StackPanel;
                                    sp1.Children.Clear();
                                    //TextBlock tb = new TextBlock();
                                    sp1.Visibility = Visibility.Visible;

                                    for (int j = 0; j < 2; j++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Width = 50;
                                        tb.VerticalAlignment = VerticalAlignment.Center;
                                        tb.TextAlignment = TextAlignment.Left;
                                        //每列排两个数据
                                        int iTemp = 7 - 2 * i - j;
                                        int iTemp2 = 2 * i + j;
                                        tb.Text = "T" + (n * 8 + 7 - iTemp) + "：";
                                        sp1.Children.Add(tb);

                                        Button btn1 = new Button();
                                        // btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                                        //btn1.Margin = new Thickness(2, 0, 0, 0);
                                        btn1.Margin = new Thickness(2, 0, -3, 0);
                                        btn1.SetValue(Button.StyleProperty, Application.Current.Resources[(data[2 + n] >> iTemp2 & 0x1) == 0 ? "NormalButton" : "ErrorButton"]);
                                        sp1.Children.Add(btn1);

                                        // tb.Text = (data[2 + n] >> iTemp & 0x1) == 0 ? tb.Text + "C" + (n * 8 + 7 - iTemp) + "：正常 " : tb.Text + "C" + (n * 8 + 7 - iTemp) + "：开路";
                                    }

                                }
                            }

                        });
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //Console.WriteLine("接收到主机数据：4E");
                            //Info = "接收到4E帧数据";
                            Info = (string)page.Resources["recframe4E"];
                        });
                    }
                }
            }
        }

        //解析从机ID
        private void analysisID(CANSDK.VCI_CAN_OBJ obj)
        {
            uint canid = obj.ID;
            uint can = canid >> 8;
            if (isGetID == 0 || canid >> 8 != 0x0C1041) { return; }
            if (firstIDTime == 0)
            { firstIDTime = DateTime.Now.Ticks; }
            if ((DateTime.Now.Ticks - firstIDTime) / 10000 > (2 * 1000))
            {
                //超时
                if (firstID == null)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //ModernDialog.ShowMessage("请连接一个从机", "提示", MessageBoxButton.OK);
                        ModernDialog.ShowMessage((string)page.Resources["plsconnectonebmu"], (string)page.Resources["tips"], MessageBoxButton.OK);
                    });
                }
                else { SlaveNumTarget = firstID.ToString(); }
                isGetID = 0;
                firstIDTime = 0;
                firstID = null;
                return;
            }
            else
            {
                if (firstID == null) { firstID = canid & 0x000000FF; }
                else if ((canid & 0x000000FF) != firstID)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                               {
                                   //ModernDialog.ShowMessage("只能连接一个从机", "提示", MessageBoxButton.OK);
                                   ModernDialog.ShowMessage((string)page.Resources["onlyonebmu"], (string)page.Resources["tips"], MessageBoxButton.OK);

                               });
                    isGetID = 0;
                    firstIDTime = 0;
                    firstID = null;
                    return;
                }
            }

        }
        long firstIDTime = 0;
        uint? firstID = null;
        private long masterHeartTime = 0;
        private bool isMasterOnline = false;

        private List<byte> receiveHead = new List<byte>();
        //解析包
        private void analysisPackage(CANSDK.VCI_CAN_OBJ obj)
        {
            //解析ID
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string id = DataConverter.byteToHexStrForId(intBuff);


            //接收到下位机的报错校验
            if (new Regex(MASTERBOOTLOADER).IsMatch(id))
            {
                if (obj.Data[0] == obj.Data[1] && obj.Data[1] == obj.Data[2])
                {
                    //这一帧是主机bootloader下位机发送的报错帧，报错帧的格式为3个相同字节的错误码，连续收到三次则视为报错
                    if (obj.Data[0] == 0xF5 || obj.Data[1] == 0xF6)
                    {
                        if (receiveHead.Count == 0)
                        {
                            //list为空，则放入id
                            receiveHead.Add(obj.Data[0]);
                            return;
                        }
                        else if (receiveHead.Count == 1)
                        {
                            if (receiveHead[0].Equals(obj.Data[0]))
                            {
                                receiveHead.Add(obj.Data[0]);
                                return;
                            }
                            else
                            {
                                //这一帧跟之前存的帧不符合，则清空链表
                                receiveHead = new List<byte>();
                                return;
                            }
                        }
                        else if (receiveHead.Count == 2)
                        {
                            if (receiveHead[0].Equals(obj.Data[0]))
                            {
                                //连续收到三次该报文，报错并停止程序
                                if (obj.Data[0] == 0xF5)
                                {
                                    isReceive = false;
                                    receiveHead = new List<byte>();
                                    Application.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        ModernDialog.ShowMessage("重新上电刷写，超过三次则MPC5746R microcontroller code flash损坏", "提示", MessageBoxButton.OK);
                                        return;

                                    });
                                }
                            }
                            else
                            {
                                receiveHead = new List<byte>();
                                return;
                            }
                        }

                    }
                }

            }


            if (isGaosProfram == 0 && !(new Regex(MASTERHS).IsMatch(id))) { return; }
            //if (isGaosProfram == 0 && !(new Regex(MASTERHS).IsMatch(id)) && !(new Regex(MASTERINFO).IsMatch(id) && isGetMasterInfo == 1)) { return; }
            Console.WriteLine("receive：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            // Console.WriteLine("receive:" + DataConverter.byteToHexStrForData(obj.Data) + ",id:" + obj.ID.ToString("X2"));

            // Console.WriteLine("@@@@@@@@@@@@收到数据包的id：" + id);
            Regex reg = new Regex(CONFIG_REG);
            if (reg.IsMatch(id))
            {
                //匹配了配置信息的报文
                // Console.WriteLine("收到了配置信息报文，id：" + id);
                string hexdata = DataConverter.byteToHexStrForDataWithoutSpace(obj.Data);
                byte[] hexdatafobyte = obj.Data;
                string h = hexdata.Substring(0, 2);//取出第一个字节
                // Console.WriteLine("h:" + h + ",data:" + hexdata);
                //reg = new Regex(INFO1_REG);
                if (new Regex(INFO1_REG).IsMatch(h))
                {
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    String d1 = hexdata.Substring(2, 4);
                    // int temp = DataConverter.string2Hex(d1);
                    // int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    // int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac1") && isWaitting["fac1"] == 1)
                    {
                        isWaitting["fac1"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellVolHighAlarmFirst_rec = (temp * ResolutionRatioModel.cellVolHighAlarmFirst_rr + ResolutionRatioModel.cellVolHighAlarmFirst_offset).ToString();
                            FactoryConfig.CellVolHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellVolHighAlarmSecond_rr + ResolutionRatioModel.cellVolHighAlarmSecond_offset).ToString();
                            FactoryConfig.CellVolHighAlarmThird_rec = (temp3 * ResolutionRatioModel.cellVolHighAlarmThird_rr + ResolutionRatioModel.cellVolHighAlarmThird_offset).ToString();
                        });
                        Info = "初始化info1配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info1") && isWaitting["info1"] == 0) { return; }
                    else if (isWaitting.ContainsKey("info1") && isWaitting["info1"] == 1)
                    {
                        isWaitting["info1"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        isGaosProfram = 0;


                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];
                        Application.Current.Dispatcher.Invoke((Action)delegate
                       {
                           ScrollViewer sv = t.Content as ScrollViewer;
                           StackPanel sp = sv.Content as StackPanel;
                           BMUConfig bc = sp.Children[0] as BMUConfig;
                           BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                           bcm.CellVolHighAlarmFirst_rec = (temp * ResolutionRatioModel.cellVolHighAlarmFirst_rr + ResolutionRatioModel.cellVolHighAlarmFirst_offset).ToString();
                           bcm.CellVolHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellVolHighAlarmSecond_rr + ResolutionRatioModel.cellVolHighAlarmSecond_offset).ToString();
                           bcm.CellVolHighAlarmThird_rec = (temp3 * ResolutionRatioModel.cellVolHighAlarmThird_rr + ResolutionRatioModel.cellVolHighAlarmThird_offset).ToString();
                       });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info1";
                    }
                }
                else if (new Regex(INFO2_REG).IsMatch(h))
                {
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    String d1 = hexdata.Substring(2, 4);
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac2") && isWaitting["fac2"] == 1)
                    {
                        isWaitting["fac2"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellVolHighAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.cellVolHighAlarmRemoveFirst_rr + ResolutionRatioModel.cellVolHighAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.CellVolHighAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.cellVolHighAlarmRemoveSecond_rr + ResolutionRatioModel.cellVolHighAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.CellVolHighAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.cellVolHighAlarmRemoveThird_rr + ResolutionRatioModel.cellVolHighAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info2配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info2") && isWaitting["info2"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info2") && isWaitting["info2"] == 1)
                    {
                        isWaitting["info2"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellVolHighAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.cellVolHighAlarmRemoveFirst_rr + ResolutionRatioModel.cellVolHighAlarmRemoveFirst_offset).ToString();
                            bcm.CellVolHighAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.cellVolHighAlarmRemoveSecond_rr + ResolutionRatioModel.cellVolHighAlarmRemoveSecond_offset).ToString();
                            bcm.CellVolHighAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.cellVolHighAlarmRemoveThird_rr + ResolutionRatioModel.cellVolHighAlarmRemoveThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info2";
                    }
                }
                else if (new Regex(INFO3_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac3") && isWaitting["fac3"] == 1)
                    {
                        isWaitting["fac3"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellVolLowAlarmFirst_rec = (temp * ResolutionRatioModel.cellVolLowAlarmFirst_rr + ResolutionRatioModel.cellVolLowAlarmFirst_offset).ToString();
                            FactoryConfig.CellVolLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellVolLowAlarmSecond_rr + ResolutionRatioModel.cellVolLowAlarmSecond_offset).ToString();
                            FactoryConfig.CellVolLowAlarmThird_rec = (temp3 * ResolutionRatioModel.cellVolLowAlarmThird_rr + ResolutionRatioModel.cellVolLowAlarmThird_offset).ToString();
                        });
                        Info = "初始化info3配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info3") && isWaitting["info3"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info3") && isWaitting["info3"] == 1)
                    {
                        isWaitting["info3"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellVolLowAlarmFirst_rec = (temp * ResolutionRatioModel.cellVolLowAlarmFirst_rr + ResolutionRatioModel.cellVolLowAlarmFirst_offset).ToString();
                            bcm.CellVolLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellVolLowAlarmSecond_rr + ResolutionRatioModel.cellVolLowAlarmSecond_offset).ToString();
                            bcm.CellVolLowAlarmThird_rec = (temp3 * ResolutionRatioModel.cellVolLowAlarmThird_rr + ResolutionRatioModel.cellVolLowAlarmThird_offset).ToString();
                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info3";
                    }
                }
                else if (new Regex(INFO4_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac4") && isWaitting["fac4"] == 1)
                    {
                        isWaitting["fac4"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellVolLowAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr + ResolutionRatioModel.cellVolLowAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.CellVolLowAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.cellVolLowAlarmRemoveSecond_rr + ResolutionRatioModel.cellVolLowAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.CellVolLowAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.cellVolLowAlarmRemoveThird_rr + ResolutionRatioModel.cellVolLowAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info4配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info4") && isWaitting["info4"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info4") && isWaitting["info4"] == 1)
                    {
                        isWaitting["info4"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellVolLowAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr + ResolutionRatioModel.cellVolLowAlarmRemoveFirst_offset).ToString();
                            bcm.CellVolLowAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.cellVolLowAlarmRemoveSecond_rr + ResolutionRatioModel.cellVolLowAlarmRemoveSecond_offset).ToString();
                            bcm.CellVolLowAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.cellVolLowAlarmRemoveThird_rr + ResolutionRatioModel.cellVolLowAlarmRemoveThird_offset).ToString();
                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info4";
                    }
                }
                else if (new Regex(INFO5_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                    int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                    int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                    int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));
                    if (isWaitting.ContainsKey("fac5") && isWaitting["fac5"] == 1)
                    {
                        isWaitting["fac5"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellTemperatureHighAlarmFirst_rec = (temp * ResolutionRatioModel.cellTemperatureHighAlarmFirst_rr + ResolutionRatioModel.cellTemperatureHighAlarmFirst_offset).ToString();
                            FactoryConfig.CellTemperatureHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellTemperatureHighAlarmSecond_rr + ResolutionRatioModel.cellTemperatureHighAlarmSecond_offset).ToString();
                            FactoryConfig.CellTemperatureHighAlarmThird_rec = (temp3 * ResolutionRatioModel.cellTemperatureHighAlarmThird_rr + ResolutionRatioModel.cellTemperatureHighAlarmThird_offset).ToString();
                            FactoryConfig.CellTemperatureHighAlarmRemoveFirst_rec = (temp4 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.CellTemperatureHighAlarmRemoveSecond_rec = (temp5 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.CellTemperatureHighAlarmRemoveThird_rec = (temp6 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info5配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info5") && isWaitting["info5"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info5") && isWaitting["info5"] == 1)
                    {
                        isWaitting["info5"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellTemperatureHighAlarmFirst_rec = (temp * ResolutionRatioModel.cellTemperatureHighAlarmFirst_rr + ResolutionRatioModel.cellTemperatureHighAlarmFirst_offset).ToString();
                            bcm.CellTemperatureHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellTemperatureHighAlarmSecond_rr + ResolutionRatioModel.cellTemperatureHighAlarmSecond_offset).ToString();
                            bcm.CellTemperatureHighAlarmThird_rec = (temp3 * ResolutionRatioModel.cellTemperatureHighAlarmThird_rr + ResolutionRatioModel.cellTemperatureHighAlarmThird_offset).ToString();
                            bcm.CellTemperatureHighAlarmRemoveFirst_rec = (temp4 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_offset).ToString();
                            bcm.CellTemperatureHighAlarmRemoveSecond_rec = (temp5 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_offset).ToString();
                            bcm.CellTemperatureHighAlarmRemoveThird_rec = (temp6 * ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_rr + ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_offset).ToString();
                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info5";
                    }
                }
                else if (new Regex(INFO6_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                    int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                    int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                    int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));
                    if (isWaitting.ContainsKey("fac6") && isWaitting["fac6"] == 1)
                    {
                        isWaitting["fac6"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellTemperatureLowAlarmFirst_rec = (temp * ResolutionRatioModel.cellTemperatureLowAlarmFirst_rr + ResolutionRatioModel.cellTemperatureLowAlarmFirst_offset).ToString();
                            FactoryConfig.CellTemperatureLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellTemperatureLowAlarmSecond_rr + ResolutionRatioModel.cellTemperatureLowAlarmSecond_offset).ToString();
                            FactoryConfig.CellTemperatureLowAlarmThird_rec = (temp3 * ResolutionRatioModel.cellTemperatureLowAlarmThird_rr + ResolutionRatioModel.cellTemperatureLowAlarmThird_offset).ToString();
                            FactoryConfig.CellTemperatureLowAlarmRemoveFirst_rec = (temp4 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.CellTemperatureLowAlarmRemoveSecond_rec = (temp5 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.CellTemperatureLowAlarmRemoveThird_rec = (temp6 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info6配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info6") && isWaitting["info6"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info6") && isWaitting["info6"] == 1)
                    {
                        isWaitting["info6"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellTemperatureLowAlarmFirst_rec = (temp * ResolutionRatioModel.cellTemperatureLowAlarmFirst_rr + ResolutionRatioModel.cellTemperatureLowAlarmFirst_offset).ToString();
                            bcm.CellTemperatureLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.cellTemperatureLowAlarmSecond_rr + ResolutionRatioModel.cellTemperatureLowAlarmSecond_offset).ToString();
                            bcm.CellTemperatureLowAlarmThird_rec = (temp3 * ResolutionRatioModel.cellTemperatureLowAlarmThird_rr + ResolutionRatioModel.cellTemperatureLowAlarmThird_offset).ToString();
                            bcm.CellTemperatureLowAlarmRemoveFirst_rec = (temp4 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_offset).ToString();
                            bcm.CellTemperatureLowAlarmRemoveSecond_rec = (temp5 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_offset).ToString();
                            bcm.CellTemperatureLowAlarmRemoveThird_rec = (temp6 * ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_rr + ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_offset).ToString();
                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info6";
                    }
                }
                else if (new Regex(INFO7_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac7") && isWaitting["fac7"] == 1)
                    {
                        isWaitting["fac7"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanCurrentHighAlarmFirst_rec = (temp * ResolutionRatioModel.balanCurrentHighAlarmFirst_rr + ResolutionRatioModel.balanCurrentHighAlarmFirst_offset).ToString();
                            FactoryConfig.BalanCurrentHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentHighAlarmSecond_rr + ResolutionRatioModel.balanCurrentHighAlarmSecond_offset).ToString();
                            FactoryConfig.BalanCurrentHighAlarmThird_rec = (temp3 * ResolutionRatioModel.balanCurrentHighAlarmThird_rr + ResolutionRatioModel.balanCurrentHighAlarmThird_offset).ToString();
                        });
                        Info = "初始化info7配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info7") && isWaitting["info7"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info7") && isWaitting["info7"] == 1)
                    {
                        isWaitting["info7"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanCurrentHighAlarmFirst_rec = (temp * ResolutionRatioModel.balanCurrentHighAlarmFirst_rr + ResolutionRatioModel.balanCurrentHighAlarmFirst_offset).ToString();
                            bcm.BalanCurrentHighAlarmSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentHighAlarmSecond_rr + ResolutionRatioModel.balanCurrentHighAlarmSecond_offset).ToString();
                            bcm.BalanCurrentHighAlarmThird_rec = (temp3 * ResolutionRatioModel.balanCurrentHighAlarmThird_rr + ResolutionRatioModel.balanCurrentHighAlarmThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info7";
                    }
                }
                else if (new Regex(INFO8_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac8") && isWaitting["fac8"] == 1)
                    {
                        isWaitting["fac8"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanCurrentHighAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.BalanCurrentHighAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.BalanCurrentHighAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info8配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info8") && isWaitting["info8"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info8") && isWaitting["info8"] == 1)
                    {
                        isWaitting["info8"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanCurrentHighAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_offset).ToString();
                            bcm.BalanCurrentHighAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_offset).ToString();
                            bcm.BalanCurrentHighAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_rr + ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info8";
                    }
                }
                else if (new Regex(INFO9_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac9") && isWaitting["fac9"] == 1)
                    {
                        isWaitting["fac9"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanCurrentLowAlarmFirst_rec = (temp * ResolutionRatioModel.balanCurrentLowAlarmFirst_rr + ResolutionRatioModel.balanCurrentLowAlarmFirst_offset).ToString();
                            FactoryConfig.BalanCurrentLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentLowAlarmSecond_rr + ResolutionRatioModel.balanCurrentLowAlarmSecond_offset).ToString();
                            FactoryConfig.BalanCurrentLowAlarmThird_rec = (temp3 * ResolutionRatioModel.balanCurrentLowAlarmThird_rr + ResolutionRatioModel.balanCurrentLowAlarmThird_offset).ToString();
                        });
                        Info = "初始化info9配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info9") && isWaitting["info9"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info9") && isWaitting["info9"] == 1)
                    {
                        isWaitting["info9"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanCurrentLowAlarmFirst_rec = (temp * ResolutionRatioModel.balanCurrentLowAlarmFirst_rr + ResolutionRatioModel.balanCurrentLowAlarmFirst_offset).ToString();
                            bcm.BalanCurrentLowAlarmSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentLowAlarmSecond_rr + ResolutionRatioModel.balanCurrentLowAlarmSecond_offset).ToString();
                            bcm.BalanCurrentLowAlarmThird_rec = (temp3 * ResolutionRatioModel.balanCurrentLowAlarmThird_rr + ResolutionRatioModel.balanCurrentLowAlarmThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info9";
                    }
                }
                else if (new Regex(INFO10_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac10") && isWaitting["fac10"] == 1)
                    {
                        isWaitting["fac10"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanCurrentLowAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_offset).ToString();
                            FactoryConfig.BalanCurrentLowAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_offset).ToString();
                            FactoryConfig.BalanCurrentLowAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_offset).ToString();
                        });
                        Info = "初始化info10配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info10") && isWaitting["info10"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info10") && isWaitting["info10"] == 1)
                    {
                        isWaitting["info10"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanCurrentLowAlarmRemoveFirst_rec = (temp * ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_offset).ToString();
                            bcm.BalanCurrentLowAlarmRemoveSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_offset).ToString();
                            bcm.BalanCurrentLowAlarmRemoveThird_rec = (temp3 * ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_rr + ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info10";
                    }
                }
                else if (new Regex(INFO11_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    int temp2 = hexdatafobyte[4] << 8 | hexdatafobyte[3];
                    int temp3 = hexdatafobyte[6] << 8 | hexdatafobyte[5];
                    //int temp = DataConverter.string2Hex(d1);
                    //int temp2 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    //int temp3 = DataConverter.string2Hex(hexdata.Substring(10, 4));
                    if (isWaitting.ContainsKey("fac11") && isWaitting["fac11"] == 1)
                    {
                        isWaitting["fac11"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanCurrentSetValueFirst_rec = (temp * ResolutionRatioModel.balanCurrentSetValueFirst_rr + ResolutionRatioModel.balanCurrentSetValueFirst_offset).ToString();
                            FactoryConfig.BalanCurrentSetValueSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentSetValueSecond_rr + ResolutionRatioModel.balanCurrentSetValueSecond_offset).ToString();
                            FactoryConfig.BalanCurrentSetValueThird_rec = (temp3 * ResolutionRatioModel.balanCurrentSetValueThird_rr + ResolutionRatioModel.balanCurrentSetValueThird_offset).ToString();
                        });
                        Info = "初始化info11配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info11") && isWaitting["info11"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info11") && isWaitting["info11"] == 1)
                    {
                        isWaitting["info11"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanCurrentSetValueFirst_rec = (temp * ResolutionRatioModel.balanCurrentSetValueFirst_rr + ResolutionRatioModel.balanCurrentSetValueFirst_offset).ToString();
                            bcm.BalanCurrentSetValueSecond_rec = (temp2 * ResolutionRatioModel.balanCurrentSetValueSecond_rr + ResolutionRatioModel.balanCurrentSetValueSecond_offset).ToString();
                            bcm.BalanCurrentSetValueThird_rec = (temp3 * ResolutionRatioModel.balanCurrentSetValueThird_rr + ResolutionRatioModel.balanCurrentSetValueThird_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info11";
                    }
                }
                else if (new Regex(INFO12_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];

                    // int temp = DataConverter.string2Hex(d1);
                    if (isWaitting.ContainsKey("fac12") && isWaitting["fac12"] == 1)
                    {
                        isWaitting["fac12"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanVolOpenValue_rec = (temp * ResolutionRatioModel.balanVolOpenValue_rr + ResolutionRatioModel.balanVolOpenValue_offset).ToString();
                        });
                        Info = "初始化info12配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info12") && isWaitting["info12"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info12") && isWaitting["info12"] == 1)
                    {
                        isWaitting["info12"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanVolOpenValue_rec = (temp * ResolutionRatioModel.balanVolOpenValue_rr + ResolutionRatioModel.balanVolOpenValue_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info12";
                    }
                }
                else if (new Regex(INFO13_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    // int temp = DataConverter.string2Hex(d1);
                    if (isWaitting.ContainsKey("fac13") && isWaitting["fac13"] == 1)
                    {
                        isWaitting["fac13"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanVolCloseValue_rec = (temp * ResolutionRatioModel.balanVolCloseValue_rr + ResolutionRatioModel.balanVolCloseValue_offset).ToString();
                        });
                        Info = "初始化info13配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info13") && isWaitting["info13"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info13") && isWaitting["info13"] == 1)
                    {
                        isWaitting["info13"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanVolCloseValue_rec = (temp * ResolutionRatioModel.balanVolCloseValue_rr + ResolutionRatioModel.balanVolCloseValue_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info13";
                    }
                }
                else if (new Regex(INFO14_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    //  int temp = DataConverter.string2Hex(d1);
                    if (isWaitting.ContainsKey("fac14") && isWaitting["fac14"] == 1)
                    {
                        isWaitting["fac14"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanVolDifOpenValue_rec = (temp * ResolutionRatioModel.balanVolDifOpenValue_rr + ResolutionRatioModel.balanVolDifOpenValue_offset).ToString();
                        });
                        Info = "初始化info14配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info14") && isWaitting["info14"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info14") && isWaitting["info14"] == 1)
                    {
                        isWaitting["info14"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanVolDifOpenValue_rec = (temp * ResolutionRatioModel.balanVolDifOpenValue_rr + ResolutionRatioModel.balanVolDifOpenValue_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info14";
                    }
                }
                else if (new Regex(INFO15_REG).IsMatch(h))
                {
                    String d1 = hexdata.Substring(2, 4);
                    int temp = hexdatafobyte[2] << 8 | hexdatafobyte[1];
                    //  int temp = DataConverter.string2Hex(d1);
                    if (isWaitting.ContainsKey("fac15") && isWaitting["fac15"] == 1)
                    {
                        isWaitting["fac15"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.BalanVolDifCloseValue_rec = (temp * ResolutionRatioModel.balanVolDifCloseValue_rr + ResolutionRatioModel.balanVolDifCloseValue_offset).ToString();
                        });
                        Info = "初始化info15配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info15") && isWaitting["info15"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info15") && isWaitting["info15"] == 1)
                    {
                        isWaitting["info15"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.BalanVolDifCloseValue_rec = (temp * ResolutionRatioModel.balanVolDifCloseValue_rr + ResolutionRatioModel.balanVolDifCloseValue_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info15";
                    }
                }
                else if (new Regex(INFO16_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    if (isWaitting.ContainsKey("fac16") && isWaitting["fac16"] == 1)
                    {
                        isWaitting["fac16"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.CellBalanTemperatureOpenValue_rec = (temp * ResolutionRatioModel.cellBalanTemperatureOpenValue_rr + ResolutionRatioModel.cellBalanTemperatureOpenValue_offset).ToString();
                            FactoryConfig.CellBalanTemperatureCloseValue_rec = (temp2 * ResolutionRatioModel.cellBalanTemperatureCloseValue_rr + ResolutionRatioModel.cellBalanTemperatureCloseValue_offset).ToString();
                        });
                        Info = "初始化info16配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info16") && isWaitting["info16"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info16") && isWaitting["info16"] == 1)
                    {
                        isWaitting["info16"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.CellBalanTemperatureOpenValue_rec = (temp * ResolutionRatioModel.cellBalanTemperatureOpenValue_rr + ResolutionRatioModel.cellBalanTemperatureOpenValue_offset).ToString();
                            bcm.CellBalanTemperatureCloseValue_rec = (temp2 * ResolutionRatioModel.cellBalanTemperatureCloseValue_rr + ResolutionRatioModel.cellBalanTemperatureCloseValue_offset).ToString();

                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info16";
                    }
                }
                else if (new Regex(INFO17_REG).IsMatch(h))
                {
                    Console.WriteLine("收到info17"+obj);
                    if (isWaitting.ContainsKey("fac17") && isWaitting["fac17"] == 1)
                    {
                        isWaitting["fac17"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                        int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                        int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                        int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                        int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                        int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.SlaveNum_rec = (temp * ResolutionRatioModel.slaveNum_rr + ResolutionRatioModel.slaveNum_offset).ToString();
                            FactoryConfig.CellBalanMode_rec = (temp2 * ResolutionRatioModel.cellBalanMode_rr + ResolutionRatioModel.cellBalanMode_offset).ToString();
                            FactoryConfig.ChildModuleMonCellNumber_rec = (temp3 * ResolutionRatioModel.childModuleMonCellNumber_rr + ResolutionRatioModel.childModuleMonCellNumber_offset).ToString();
                            FactoryConfig.ChildMonModuleTemperatureNumber_rec = (temp4 * ResolutionRatioModel.childMonModuleTemperatureNumber_rr + ResolutionRatioModel.childMonModuleTemperatureNumber_offset).ToString();
                            FactoryConfig.ModuleAMonCellNum_rec = (temp5 * ResolutionRatioModel.moduleAMonCellNum_rr + ResolutionRatioModel.moduleAMonCellNum_offset).ToString();
                            FactoryConfig.ModuleAMonTemperatureNum_rec = (temp6 * ResolutionRatioModel.moduleAMonTemperatureNum_rr + ResolutionRatioModel.moduleAMonTemperatureNum_offset).ToString();
                        });
                        // runGetID();
                        isGetID = 1;
                        Info = "初始化info17配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info17") && isWaitting["info17"] == 1)
                    {
                        isWaitting["info17"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                        int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                        int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                        int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                        int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                        int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));
                        //Console.WriteLine("first:" + temp);

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.SlaveNum_rec = (temp * ResolutionRatioModel.slaveNum_rr + ResolutionRatioModel.slaveNum_offset).ToString();
                            bcm.CellBalanMode_rec = (temp2 * ResolutionRatioModel.cellBalanMode_rr + ResolutionRatioModel.cellBalanMode_offset).ToString();
                            bcm.ChildModuleMonCellNumber_rec = (temp3 * ResolutionRatioModel.childModuleMonCellNumber_rr + ResolutionRatioModel.childModuleMonCellNumber_offset).ToString();
                            bcm.ChildMonModuleTemperatureNumber_rec = (temp4 * ResolutionRatioModel.childMonModuleTemperatureNumber_rr + ResolutionRatioModel.childMonModuleTemperatureNumber_offset).ToString();
                            bcm.ModuleAMonCellNum_rec = (temp5 * ResolutionRatioModel.moduleAMonCellNum_rr + ResolutionRatioModel.moduleAMonCellNum_offset).ToString();
                            bcm.ModuleAMonTemperatureNum_rec = (temp6 * ResolutionRatioModel.moduleAMonTemperatureNum_rr + ResolutionRatioModel.moduleAMonTemperatureNum_offset).ToString();
                            // bcm.CellVolLowAlarmThird_rec = "10";
                        });
                        isGetID = 1;
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info6";
                    }



                    if (isWaitting.ContainsKey("info17") && isWaitting["info17"] == 0)
                    {
                        return;
                    }

                }
                else if (new Regex(INFO18_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                    int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                    int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                    int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));

                    if (isWaitting.ContainsKey("fac18") && isWaitting["fac18"] == 1)
                    {
                        isWaitting["fac18"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.ModuleBMonCellNum_rec = (temp * ResolutionRatioModel.moduleBMonCellNum_rr + ResolutionRatioModel.moduleBMonCellNum_offset).ToString();
                            FactoryConfig.ModuleBMonTemperatureNum_rec = (temp2 * ResolutionRatioModel.moduleBMonTemperatureNum_rr + ResolutionRatioModel.moduleBMonTemperatureNum_offset).ToString();
                            FactoryConfig.ModuleCMonCellNum_rec = (temp3 * ResolutionRatioModel.moduleCMonCellNum_rr + ResolutionRatioModel.moduleCMonCellNum_offset).ToString();
                            FactoryConfig.ModuleCMonTemperatureNum_rec = (temp4 * ResolutionRatioModel.moduleCMonTemperatureNum_rr + ResolutionRatioModel.moduleCMonTemperatureNum_offset).ToString();
                            FactoryConfig.ModuleDMonCellNum_rec = (temp5 * ResolutionRatioModel.moduleDMonCellNum_rr + ResolutionRatioModel.moduleDMonCellNum_offset).ToString();
                            FactoryConfig.ModuleDMonTemperatureNum_rec = (temp6 * ResolutionRatioModel.moduleDMonTemperatureNum_rr + ResolutionRatioModel.moduleDMonTemperatureNum_offset).ToString();
                        });
                        Info = "初始化info18配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info18") && isWaitting["info18"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info18") && isWaitting["info18"] == 1)
                    {
                        isWaitting["info18"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了

                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.ModuleBMonCellNum_rec = (temp * ResolutionRatioModel.moduleBMonCellNum_rr + ResolutionRatioModel.moduleBMonCellNum_offset).ToString();
                            bcm.ModuleBMonTemperatureNum_rec = (temp2 * ResolutionRatioModel.moduleBMonTemperatureNum_rr + ResolutionRatioModel.moduleBMonTemperatureNum_offset).ToString();
                            bcm.ModuleCMonCellNum_rec = (temp3 * ResolutionRatioModel.moduleCMonCellNum_rr + ResolutionRatioModel.moduleCMonCellNum_offset).ToString();
                            bcm.ModuleCMonTemperatureNum_rec = (temp4 * ResolutionRatioModel.moduleCMonTemperatureNum_rr + ResolutionRatioModel.moduleCMonTemperatureNum_offset).ToString();
                            bcm.ModuleDMonCellNum_rec = (temp5 * ResolutionRatioModel.moduleDMonCellNum_rr + ResolutionRatioModel.moduleDMonCellNum_offset).ToString();
                            bcm.ModuleDMonTemperatureNum_rec = (temp6 * ResolutionRatioModel.moduleDMonTemperatureNum_rr + ResolutionRatioModel.moduleDMonTemperatureNum_offset).ToString();

                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info18";
                    }
                }
                else if (new Regex(INFO19_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 4));
                    int temp4 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                    int temp5 = DataConverter.string2Hex(hexdata.Substring(12, 2));
                    if (isWaitting.ContainsKey("fac19") && isWaitting["fac19"] == 1)
                    {
                        isWaitting["fac19"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.ModuleEMonCellNum_rec = (temp * ResolutionRatioModel.moduleEMonCellNum_rr + ResolutionRatioModel.moduleEMonCellNum_offset).ToString();
                            FactoryConfig.ModuleEMonTemperatureNum_rec = (temp2 * ResolutionRatioModel.moduleEMonTemperatureNum_rr + ResolutionRatioModel.moduleEMonTemperatureNum_offset).ToString();
                            FactoryConfig.PackProYear_rec = (temp3 * ResolutionRatioModel.packProYear_rr + ResolutionRatioModel.packProYear_offset).ToString();
                            FactoryConfig.PackProMonth_rec = (temp4 * ResolutionRatioModel.packProMonth_rr + ResolutionRatioModel.packProMonth_offset).ToString();
                            FactoryConfig.PackProDay_rec = (temp5 * ResolutionRatioModel.packProDay_rr + ResolutionRatioModel.packProDay_offset).ToString();
                        });
                        Info = "初始化info19配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info19") && isWaitting["info19"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info19") && isWaitting["info19"] == 1)
                    {
                        isWaitting["info19"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                            bcm.ModuleEMonCellNum_rec = (temp * ResolutionRatioModel.moduleEMonCellNum_rr + ResolutionRatioModel.moduleEMonCellNum_offset).ToString();
                            bcm.ModuleEMonTemperatureNum_rec = (temp2 * ResolutionRatioModel.moduleEMonTemperatureNum_rr + ResolutionRatioModel.moduleEMonTemperatureNum_offset).ToString();
                            bcm.PackProYear_rec = (temp3 * ResolutionRatioModel.packProYear_rr + ResolutionRatioModel.packProYear_offset).ToString();
                            bcm.PackProMonth_rec = (temp4 * ResolutionRatioModel.packProMonth_rr + ResolutionRatioModel.packProMonth_offset).ToString();
                            bcm.PackProDay_rec = (temp5 * ResolutionRatioModel.packProDay_rr + ResolutionRatioModel.packProDay_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info19";
                    }
                }
                else if (new Regex(INFO20_REG).IsMatch(h))
                {
                    int temp = DataConverter.string2Hex(hexdata.Substring(2, 2));
                    int temp2 = DataConverter.string2Hex(hexdata.Substring(4, 2));
                    int temp3 = DataConverter.string2Hex(hexdata.Substring(6, 2));
                    int temp4 = DataConverter.string2Hex(hexdata.Substring(8, 2));
                    int temp5 = DataConverter.string2Hex(hexdata.Substring(10, 2));
                    int temp6 = DataConverter.string2Hex(hexdata.Substring(12, 2));
                    if (isWaitting.ContainsKey("fac20") && isWaitting["fac20"] == 1)
                    {
                        isWaitting["fac20"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            FactoryConfig.SerialNum_rec = ((int)(temp * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                            ((int)(temp2 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                            ((int)(temp3 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                            ((int)(temp4 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                            ((int)(temp5 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                            ((int)(temp6 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X");

                            //FactoryConfig.PackBatchNumberData1_rec = (temp * ResolutionRatioModel.packBatchNumberData1_rr + ResolutionRatioModel.packBatchNumberData1_offset).ToString();
                            //FactoryConfig.PackBatchNumberData2_rec = (temp2 * ResolutionRatioModel.packBatchNumberData2_rr + ResolutionRatioModel.packBatchNumberData2_offset).ToString();
                            //FactoryConfig.PackBatchNumberData3_rec = (temp3 * ResolutionRatioModel.packBatchNumberData3_rr + ResolutionRatioModel.packBatchNumberData3_offset).ToString();
                            //FactoryConfig.PackBatchNumberData4_rec = (temp4 * ResolutionRatioModel.packBatchNumberData4_rr + ResolutionRatioModel.packBatchNumberData4_offset).ToString();
                            //FactoryConfig.PackBatchNumberData5_rec = (temp5 * ResolutionRatioModel.packBatchNumberData5_rr + ResolutionRatioModel.packBatchNumberData5_offset).ToString();
                            //FactoryConfig.PackBatchNumberData6_rec = (temp6 * ResolutionRatioModel.packBatchNumberData6_rr + ResolutionRatioModel.packBatchNumberData6_offset).ToString();
                        });
                        Info = "初始化info20配置完毕";
                    }
                    else if (isWaitting.ContainsKey("info20") && isWaitting["info20"] == 0)
                    {
                        return;
                    }
                    else if (isWaitting.ContainsKey("info20") && isWaitting["info20"] == 1)
                    {
                        isWaitting["info20"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了


                        List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
                        TabItem t = tabItemList[bmuConfigListNum];

                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ScrollViewer sv = t.Content as ScrollViewer;
                            StackPanel sp = sv.Content as StackPanel;
                            BMUConfig bc = sp.Children[0] as BMUConfig;
                            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;

                            bcm.SerialNum_rec = ((int)(temp * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                           ((int)(temp2 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                           ((int)(temp3 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                           ((int)(temp4 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                           ((int)(temp5 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X") +
                           ((int)(temp6 * ResolutionRatioModel.serialNum_rr + ResolutionRatioModel.serialNum_offset)).ToString("X");

                            //bcm.PackBatchNumberData1_rec = (temp * ResolutionRatioModel.packBatchNumberData1_rr + ResolutionRatioModel.packBatchNumberData1_offset).ToString();
                            //bcm.PackBatchNumberData2_rec = (temp2 * ResolutionRatioModel.packBatchNumberData2_rr + ResolutionRatioModel.packBatchNumberData2_offset).ToString();
                            //bcm.PackBatchNumberData3_rec = (temp3 * ResolutionRatioModel.packBatchNumberData3_rr + ResolutionRatioModel.packBatchNumberData3_offset).ToString();
                            //bcm.PackBatchNumberData4_rec = (temp4 * ResolutionRatioModel.packBatchNumberData4_rr + ResolutionRatioModel.packBatchNumberData4_offset).ToString();
                            //bcm.PackBatchNumberData5_rec = (temp5 * ResolutionRatioModel.packBatchNumberData5_rr + ResolutionRatioModel.packBatchNumberData5_offset).ToString();
                            //bcm.PackBatchNumberData6_rec = (temp6 * ResolutionRatioModel.packBatchNumberData6_rr + ResolutionRatioModel.packBatchNumberData6_offset).ToString();
                        });
                        Info = "接收到第" + (bmuConfigListNum + 1) + "页配置的info20";
                        if (bmuConfigListNum == tabItemList.Count)
                        {
                            Info = "配置完毕";
                        }
                    }
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
                    if (isWaitting.ContainsKey("bootHS") && isWaitting["bootHS"] == 1)
                    {
                        isWaitting["bootHS"] = 0;
                        retryTimes = 0;
                        dataCacheIndex = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        canBootNext = 1;
                    }
                    else if (isWaitting.ContainsKey("bmasterHS") && isWaitting["bmasterHS"] == 1)
                    {
                        isWaitting["bmasterHS"] = 0;
                        retryTimes = 0;
                        dataCacheIndex = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        canBootNext = 1;
                    }
                    else if (isWaitting.ContainsKey("bootloaderFE") && (isWaitting["bootloaderFE"] == 1 || isWaitting["bootloaderFE"] == 2))
                    {
                        isWaitting["bootloaderFE"] = 0;
                        retryTimes = 0;
                        dataCacheIndex = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        canBootNext = 1;
                        dataCache = new CANSDK.VCI_CAN_OBJ[17];
                    }

                }
                else if (new Regex(BOOTLOADERF2).IsMatch(h))
                {
                    Console.WriteLine("F2");
                }
                else if (new Regex(BOOTLOADERF1).IsMatch(h))
                {
                    Console.WriteLine("F1");
                    if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 1)
                    {

                        Info = "数据重传";
                        isWaitting["bootloaderFE"] = 2;//2表示需要重传数据
                        //isWaitting["bootloaderFE"] = 0;
                        //retryTimes = 0;
                        //isGetReceived = 1;//已经收到了回送，不需要再发送了
                        //// Console.WriteLine("握手成功！");
                        //canBootNext = 1;
                    }
                }
                else if (new Regex(BOOTLOADERF0).IsMatch(h))
                {
                    Console.WriteLine("F0");
                    if (isWaitting.ContainsKey("over") && isWaitting["over"] == 1)
                    {
                        isWaitting["over"] = 0;
                        retryTimes = 0;

                        dataCacheIndex = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了
                        // Console.WriteLine("握手成功！");
                        canBootNext = 0;//bootloader刷写完成不用继续进行
                        Info = "程序下载完毕！";
                    }
                }
            }
            else if ((new Regex(MASTERHS).IsMatch(id)))
            {
                byte[] data = obj.Data;
                if (data[0] == 0x53)
                {
                    Console.WriteLine("收到0x53心跳响应包");
                    if (isWaitting.ContainsKey("masterHS") && isWaitting["masterHS"] == 1)
                    {
                        isWaitting["masterHS"] = 0;
                        retryTimes = 0;
                        isGetReceived = 1;//已经收到了回送，不需要再发送了     `   
                    }
                }
                //软件版本号和电池组信息是发送握手包后，下位机跟着握手确认包一起回送的
                if (data[0] == 0x41)
                {
                    byte[] b = new byte[6];
                    Array.Copy(data, 1, b, 0, 6);
                    Console.WriteLine(b.Length);
                    versionNum1 = DataConverter.bytetoAscString(b).Trim();
                    //versionNum1 = (DataConverter.bytetoAscString(data).Remove(0, 1)).Remove(12,1);
                    Console.WriteLine("收到软件版本号1:" + versionNum1);
                    if (!versionNum2.Equals("") && !versionNum3.Equals("") && !versionNum4.Equals(""))
                    {
                        Version = "主机软件版本号：" + versionNum1 + versionNum2 + versionNum3 + versionNum4;
                        versionNum1 = "";
                        versionNum2 = "";
                        versionNum3 = "";
                        versionNum4 = "";
                        VersionVisibility[0] = "Visible";
                    }
                }
                else if (data[0] == 0x42)
                {
                    byte[] b = new byte[6];
                    Array.Copy(data, 1, b, 0, 6);
                    Console.WriteLine(b.Length);
                    versionNum2 = DataConverter.bytetoAscString(b).Trim();
                    //versionNum2 = (DataConverter.bytetoAscString(data).Remove(0, 1)).Remove(12,1);
                    Console.WriteLine("收到软件版本号2:" + versionNum2);
                    if (!versionNum1.Equals("") && !versionNum3.Equals("") && !versionNum4.Equals(""))
                    {
                        Version = "主机软件版本号：" + versionNum1 + versionNum2 + versionNum3 + versionNum4;
                        versionNum1 = "";
                        versionNum2 = "";
                        versionNum3 = "";
                        versionNum4 = "";
                        VersionVisibility[0] = "Visible";
                    }
                }
                else if (data[0] == 0x45)
                {

                    byte[] b = new byte[6];
                    Array.Copy(data, 1, b, 0, 6);
                    Console.WriteLine(b.Length);
                    versionNum3 = DataConverter.bytetoAscString(b).Trim();
                    // versionNum3 = (DataConverter.bytetoAscString(data).Remove(0, 1)).Remove(12,1);
                    Console.WriteLine("收到软件版本号3:" + versionNum3);
                    if (!versionNum1.Equals("") && !versionNum2.Equals("") && !versionNum4.Equals(""))
                    {
                        Version = "主机软件版本号：" + versionNum1 + versionNum2 + versionNum3 + versionNum4;
                        versionNum1 = "";
                        versionNum2 = "";
                        versionNum3 = "";
                        versionNum4 = "";
                        VersionVisibility[0] = "Visible";
                    }
                }
                else if (data[0] == 0x46)
                {
                    byte[] b = new byte[6];
                    Array.Copy(data, 1, b, 0, 6);
                    Console.WriteLine(b.Length);
                    versionNum4 = DataConverter.bytetoAscString(b).Trim();
                    //versionNum4 =(DataConverter.bytetoAscString(data).Remove(0, 1)).Remove(12,1);
                    Console.WriteLine("收到软件版本号4:" + versionNum4);
                    if (!versionNum1.Equals("") && !versionNum2.Equals("") && !versionNum3.Equals(""))
                    {
                        Version = "主机软件版本号：" + versionNum1 + versionNum2 + versionNum3 + versionNum4;
                        versionNum1 = "";
                        versionNum2 = "";
                        versionNum3 = "";
                        versionNum4 = "";
                        VersionVisibility[0] = "Visible";
                    }
                }
                else if (data[0] == 0x43)
                {
                    int xishu = data[1];
                    if (xishu >= 1 && xishu <= 4)
                    {
                        int num = (xishu - 1) * 6;
                        for (int i = 0; i < 6; i++)
                        {
                            int p = num + i;
                            chuanshu[p] = (int)data[2 + i];
                            
                        }
                    }
                    // Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    StackPanel spf = page.FindName("masterChuanShuInfo") as StackPanel;
                    //    int xishu = data[1];
                    //    int num = (xishu - 1) * 6;
                    //    for (int i = 0; i < 6; i++)
                    //    {
                    //        int p = num + i;
                    //        int q = xishu - 1;
                    //        StackPanel sp = spf.Children[q] as StackPanel;

                    //        TextBox tb = sp.Children[i] as TextBox;
                    //        tb.Visibility = Visibility.Visible;
                    //        if ((p + 1) < 10)
                    //        {
                    //            tb.Text = "0" + (p + 1) + "号从机串数：" + (int)data[2 + i];
                    //        }
                    //        else
                    //        {
                    //            tb.Text = (p + 1) + "号从机串数：" + (int)data[2 + i];
                    //        }
                    //    }
                    //});
                    Console.WriteLine("收到电池组信息1");
                }
                else if (data[0] == 0x44)
                {
                    int xishu = data[1];
                    if (xishu >= 1 && xishu <= 4)
                    {

                        int num = (xishu - 1) * 6;
                        for (int i = 0; i < 6; i++)
                        {
                            int p = num + i;
                            wenganshu[p] = (int)data[2 + i];
                        }
                    }
                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    StackPanel spf = page.FindName("masterWenGanInfo") as StackPanel;
                    //    int xishu = data[1];
                    //    int num = (xishu - 1) * 6;
                    //    for (int i = 0; i < 6; i++)
                    //    {
                    //        int p = num + i;
                    //        int q = xishu - 1;
                    //        StackPanel sp = spf.Children[q] as StackPanel;

                    //        TextBox tb = sp.Children[i] as TextBox;
                    //        tb.Visibility = Visibility.Visible;
                    //        if ((p + 1) < 10)
                    //        {
                    //            tb.Text = "0" + (p + 1) + "号从机温感数：" + (int)data[2 + i];
                    //        }
                    //        else
                    //        {
                    //            tb.Text = (p + 1) + "号从机温感数：" + (int)data[2 + i];
                    //        }
                    //    }
                    //});
                    Console.WriteLine("收到电池组信息2");
                }

            }

        }

        ////发送数据
        //public void sendData(Object o)
        //{
        //   // Object o=s.Obj;
        //    isGaosProfram = 1;
        //    isGetReceived = 0;
        //    int i = 1;
        //    sendInfo send = (sendInfo)o;
        //    CANSDK.VCI_CAN_OBJ obj = send.Obj;
        //    while (true)
        //    {
        //        Thread.Sleep(5);
        //       // Console.WriteLine("isGetReceived:" + isGetReceived);
        //        if (retryTimes < RETRYTIMES && isGetReceived == 0)
        //        {
        //            //如果未收到回应且还有重发次数
        //            Console.WriteLine(DataConverter.byteToHexStrForData(obj.Data)+","+obj.ID);
        //            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        //            isWaitting[send.Flag] = 1;//开始等待接收回送数据包
        //            //for (int v = 0; v < isWaitting.Count(); v++) {
        //            //    Console.WriteLine(isWaitting.ContainsKey("info1").ToString() +","+ isWaitting.ContainsKey("info2").ToString()) ;
        //            //}                      
        //            retryTimes++;
        //            Console.WriteLine("发送次数：" + (i++));
        //            long sendTicks = DateTime.Now.Ticks; //记录数据发出的时间
        //            while (true)
        //            {
        //                Thread.Sleep(5);
        //                //如果正在等待接收回送数据包
        //                if (isWaitting.ContainsKey(send.Flag) && isWaitting[send.Flag] == 1 && isGetReceived == 0)
        //                {

        //                    if ((DateTime.Now.Ticks - sendTicks) / 10000 < (OVERTIME * 1000))//没有超时
        //                    {
        //                        continue;
        //                    }
        //                    else
        //                    { //超时
        //                        isWaitting[send.Flag] = 0;
        //                        break;
        //                    }
        //                }
        //                else { break; }
        //            }
        //        }
        //        else
        //        {
        //            if (retryTimes >= RETRYTIMES) { Info = "第" + (bmuConfigListNum + 1) + "号配置的" + send.Flag + "超时"; }                   
        //            //Console.WriteLine("复位");
        //            //复位
        //            retryTimes = 0;
        //            isWaitting[send.Flag] = 0;
        //            isGetReceived = 1;
        //            isGaosProfram = 0;
        //            //saveBmuConfigList.Clear();
        //            return;
        //        }
        //    }

        //}

        TimeSpan ts;


        /// <summary>
        /// zhengzhonghua
        /// 接受BMU总体信息并解析
        /// </summary>
        /// <param name="canid"></param>
        private void receiveBMUInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //BMU总体信息
                Regex rinfo = new Regex(bmuinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)//数据帧
                {
                    //开始根据BMU总信息实时更新界面数据
                    string currentbmubox = canidhex.Substring(6, 2); //解析出两位字符串，将16位进制转换为10进制
                    int currentbmunum = Int32.Parse(currentbmubox, System.Globalization.NumberStyles.HexNumber);//当前BMU箱号
                    int currentbmuheart = obj.Data[0] & 0x0f; //从机心跳值
                    int currentbmubcnt = ((obj.Data[1] & 0x03) << 4) | ((obj.Data[0] & 0xf0) >> 4);//BCNT电池数目
                    int currenttmpbcnt = (obj.Data[1] & 0xfc) >> 2;//温度数目
                    double totalvol = (double)(((obj.Data[5] & 0x0f) << 8) | (obj.Data[4] & 0xff)) * 0.1; //总压值
                    int tmphigh = (obj.Data[2] & 0xff) - 40;//最高温度
                    int tmplow = (obj.Data[3] & 0xff) - 40;//最低温度
                    int tmphighnum = ((obj.Data[6] & 0x03) << 4) | ((obj.Data[5] & 0x0f0) >> 4);
                    int tmplownum = (obj.Data[6] & 0xfc) >> 2;
                    int batterystatus = obj.Data[7] & 0x03;//电池状态
                    int tmpstatus = (obj.Data[7] & 0x04) >> 2;//温度状态
                    int hardwarestatus = (obj.Data[7] & 0x08) >> 3;//硬件状态
                    int signalstatus = (obj.Data[7] & 0x10) >> 4;//信号线状态
                    int balancestatus = (obj.Data[7] & 0x20) >> 5;//均衡状态

                    //根据bmu索引号查找对应tabitem
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == currentbmunum)
                                {
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    GroupBox gb = parent.Children[0] as GroupBox;
                                    List<TextBox> bmu_textboxList = MyVisualTreeHelper.FindVisualChild<TextBox>(gb);
                                    List<ComboBox> bmu_comboboxList = MyVisualTreeHelper.FindVisualChild<ComboBox>(gb);
                                    List<Button> bmu_buttonList = MyVisualTreeHelper.FindVisualChild<Button>(gb);

                                    if (bmu_comboboxList.Count != 0)
                                    {
                                        var query = from tt in bmu_comboboxList where tt.Name == "batterycb_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().SelectedIndex = batterystatus;
                                        }
                                    }

                                    if (bmu_textboxList.Count != 0)
                                    {
                                        var query = from tt in bmu_textboxList where tt.Name == "slavetx_" + currentbmunum.ToString() select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = currentbmuheart.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "cellcountstx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = currentbmubcnt.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "tempcountstx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = currenttmpbcnt.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "totalvoltbtx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = totalvol.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "tmphightx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = tmphigh.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "tmphighnumtx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = tmphighnum.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "tmplowtx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = tmplow.ToString();
                                        }
                                        query = from tt in bmu_textboxList where tt.Name == "tmplownumtx_" + currentbmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = tmplownum.ToString();
                                        }
                                        var btnquery = from mm in bmu_buttonList where mm.Name == "tmpbtn_" + currentbmunum select mm;
                                        if (btnquery != null && btnquery.Count() > 0)
                                        {
                                            if (tmpstatus == 0)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalEnableButton"]);
                                            else if (tmpstatus == 1)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                                        }
                                        btnquery = from mm in bmu_buttonList where mm.Name == "hardwarestatusbtn_" + currentbmunum select mm;
                                        if (btnquery != null && btnquery.Count() > 0)
                                        {
                                            if (hardwarestatus == 0)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalEnableButton"]);
                                            else if (hardwarestatus == 1)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                                        }
                                        btnquery = from mm in bmu_buttonList where mm.Name == "signalstatusbtn_" + currentbmunum select mm;
                                        if (btnquery != null && btnquery.Count() > 0)
                                        {
                                            if (signalstatus == 0)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalEnableButton"]);
                                            else if (signalstatus == 1)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                                        }
                                        btnquery = from mm in bmu_buttonList where mm.Name == "balancestatusbtn_" + currentbmunum select mm;
                                        if (btnquery != null && btnquery.Count() > 0)
                                        {
                                            if (balancestatus == 0)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalEnableButton"]);
                                            else if (balancestatus == 1)
                                                btnquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                                        }

                                    }

                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


        }

        /// <summary>
        /// zhengzhonghua 2018-05-15
        /// 解析BMU极值信息2
        /// </summary>
        /// <param name="obj"></param>
        private void receiveBmuInfo2(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                Regex rinfo = new Regex(bmuinfo2);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    string box = canidhex.Substring(6, 2);
                    int bmunum = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);
                    double cellhigh = (((obj.Data[1] & 0x00ff) << 8) | (obj.Data[0] & 0x00ff)) * 0.001;//单体最高电压
                    double celllow = (((obj.Data[3] & 0x00ff) << 8) | (obj.Data[2] & 0x00ff)) * 0.001;//单体最低电压
                    double celllavg = (((obj.Data[5] & 0x00ff) << 8) | (obj.Data[4] & 0x00ff)) * 0.001;//平均电压
                    int cellhighnum = obj.Data[6] & 0xff; //最高电压序号
                    int celllownum = obj.Data[7] & 0xff;//最低电压序号
                    //根据bmu索引号查找对应tabitem
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == bmunum)
                                {
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    GroupBox bmugb = parent.Children[0] as GroupBox;
                                    List<TextBox> textboxList = MyVisualTreeHelper.FindVisualChild<TextBox>(bmugb);
                                    if (textboxList.Count != 0)
                                    {
                                        var query = from tt in textboxList where tt.Name == "cellhightx_" + bmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = string.Format("{0:f3}", cellhigh);
                                        }
                                        query = from tt in textboxList where tt.Name == "cellhighnumtx_" + bmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = cellhighnum.ToString();
                                        }
                                        query = from tt in textboxList where tt.Name == "celllowtx_" + bmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = string.Format("{0:f3}", celllow);
                                        }
                                        query = from tt in textboxList where tt.Name == "celllownumtx_" + bmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = celllownum.ToString();
                                        }
                                        query = from tt in textboxList where tt.Name == "cellavgtx_" + bmunum select tt;
                                        if (query != null && query.Count() > 0)
                                        {
                                            query.First().Text = celllavg.ToString();
                                        }

                                    }

                                }
                            }
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


        }


        /// <summary>
        /// zhengzhonghua 2018-04-26
        /// 解析BMU上传上来的实时单体电压
        /// </summary>
        /// <param name="obj"></param>
        private void receiveCellInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {if (obj.DataLen == 0) { return; }
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //单体信息
                Regex rinfo = new Regex(cellinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //获取BMU从机号
                    string index = canidhex.Substring(6, 2);
                    int bmuindex = Int32.Parse(index, System.Globalization.NumberStyles.HexNumber);
                    //获取该箱单体序号值
                    string num = canidhex.Substring(3, 1);
                    int cellnum = Int32.Parse(num, System.Globalization.NumberStyles.HexNumber);

                    //单体数值1
                    byte celllow1 = obj.Data[0];//低字节
                    byte cellhigh1 = obj.Data[1];//高字节
                    ushort cell1 = (ushort)(((cellhigh1 & 0x00ff) << 8) + (celllow1 & 0x00ff));
                    double cellshow1 = cell1 * 0.001;

                    //if (BMU_Vol[bmuindex][(cellnum - 1) * 4 + 1] - cellshow1 > 0.01)
                    //{
                    //    Info = "从机"+index+"单体" + ((cellnum - 1) * 4 + 1) + "压降过大!";
                    //}
                    //else if (cellshow1 > BMU_Vol[bmuindex][(cellnum - 1) * 4 + 1])
                    //{
                    //    BMU_Vol[bmuindex][(cellnum - 1) * 4 + 1] = cellshow1;
                    //}

                    //单体数值2
                    byte celllow2 = obj.Data[2];//低字节
                    byte cellhigh2 = obj.Data[3];//高字节
                    ushort cell2 = (ushort)(((cellhigh2 & 0x00ff) << 8) + (celllow2 & 0x00ff));
                    double cellshow2 = cell2 * 0.001;
                    //if (BMU_Vol[bmuindex][(cellnum - 1) * 4 + 2] - cellshow2 > 0.01)
                    //{
                    //    Info = "从机" + index + "单体" + ((cellnum - 1) * 4 + 2) + "压降过大!";
                    //}
                    //else if (cellshow2 > BMU_Vol[bmuindex][(cellnum - 1) * 4 + 2])
                    //{
                    //    BMU_Vol[bmuindex][(cellnum - 1) * 4 + 2] = cellshow2;
                    //}
                    //单体数值3
                    byte celllow3 = obj.Data[4];//低字节
                    byte cellhigh3 = obj.Data[5];//高字节
                    ushort cell3 = (ushort)(((cellhigh3 & 0x00ff) << 8) + (celllow3 & 0x00ff));
                    double cellshow3 = cell3 * 0.001;
                    //if (BMU_Vol[bmuindex][(cellnum - 1) * 4 + 3] - cellshow3 > 0.01)
                    //{
                    //    Info = "从机" + index + "单体" + ((cellnum - 1) * 4 + 3) + "压降过大!";
                    //}
                    //else if (cellshow3 > BMU_Vol[bmuindex][(cellnum - 1) * 4 + 3])
                    //{
                    //    BMU_Vol[bmuindex][(cellnum - 1) * 4 + 3] = cellshow3;
                    //}
                    //单体数值4
                    byte celllow4 = obj.Data[6];//低字节
                    byte cellhigh4 = obj.Data[7];//高字节
                    ushort cell4 = (ushort)(((cellhigh4 & 0x00ff) << 8) + (celllow4 & 0x00ff));
                    double cellshow4 = cell4 * 0.001;
                    //if (BMU_Vol[bmuindex][(cellnum - 1) * 4 + 4] - cellshow4 > 0.01)
                    //{
                    //    Info = "从机" + index + "单体" + ((cellnum - 1) * 4 + 4) + "压降过大!";
                    //}
                    //else if (cellshow4 > BMU_Vol[bmuindex][(cellnum - 1) * 4 + 4])
                    //{
                    //    BMU_Vol[bmuindex][(cellnum - 1) * 4 + 4] = cellshow4;
                    //}

                    var q = from tt in receiveBmuList where tt.Bmuindex == bmuindex select tt;
                    if (q != null && q.Count() > 0)
                    {

                        if (!q.First().CellDic.ContainsKey(((cellnum - 1) * 4 + 1)))
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow1;
                            model.Cellstatus = 1;
                            q.First().CellDic.Add(((cellnum - 1) * 4 + 1), model);
                        }
                        else
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow1;
                            model.Cellstatus = 1;
                            q.First().CellDic[((cellnum - 1) * 4 + 1)] = model;
                        }

                        if (!q.First().CellDic.ContainsKey(((cellnum - 1) * 4 + 2)))
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow2;
                            model.Cellstatus = 1;
                            q.First().CellDic.Add(((cellnum - 1) * 4 + 2), model);
                        }
                        else
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow2;
                            model.Cellstatus = 1;
                            q.First().CellDic[((cellnum - 1) * 4 + 2)] = model;
                        }

                        if (!q.First().CellDic.ContainsKey(((cellnum - 1) * 4 + 3)))
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow3;
                            model.Cellstatus = 1;
                            q.First().CellDic.Add(((cellnum - 1) * 4 + 3), model);
                        }
                        else
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow3;
                            model.Cellstatus = 1;
                            q.First().CellDic[((cellnum - 1) * 4 + 3)] = model;
                        }

                        if (!q.First().CellDic.ContainsKey(((cellnum - 1) * 4 + 4)))
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow4;
                            model.Cellstatus = 1;
                            q.First().CellDic.Add(((cellnum - 1) * 4 + 4), model);
                        }
                        else
                        {
                            CellModel model = new CellModel();
                            model.Cellvol = (float)cellshow4;
                            model.Cellstatus = 1;
                            q.First().CellDic[((cellnum - 1) * 4 + 4)] = model;
                        }
                    }


                    //String time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff");
                    //lock (lockercell)
                    //{
                    //    DataRow dr = dt.NewRow();
                    //    dr[0] = cellbox;
                    //    dr[(cellnum - 1) * 4 + 1] = cellshow1;
                    //    dr[(cellnum - 1) * 4 + 2] = cellshow2;
                    //    dr[(cellnum - 1) * 4 + 3] = cellshow3;
                    //    dr[(cellnum - 1) * 4 + 4] = cellshow4;
                    //    dr[63] = time;
                    //    dt.Rows.Add(dr);
                    //    string data = export.ExportCSV(dt);
                    //    sw.Write(data);
                    //    sw.Flush();
                    //    dt.Clear();
                    //}


                    //查找界面元素并显示单体数据
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == bmuindex)
                                {
                                    //查找对应的BMU从机号
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    GroupBox gb = parent.Children[2] as GroupBox;
                                    List<TextBox> celll_textList = MyVisualTreeHelper.FindVisualChild<TextBox>(gb);
                                    var query = from tt in celll_textList where tt.Name == "V" + bmuindex + "_" + ((cellnum - 1) * 4 + 1).ToString() select tt;
                                    if (query != null && query.Count() > 0)
                                    {
                                        lastcellshow1 = double.Parse(query.First().Text);
                                        if (lastcellshow1 != double.Parse(string.Format("{0:f3}", cellshow1)))
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Red);
                                        }
                                        else
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Black);
                                        }
                                        query.First().Text = string.Format("{0:f3}", cellshow1);
                                    }



                                    query = from tt in celll_textList where tt.Name == "V" + bmuindex + "_" + ((cellnum - 1) * 4 + 2).ToString() select tt;
                                    if (query != null && query.Count() > 0)
                                    {
                                        lastcellshow2 = double.Parse(query.First().Text);
                                        if (lastcellshow2 != double.Parse(string.Format("{0:f3}", cellshow2)))
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Red);
                                        }
                                        else
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Black);
                                        }
                                        query.First().Text = string.Format("{0:f3}", cellshow2);
                                    }



                                    query = from tt in celll_textList where tt.Name == "V" + bmuindex + "_" + ((cellnum - 1) * 4 + 3).ToString() select tt;
                                    if (query != null && query.Count() > 0)
                                    {
                                        lastcellshow3 = double.Parse(query.First().Text);
                                        if (lastcellshow3 != double.Parse(string.Format("{0:f3}", cellshow3)))
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Red);
                                        }
                                        else
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Black);
                                        }
                                        query.First().Text = string.Format("{0:f3}", cellshow3);

                                    }

                                    query = from tt in celll_textList where tt.Name == "V" + bmuindex + "_" + ((cellnum - 1) * 4 + 4).ToString() select tt;
                                    if (query != null && query.Count() > 0)
                                    {
                                        lastcellshow4 = double.Parse(query.First().Text);
                                        if (lastcellshow4 != double.Parse(string.Format("{0:f3}", cellshow4)))
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Red);
                                        }
                                        else
                                        {
                                            query.First().Foreground = new SolidColorBrush(Colors.Black);
                                        }
                                        query.First().Text = string.Format("{0:f3}", cellshow4);
                                    }
                                }
                            }
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// zhengzhonghua 2018-04-28
        /// 电池模块温度BMU_TMP
        /// </summary>
        /// <param name="obj"></param>
        private void receiveTmpInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //单体信息
                Regex rinfo = new Regex(tmpinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //获取BMU从机号
                    string box = canidhex.Substring(6, 2);
                    int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);
                    //add by zhengzhonghua 2018-09-25 增加温度模块16、17、18检测
                    string pf = canidhex.Substring(2, 2);

                    int pfint = Int32.Parse(pf, System.Globalization.NumberStyles.HexNumber);
                    if (pfint >= 16)
                    {
                        //查找界面元素并显示单体数据
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            if (page.grid.Children.Count > 0)
                            {
                                TabControl tc = page.grid.Children[0] as TabControl;
                                foreach (var tabitem in tc.Items)
                                {
                                    TabItem item = tabitem as TabItem;
                                    if ((int)item.Tag == cellbox)
                                    {  //查找对应的BMU从机号
                                        ScrollViewer sc = item.Content as ScrollViewer;
                                        StackPanel parent = sc.Content as StackPanel;
                                        GroupBox tmpgb = parent.Children[2] as GroupBox;
                                        List<TextBox> tmp_textList = MyVisualTreeHelper.FindVisualChild<TextBox>(tmpgb);
                                        for (int t = 0; t < 8; t++)
                                        {
                                            var query = from tt in tmp_textList where tt.Name == "T" + cellbox + "_" + ((pfint - 16) * 8 + t + 1).ToString() select tt;
                                            if (query != null && query.Count() > 0)
                                            {
                                                //logger.Debug("count:" + query.Count() + ":" + query.First().Name + ":" + t + ":" + obj.Data[t]);
                                                lasttempshow = int.Parse(query.First().Text);
                                                if (lasttempshow != (obj.Data[t] - 40))
                                                {
                                                    query.First().Foreground = new SolidColorBrush(Colors.Red);
                                                }
                                                else
                                                {
                                                    query.First().Foreground = new SolidColorBrush(Colors.Black);
                                                }
                                                query.First().Text = (obj.Data[t] - 40).ToString();
                                            }
                                        }
                                        //记录系统日志
                                        //String time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff");
                                        //lock (lockercell)
                                        //{
                                        //    DataRow dr = dt.NewRow();
                                        //    dr[0] = cellbox;
                                        //    dr[61] = obj.Data[0] - 40;
                                        //    dr[62] = obj.Data[1] - 40;
                                        //    dr[63] = time;
                                        //    dt.Rows.Add(dr);

                                        //    string data = export.ExportCSV(dt);
                                        //    sw.Write(data);
                                        //    sw.Flush();
                                        //    dt.Clear();
                                        //}
                                    }

                                }
                            }
                        });

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

        }


        private TextBlock moduleA = null;
        private TextBlock moduleB = null;
        private TextBlock moduleC = null;
        private TextBlock moduleD = null;
        private TextBlock moduleE = null;

        /// <summary>
        /// 电压信号线状态信息
        /// </summary>
        /// <param name="obj"></param>
        private void receiveSignalVolInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //电压信号采集线连接状态
                Regex rinfo = new Regex(signalvolstatus);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //当前BMU从机号
                    string box = canidhex.Substring(6, 2);
                    int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);

                    //解析byte1信号线状态
                    for (int i = 0; i < obj.Data.Count(); i++)
                    {
                        for (int t = 0; t < 8; t++)
                        {
                            int cellnum = i * 8 + (t + 1);
                            
                            if (cellnum>receiveBmuList[cellbox - 1].Cellcouts||cellnum > 60) return; //最多配置到60串
                            int status = (obj.Data[i] >> t) & 0x01;
                            //查找对应界面信号线状态
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                
                                if (page.grid.Children.Count > 0)
                                {
                                    TabControl tc = page.grid.Children[0] as TabControl;
                                    foreach (var tabitem in tc.Items)
                                    {
                                        TabItem item = tabitem as TabItem;
                                        if ((int)item.Tag == cellbox)
                                        { //查找对应的BMU从机号
                                            ScrollViewer sc = item.Content as ScrollViewer;
                                            StackPanel parent = sc.Content as StackPanel;
                                            GroupBox gb = parent.Children[2] as GroupBox;
                                            List<Button> signalVolList = MyVisualTreeHelper.FindVisualChild<Button>(gb);
                                            List<TextBlock> signalVolList2 = MyVisualTreeHelper.FindVisualChild<TextBlock>(gb);
                                            
                                            String moduleCount = "A";
                                            if (cellnum <= receiveBmuList[cellbox - 1].CellmodelAcounts)
                                            {
                                                moduleCount = "A";
                                            }
                                            else if (cellnum <= receiveBmuList[cellbox - 1].CellmodelAcounts + receiveBmuList[cellbox - 1].CellmodelBcounts)
                                            {
                                                moduleCount = "B";
                                            }
                                            else if (cellnum <= receiveBmuList[cellbox - 1].CellmodelAcounts + receiveBmuList[cellbox - 1].CellmodelBcounts + receiveBmuList[cellbox - 1].CellmodelCcounts)
                                            {
                                                moduleCount = "C";
                                            }
                                            else if (cellnum <= receiveBmuList[cellbox - 1].CellmodelAcounts + receiveBmuList[cellbox - 1].CellmodelBcounts + receiveBmuList[cellbox - 1].CellmodelCcounts + receiveBmuList[cellbox - 1].CellmodelDcounts)
                                            {
                                                moduleCount = "D";
                                            }
                                            else if (cellnum <= receiveBmuList[cellbox - 1].CellmodelAcounts + receiveBmuList[cellbox - 1].CellmodelBcounts + receiveBmuList[cellbox - 1].CellmodelCcounts + receiveBmuList[cellbox - 1].CellmodelDcounts + receiveBmuList[cellbox - 1].CellmodelEcounts)

                                            {
                                                moduleCount = "E";
                                            }
                                            else {
                                                return;
                                            }
                                            var query2 = from tt in signalVolList2 where tt.Name == "Module_"+ moduleCount + cellbox select tt;

                                            if (query2 != null && query2.Count() > 0)
                                            {
                                                query2.First().Visibility = Visibility.Visible;
                                            }
                                            //if (moduleA == null)
                                            //{
                                            //   // List<TextBlock> signalVolList2 = MyVisualTreeHelper.FindVisualChild<TextBlock>(gb);
                                                
                                            //    var query2 = from tt in signalVolList2 where tt.Name == "Module_A" + cellbox select tt;
                                            //    if (query2 != null && query2.Count() > 0)
                                            //    {
                                            //        moduleA = query2.First();
                                            //        moduleA.Visibility = Visibility.Visible;
                                            //    }
                                            //}




                                            if (signalVolList != null && signalVolList.Count() > 0)
                                            {
                                                var query = from tt in signalVolList where tt.Name == "SLS" + cellbox + "_" + cellnum.ToString() select tt;
                                                if (query != null && query.Count() > 0)
                                                {
                                                    if (status == 0)
                                                        query.First().SetValue(Button.StyleProperty, Application.Current.Resources["BatteryEnableButton"]);
                                                    else if (status == 1)
                                                        query.First().SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                                                }
                                            }
                                        }
                                    }
                                }

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


        }

        /// <summary>
        /// 温度信号状态
        /// </summary>
        /// <param name="obj"></param>
        private void receiveSignalTmpInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //温度信号采集线连接状态
                Regex rinfo = new Regex(signaltmpstatus);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //当前BMU从机号
                    string box = canidhex.Substring(6, 2);
                    int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);

                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == cellbox)
                                {
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    GroupBox gb = parent.Children[2] as GroupBox;
                                    List<Button> signalTmpList = MyVisualTreeHelper.FindVisualChild<Button>(gb);

                                    //获取温感数目
                                    var query = from tt in receiveBmuList where tt.Bmuindex == cellbox select tt;
                                    if (query != null && query.Count() > 0)
                                    {
                                        int SignalTmpCounts = query.First().Tempcounts;
                                        int tmpnums = SignalTmpCounts / 8; // 20/8=2  
                                        int tmpnumsleft = SignalTmpCounts % 8;// 20%8=4
                                        for (int i = 0; i < tmpnums; i++)
                                        {
                                            for (int t = 0; t < 8; t++)
                                            {
                                                int tmpindex = i * 8 + (t + 1);
                                                int status = (obj.Data[i] >> t) & 0x01;
                                                if (signalTmpList != null && signalTmpList.Count() > 0)
                                                {
                                                    var tmpquery = from tt in signalTmpList where tt.Name == "PLS" + cellbox + "_" + tmpindex.ToString() select tt;
                                                    if (tmpquery != null && tmpquery.Count() > 0)
                                                    {
                                                        if (status == 0)
                                                            tmpquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                        else if (status == 1)
                                                            tmpquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                                    }

                                                }
                                            }
                                        }
                                        //计算剩余温感状态变化
                                        for (int t = 0; t < tmpnumsleft; t++)
                                        {
                                            int status = (obj.Data[tmpnums] >> t) & 0x01;
                                            if (signalTmpList != null && signalTmpList.Count() > 0)
                                            {
                                                var tmpquery = from tt in signalTmpList where tt.Name == "PLS" + cellbox + "_" + (tmpnums * 8 + (t + 1)).ToString() select tt;
                                                if (tmpquery != null && tmpquery.Count() > 0)
                                                {
                                                    if (status == 0)
                                                        tmpquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                    else if (status == 1)
                                                        tmpquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                                }
                                            }
                                        }

                                        //tmos温度信息
                                        for (int t = 0; t < 8; t++)
                                        {
                                            int status = (obj.Data[4] >> t) & 0x01;
                                            var tmosquery = from tt in signalTmpList where tt.Name == "MOS" + cellbox + "_" + (t + 1).ToString() select tt;
                                            if (tmosquery != null && tmosquery.Count() > 0)
                                            {
                                                //TMOS信号线状态
                                                if (status == 0)
                                                    tmosquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                else if (status == 1)
                                                    tmosquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                            }
                                            tmosquery = from tt in signalTmpList where tt.Name == "CPU" + cellbox.ToString() select tt;
                                            if (tmosquery != null && tmosquery.Count() > 0)
                                            {
                                                //TCPU信号线状态
                                                if (status == 0)
                                                    tmosquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                else if (status == 1)
                                                    tmosquery.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }


        /// <summary>
        /// TMOS温度状态信息
        /// </summary>
        /// <param name="obj"></param>
        private void receiveSignalTMOSInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //温度信号采集线连接状态
                Regex rinfo = new Regex(signaltmpstatus);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //当前BMU从机号
                    string box = canidhex.Substring(6, 2);
                    int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);

                    for (int t = 0; t < 7; t++)
                    {
                        int status = (obj.Data[4] >> t) & 0x01;
                        //查找对应界面信号线状态
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            if (page.grid.Children.Count > 0)
                            {
                                TabControl tc = page.grid.Children[0] as TabControl;
                                foreach (var tabitem in tc.Items)
                                {
                                    TabItem item = tabitem as TabItem;
                                    if ((int)item.Tag == cellbox)
                                    { //查找对应的BMU从机号
                                        ScrollViewer sc = item.Content as ScrollViewer;
                                        StackPanel parent = sc.Content as StackPanel;
                                        GroupBox tmpgb = parent.Children[2] as GroupBox;
                                        List<Button> signalTMOSList = MyVisualTreeHelper.FindVisualChild<Button>(tmpgb);

                                        if (signalTMOSList != null && signalTMOSList.Count() > 0)
                                        {
                                            var query = from tt in signalTMOSList where tt.Name == "MOS" + cellbox + "_" + (t + 1).ToString() select tt;
                                            if (query != null && query.Count() > 0)
                                            {
                                                //TMOS信号线状态
                                                if (status == 0)
                                                    query.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                else if (status == 1)
                                                    query.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                            }
                                            query = from tt in signalTMOSList where tt.Name == "CPU" + cellbox.ToString() select tt;
                                            if (query != null && query.Count() > 0)
                                            {
                                                //TCPU信号线状态
                                                if (status == 0)
                                                    query.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpEnableButton"]);
                                                else if (status == 1)
                                                    query.First().SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                            }

                                        }
                                    }

                                }

                            }

                        });
                    }

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


        }


        /// <summary>
        /// zhengzhonghua
        /// 2018-05-02
        /// </summary>
        /// <param name="obj"></param>
        private void receiveTMOSInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //TMOS温度信息
                Regex rinfo = new Regex(tmosinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //获取BMU从机号
                    string box = canidhex.Substring(6, 2);
                    int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == cellbox)
                                {  //查找对应的BMU从机号
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    GroupBox gb = parent.Children[2] as GroupBox;
                                    List<TextBox> tmos_textList = MyVisualTreeHelper.FindVisualChild<TextBox>(gb);
                                    if (tmos_textList != null && tmos_textList.Count() > 0)
                                    {
                                        for (int t = 0; t < 6; t++)
                                        {
                                            var query = from tt in tmos_textList where tt.Name == "TM" + cellbox + "_" + (t + 1).ToString() select tt;
                                            if (query != null && query.Count() > 0)
                                            {
                                                lasttmosshow = int.Parse(query.First().Text);
                                                if (lasttmosshow != obj.Data[t] - 40)
                                                {
                                                    query.First().Foreground = new SolidColorBrush(Colors.Red);
                                                }
                                                else
                                                {
                                                    query.First().Foreground = new SolidColorBrush(Colors.Black);
                                                }
                                                query.First().Text = (obj.Data[t] - 40).ToString();
                                            }


                                        }
                                        var tcquery = from tt in tmos_textList where tt.Name == "TC" + cellbox.ToString() select tt;
                                        if (tcquery != null && tcquery.Count() > 0)
                                        {
                                            lasttcshow = int.Parse(tcquery.First().Text);
                                            if (lasttcshow != obj.Data[6] - 40)
                                            {
                                                tcquery.First().Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                            else
                                            {
                                                tcquery.First().Foreground = new SolidColorBrush(Colors.Black);
                                            }
                                            tcquery.First().Text = (obj.Data[6] - 40).ToString();
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

        }

        //接收从机被动均衡控制
        private void receiveSlavePassiveEqu(CANSDK.VCI_CAN_OBJ obj)
        {
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
            if (new Regex(SLAVEPE).IsMatch(canidhex))
            {
                Console.WriteLine("接收从机被动均衡控制：" + DataConverter.byteToHexStrForData(obj.Data));
               // byte[] data = obj.Data;
               // //第一个字节data[0]
               // for (int i = 4; i < 8; i++) { 
               //     bool b=(data[0]>>(4+i)&1)==0?false:true;
               //     if (Spe.SlavePECheck[i - 4] == b)
               //     {
               //         String labelName = "slavePE_label" + (i - 3);
               //         Label l = (Label)page.FindName(labelName);
               //         l.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
               //     }
               //     else {
               //         Label l = (Label)page.FindName("slavePE_label" + (i - 3));
               //         l.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
               //     }
               // }
               // for (int i = 4; i < 60; i++) { 
               //// bool b=(data[(i+4)/8]>>)
               // }
            }
        }

        //接收铁电数据
        private void receiveTiedianInfo(CANSDK.VCI_CAN_OBJ obj)
        { 
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
            if (new Regex(TIEDIANHS).IsMatch(canidhex)) {
            //收到铁电握手数回送据
                isTiedianHandShake = false;
            }
            else if (new Regex(TIEDIANDATA).IsMatch(canidhex))
            {
                //收到铁电数据
            
            }
        }

        private void receiveDiaInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
            //BMU_CFG配置信息
            Regex dia = new Regex(DIA);
            if (dia.IsMatch(canidhex) && obj.RemoteFlag == 0)
            {
                Console.WriteLine("接收到dia数据，id：" + canidhex);
                string temp = canidhex.Substring(3, 1);
                isWaitting["dia" + temp] = 0;

                if (DiagnoseEvent != null)
                {
                    ReadCfgArgs args = new ReadCfgArgs();
                    args.Args = obj;
                    DiagnoseEvent(null, args);
                }
                diaEvent.Set();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// zhengzhonghua
        /// 读取BMU配置信息
        /// 2018-05-03 08:30
        /// </summary>
        /// <param name="obj"></param>
        private void receiveBmucfgInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                //BMU_CFG配置信息
                Regex rinfo = new Regex(bmucfginfo);
                //接受配置信息中模块A、B、C数量
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    //接受到bmu配置信息，将obj数据放入线程池中处理
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessBMU), obj);

                    //计算累加校验和
                    if (obj.Data[7] == checkSum(obj.Data))
                    {
                        //消息通知发送部分
                        isReadCfgSendReapeat = false;
                        string box = canidhex.Substring(6, 2);
                        int cellbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);
                        if (ReadCfgEvent != null)
                        {
                            ReadCfgArgs args = new ReadCfgArgs();
                            args.Args = obj;
                            ReadCfgEvent(null, args);
                        }
                    }
                    else
                    {
                        isReadCfgSendReapeat = true;
                    }
                    autoResetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

        }

        /// <summary>
        /// 线程池集中处理配置报文信息
        /// 2018-11-15
        /// add by zhengzhonghua
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessBMU(object obj)
        {
            CANSDK.VCI_CAN_OBJ data = (CANSDK.VCI_CAN_OBJ)obj;
            uint canid = data.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
            string bmubox = canidhex.Substring(6, 2);
            int bmunum = Int32.Parse(bmubox, System.Globalization.NumberStyles.HexNumber);
            //数据校验
            if (data.Data[7] == checkSum(data.Data))
            {
                //打印配置文件数据帧
                logger.Info("获取配置信息数据帧:0x" + canidhex + " " + HexCommon.byteToHexStrSpace(data.Data));
                byte id = data.Data[0];
                if (id == 0x51)
                {
                    //申请写锁
                    rwlock.AcquireWriterLock(Timeout.Infinite);
                    int sn = (int)(data.Data[1] + ResolutionRatioModel.slaveNum_offset);
                    int cmmcn = (int)(data.Data[3] + ResolutionRatioModel.childModuleMonCellNumber_offset);
                    int cmmtn = (int)(data.Data[4] + ResolutionRatioModel.childMonModuleTemperatureNumber_offset);
                    int mamcn = (int)(data.Data[5] + ResolutionRatioModel.moduleAMonCellNum_offset);
                    var query = from tt in receiveBmuList where tt.Bmuindex == bmunum select tt;
                    if (query != null && query.Count() == 0)
                    {
                        BMUConfigModel model = new BMUConfigModel();
                        model.Bmuindex = sn;
                        model.Cellcouts = cmmcn;
                        model.Tempcounts = cmmtn;
                        model.CellmodelAcounts = mamcn;
                        receiveBmuList.Add(model);
                    }
                    logger.Debug("0x51:" + bmunum);
                    //释放写锁
                    rwlock.ReleaseWriterLock();

                }

                else if (id == 0x52)
                {
                    //申请写锁
                    rwlock.AcquireWriterLock(Timeout.Infinite);
                    int mbcn = (int)(data.Data[1] + ResolutionRatioModel.moduleBMonCellNum_offset);
                    int mccn = (int)(data.Data[3] + ResolutionRatioModel.moduleCMonCellNum_offset);
                    int mdcn = (int)(data.Data[5] + ResolutionRatioModel.moduleDMonCellNum_offset);
                    var query = from tt in receiveBmuList where tt.Bmuindex == bmunum select tt;
                    if (query != null && query.Count() == 0)
                    {
                        BMUConfigModel model = new BMUConfigModel();
                        model.Bmuindex = bmunum;
                        model.CellmodelBcounts = mbcn;
                        model.CellmodelCcounts = mccn;
                        model.CellmodelDcounts = mdcn;
                        receiveBmuList.Add(model);
                    }
                    else if (query != null && query.Count() > 0)
                    {
                        BMUConfigModel model = query.First();
                        model.CellmodelBcounts = mbcn;
                        model.CellmodelCcounts = mccn;
                        model.CellmodelDcounts = mdcn;
                    }
                    logger.Debug("0x52:" + bmunum);
                    //释放写锁
                    rwlock.ReleaseWriterLock();

                }
                else if (id == 0x53)
                {
                    //申请写锁
                    rwlock.AcquireWriterLock(Timeout.Infinite);
                    int mecn = (int)(data.Data[1] + ResolutionRatioModel.moduleBMonCellNum_offset);
                    //int mccn = (int)(data.Data[3] + ResolutionRatioModel.moduleCMonCellNum_offset);
                    //int mdcn = (int)(data.Data[5] + ResolutionRatioModel.moduleDMonCellNum_offset);
                    var query = from tt in receiveBmuList where tt.Bmuindex == bmunum select tt;
                    if (query != null && query.Count() == 0)
                    {
                        BMUConfigModel model = new BMUConfigModel();
                        model.Bmuindex = bmunum;
                        model.CellmodelEcounts = mecn;
                        //model.CellmodelCcounts = mccn;
                        //model.CellmodelDcounts = mdcn;
                        receiveBmuList.Add(model);
                    }
                    else if (query != null && query.Count() > 0)
                    {
                        BMUConfigModel model = query.First();
                        model.CellmodelEcounts = mecn;
                        //model.CellmodelCcounts = mccn;
                        //model.CellmodelDcounts = mdcn;
                    }
                    logger.Debug("0x53:" + bmunum);
                    //释放写锁
                    rwlock.ReleaseWriterLock();
                }
            }

        }


        /// <summary>
        /// 创建界面控件元素
        /// </summary>
        private void CreateElement()
        {
            if (!isCreateBmuFlag)
            {
                moduleA = null;
                moduleB = null;
                moduleC = null;
                moduleD = null;
                moduleE = null;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {


                    TabControl tc = new TabControl();
                    //对receiveBmuList进行数据排序
                    receiveBmuList = (from s in receiveBmuList orderby s.Bmuindex ascending select s).ToList();
                    for (int a = 0; a < receiveBmuList.Count; a++)
                    {
                        TabItem ti = new TabItem();
                        ti.SetValue(TabItem.StyleProperty, Application.Current.Resources["ModelTabItemStyle"]);
                        ti.Header = "BMU#" + receiveBmuList[a].Bmuindex;
                        ti.Tag = receiveBmuList[a].Bmuindex;//用于查找对应的BMU索引值
                        ScrollViewer sc = new ScrollViewer();

                                                
                        StackPanel rootsp = new StackPanel();
                       
                        //创建BMU总信息bmuspsub1
                        GroupBox bmugb = new GroupBox();
                        bmugb.Header = "总体信息";
                        bmugb.Margin = new Thickness(0, 0, 0, 5);
                        Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                        bmugb.BorderBrush = new SolidColorBrush(color);
                        bmugb.BorderThickness = new Thickness(1);

                        StackPanel bmusp = new StackPanel();

                        StackPanel bmuspsub1 = new StackPanel();
                        bmuspsub1.Orientation = Orientation.Horizontal;
                        bmuspsub1.Margin = new Thickness(0, 10, 0, 5);
                        bmusp.Children.Add(bmuspsub1);
                        bmugb.Content = bmusp;
                        rootsp.Children.Add(bmugb);


                        //从机心跳值
                        TextBlock slavetb = new TextBlock();
                        slavetb.Text = "从机心跳值：";
                        slavetb.Text = (string)page.Resources["bmuheart"];
                        slavetb.Width = 160;
                        //slavetb.Width = 120;
                        slavetb.TextAlignment = TextAlignment.Right;
                        slavetb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub1.Children.Add(slavetb);
                        TextBox slavetx = new TextBox();
                        slavetx.Text = "0";
                        slavetx.Margin = new Thickness(2, 0, 0, 0);
                        slavetx.Name = "slavetx_" + receiveBmuList[a].Bmuindex.ToString();
                        slavetx.Width = 80;
                        slavetx.IsReadOnly = true;
                        bmuspsub1.Children.Add(slavetx);

                        //电池数目
                        TextBlock cellcountstb = new TextBlock();
                        cellcountstb.Text = "电池数目：";
                        cellcountstb.Text = (string)page.Resources["batnum"];
                        cellcountstb.Width = 160;
                        //cellcountstb.Width = 120;
                        cellcountstb.TextAlignment = TextAlignment.Right;
                        cellcountstb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub1.Children.Add(cellcountstb);
                        TextBox cellcountstx = new TextBox();
                        cellcountstx.Margin = new Thickness(2, 0, 0, 0);
                        cellcountstx.Text = "0";
                        cellcountstx.Name = "cellcountstx_" + receiveBmuList[a].Bmuindex.ToString();
                        cellcountstx.Width = 80;
                        cellcountstx.IsReadOnly = true;
                        bmuspsub1.Children.Add(cellcountstx);

                        //温度数目
                        TextBlock tempcountstb = new TextBlock();
                        tempcountstb.Text = "温度数目：";
                        tempcountstb.Text = (string)page.Resources["wgnum"];
                        tempcountstb.Width = 160;
                        //tempcountstb.Width = 120;
                        tempcountstb.TextAlignment = TextAlignment.Right;
                        tempcountstb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub1.Children.Add(tempcountstb);
                        TextBox tempcountstx = new TextBox();
                        tempcountstx.Margin = new Thickness(2, 0, 0, 0);
                        tempcountstx.Text = "0";
                        tempcountstx.Name = "tempcountstx_" + receiveBmuList[a].Bmuindex.ToString();
                        tempcountstx.Width = 80;
                        tempcountstx.IsReadOnly = true;
                        bmuspsub1.Children.Add(tempcountstx);

                        //总电压
                        TextBlock totalvoltb = new TextBlock();
                        totalvoltb.Text = "总电压：";
                        totalvoltb.Text = (string)page.Resources["volsum"];
                        totalvoltb.Width = 160;
                        //totalvoltb.Width = 120;
                        totalvoltb.TextAlignment = TextAlignment.Right;
                        totalvoltb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub1.Children.Add(totalvoltb);
                        TextBox totalvoltbtx = new TextBox();
                        totalvoltbtx.Margin = new Thickness(2, 0, 0, 0);
                        totalvoltbtx.Text = "0";
                        totalvoltbtx.Name = "totalvoltbtx_" + receiveBmuList[a].Bmuindex.ToString();
                        totalvoltbtx.Width = 80;
                        totalvoltbtx.IsReadOnly = true;
                        bmuspsub1.Children.Add(totalvoltbtx);

                        //电池状态
                        TextBlock batterytb = new TextBlock();
                        batterytb.Text = "电池状态：";
                        batterytb.Text = (string)page.Resources["batstatus"];
                        batterytb.Width = 160;
                        //batterytb.Width = 120;
                        batterytb.TextAlignment = TextAlignment.Right;
                        batterytb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub1.Children.Add(batterytb);

                        ComboBox batterycb = new ComboBox();
                        batterycb.Margin = new Thickness(2, 0, 0, 0);
                        batterycb.Name = "batterycb_" + receiveBmuList[a].Bmuindex.ToString();
                        batterycb.Width = 160;
                        //batterycb.Width = 120;
                        batterycb.IsEnabled = false;
                        bmuspsub1.Children.Add(batterycb);
                        //batterycb.Items.Add(new ComboBoxItem { Content = "电池电压正常" });
                        batterycb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["batvolnor"] });
                        //batterycb.Items.Add(new ComboBoxItem { Content = "单体一致性差" });
                        batterycb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["cellyizhicha"] });
                        //batterycb.Items.Add(new ComboBoxItem { Content = "电池过压" });
                        batterycb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["batovervol"] });
                        //batterycb.Items.Add(new ComboBoxItem { Content = "电池欠压" });
                        batterycb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["batundervol"] });
                        batterycb.SelectedIndex = 0;

                        //创建BMU总信息bmuspsub2
                        StackPanel bmuspsub2 = new StackPanel();
                        bmuspsub2.Margin = new Thickness(0, 0, 0, 5);
                        bmuspsub2.Orientation = Orientation.Horizontal;
                        bmusp.Children.Add(bmuspsub2);



                        //最高温度
                        TextBlock tmphightb = new TextBlock();
                        tmphightb.Text = "最高温度：";
                        tmphightb.Text = (string)page.Resources["maxtem"];
                        tmphightb.Width = 160;
                        //tmphightb.Width = 120;
                        tmphightb.TextAlignment = TextAlignment.Right;
                        tmphightb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub2.Children.Add(tmphightb);

                        TextBox tmphightx = new TextBox();
                        tmphightx.Margin = new Thickness(2, 0, 0, 0);
                        tmphightx.Text = "0";
                        tmphightx.Name = "tmphightx_" + receiveBmuList[a].Bmuindex.ToString();
                        tmphightx.Width = 80;
                        tmphightx.IsReadOnly = true;
                        bmuspsub2.Children.Add(tmphightx);

                        //最高温度序号
                        TextBlock tmphighnumtb = new TextBlock();
                        tmphighnumtb.Text = "最高温度序号：";
                        tmphighnumtb.Text = (string)page.Resources["maxtemnum"];
                        tmphighnumtb.Width = 160;
                        //tmphighnumtb.Width = 120;
                        tmphighnumtb.TextAlignment = TextAlignment.Right;
                        tmphighnumtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub2.Children.Add(tmphighnumtb);

                        TextBox tmphighnumtx = new TextBox();
                        tmphighnumtx.Margin = new Thickness(2, 0, 0, 0);
                        tmphighnumtx.Text = "0";
                        tmphighnumtx.Name = "tmphighnumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        tmphighnumtx.Width = 80;
                        tmphighnumtx.IsReadOnly = true;
                        bmuspsub2.Children.Add(tmphighnumtx);


                        //最低温度
                        TextBlock tmplowtb = new TextBlock();
                        tmplowtb.Text = "最低温度：";
                        tmplowtb.Text = (string)page.Resources["mintem"];
                        tmplowtb.Width = 160;
                        //tmplowtb.Width = 120;
                        tmplowtb.TextAlignment = TextAlignment.Right;
                        tmplowtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub2.Children.Add(tmplowtb);

                        TextBox tmplowtx = new TextBox();
                        tmplowtx.Margin = new Thickness(2, 0, 0, 0);
                        tmplowtx.Text = "0";
                        tmplowtx.Name = "tmplowtx_" + receiveBmuList[a].Bmuindex.ToString();
                        tmplowtx.Width = 80;
                        tmplowtx.IsReadOnly = true;
                        bmuspsub2.Children.Add(tmplowtx);


                        //最低温度序号
                        TextBlock tmplownumtb = new TextBlock();
                        tmplownumtb.Text = "最低温度序号：";
                        tmplownumtb.Text = (string)page.Resources["mintemnum"];
                        tmplownumtb.Width = 160;
                        //tmplownumtb.Width = 120;
                        tmplownumtb.TextAlignment = TextAlignment.Right;
                        tmplownumtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub2.Children.Add(tmplownumtb);

                        TextBox tmplownumtx = new TextBox();
                        tmplownumtx.Margin = new Thickness(2, 0, 0, 0);
                        tmplownumtx.Text = "0";
                        tmplownumtx.Name = "tmplownumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        tmplownumtx.Width = 80;
                        tmplownumtx.IsReadOnly = true;
                        bmuspsub2.Children.Add(tmplownumtx);


                        //创建BMU总信息bmuspsub4
                        StackPanel bmuspsub4 = new StackPanel();
                        bmuspsub4.Margin = new Thickness(0, 0, 0, 5);
                        bmuspsub4.Orientation = Orientation.Horizontal;
                        bmusp.Children.Add(bmuspsub4);

                        //最高单体
                        TextBlock cellhightb = new TextBlock();
                        cellhightb.Text = "最高单体：";
                        cellhightb.Text = (string)page.Resources["maxcell"];
                        cellhightb.Width = 160;
                        //cellhightb.Width = 120;
                        cellhightb.TextAlignment = TextAlignment.Right;
                        cellhightb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub4.Children.Add(cellhightb);

                        TextBox cellhightx = new TextBox();
                        cellhightx.Margin = new Thickness(2, 0, 0, 0);
                        cellhightx.Text = "0";
                        cellhightx.Name = "cellhightx_" + receiveBmuList[a].Bmuindex.ToString();
                        cellhightx.Width = 80;
                        cellhightx.IsReadOnly = true;
                        bmuspsub4.Children.Add(cellhightx);

                        //最高单体序号
                        TextBlock cellhighnumtb = new TextBlock();
                        cellhighnumtb.Text = "最高单体序号：";
                        cellhighnumtb.Text = (string)page.Resources["maxcellnum"];
                        cellhighnumtb.Width = 160;
                        //cellhighnumtb.Width = 120;
                        cellhighnumtb.TextAlignment = TextAlignment.Right;
                        cellhighnumtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub4.Children.Add(cellhighnumtb);
                        TextBox cellhighnumtx = new TextBox();
                        cellhighnumtx.Margin = new Thickness(2, 0, 0, 0);
                        cellhighnumtx.Text = "0";
                        cellhighnumtx.Name = "cellhighnumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        cellhighnumtx.Width = 80;
                        cellhighnumtx.IsReadOnly = true;
                        bmuspsub4.Children.Add(cellhighnumtx);

                        //最低单体
                        TextBlock celllowtb = new TextBlock();
                        celllowtb.Text = "最低单体：";
                        celllowtb.Text = (string)page.Resources["mincell"];
                        celllowtb.Width = 160;
                        //celllowtb.Width = 120;
                        celllowtb.TextAlignment = TextAlignment.Right;
                        celllowtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub4.Children.Add(celllowtb);
                        TextBox celllowtx = new TextBox();
                        celllowtx.Margin = new Thickness(2, 0, 0, 0);
                        celllowtx.Text = "0";
                        celllowtx.Name = "celllowtx_" + receiveBmuList[a].Bmuindex.ToString();
                        celllowtx.Width = 80;
                        celllowtx.IsReadOnly = true;
                        bmuspsub4.Children.Add(celllowtx);

                        //最低单体序号
                        TextBlock celllownumtb = new TextBlock();
                        celllownumtb.Text = "最低单体序号：";
                        celllownumtb.Text = (string)page.Resources["mincellnum"];
                        celllownumtb.Width = 160;
                        //celllownumtb.Width = 120;
                        celllownumtb.TextAlignment = TextAlignment.Right;
                        celllownumtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub4.Children.Add(celllownumtb);
                        TextBox celllownumtx = new TextBox();
                        celllownumtx.Margin = new Thickness(2, 0, 0, 0);
                        celllownumtx.Text = "0";
                        celllownumtx.Name = "celllownumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        celllownumtx.Width = 80;
                        celllownumtx.IsReadOnly = true;
                        bmuspsub4.Children.Add(celllownumtx);


                        //创建BMU总信息bmuspsub5
                        //StackPanel bmuspsub5 = new StackPanel();
                        //bmuspsub5.Margin = new Thickness(0, 0, 0, 5);
                        //bmuspsub5.Orientation = Orientation.Horizontal;
                        //bmusp.Children.Add(bmuspsub5);



                        //创建BMU总信息bmuspsub6
                        StackPanel bmuspsub6 = new StackPanel();
                        bmuspsub6.Margin = new Thickness(0, 0, 0, 5);
                        bmuspsub6.Orientation = Orientation.Horizontal;
                        bmusp.Children.Add(bmuspsub6);


                        //平均电压
                        TextBlock cellavgtb = new TextBlock();
                        cellavgtb.Text = "平均电压：";
                        cellavgtb.Text = (string)page.Resources["avevol"];
                        cellavgtb.Width = 160;
                        //cellavgtb.Width = 120;
                        cellavgtb.TextAlignment = TextAlignment.Right;
                        cellavgtb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub6.Children.Add(cellavgtb);
                        TextBox cellavgtx = new TextBox();
                        cellavgtx.Margin = new Thickness(2, 0, 0, 0);
                        cellavgtx.Text = "0";
                        cellavgtx.Name = "cellavgtx_" + receiveBmuList[a].Bmuindex.ToString();
                        cellavgtx.Width = 80;
                        cellavgtx.IsReadOnly = true;
                        bmuspsub6.Children.Add(cellavgtx);


                        //创建BMU总信息bmuspsub3
                        StackPanel bmuspsub3 = new StackPanel();
                        bmuspsub3.Margin = new Thickness(0, 0, 0, 5);
                        bmuspsub3.Orientation = Orientation.Horizontal;
                        bmusp.Children.Add(bmuspsub3);

                        //温度状态
                        TextBlock tmpstatustb = new TextBlock();
                        tmpstatustb.Text = "温度状态：";
                        tmpstatustb.Text = (string)page.Resources["temstatus"];
                        tmpstatustb.Width = 160;
                        //tmpstatustb.Width = 120;
                        tmpstatustb.TextAlignment = TextAlignment.Right;
                        tmpstatustb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub3.Children.Add(tmpstatustb);

                        Button tmpbtn = new Button();
                        tmpbtn.Name = "tmpbtn_" + receiveBmuList[a].Bmuindex.ToString();
                        tmpbtn.Margin = new Thickness(5, 0, 55, 0);
                        tmpbtn.SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                        bmuspsub3.Children.Add(tmpbtn);


                        //BMU硬件状态
                        TextBlock hardwarestatustb = new TextBlock();
                        hardwarestatustb.Text = "硬件状态：";
                        tmpstatustb.Text = (string)page.Resources["hardwarestatus"];
                        hardwarestatustb.Width = 160;
                        //hardwarestatustb.Width = 120;
                        hardwarestatustb.TextAlignment = TextAlignment.Right;
                        hardwarestatustb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub3.Children.Add(hardwarestatustb);

                        Button hardwarestatusbtn = new Button();
                        hardwarestatusbtn.Name = "hardwarestatusbtn_" + receiveBmuList[a].Bmuindex.ToString();
                        hardwarestatusbtn.Margin = new Thickness(5, 0, 55, 0);
                        hardwarestatusbtn.SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                        bmuspsub3.Children.Add(hardwarestatusbtn);


                        //信号线状态
                        TextBlock signalstatustb = new TextBlock();
                        signalstatustb.Text = "信号线状态：";
                        signalstatustb.Text = (string)page.Resources["xhxstatus"];
                        signalstatustb.Width = 160;
                        //signalstatustb.Width = 120;
                        signalstatustb.TextAlignment = TextAlignment.Right;
                        signalstatustb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub3.Children.Add(signalstatustb);

                        Button signalstatusbtn = new Button();
                        signalstatusbtn.Name = "signalstatusbtn_" + receiveBmuList[a].Bmuindex.ToString();
                        signalstatusbtn.Margin = new Thickness(5, 0, 55, 0);
                        signalstatusbtn.SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                        bmuspsub3.Children.Add(signalstatusbtn);

                        //均衡状态
                        TextBlock balancestatustb = new TextBlock();
                        balancestatustb.Text = "均衡状态：";
                        balancestatustb.Text = (string)page.Resources["jhstatus"];
                        balancestatustb.Width = 160;
                        //balancestatustb.Width = 120;
                        balancestatustb.TextAlignment = TextAlignment.Right;
                        balancestatustb.VerticalAlignment = VerticalAlignment.Center;
                        bmuspsub3.Children.Add(balancestatustb);

                        Button balancestatusbtn = new Button();
                        balancestatusbtn.Name = "balancestatusbtn_" + receiveBmuList[a].Bmuindex.ToString();
                        balancestatusbtn.Margin = new Thickness(5, 0, 55, 0);
                        balancestatusbtn.SetValue(Button.StyleProperty, Application.Current.Resources["SignalDisableButton"]);
                        bmuspsub3.Children.Add(balancestatusbtn);

                        //add by zhengzhonghua 2018-10-07 添加自动均衡和手动均衡模块
                        TabControl balancetc = new TabControl();
                        balancetc.Height = 350;
                        //自动均衡模块1-3
                        GroupBox activem1gb = new GroupBox();
                        activem1gb.Margin = new Thickness(0, 0, 0, 5);
                        activem1gb.BorderBrush = new SolidColorBrush(color);
                        activem1gb.BorderThickness = new Thickness(1);
                        StackPanel activesp = new StackPanel();
                        activem1gb.Content = activesp;

                        //自动均衡
                        TabItem balanceauto = new TabItem();
                        balanceauto.SetValue(TabItem.StyleProperty, Application.Current.Resources["ModelTabItemStyle"]);
                        balanceauto.Header = "自动均衡";
                        balanceauto.Header = (string)page.Resources["autojh"];
                        balanceauto.Content = activem1gb;
                        balancetc.Items.Add(balanceauto);

                        //手动均衡groupbox
                        //ScrollViewer handsc = new ScrollViewer();
                        //handsc.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        GroupBox handgb = new GroupBox();
                        handgb.Margin = new Thickness(0, 0, 0, 5);
                        handgb.BorderBrush = new SolidColorBrush(color);
                        handgb.BorderThickness = new Thickness(1);
                        StackPanel handsp = new StackPanel();
                        handgb.Content = handsp;
                        //handsc.Content = handgb;

                        //手动均衡
                        TabItem balancehand = new TabItem();
                        balancehand.SetValue(TabItem.StyleProperty, Application.Current.Resources["ModelTabItemStyle"]);
                        balancehand.Header = "手动均衡";
                        balancehand.Header = (string)page.Resources["manualjh"];
                        balancehand.Content = handgb;
                        balancetc.Items.Add(balancehand);

                        //创建A、B、C
                        StackPanel handsub1 = new StackPanel();
                        handsub1.Orientation = Orientation.Horizontal;
                        handsub1.Margin = new Thickness(0, 5, 0, 5);
                        handsp.Children.Add(handsub1);

                        Button sendHandBtn = new Button();
                        sendHandBtn.Name = "Hand_" + receiveBmuList[a].Bmuindex;
                        sendHandBtn.Content = "开始发送";
                        sendHandBtn.SetResourceReference(ContentControl.ContentProperty, "startsent");

                        sendHandBtn.Tag = ti;

                        sendHandBtn.Click += new RoutedEventHandler(delegate(object obj, RoutedEventArgs r)
                        {
                            //Thread sendBalanceThread = null;
                            Button b = r.OriginalSource as Button;
                            //if (b.Content.Equals("开始发送"))
                            if (b.Content.Equals((string)page.Resources["startsent"]))
                            {
                                b.Content = "停止发送";
                                b.SetResourceReference(ContentControl.ContentProperty, "stopsent");
                                sendBalanceThread = new Thread(new ParameterizedThreadStart(runSendBalanceThread));
                                sendBalanceThread.IsBackground = true;
                                sendBalanceThread.Start(ti);
                            }
                            else
                            {
                                b.Content = "开始发送";
                                b.SetResourceReference(ContentControl.ContentProperty, "startsent");
                                if (sendBalanceThread != null)
                                {
                                    sendBalanceThread.Abort();
                                    sendBalanceThread = null;
                                }

                            }

                        });
                        handsub1.Children.Add(sendHandBtn);

                        //BMU工作模式
                        TextBlock handbmuworkerstatustb = new TextBlock();
                        handbmuworkerstatustb.Text = "BMU模式：";
                        handbmuworkerstatustb.Text = (string)page.Resources["bmustatus"];
                        handbmuworkerstatustb.Width = 140;
                        //handbmuworkerstatustb.Width = 100;
                        handbmuworkerstatustb.TextAlignment = TextAlignment.Right;
                        handbmuworkerstatustb.VerticalAlignment = VerticalAlignment.Center;
                        handsub1.Children.Add(handbmuworkerstatustb);

                        ComboBox handbmuworkerstatuscb = new ComboBox();
                        handbmuworkerstatuscb.Margin = new Thickness(2, 0, 0, 0);
                        handbmuworkerstatuscb.Width = 170;
                        //handbmuworkerstatuscb.Width = 130;
                        handbmuworkerstatuscb.VerticalAlignment = VerticalAlignment.Center;
                        handbmuworkerstatuscb.Name = "handbmuworkerstatuscb_" + receiveBmuList[a].Bmuindex.ToString();
                        //handbmuworkerstatuscb.Items.Add(new ComboBoxItem { Content = "退出手动均衡" });
                        handbmuworkerstatuscb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["quitmanual"] });
                        //handbmuworkerstatuscb.Items.Add(new ComboBoxItem { Content = "进入手动均衡" });
                        handbmuworkerstatuscb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["inmanual"] });
                        handbmuworkerstatuscb.SelectedIndex = 1;
                        handsub1.Children.Add(handbmuworkerstatuscb);

                        //整车工作模式
                        TextBlock handcarworkerstatustb = new TextBlock();
                        handcarworkerstatustb.Text = "整车模式：";
                        handcarworkerstatustb.Text = (string)page.Resources["zcmode"];
                        handcarworkerstatustb.Width = 140;
                        //handcarworkerstatustb.Width = 100;
                        handcarworkerstatustb.TextAlignment = TextAlignment.Right;
                        handcarworkerstatustb.VerticalAlignment = VerticalAlignment.Center;
                        handsub1.Children.Add(handcarworkerstatustb);

                        ComboBox handcarworkerstatuscb = new ComboBox();
                        handcarworkerstatuscb.Margin = new Thickness(2, 0, 0, 0);
                        handcarworkerstatuscb.Width = 100;
                        //handcarworkerstatuscb.Width = 60;
                        handcarworkerstatuscb.VerticalAlignment = VerticalAlignment.Center;
                        handcarworkerstatuscb.Name = "handcarworkerstatuscb_" + receiveBmuList[a].Bmuindex.ToString();
                        //handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = "静置" });
                        handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["jingzhi"] });
                        //handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = "放电" });
                        handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["discharge"] });
                        //handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = "充电" });
                        handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["charge"] });
                        //handcarworkerstatuscb.Items.Add(new ComboBoxItem { Content = "保留" });
                        handcarworkerstatuscb.SelectedIndex = 0;
                        handsub1.Children.Add(handcarworkerstatuscb);



                        if (receiveBmuList[a].CellmodelAcounts > 0 && receiveBmuList[a].CellmodelAcounts < 255)
                        {
                            //存放模块A所在行
                            StackPanel handsub2 = new StackPanel();
                            handsub2.Orientation = Orientation.Horizontal;
                            handsub2.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub2);

                            TextBlock handmodelAcurrentb = new TextBlock();
                           // handmodelAcurrentb.Text = "A均衡电流：";
                            handmodelAcurrentb.Text = (string)page.Resources["ajhcur"];
                            //handmodelAcurrentb.Width = 80;
                            handmodelAcurrentb.Width = 140;
                            handmodelAcurrentb.TextAlignment = TextAlignment.Left;
                            handmodelAcurrentb.VerticalAlignment = VerticalAlignment.Center;
                            handsub2.Children.Add(handmodelAcurrentb);

                            TextBox handmodelAcurrentx = new TextBox();
                            handmodelAcurrentx.Margin = new Thickness(2, 0, 0, 0);
                            handmodelAcurrentx.Name = "handmodelAcurrentx_" + receiveBmuList[a].Bmuindex.ToString();
                            handmodelAcurrentx.Width = 60;
                            handmodelAcurrentx.Text = "0";
                            handsub2.Children.Add(handmodelAcurrentx);

                            TextBlock handmodelAbalancetb = new TextBlock();
                           // handmodelAbalancetb.Text = "A均衡状态：";
                            handmodelAbalancetb.Text = (string)page.Resources["ajhstatus"];
                            handmodelAbalancetb.Margin = new Thickness(5, 0, 0, 0);
                            handmodelAbalancetb.Width = 140;
                            //handmodelAbalancetb.Width = 80;
                            handmodelAbalancetb.TextAlignment = TextAlignment.Left;
                            handmodelAbalancetb.VerticalAlignment = VerticalAlignment.Center;
                            handsub2.Children.Add(handmodelAbalancetb);

                            //均衡状态
                            ComboBox handmodelAbalancecb = new ComboBox();
                            handmodelAbalancecb.Margin = new Thickness(2, 0, 0, 0);
                            handmodelAbalancecb.Name = "handmodelAbalancecb_" + receiveBmuList[a].Bmuindex.ToString();
                            handmodelAbalancecb.Width = 140;
                            //handmodelAbalancecb.Width = 60;
                            //handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = "不动作" });
                            handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["nodz"] });
                            //handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = "放电" });
                            handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["discharge"] });
                            handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["charge"] });
                            //handmodelAbalancecb.Items.Add(new ComboBoxItem { Content = "保留" });
                            handmodelAbalancecb.SelectedIndex = 0;
                            handsub2.Children.Add(handmodelAbalancecb);

                            StackPanel handsub33 = new StackPanel();
                            handsub33.Orientation = Orientation.Horizontal;
                            handsub33.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub33);

                            for (int i = 0; i < receiveBmuList[a].CellmodelAcounts; i++)
                            {
                                RadioButton rb = new RadioButton();
                                rb.Name = "modelA_" + receiveBmuList[a].Bmuindex.ToString() + "_" + (i + 1).ToString();
                                //rb.Content = "单体" + (i + 1).ToString();
                                rb.Content = (string)page.Resources["cell"] + (i + 1).ToString();
                                rb.Margin = new Thickness(10, 0, 0, 0);
                                handsub33.Children.Add(rb);
                            }

                        }


                        if (receiveBmuList[a].CellmodelBcounts > 0 && receiveBmuList[a].CellmodelBcounts < 255)
                        {
                            //存放模块B所在行
                            StackPanel handsub3 = new StackPanel();
                            handsub3.Orientation = Orientation.Horizontal;
                            handsub3.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub3);

                            TextBlock handmodelBcurrentb = new TextBlock();
                           // handmodelBcurrentb.Text = "B均衡电流：";
                            handmodelBcurrentb.Text = (string)page.Resources["bjhcur"];

                            handmodelBcurrentb.Width = 140;
                            handmodelBcurrentb.TextAlignment = TextAlignment.Left;
                            handmodelBcurrentb.VerticalAlignment = VerticalAlignment.Center;
                            handsub3.Children.Add(handmodelBcurrentb);

                            TextBox handmodelBcurrentx = new TextBox();
                            handmodelBcurrentx.Margin = new Thickness(2, 0, 0, 0);
                            handmodelBcurrentx.Name = "handmodelBcurrentx_" + receiveBmuList[a].Bmuindex.ToString();
                            handmodelBcurrentx.Width = 60;
                            handmodelBcurrentx.Text = "0";
                            handsub3.Children.Add(handmodelBcurrentx);

                            TextBlock handmodelBbalancetb = new TextBlock();
                           // handmodelBbalancetb.Text = "B均衡状态：";
                            handmodelBbalancetb.Text = (string)page.Resources["bjhstatus"];
                            handmodelBbalancetb.Margin = new Thickness(5, 0, 0, 0);
                            handmodelBbalancetb.Width = 140;
                            handmodelBbalancetb.TextAlignment = TextAlignment.Left;
                            handmodelBbalancetb.VerticalAlignment = VerticalAlignment.Center;
                            handsub3.Children.Add(handmodelBbalancetb);

                            //均衡状态
                            ComboBox handmodelBbalancecb = new ComboBox();
                            handmodelBbalancecb.Margin = new Thickness(2, 0, 0, 0);
                            handmodelBbalancecb.Width = 140;
                            handmodelBbalancecb.Name = "handmodelBbalancecb_" + receiveBmuList[a].Bmuindex.ToString();
                            //handmodelBbalancecb.Items.Add(new ComboBoxItem { Content = "不动作" });
                            handmodelBbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["nodz"] });
                            handmodelBbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["discharge"] });
                            handmodelBbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["charge"] });
                            //handmodelBbalancecb.Items.Add(new ComboBoxItem { Content = "保留" });
                            handmodelBbalancecb.SelectedIndex = 0;
                            handsub3.Children.Add(handmodelBbalancecb);

                            StackPanel handsub44 = new StackPanel();
                            handsub44.Orientation = Orientation.Horizontal;
                            handsub44.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub44);

                            for (int i = 0; i < receiveBmuList[a].CellmodelBcounts; i++)
                            {
                                RadioButton rb = new RadioButton();
                                rb.Name = "modelB_" + receiveBmuList[a].Bmuindex.ToString() + "_" + (i + 1).ToString();
                                //rb.Content = "单体" + (receiveBmuList[a].CellmodelAcounts + i + 1).ToString();
                                rb.Content = (string)page.Resources["cell"] + (receiveBmuList[a].CellmodelAcounts + i + 1).ToString();
                                rb.Margin = new Thickness(10, 0, 0, 0);
                                handsub44.Children.Add(rb);
                            }

                        }



                        //存放模块C所在行
                        if (receiveBmuList[a].CellmodelCcounts > 0 && receiveBmuList[a].CellmodelCcounts < 255)
                        {
                            StackPanel handsub4 = new StackPanel();
                            handsub4.Orientation = Orientation.Horizontal;
                            handsub4.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub4);

                            TextBlock handmodelCcurrentb = new TextBlock();
                            handmodelCcurrentb.Text = "C均衡电流：";
                            handmodelCcurrentb.Text = (string)page.Resources["cjhcur"];
                            handmodelCcurrentb.Width = 140;
                            handmodelCcurrentb.TextAlignment = TextAlignment.Left;
                            handmodelCcurrentb.VerticalAlignment = VerticalAlignment.Center;
                            handsub4.Children.Add(handmodelCcurrentb);

                            TextBox handmodelCcurrentx = new TextBox();
                            handmodelCcurrentx.Margin = new Thickness(2, 0, 0, 0);
                            handmodelCcurrentx.Name = "handmodelCcurrentx_" + receiveBmuList[a].Bmuindex.ToString();
                            handmodelCcurrentx.Width = 60;
                            handmodelCcurrentx.Text = "0";
                            handsub4.Children.Add(handmodelCcurrentx);

                            TextBlock handmodelCbalancetb = new TextBlock();
                            handmodelCbalancetb.Text = "C均衡状态：";
                            handmodelCbalancetb.Text = (string)page.Resources["cjhstatus"];
                            handmodelCbalancetb.Width = 140;
                            handmodelCbalancetb.TextAlignment = TextAlignment.Left;
                            handmodelCbalancetb.VerticalAlignment = VerticalAlignment.Center;
                            handsub4.Children.Add(handmodelCbalancetb);

                            //均衡状态
                            ComboBox handmodelCbalancecb = new ComboBox();
                            handmodelCbalancecb.Margin = new Thickness(2, 0, 0, 0);
                            handmodelCbalancecb.Width = 140;
                            handmodelCbalancecb.Name = "handmodelCbalancecb_" + receiveBmuList[a].Bmuindex.ToString();
                            handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["nodz"] });
                            handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["discharge"] });
                            handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = (string)page.Resources["charge"] });
                            //handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = "不动作" });
                            //handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = "放电" });
                            //handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = "充电" });
                            //handmodelCbalancecb.Items.Add(new ComboBoxItem { Content = "保留" });
                            handmodelCbalancecb.SelectedIndex = 0;
                            handsub4.Children.Add(handmodelCbalancecb);

                            StackPanel handsub55 = new StackPanel();
                            handsub55.Orientation = Orientation.Horizontal;
                            handsub55.Margin = new Thickness(0, 0, 0, 5);
                            handsp.Children.Add(handsub55);

                            for (int i = 0; i < receiveBmuList[a].CellmodelCcounts; i++)
                            {
                                RadioButton rb = new RadioButton();
                                rb.Name = "modelC_" + receiveBmuList[a].Bmuindex.ToString() + "_" + (i + 1).ToString();
                                //rb.Content = "单体" + (receiveBmuList[a].CellmodelAcounts + receiveBmuList[a].CellmodelBcounts + i + 1).ToString();
                                rb.Content = (string)page.Resources["cell"] + (receiveBmuList[a].CellmodelAcounts + receiveBmuList[a].CellmodelBcounts + i + 1).ToString();
                                rb.Margin = new Thickness(10, 0, 0, 0);
                                handsub55.Children.Add(rb);
                            }

                        }


                        rootsp.Children.Add(balancetc);

                        //ScrollViewer scview = new ScrollViewer();
                        //scview.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        //scview.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        ////scview.Height = 12000;
                        ////scview.Width = 1200;

                        StackPanel activesub11 = new StackPanel();
                        
                        activesub11.Orientation = Orientation.Horizontal;
                        activesub11.Margin = new Thickness(0, 10, 0, 5);


                        //scview.Content = activesub11;
                        //activesp.Children.Add(scview);

                        activesp.Children.Add(activesub11);

                        //BMU工作模式
                        TextBlock activembmustatusnumtb = new TextBlock();
                        activembmustatusnumtb.Text = "BMU工作模式：";
                        activembmustatusnumtb.Text = (string)page.Resources["bmuworkmode"];
                        activembmustatusnumtb.Width = 190;
                        //activembmustatusnumtb.Width = 120;
                        activembmustatusnumtb.TextAlignment = TextAlignment.Right;
                        activembmustatusnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub11.Children.Add(activembmustatusnumtb);
                        TextBox activembmustatusnumtx = new TextBox();
                        activembmustatusnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activembmustatusnumtx.Name = "activembmustatusnumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        activembmustatusnumtx.Width = 80;
                        activembmustatusnumtx.IsReadOnly = true;
                        activesub11.Children.Add(activembmustatusnumtx);


                        //整车工作模式
                        TextBlock activemvcustatusnumtb = new TextBlock();
                        activemvcustatusnumtb.Text = "整车工作模式：";
                        activemvcustatusnumtb.Text = (string)page.Resources["zcworkmode"];
                        activemvcustatusnumtb.Width = 190;
                        //activemvcustatusnumtb.Width = 120;
                        activemvcustatusnumtb.TextAlignment = TextAlignment.Right;
                        activemvcustatusnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub11.Children.Add(activemvcustatusnumtb);
                        TextBox activemvcustatusnumtx = new TextBox();
                        activemvcustatusnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activemvcustatusnumtx.Name = "activemvcustatusnumtx_" + receiveBmuList[a].Bmuindex.ToString();
                        activemvcustatusnumtx.Width = 80;
                        activemvcustatusnumtx.IsReadOnly = true;
                        activesub11.Children.Add(activemvcustatusnumtx);



                        //均衡模块1
                        StackPanel activesub1 = new StackPanel();
                        activesub1.Orientation = Orientation.Horizontal;
                        activesub1.Margin = new Thickness(0, 0, 0, 5);
                        activesp.Children.Add(activesub1);

                        //设置均衡电流1
                        TextBlock setbalanceaol1tb = new TextBlock();
                        setbalanceaol1tb.Text = "设置均衡电流1：";
                        setbalanceaol1tb.Text = (string)page.Resources["configjhcur1"];
                        setbalanceaol1tb.Width = 190;
                        //setbalanceaol1tb.Width = 120;
                        setbalanceaol1tb.TextAlignment = TextAlignment.Right;
                        setbalanceaol1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub1.Children.Add(setbalanceaol1tb);
                        TextBox setbalanceaol1tx = new TextBox();
                        setbalanceaol1tx.Margin = new Thickness(2, 0, 0, 0);
                        setbalanceaol1tx.Name = "setbalanceaoltx3_" + receiveBmuList[a].Bmuindex.ToString();
                        setbalanceaol1tx.Text = "0";
                        setbalanceaol1tx.Width = 80;
                        setbalanceaol1tx.IsReadOnly = true;
                        activesub1.Children.Add(setbalanceaol1tx);


                        ////实时均衡电流1
                        TextBlock getbalanceaol1tb = new TextBlock();
                        getbalanceaol1tb.Text = "实时均衡电流1：";
                        getbalanceaol1tb.Text = (string)page.Resources["ssjhcur1"];
                        getbalanceaol1tb.Width = 190;
                        //getbalanceaol1tb.Width = 120;
                        getbalanceaol1tb.TextAlignment = TextAlignment.Right;
                        getbalanceaol1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub1.Children.Add(getbalanceaol1tb);
                        TextBox getbalanceaol1tx = new TextBox();
                        getbalanceaol1tx.Margin = new Thickness(2, 0, 0, 0);
                        getbalanceaol1tx.Name = "getbalanceaoltx3_" + receiveBmuList[a].Bmuindex.ToString();
                        getbalanceaol1tx.Width = 80;
                        getbalanceaol1tx.Text = "0";
                        getbalanceaol1tx.IsReadOnly = true;
                        activesub1.Children.Add(getbalanceaol1tx);


                        //设置单体电池序号1
                        TextBlock setcellnum1tb = new TextBlock();
                        setcellnum1tb.Text = "设置单体序号1：";
                        setcellnum1tb.Text = (string)page.Resources["configcellnum1"];
                        setcellnum1tb.Width = 190;
                        //setcellnum1tb.Width = 120;
                        setcellnum1tb.TextAlignment = TextAlignment.Right;
                        setcellnum1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub1.Children.Add(setcellnum1tb);
                        TextBox setcellnum1tx = new TextBox();
                        setcellnum1tx.Margin = new Thickness(2, 0, 0, 0);
                        setcellnum1tx.Name = "setcellnumtx3_" + receiveBmuList[a].Bmuindex.ToString();
                        setcellnum1tx.Text = "0";
                        setcellnum1tx.Width = 80;
                        setcellnum1tx.IsReadOnly = true;
                        activesub1.Children.Add(setcellnum1tx);

                        StackPanel activesub40 = new StackPanel();
                        activesub40.Margin = new Thickness(0, 0, 0, 5);
                        activesub40.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub40);

                        //实时单体电池序号1
                        TextBlock getcellnum1tb = new TextBlock();
                        getcellnum1tb.Text = "实时单体序号1：";
                        getcellnum1tb.Text = (string)page.Resources["sscellnum1"];
                        getcellnum1tb.Width = 190;
                        //getcellnum1tb.Width = 120;
                        getcellnum1tb.TextAlignment = TextAlignment.Right;
                        getcellnum1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub40.Children.Add(getcellnum1tb);
                        //activesub1.Children.Add(getcellnum1tb);
                        TextBox getcellnum1tx = new TextBox();
                        getcellnum1tx.Margin = new Thickness(2, 0, 0, 0);
                        getcellnum1tx.Name = "getcellnumtx3_" + receiveBmuList[a].Bmuindex.ToString();
                        getcellnum1tx.Width = 80;
                        getcellnum1tx.Text = "0";
                        getcellnum1tx.IsReadOnly = true;
                        activesub40.Children.Add(getcellnum1tx);
                        //activesub1.Children.Add(getcellnum1tx);

                        //最高单体1
                        TextBlock cellhighabs1tb = new TextBlock();
                        cellhighabs1tb.Text = "最高单体1：";
                        cellhighabs1tb.Text = (string)page.Resources["maxcell1"];
                        cellhighabs1tb.Width = 190;
                        //cellhighabs1tb.Width =120;
                        cellhighabs1tb.TextAlignment = TextAlignment.Right;
                        cellhighabs1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub40.Children.Add(cellhighabs1tb);
                        TextBox cellhighabs1tx = new TextBox();
                        cellhighabs1tx.Margin = new Thickness(2, 0, 0, 0);
                        cellhighabs1tx.Name = "cellhighabstx3_" + receiveBmuList[a].Bmuindex.ToString();
                        cellhighabs1tx.Width = 80;
                        cellhighabs1tx.Text = "0";
                        cellhighabs1tx.IsReadOnly = true;
                        activesub40.Children.Add(cellhighabs1tx);


                        //最低单体1
                        TextBlock celllowabs1tb = new TextBlock();
                        celllowabs1tb.Text = "最低单体1：";
                        celllowabs1tb.Text = (string)page.Resources["mincell1"];
                        celllowabs1tb.Width = 190;
                        //celllowabs1tb.Width = 120;
                        celllowabs1tb.TextAlignment = TextAlignment.Right;
                        celllowabs1tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub40.Children.Add(celllowabs1tb);
                        TextBox celllowabs1tx = new TextBox();
                        celllowabs1tx.Margin = new Thickness(2, 0, 0, 0);
                        celllowabs1tx.Name = "celllowabstx3_" + receiveBmuList[a].Bmuindex.ToString();
                        celllowabs1tx.Width = 80;
                        celllowabs1tx.Text = "0";
                        celllowabs1tx.IsReadOnly = true;
                        activesub40.Children.Add(celllowabs1tx);

                        //BMU均衡信息
                        StackPanel activesub10 = new StackPanel();
                        activesub10.Orientation = Orientation.Horizontal;
                        activesub10.Margin = new Thickness(0, 0, 0, 5);
                        activesp.Children.Add(activesub10);


                        //模块1均衡单体序号
                        TextBlock activem1cellnumtb = new TextBlock();
                        activem1cellnumtb.Text = "模块1均衡序号：";
                        activem1cellnumtb.Text = (string)page.Resources["module1num"];
                        activem1cellnumtb.Width = 190;
                        //activem1cellnumtb.Width = 120;
                        activem1cellnumtb.TextAlignment = TextAlignment.Right;
                        activem1cellnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub10.Children.Add(activem1cellnumtb);
                        TextBox activem1cellnumtx = new TextBox();
                        activem1cellnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem1cellnumtx.Name = "activemcellnumtx1_" + receiveBmuList[a].Bmuindex.ToString();
                        activem1cellnumtx.Width = 80;
                        activem1cellnumtx.Text = "0";
                        activem1cellnumtx.IsReadOnly = true;
                        activesub10.Children.Add(activem1cellnumtx);

                        //充放电状态
                        TextBlock activem1chargenumtb = new TextBlock();
                        activem1chargenumtb.Text = "充放电状态：";
                        activem1chargenumtb.Text = (string)page.Resources["module1num"];
                        activem1chargenumtb.Width = 190;
                        //activem1chargenumtb.Width = 120;
                        activem1chargenumtb.TextAlignment = TextAlignment.Right;
                        activem1chargenumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub10.Children.Add(activem1chargenumtb);
                        TextBox activem1chargenumtx = new TextBox();
                        activem1chargenumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem1chargenumtx.Name = "activemchargenumtx1_" + receiveBmuList[a].Bmuindex.ToString();
                        activem1chargenumtx.Width = 80;
                        activem1chargenumtx.Text = "0";
                        activem1chargenumtx.IsReadOnly = true;
                        activesub10.Children.Add(activem1chargenumtx);


                        //模块1均衡电流
                        TextBlock activem1chargeaolnumtb = new TextBlock();
                        activem1chargeaolnumtb.Text = "模块1均衡电流：";
                        activem1chargeaolnumtb.Text = (string)page.Resources["module1jhcur"];
                        activem1chargeaolnumtb.Width = 190;
                        //activem1chargeaolnumtb.Width = 120;
                        activem1chargeaolnumtb.TextAlignment = TextAlignment.Right;
                        activem1chargeaolnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub10.Children.Add(activem1chargeaolnumtb);
                        TextBox activem1chargeaolnumtx = new TextBox();
                        activem1chargeaolnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem1chargeaolnumtx.Name = "activemchargeaolnumtx1_" + receiveBmuList[a].Bmuindex.ToString();
                        activem1chargeaolnumtx.Width = 80;
                        activem1chargeaolnumtx.Text = "0";
                        activem1chargeaolnumtx.IsReadOnly = true;
                        activesub10.Children.Add(activem1chargeaolnumtx);


                        //均衡模块2
                        StackPanel activesub2 = new StackPanel();
                        activesub2.Margin = new Thickness(0, 0, 0, 5);
                        activesub2.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub2);


                        //设置均衡电流2
                        TextBlock setbalanceaol2tb = new TextBlock();
                        setbalanceaol2tb.Text = "设置均衡电流2：";
                        setbalanceaol2tb.Text = (string)page.Resources["configjhcur2"];
                        setbalanceaol2tb.Width = 190;
                        //setbalanceaol2tb.Width = 120;
                        setbalanceaol2tb.TextAlignment = TextAlignment.Right;
                        setbalanceaol2tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub2.Children.Add(setbalanceaol2tb);
                        TextBox setbalanceaol2tx = new TextBox();
                        setbalanceaol2tx.Margin = new Thickness(2, 0, 0, 0);
                        setbalanceaol2tx.Name = "setbalanceaoltx4_" + receiveBmuList[a].Bmuindex.ToString();
                        setbalanceaol2tx.Width = 80;
                        setbalanceaol2tx.Text = "0";
                        setbalanceaol2tx.IsReadOnly = true;
                        activesub2.Children.Add(setbalanceaol2tx);


                        ////实时均衡电流2
                        TextBlock getbalanceaol2tb = new TextBlock();
                        getbalanceaol2tb.Text = "实时均衡电流2：";
                        getbalanceaol2tb.Text = (string)page.Resources["ssjhcur2"];
                        getbalanceaol2tb.Width = 190;
                        //getbalanceaol2tb.Width = 120;
                        getbalanceaol2tb.TextAlignment = TextAlignment.Right;
                        getbalanceaol2tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub2.Children.Add(getbalanceaol2tb);
                        TextBox getbalanceaol2tx = new TextBox();
                        getbalanceaol2tx.Margin = new Thickness(2, 0, 0, 0);
                        getbalanceaol2tx.Name = "getbalanceaoltx4_" + receiveBmuList[a].Bmuindex.ToString();
                        getbalanceaol2tx.Width = 80;
                        getbalanceaol2tx.Text = "0";
                        getbalanceaol2tx.IsReadOnly = true;
                        activesub2.Children.Add(getbalanceaol2tx);


                        //设置单体电池序号2
                        TextBlock setcellnum2tb = new TextBlock();
                        setcellnum2tb.Text = "设置单体序号2：";
                        setcellnum2tb.Text = (string)page.Resources["configcellnum2"];
                        setcellnum2tb.Width = 190;
                        //setcellnum2tb.Width = 120;
                        setcellnum2tb.TextAlignment = TextAlignment.Right;
                        setcellnum2tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub2.Children.Add(setcellnum2tb);
                        TextBox setcellnum2tx = new TextBox();
                        setcellnum2tx.Margin = new Thickness(2, 0, 0, 0);
                        setcellnum2tx.Name = "setcellnumtx4_" + receiveBmuList[a].Bmuindex.ToString();
                        setcellnum2tx.Width = 80;
                        setcellnum2tx.Text = "0";
                        setcellnum2tx.IsReadOnly = true;
                        activesub2.Children.Add(setcellnum2tx);

                        StackPanel activesub50 = new StackPanel();
                        activesub50.Margin = new Thickness(0, 0, 0, 5);
                        activesub50.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub50);

                        //实时单体电池序号2
                        TextBlock getcellnum2tb = new TextBlock();
                        getcellnum2tb.Text = "实时单体序号2：";
                        getcellnum2tb.Text = (string)page.Resources["sscellnum2"];
                        getcellnum2tb.Width = 190;
                        //getcellnum2tb.Width = 120;
                        getcellnum2tb.TextAlignment = TextAlignment.Right;
                        getcellnum2tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub50.Children.Add(getcellnum2tb);
                        TextBox getcellnum2tx = new TextBox();
                        getcellnum2tx.Margin = new Thickness(2, 0, 0, 0);
                        getcellnum2tx.Name = "getcellnumtx4_" + receiveBmuList[a].Bmuindex.ToString();
                        getcellnum2tx.Width = 80;
                        getcellnum2tx.Text = "0";
                        getcellnum2tx.IsReadOnly = true;
                        activesub50.Children.Add(getcellnum2tx);

                        //最高单体压差2
                        TextBlock cellhighabs2tb = new TextBlock();
                        cellhighabs2tb.Text = "最高单体2：";
                        cellhighabs2tb.Text = (string)page.Resources["maxcell2"];
                        cellhighabs2tb.Width = 190;
                        //cellhighabs2tb.Width = 120;
                        cellhighabs2tb.TextAlignment = TextAlignment.Right;
                        cellhighabs2tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub50.Children.Add(cellhighabs2tb);
                        TextBox cellhighabs2tx = new TextBox();
                        cellhighabs2tx.Margin = new Thickness(2, 0, 0, 0);
                        cellhighabs2tx.Name = "cellhighabstx4_" + receiveBmuList[a].Bmuindex.ToString();
                        cellhighabs2tx.Width = 80;
                        cellhighabs2tx.Text = "0";
                        cellhighabs2tx.IsReadOnly = true;
                        activesub50.Children.Add(cellhighabs2tx);


                        //最低单体2
                        TextBlock celllowabstb = new TextBlock();
                        celllowabstb.Text = "最低单体2：";
                        celllowabstb.Text = (string)page.Resources["mincell2"];
                        celllowabstb.Width = 190;
                        //celllowabstb.Width = 120;
                        celllowabstb.TextAlignment = TextAlignment.Right;
                        celllowabstb.VerticalAlignment = VerticalAlignment.Center;
                        activesub50.Children.Add(celllowabstb);
                        TextBox celllowabstx = new TextBox();
                        celllowabstx.Margin = new Thickness(2, 0, 0, 0);
                        celllowabstx.Name = "celllowabstx4_" + receiveBmuList[a].Bmuindex.ToString();
                        celllowabstx.Width = 80;
                        celllowabstx.Text = "0";
                        celllowabstx.IsReadOnly = true;
                        activesub50.Children.Add(celllowabstx);

                        //BMU均衡模块2
                        StackPanel activesub20 = new StackPanel();
                        activesub20.Margin = new Thickness(0, 0, 0, 5);
                        activesub20.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub20);



                        //模块2均衡单体序号
                        TextBlock activem2cellnumtb = new TextBlock();
                        activem2cellnumtb.Text = "模块2均衡序号：";
                        activem2cellnumtb.Text = (string)page.Resources["module2jhnum"];
                        activem2cellnumtb.Width = 190;
                        //activem2cellnumtb.Width = 120;
                        activem2cellnumtb.TextAlignment = TextAlignment.Right;
                        activem2cellnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub20.Children.Add(activem2cellnumtb);
                        TextBox activem2cellnumtx = new TextBox();
                        
                        activem2cellnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem2cellnumtx.Name = "activemcellnumtx2_" + receiveBmuList[a].Bmuindex.ToString();
                        activem2cellnumtx.Width = 80;
                        activem2cellnumtx.Text = "0";
                        activem2cellnumtx.IsReadOnly = true;
                        activesub20.Children.Add(activem2cellnumtx);

                        //均衡请求充放电状态
                        TextBlock activem2chargenumtb = new TextBlock();
                        activem2chargenumtb.Text = "均衡充放电状态：";
                        activem2chargenumtb.Text = (string)page.Resources["jhdischargestatus"];
                        activem2chargenumtb.Width = 190;
                        //activem2chargenumtb.Width = 120;
                        activem2chargenumtb.TextAlignment = TextAlignment.Right;
                        activem2chargenumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub20.Children.Add(activem2chargenumtb);
                        TextBox activem2chargenumtx = new TextBox();
                        activem2chargenumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem2chargenumtx.Name = "activemchargenumtx2_" + receiveBmuList[a].Bmuindex.ToString();
                        activem2chargenumtx.Width = 80;
                        activem2chargenumtx.Text = "0";
                        activem2chargenumtx.IsReadOnly = true;
                        activesub20.Children.Add(activem2chargenumtx);


                        //模块2均衡电流
                        TextBlock activem2chargeaolnumtb = new TextBlock();
                        activem2chargeaolnumtb.Text = "模块2均衡电流：";
                        activem2chargeaolnumtb.Text = (string)page.Resources["module2jhcur"];
                        activem2chargeaolnumtb.Width = 190;
                        //activem2chargeaolnumtb.Width = 120;
                        activem2chargeaolnumtb.TextAlignment = TextAlignment.Right;
                        activem2chargeaolnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub20.Children.Add(activem2chargeaolnumtb);
                        TextBox activem2chargeaolnumtx = new TextBox();
                        activem2chargeaolnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem2chargeaolnumtx.Name = "activemchargeaolnumtx2_" + receiveBmuList[a].Bmuindex.ToString();
                        activem2chargeaolnumtx.Width = 80;
                        activem2chargeaolnumtx.Text = "0";
                        activem2chargeaolnumtx.IsReadOnly = true;
                        activesub20.Children.Add(activem2chargeaolnumtx);


                        //均衡模块3
                        StackPanel activesub3 = new StackPanel();
                        activesub3.Margin = new Thickness(0, 0, 0, 5);
                        activesub3.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub3);

                        //设置均衡电流3
                        TextBlock setbalanceaol3tb = new TextBlock();
                        setbalanceaol3tb.Text = "设置均衡电流3：";
                        setbalanceaol3tb.Text = (string)page.Resources["configjhcur3"];
                        setbalanceaol3tb.Width = 190;
                        //setbalanceaol3tb.Width = 120;
                        setbalanceaol3tb.TextAlignment = TextAlignment.Right;
                        setbalanceaol3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub3.Children.Add(setbalanceaol3tb);
                        TextBox setbalanceaol3tx = new TextBox();
                        setbalanceaol3tx.Margin = new Thickness(2, 0, 0, 0);
                        setbalanceaol3tx.Name = "setbalanceaoltx5_" + receiveBmuList[a].Bmuindex.ToString();
                        setbalanceaol3tx.Width = 80;
                        setbalanceaol3tx.Text = "0";
                        setbalanceaol3tx.IsReadOnly = true;
                        activesub3.Children.Add(setbalanceaol3tx);


                        ////实时均衡电流3
                        TextBlock getbalanceaol3tb = new TextBlock();
                        getbalanceaol3tb.Text = "实时均衡电流3：";
                        getbalanceaol3tb.Text = (string)page.Resources["ssjhcur3"];
                        getbalanceaol3tb.Width = 190;
                        //getbalanceaol3tb.Width = 120;
                        getbalanceaol3tb.TextAlignment = TextAlignment.Right;
                        getbalanceaol3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub3.Children.Add(getbalanceaol3tb);
                        TextBox getbalanceaol3tx = new TextBox();
                        getbalanceaol3tx.Margin = new Thickness(2, 0, 0, 0);
                        getbalanceaol3tx.Name = "getbalanceaoltx5_" + receiveBmuList[a].Bmuindex.ToString();
                        getbalanceaol3tx.Width = 80;
                        getbalanceaol3tx.Text = "0";
                        getbalanceaol3tx.IsReadOnly = true;
                        activesub3.Children.Add(getbalanceaol3tx);


                        //设置单体电池序号3
                        TextBlock setcellnum3tb = new TextBlock();
                        setcellnum3tb.Text = "设置单体序号3：";
                        setcellnum3tb.Text = (string)page.Resources["configcellnum3"];
                        setcellnum3tb.Width = 190;
                        //setcellnum3tb.Width = 120;
                        setcellnum3tb.TextAlignment = TextAlignment.Right;
                        setcellnum3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub3.Children.Add(setcellnum3tb);
                        TextBox setcellnum3tx = new TextBox();
                        setcellnum3tx.Margin = new Thickness(2, 0, 0, 0);
                        setcellnum3tx.Name = "setcellnumtx5_" + receiveBmuList[a].Bmuindex.ToString();
                        setcellnum3tx.Width = 80;
                        setcellnum3tx.Text = "0";
                        setcellnum3tx.IsReadOnly = true;
                        activesub3.Children.Add(setcellnum3tx);

                        StackPanel activesub60 = new StackPanel();
                        activesub60.Margin = new Thickness(0, 0, 0, 5);
                        activesub60.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub60);

                        //实时单体电池序号3
                        TextBlock getcellnum3tb = new TextBlock();
                        getcellnum3tb.Text = "实时单体序号3：";
                        getcellnum3tb.Text = (string)page.Resources["sscellnum3"];
                        getcellnum3tb.Width = 190;
                        //getcellnum3tb.Width = 120;
                        getcellnum3tb.TextAlignment = TextAlignment.Right;
                        getcellnum3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub60.Children.Add(getcellnum3tb);
                        TextBox getcellnum3tx = new TextBox();
                        getcellnum3tx.Margin = new Thickness(2, 0, 0, 0);
                        getcellnum3tx.Name = "getcellnumtx5_" + receiveBmuList[a].Bmuindex.ToString();
                        getcellnum3tx.Width = 80;
                        getcellnum3tx.Text = "0";
                        getcellnum3tx.IsReadOnly = true;
                        activesub60.Children.Add(getcellnum3tx);

                        //最高单体压差3
                        TextBlock cellhighabs3tb = new TextBlock();
                        cellhighabs3tb.Text = "最高单体3：";
                        cellhighabs3tb.Text = (string)page.Resources["maxcell3"];
                        //cellhighabs3tb.Width = 185;
                        cellhighabs3tb.Width = 190;
                        cellhighabs3tb.TextAlignment = TextAlignment.Right;
                        cellhighabs3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub60.Children.Add(cellhighabs3tb);
                        TextBox cellhighabs3tx = new TextBox();
                        cellhighabs3tx.Margin = new Thickness(2, 0, 0, 0);
                        cellhighabs3tx.Name = "cellhighabstx5_" + receiveBmuList[a].Bmuindex.ToString();
                        cellhighabs3tx.Width = 80;
                        cellhighabs3tx.Text = "0";
                        cellhighabs3tx.IsReadOnly = true;
                        activesub60.Children.Add(cellhighabs3tx);


                        //最低单体3
                        TextBlock celllowabs3tb = new TextBlock();
                        celllowabs3tb.Text = "最低单体3：";
                        celllowabs3tb.Text = (string)page.Resources["mincell3"];
                        //celllowabs3tb.Width = 185;
                        celllowabs3tb.Width = 190;
                        celllowabs3tb.TextAlignment = TextAlignment.Right;
                        celllowabs3tb.VerticalAlignment = VerticalAlignment.Center;
                        activesub60.Children.Add(celllowabs3tb);
                        TextBox celllowabs3tx = new TextBox();
                        celllowabs3tx.Margin = new Thickness(2, 0, 0, 0);
                        celllowabs3tx.Name = "celllowabstx5_" + receiveBmuList[a].Bmuindex.ToString();
                        celllowabs3tx.Width = 80;
                        celllowabs3tx.Text = "0";
                        celllowabs3tx.IsReadOnly = true;
                        activesub60.Children.Add(celllowabs3tx);

                        //BMU均衡3
                        StackPanel activesub30 = new StackPanel();
                        activesub30.Margin = new Thickness(0, 0, 0, 5);
                        activesub30.Orientation = Orientation.Horizontal;
                        activesp.Children.Add(activesub30);



                        //模块3均衡单体序号
                        TextBlock activem3cellnumtb = new TextBlock();
                        activem3cellnumtb.Text = "模块3均衡序号：";
                        activem3cellnumtb.Text = (string)page.Resources["jh3modulenum"];
                        activem3cellnumtb.Width = 190;
                        //activem3cellnumtb.Width = 120;
                        activem3cellnumtb.TextAlignment = TextAlignment.Right;
                        activem3cellnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub30.Children.Add(activem3cellnumtb);
                        TextBox activem3cellnumtx = new TextBox();
                        activem3cellnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem3cellnumtx.Name = "activemcellnumtx3_" + receiveBmuList[a].Bmuindex.ToString();
                        activem3cellnumtx.Width = 80;
                        activem3cellnumtx.Text = "0";
                        activem3cellnumtx.IsReadOnly = true;
                        activesub30.Children.Add(activem3cellnumtx);

                        //均衡请求充放电状态
                        TextBlock activem3chargenumtb = new TextBlock();
                        activem3chargenumtb.Text = "均衡充放电状态：";
                        activem3chargenumtb.Text = (string)page.Resources["jhcfd"];
                        activem3chargenumtb.Width = 190;
                        //activem3chargenumtb.Width = 120;
                        activem3chargenumtb.TextAlignment = TextAlignment.Right;
                        activem3chargenumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub30.Children.Add(activem3chargenumtb);
                        TextBox activem3chargenumtx = new TextBox();
                        activem3chargenumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem3chargenumtx.Name = "activemchargenumtx3_" + receiveBmuList[a].Bmuindex.ToString();
                        activem3chargenumtx.Width = 80;
                        activem3chargenumtx.Text = "0";
                        activem3chargenumtx.IsReadOnly = true;
                        activesub30.Children.Add(activem3chargenumtx);


                        //模块3均衡电流
                        TextBlock activem3chargeaolnumtb = new TextBlock();
                        activem3chargeaolnumtb.Text = "模块3均衡电流：";
                        activem3chargeaolnumtb.Text = (string)page.Resources["mocule3jhcur"];
                        activem3chargeaolnumtb.Width = 190;
                        //activem3chargeaolnumtb.Width = 120;
                        activem3chargeaolnumtb.TextAlignment = TextAlignment.Right;
                        activem3chargeaolnumtb.VerticalAlignment = VerticalAlignment.Center;
                        activesub30.Children.Add(activem3chargeaolnumtb);
                        TextBox activem3chargeaolnumtx = new TextBox();
                        activem3chargeaolnumtx.Margin = new Thickness(2, 0, 0, 0);
                        activem3chargeaolnumtx.Name = "activemchargeaolnumtx3_" + receiveBmuList[a].Bmuindex.ToString();
                        activem3chargeaolnumtx.Width = 80;
                        activem3chargeaolnumtx.Text = "0";
                        activem3chargeaolnumtx.IsReadOnly = true;
                        activesub30.Children.Add(activem3chargeaolnumtx);

                        //单体信息
                        GroupBox cellgb = new GroupBox();
                        //cellgb.Header = "单体信息";
                        cellgb.Header = (string)page.Resources["cellinfo"];
                        cellgb.Margin = new Thickness(0, 0, 0, 5);
                        cellgb.BorderBrush = new SolidColorBrush(color);
                        cellgb.BorderThickness = new Thickness(1);
                        StackPanel cellsp = new StackPanel();
                        cellgb.Content = cellsp;
                        rootsp.Children.Add(cellgb);

                        //2020.3.16将电压信号线状态按照模块分行显示
                        int VNum = 1;
                        //子模块A
                        StackPanel line_A= new StackPanel();
                        line_A.Orientation = Orientation.Horizontal;
                        TextBlock moduleA = new TextBlock();
                        moduleA.Width = 100;
                        moduleA.FontSize = 13;
                        moduleA.TextAlignment = TextAlignment.Left;
                        moduleA.Text = "Module_A:";
                        moduleA.Name = "Module_A"+ receiveBmuList[a].Bmuindex;
                        moduleA.Visibility = Visibility.Collapsed;
                        line_A.Children.Add(moduleA);
                        cellsp.Children.Add(line_A);
                        StackPanel spLine = new StackPanel();
                        spLine.Orientation = Orientation.Horizontal;
                        
                        for (int i = 0; i < receiveBmuList[a].CellmodelAcounts; i++)
                        {   if (i >= receiveBmuList[a].Cellcouts) {
                                break;
                            }
                                StackPanel sp = new StackPanel();
                                sp.Margin = new Thickness(0, 0, 0, 5);
                                sp.Orientation = Orientation.Horizontal;
                                //for (int j = 0; j < linecounts; j++)
                                //{
                                TextBlock cellltb = new TextBlock();
                                //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                                //cellltb.Foreground = new SolidColorBrush(color); 
                                cellltb.Width = 50;
                                cellltb.FontSize = 13;
                                cellltb.TextAlignment = TextAlignment.Right;
                                cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                                cellltb.Text = "V" + (VNum + i) + ":";
                                cellltb.VerticalAlignment = VerticalAlignment.Center;
                                sp.Children.Add(cellltb);

                                TextBox cellval = new TextBox();
                                cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                                cellval.IsReadOnly = true;
                                cellval.Text = "0.00";
                                cellval.Width = 45;
                                cellval.FontSize = 13;
                                cellval.TextAlignment = TextAlignment.Left;
                                cellval.HorizontalAlignment = HorizontalAlignment.Left;
                                cellval.Margin = new Thickness(2, 0, 0, 0);
                                sp.Children.Add(cellval);

                                Button btn = new Button();
                                btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                                btn.ToolTip = "电压信号线";
                                btn.ToolTip = (string)page.Resources["volxhx"];
                                btn.Margin = new Thickness(2, 0, 10, 0);
                                btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                                sp.Children.Add(btn);
                                spLine.Children.Add(sp);
                            if (i!=0&&(i + 1) % 8 == 0) {
                                cellsp.Children.Add(spLine);
                                spLine = new StackPanel();
                                spLine.Orientation = Orientation.Horizontal;
                            }
                            
                        }
                        if (receiveBmuList[a].CellmodelAcounts % 8 != 0) {
                            cellsp.Children.Add(spLine);
                        }

                        //子模块B
                        VNum += receiveBmuList[a].CellmodelAcounts;                        
                        spLine = new StackPanel();
                        spLine.Orientation = Orientation.Horizontal;
                        StackPanel line_B = new StackPanel();
                        line_B.Orientation = Orientation.Horizontal;
                        TextBlock moduleB = new TextBlock();
                        moduleB.Width = 100;
                        moduleB.FontSize = 13;
                        moduleB.TextAlignment = TextAlignment.Left;
                        moduleB.Text = "Module_B:";
                        moduleB.Name = "Module_B" + receiveBmuList[a].Bmuindex;

                        moduleB.Visibility = Visibility.Collapsed;

                        line_B.Children.Add(moduleB);
                        cellsp.Children.Add(line_B);
                        for (int i = 0; i < receiveBmuList[a].CellmodelBcounts; i++)
                        {
                            if (i+ receiveBmuList[a].CellmodelAcounts >= receiveBmuList[a].Cellcouts)
                            {
                                break;
                            }
                            StackPanel sp = new StackPanel();
                            sp.Margin = new Thickness(0, 0, 0, 5);
                            sp.Orientation = Orientation.Horizontal;
                            //for (int j = 0; j < linecounts; j++)
                            //{
                            TextBlock cellltb = new TextBlock();
                            //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                            //cellltb.Foreground = new SolidColorBrush(color); 
                            cellltb.Width = 50;
                            cellltb.FontSize = 13;
                            cellltb.TextAlignment = TextAlignment.Right;
                            cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                            cellltb.Text = "V" + (VNum + i) + ":";
                            cellltb.VerticalAlignment = VerticalAlignment.Center;
                            sp.Children.Add(cellltb);

                            TextBox cellval = new TextBox();
                            cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            cellval.IsReadOnly = true;
                            cellval.Text = "0.00";
                            cellval.Width = 45;
                            cellval.FontSize = 13;
                            cellval.TextAlignment = TextAlignment.Left;
                            cellval.HorizontalAlignment = HorizontalAlignment.Left;
                            cellval.Margin = new Thickness(2, 0, 0, 0);
                            sp.Children.Add(cellval);

                            Button btn = new Button();
                            btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            btn.ToolTip = "电压信号线";
                            btn.ToolTip = (string)page.Resources["volxhx"];
                            btn.Margin = new Thickness(2, 0, 10, 0);
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                            sp.Children.Add(btn);
                            spLine.Children.Add(sp);
                            if (i != 0 && (i + 1) % 8 == 0)
                            {
                                cellsp.Children.Add(spLine);
                                spLine = new StackPanel();
                                spLine.Orientation = Orientation.Horizontal;
                            }
                        }
                        if (receiveBmuList[a].CellmodelBcounts % 8 != 0)
                        {
                            cellsp.Children.Add(spLine);
                        }
                        //cellsp.Children.Add(spLine);

                        //子模块C
                        VNum += receiveBmuList[a].CellmodelBcounts;
                        spLine = new StackPanel();
                        spLine.Orientation = Orientation.Horizontal;
                        StackPanel line_C = new StackPanel();
                        line_C.Orientation = Orientation.Horizontal;
                        TextBlock moduleC = new TextBlock();
                        moduleC.Width = 100;
                        moduleC.FontSize = 13;
                        moduleC.TextAlignment = TextAlignment.Left;
                        moduleC.Text = "Module_C:";
                        moduleC.Name = "Module_C" + receiveBmuList[a].Bmuindex;

                        moduleC.Visibility = Visibility.Collapsed;

                        line_C.Children.Add(moduleC);
                        cellsp.Children.Add(line_C);
                        for (int i = 0; i < receiveBmuList[a].CellmodelCcounts; i++)
                        {
                            if (i + receiveBmuList[a].CellmodelAcounts+ receiveBmuList[a].CellmodelBcounts >= receiveBmuList[a].Cellcouts)
                            {
                                break;
                            }
                            StackPanel sp = new StackPanel();
                            sp.Margin = new Thickness(0, 0, 0, 5);
                            sp.Orientation = Orientation.Horizontal;
                            //for (int j = 0; j < linecounts; j++)
                            //{
                            TextBlock cellltb = new TextBlock();
                            //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                            //cellltb.Foreground = new SolidColorBrush(color); 
                            cellltb.Width = 50;
                            cellltb.FontSize = 13;
                            cellltb.TextAlignment = TextAlignment.Right;
                            cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                            cellltb.Text = "V" + (VNum + i) + ":";
                            cellltb.VerticalAlignment = VerticalAlignment.Center;
                            sp.Children.Add(cellltb);

                            TextBox cellval = new TextBox();
                            cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            cellval.IsReadOnly = true;
                            cellval.Text = "0.00";
                            cellval.Width = 45;
                            cellval.FontSize = 13;
                            cellval.TextAlignment = TextAlignment.Left;
                            cellval.HorizontalAlignment = HorizontalAlignment.Left;
                            cellval.Margin = new Thickness(2, 0, 0, 0);
                            sp.Children.Add(cellval);

                            Button btn = new Button();
                            btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            btn.ToolTip = "电压信号线";
                            btn.ToolTip = (string)page.Resources["volxhx"];
                            btn.Margin = new Thickness(2, 0, 10, 0);
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                            sp.Children.Add(btn);
                            spLine.Children.Add(sp);
                            if (i != 0 && (i + 1) % 8 == 0)
                            {
                                cellsp.Children.Add(spLine);
                                spLine = new StackPanel();
                                spLine.Orientation = Orientation.Horizontal;
                            }
                        }
                        if (receiveBmuList[a].CellmodelCcounts % 8 != 0)
                        {
                            cellsp.Children.Add(spLine);
                        }

                        //子模块D
                        VNum += receiveBmuList[a].CellmodelCcounts;
                        spLine = new StackPanel();
                        spLine.Orientation = Orientation.Horizontal;
                        StackPanel line_D = new StackPanel();
                        line_D.Orientation = Orientation.Horizontal;
                        TextBlock moduleD = new TextBlock();
                        moduleD.Width = 100;
                        moduleD.FontSize = 13;
                        moduleD.TextAlignment = TextAlignment.Left;
                        moduleD.Text = "Module_D:";
                        moduleD.Name = "Module_D" + receiveBmuList[a].Bmuindex;

                        moduleD.Visibility = Visibility.Collapsed;

                        line_D.Children.Add(moduleD);
                        cellsp.Children.Add(line_D);
                        for (int i = 0; i < receiveBmuList[a].CellmodelDcounts; i++)
                        {
                            if (i + receiveBmuList[a].CellmodelAcounts + receiveBmuList[a].CellmodelBcounts+ receiveBmuList[a].CellmodelCcounts >= receiveBmuList[a].Cellcouts)
                            {
                                break;
                            }
                            StackPanel sp = new StackPanel();
                            sp.Margin = new Thickness(0, 0, 0, 5);
                            sp.Orientation = Orientation.Horizontal;
                            //for (int j = 0; j < linecounts; j++)
                            //{
                            TextBlock cellltb = new TextBlock();
                            //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                            //cellltb.Foreground = new SolidColorBrush(color); 
                            cellltb.Width = 50;
                            cellltb.FontSize = 13;
                            cellltb.TextAlignment = TextAlignment.Right;
                            cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                            cellltb.Text = "V" + (VNum + i) + ":";
                            cellltb.VerticalAlignment = VerticalAlignment.Center;
                            sp.Children.Add(cellltb);

                            TextBox cellval = new TextBox();
                            cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            cellval.IsReadOnly = true;
                            cellval.Text = "0.00";
                            cellval.Width = 45;
                            cellval.FontSize = 13;
                            cellval.TextAlignment = TextAlignment.Left;
                            cellval.HorizontalAlignment = HorizontalAlignment.Left;
                            cellval.Margin = new Thickness(2, 0, 0, 0);
                            sp.Children.Add(cellval);

                            Button btn = new Button();
                            btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            btn.ToolTip = "电压信号线";
                            btn.ToolTip = (string)page.Resources["volxhx"];
                            btn.Margin = new Thickness(2, 0, 10, 0);
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                            sp.Children.Add(btn);
                            spLine.Children.Add(sp);
                            if (i != 0 && (i + 1) % 8 == 0)
                            {
                                cellsp.Children.Add(spLine);
                                spLine = new StackPanel();
                                spLine.Orientation = Orientation.Horizontal;
                            }
                        }
                        if (receiveBmuList[a].CellmodelDcounts % 8 != 0)
                        {
                            cellsp.Children.Add(spLine);
                        }

                        //子模块E
                        VNum += receiveBmuList[a].CellmodelEcounts;
                        spLine = new StackPanel();
                        spLine.Orientation = Orientation.Horizontal;
                        StackPanel line_E = new StackPanel();
                        line_E.Orientation = Orientation.Horizontal;
                        TextBlock moduleE = new TextBlock();
                        moduleE.Width = 100;
                        moduleE.FontSize = 13;
                        moduleE.TextAlignment = TextAlignment.Left;
                        moduleE.Text = "Module_E:";
                        moduleE.Name = "Module_E" + receiveBmuList[a].Bmuindex;

                        moduleE.Visibility = Visibility.Collapsed;

                        line_E.Children.Add(moduleE);
                        cellsp.Children.Add(line_E);
                        for (int i = 0; i < receiveBmuList[a].CellmodelEcounts; i++)
                        {
                            if (i + receiveBmuList[a].CellmodelAcounts + receiveBmuList[a].CellmodelBcounts + receiveBmuList[a].CellmodelCcounts + receiveBmuList[a].CellmodelDcounts >= receiveBmuList[a].Cellcouts)
                            {
                                break;
                            }
                            StackPanel sp = new StackPanel();
                            sp.Margin = new Thickness(0, 0, 0, 5);
                            sp.Orientation = Orientation.Horizontal;
                            //for (int j = 0; j < linecounts; j++)
                            //{
                            TextBlock cellltb = new TextBlock();
                            //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                            //cellltb.Foreground = new SolidColorBrush(color); 
                            cellltb.Width = 50;
                            cellltb.FontSize = 13;
                            cellltb.TextAlignment = TextAlignment.Right;
                            cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                            cellltb.Text = "V" + (VNum + i) + ":";
                            cellltb.VerticalAlignment = VerticalAlignment.Center;
                            sp.Children.Add(cellltb);

                            TextBox cellval = new TextBox();
                            cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            cellval.IsReadOnly = true;
                            cellval.Text = "0.00";
                            cellval.Width = 45;
                            cellval.FontSize = 13;
                            cellval.TextAlignment = TextAlignment.Left;
                            cellval.HorizontalAlignment = HorizontalAlignment.Left;
                            cellval.Margin = new Thickness(2, 0, 0, 0);
                            sp.Children.Add(cellval);

                            Button btn = new Button();
                            btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (VNum + i);
                            btn.ToolTip = "电压信号线";
                            btn.ToolTip = (string)page.Resources["volxhx"];
                            btn.Margin = new Thickness(2, 0, 10, 0);
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                            sp.Children.Add(btn);
                            spLine.Children.Add(sp);
                            if (i != 0 && (i + 1) % 8 == 0)
                            {
                                cellsp.Children.Add(spLine);
                                spLine = new StackPanel();
                                spLine.Orientation = Orientation.Horizontal;
                            }
                        }
                        if (receiveBmuList[a].CellmodelEcounts % 8 != 0)
                        {
                            cellsp.Children.Add(spLine);
                        }

                        //空行
                        StackPanel blankLine = new StackPanel();
                        blankLine.Height = 20;
                        cellsp.Children.Add(blankLine);

                        int linecounts = 8;
                        int show16cell = receiveBmuList[a].Cellcouts / linecounts;
                        int show16leftcell = receiveBmuList[a].Cellcouts % linecounts;
                        ////每8个显示一行
                        //for (int t = 0; t < show16cell; t++)
                        //{
                        //    StackPanel sp = new StackPanel();
                        //    sp.Margin = new Thickness(0, 0, 0, 5);
                        //    sp.Orientation = Orientation.Horizontal;
                        //    for (int j = 0; j < linecounts; j++)
                        //    {
                        //        TextBlock cellltb = new TextBlock();
                        //        //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                        //        //cellltb.Foreground = new SolidColorBrush(color); 
                        //        cellltb.Width = 50;
                        //        cellltb.FontSize = 13;
                        //        cellltb.TextAlignment = TextAlignment.Right;
                        //        cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                        //        cellltb.Text = "V" + (t * linecounts + (j + 1)) + ":";
                        //        cellltb.VerticalAlignment = VerticalAlignment.Center;
                        //        sp.Children.Add(cellltb);

                        //        TextBox cellval = new TextBox();
                        //        cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (t * linecounts + (j + 1));
                        //        cellval.IsReadOnly = true;
                        //        cellval.Text = "0.00";
                        //        cellval.Width = 45;
                        //        cellval.FontSize = 13;
                        //        cellval.TextAlignment = TextAlignment.Left;
                        //        cellval.HorizontalAlignment = HorizontalAlignment.Left;
                        //        cellval.Margin = new Thickness(2, 0, 0, 0);
                        //        sp.Children.Add(cellval);

                        //        Button btn = new Button();
                        //        btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (t * linecounts + (j + 1));
                        //        btn.ToolTip = "电压信号线";
                        //        btn.ToolTip = (string)page.Resources["volxhx"];
                        //        btn.Margin = new Thickness(2, 0, 10, 0);
                        //        btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                        //        sp.Children.Add(btn);
                        //    }
                        //    cellsp.Children.Add(sp);

                        //}

                        //StackPanel spleft = new StackPanel();
                        //spleft.Margin = new Thickness(0, 0, 0, 5);
                        //spleft.Orientation = Orientation.Horizontal;

                        ////不足8个单体显示
                        //for (int t = 0; t < show16leftcell; t++)
                        //{
                        //    TextBlock cellltb = new TextBlock();
                        //    //Color color = (Color)ColorConverter.ConvertFromString("#5bc0de");
                        //    //cellltb.Foreground = new SolidColorBrush(color); 
                        //    cellltb.Width = 50;
                        //    cellltb.FontSize = 13;
                        //    cellltb.TextAlignment = TextAlignment.Right;
                        //    cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                        //    cellltb.Text = "V" + (show16cell * linecounts + (t + 1)).ToString() + ":";
                        //    cellltb.VerticalAlignment = VerticalAlignment.Center;
                        //    spleft.Children.Add(cellltb);

                        //    TextBox cellval = new TextBox();
                        //    cellval.IsReadOnly = true;
                        //    cellval.Name = "V" + receiveBmuList[a].Bmuindex + "_" + (show16cell * linecounts + (t + 1)).ToString();
                        //    cellval.Text = "0.00";
                        //    cellval.Width = 45;
                        //    cellval.FontSize = 13;
                        //    cellval.TextAlignment = TextAlignment.Left;
                        //    cellval.HorizontalAlignment = HorizontalAlignment.Left;
                        //    cellval.Margin = new Thickness(2, 0, 0, 0);
                        //    spleft.Children.Add(cellval);

                        //    Button btn = new Button();
                        //    btn.Name = "SLS" + receiveBmuList[a].Bmuindex + "_" + (show16cell * linecounts + (t + 1)).ToString();
                        //    btn.Margin = new Thickness(2, 0, 10, 0);
                        //    btn.ToolTip = "电压信号线";
                        //    btn.ToolTip = (string)page.Resources["volxhx"];
                        //    btn.SetValue(Button.StyleProperty, Application.Current.Resources["BatteryDisableButton"]);
                        //    spleft.Children.Add(btn);

                        //}
                        //cellsp.Children.Add(spleft);

                        //温感信息
                        int show8tmp = receiveBmuList[a].Tempcounts / linecounts;
                        int show8leftmp = receiveBmuList[a].Tempcounts % linecounts;
                        for (int t = 0; t < show8tmp; t++)
                        {
                            StackPanel sptemp = new StackPanel();
                            sptemp.Margin = new Thickness(0, 0, 0, 5);
                            sptemp.Orientation = Orientation.Horizontal;
                            for (int j = 0; j < linecounts; j++)
                            {
                                TextBlock temptb = new TextBlock();
                                //Color color = (Color)ColorConverter.ConvertFromString("#FFF9241A");
                                //temptb.Foreground = new SolidColorBrush(color); 
                                temptb.Width = 50;
                                temptb.FontSize = 13;
                                temptb.TextAlignment = TextAlignment.Right;
                                temptb.HorizontalAlignment = HorizontalAlignment.Right;
                                temptb.Text = "T" + (t * linecounts + j + 1).ToString() + ":";
                                temptb.VerticalAlignment = VerticalAlignment.Center;
                                sptemp.Children.Add(temptb);

                                TextBox tempval = new TextBox();
                                tempval.IsReadOnly = true;
                                tempval.Name = "T" + receiveBmuList[a].Bmuindex + "_" + (t * linecounts + j + 1).ToString();
                                tempval.Text = "0";
                                tempval.Width = 45;
                                tempval.FontSize = 13;
                                tempval.TextAlignment = TextAlignment.Left;
                                tempval.HorizontalAlignment = HorizontalAlignment.Left;
                                tempval.Margin = new Thickness(2, 0, 0, 0);
                                sptemp.Children.Add(tempval);

                                Button btn = new Button();
                                btn.Name = "PLS" + receiveBmuList[a].Bmuindex + "_" + (t * linecounts + (j + 1)).ToString();
                                btn.Margin = new Thickness(2, 0, 10, 0);
                                btn.ToolTip = "温感信号线";
                                btn.ToolTip = (string)page.Resources["temxhx"];
                                btn.SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                sptemp.Children.Add(btn);
                            }
                            cellsp.Children.Add(sptemp);

                        }

                        //不足8个温感信息显示
                        if (show8leftmp > 0)
                        {
                            StackPanel sptempleft = new StackPanel();
                            sptempleft.Margin = new Thickness(0, 0, 0, 5);
                            sptempleft.Orientation = Orientation.Horizontal;
                            //温感数据
                            for (int t = 0; t < show8leftmp; t++)
                            {
                                TextBlock temptb = new TextBlock();
                                //Color color = (Color)ColorConverter.ConvertFromString("#FFF9241A");
                                //temptb.Foreground = new SolidColorBrush(color); 
                                temptb.Width = 50;
                                temptb.FontSize = 13;
                                temptb.TextAlignment = TextAlignment.Right;
                                temptb.HorizontalAlignment = HorizontalAlignment.Right;
                                temptb.Text = "T" + (show8tmp * linecounts + t + 1).ToString() + ":";
                                temptb.VerticalAlignment = VerticalAlignment.Center;
                                sptempleft.Children.Add(temptb);

                                TextBox tempval = new TextBox();
                                tempval.IsReadOnly = true;
                                tempval.Name = "T" + receiveBmuList[a].Bmuindex + "_" + (show8tmp * linecounts + t + 1).ToString();
                                tempval.Text = "0";
                                tempval.Width = 45;
                                tempval.FontSize = 13;
                                tempval.TextAlignment = TextAlignment.Left;
                                tempval.HorizontalAlignment = HorizontalAlignment.Left;
                                tempval.Margin = new Thickness(2, 0, 0, 0);
                                sptempleft.Children.Add(tempval);

                                Button btn = new Button();
                                btn.Name = "PLS" + receiveBmuList[a].Bmuindex + "_" + (show8tmp * linecounts + t + 1).ToString();
                                btn.Margin = new Thickness(2, 0, 10, 0);
                                btn.ToolTip = "温感信号线";
                                btn.ToolTip = (string)page.Resources["temxhx"];
                                btn.SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                                sptempleft.Children.Add(btn);

                            }

                            

                            cellsp.Children.Add(sptempleft);
                        }


                        //TMOS温感
                        StackPanel sptempmos = new StackPanel();
                        sptempmos.Margin = new Thickness(0, 0, 0, 5);
                        sptempmos.Orientation = Orientation.Horizontal;
                        //温感数据TMOS1-TMOS6
                        for (int t = 0; t < 6; t++)
                        {
                            TextBlock temptb = new TextBlock();
                            //Color color = (Color)ColorConverter.ConvertFromString("#FFF9241A");
                            //temptb.Foreground = new SolidColorBrush(color); 
                            temptb.Width = 50;
                            temptb.FontSize = 13;
                            temptb.TextAlignment = TextAlignment.Right;
                            temptb.HorizontalAlignment = HorizontalAlignment.Right;
                            temptb.Text = "TMOS" + (t + 1).ToString() + ":";
                            temptb.VerticalAlignment = VerticalAlignment.Center;
                            sptempmos.Children.Add(temptb);

                            TextBox tempval = new TextBox();
                            tempval.IsReadOnly = true;
                            tempval.Name = "TM" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            tempval.Text = "0";
                            tempval.Width = 45;
                            tempval.FontSize = 13;
                            tempval.TextAlignment = TextAlignment.Left;
                            tempval.HorizontalAlignment = HorizontalAlignment.Left;
                            tempval.Margin = new Thickness(2, 0, 0, 0);
                            sptempmos.Children.Add(tempval);

                            Button btn = new Button();
                            btn.Name = "MOS" + receiveBmuList[a].Bmuindex + "_" + (t + 1).ToString();
                            btn.Margin = new Thickness(2, 0, 10, 0);
                            btn.ToolTip = "温感信号线";
                            btn.ToolTip = (string)page.Resources["temxhx"];
                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                            sptempmos.Children.Add(btn);

                        }
                        //温感TCPU
                        TextBlock tempcpu = new TextBlock();
                        //Color color = (Color)ColorConverter.ConvertFromString("#FFF9241A");
                        //temptb.Foreground = new SolidColorBrush(color); 
                        tempcpu.Width = 50;
                        tempcpu.FontSize = 13;
                        tempcpu.TextAlignment = TextAlignment.Right;
                        tempcpu.HorizontalAlignment = HorizontalAlignment.Right;
                        tempcpu.Text = "TCPU" + ":";
                        tempcpu.VerticalAlignment = VerticalAlignment.Center;
                        sptempmos.Children.Add(tempcpu);

                        TextBox tempcpuval = new TextBox();
                        tempcpuval.IsReadOnly = true;
                        tempcpuval.Name = "TC" + receiveBmuList[a].Bmuindex;
                        tempcpuval.Text = "0";
                        tempcpuval.Width = 45;
                        tempcpuval.FontSize = 13;
                        tempcpuval.TextAlignment = TextAlignment.Left;
                        tempcpuval.HorizontalAlignment = HorizontalAlignment.Left;
                        tempcpuval.Margin = new Thickness(2, 0, 0, 0);
                        sptempmos.Children.Add(tempcpuval);

                        Button btncpu = new Button();
                        btncpu.Name = "CPU" + receiveBmuList[a].Bmuindex;
                        btncpu.Margin = new Thickness(2, 0, 10, 0);
                        btncpu.ToolTip = "温感信号线";
                        btncpu.ToolTip = (string)page.Resources["temxhx"];
                        btncpu.SetValue(Button.StyleProperty, Application.Current.Resources["TmpDisableButton"]);
                        sptempmos.Children.Add(btncpu);


                        cellsp.Children.Add(sptempmos);

                        //创建版本号信息
                        GroupBox bmugb0 = new GroupBox();
                        bmugb0.Header = "版本信息";
                        bmugb0.Header = (string)page.Resources["version"];
                        bmugb0.Margin = new Thickness(0, 0, 0, 5);
                        Color color0 = (Color)ColorConverter.ConvertFromString("#5bc0de");
                        bmugb0.BorderBrush = new SolidColorBrush(color0);
                        bmugb0.BorderThickness = new Thickness(1);
                        //WrapPanel wp = new WrapPanel();
                        //wp.Orientation = Orientation.Horizontal;
                        //wp.HorizontalAlignment = HorizontalAlignment.Left;
                        //wp.Width = 1300;
                        //Label lb = new Label();
                        //lb.Width = 1000;
                       // tb0.Text = receiveBmuList[a].Version;
                        TextBlock tb = new TextBlock();

                        bmugb0.Content = tb;
                     
                        rootsp.Children.Add(bmugb0);

                        sc.Content = rootsp;


                        ti.Content = sc;
                        tc.Items.Add(ti);
                    }
                    page.grid.Children.Add(tc);

                });
                isCreateBmuFlag = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void runSendBalanceThread(object obj)
        {
            ushort heartbeat = 0;
            TabItem item = obj as TabItem;
            while (true)
            {
                //解析界面元素
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    try
                    {
                        CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                        data.Init();
                        data.RemoteFlag = 0;//数据帧
                        data.ExternFlag = 1;//扩展帧
                        data.SendType = 0;//正常发送
                        data.DataLen = 8;
                        if (item != null)
                        {
                            int bmuindex = (int)item.Tag;
                            string balancehandinfo = "0C11" + bmuindex.ToString("X2") + "41";
                            data.ID = uint.Parse(balancehandinfo, System.Globalization.NumberStyles.HexNumber);
                            ScrollViewer sc = item.Content as ScrollViewer;
                            StackPanel parent = sc.Content as StackPanel;
                            TabControl balancetc = parent.Children[1] as TabControl;
                            TabItem handbalanceti = balancetc.Items[1] as TabItem;
                            GroupBox handgroupbox = handbalanceti.Content as GroupBox;
                            //GroupBox handgroupbox = handbalance.Content as GroupBox;
                            List<TextBox> cell_textboxList = MyVisualTreeHelper.FindVisualChild<TextBox>(handgroupbox);
                            List<ComboBox> cell_comboboxList = MyVisualTreeHelper.FindVisualChild<ComboBox>(handgroupbox);
                            List<RadioButton> celll_radioButtonList = MyVisualTreeHelper.FindVisualChild<RadioButton>(handgroupbox);
                            if (heartbeat > 255) heartbeat = 0;
                            data.Data[0] = (byte)heartbeat;
                            var queryRadioButton = from tt in celll_radioButtonList where tt.Name.Contains("modelA_" + bmuindex.ToString()) && tt.IsChecked == true select tt;
                            if (queryRadioButton != null && queryRadioButton.Count() > 0)
                            {
                                RadioButton rb = queryRadioButton.First() as RadioButton;
                                string name = rb.Name;//modelA_1_14
                                //解析出单体序号
                                string[] balanceArr = name.Split('_');
                                string model = balanceArr[0];
                                string bumnum = balanceArr[1];
                                ushort cellnum = ushort.Parse(balanceArr[2]);
                                byte[] cellnumbytes = System.BitConverter.GetBytes(cellnum);
                                data.Data[1] = (byte)(cellnumbytes[0] & 0x3f);
                            }

                            var queryComboBoxA = from tt in cell_comboboxList where tt.Name.Contains("handmodelAbalancecb_" + bmuindex.ToString()) select tt;
                            if (queryComboBoxA != null && queryComboBoxA.Count() > 0)
                            {
                                ComboBox cb = queryComboBoxA.First() as ComboBox;
                                ushort selectindex = (ushort)cb.SelectedIndex;
                                byte[] selectindexbytes = System.BitConverter.GetBytes(selectindex);

                                data.Data[1] = (byte)(data.Data[1] | ((selectindexbytes[0] & 0x03) << 6));
                            }

                            var queryTextBox = from tt in cell_textboxList where tt.Name == "handmodelAcurrentx_" + bmuindex.ToString() select tt;
                            if (queryTextBox != null && queryTextBox.Count() > 0)
                            {
                                TextBox tb = queryTextBox.First() as TextBox;
                                float modelAcurrent = 0;
                                if (float.TryParse(tb.Text, out modelAcurrent))
                                {
                                    data.Data[2] = (byte)((byte)((int)modelAcurrent / 0.05) & 0xff);
                                }
                                else
                                {
                                    data.Data[2] = 0x00;
                                }
                            }
                            //modelB
                            var queryRadioButtonB = from tt in celll_radioButtonList where tt.Name.Contains("modelB_" + bmuindex.ToString()) && tt.IsChecked == true select tt;
                            if (queryRadioButtonB != null && queryRadioButtonB.Count() > 0)
                            {
                                RadioButton rb = queryRadioButtonB.First() as RadioButton;
                                string name = rb.Name;//modelA_1_14
                                //解析出单体序号
                                string[] balanceArr = name.Split('_');
                                string model = balanceArr[0];
                                string bumnum = balanceArr[1];
                                ushort cellnum = ushort.Parse(balanceArr[2]);
                                byte[] cellnumbytes = System.BitConverter.GetBytes(cellnum);
                                data.Data[3] = (byte)(cellnumbytes[0] & 0x3f);
                            }

                            var queryComboBoxB = from tt in cell_comboboxList where tt.Name.Contains("handmodelBbalancecb_" + bmuindex.ToString()) select tt;
                            if (queryComboBoxB != null && queryComboBoxB.Count() > 0)
                            {
                                ComboBox cb = queryComboBoxB.First() as ComboBox;
                                ushort selectindex = (ushort)cb.SelectedIndex;
                                byte[] selectindexbytes = System.BitConverter.GetBytes(selectindex);
                                data.Data[3] = (byte)(data.Data[3] | ((selectindexbytes[0] & 0x03) << 6));
                            }

                            var queryTextBoxB = from tt in cell_textboxList where tt.Name == "handmodelBcurrentx_" + bmuindex.ToString() select tt;
                            if (queryTextBoxB != null && queryTextBoxB.Count() > 0)
                            {
                                TextBox tb = queryTextBoxB.First() as TextBox;
                                float modelBcurrent = 0;
                                if (float.TryParse(tb.Text, out modelBcurrent))
                                {
                                    data.Data[4] = (byte)((byte)((int)modelBcurrent / 0.05) & 0xff);
                                }
                                else
                                {
                                    data.Data[4] = 0x00;
                                }
                            }

                            //modelC
                            var queryRadioButtonC = from tt in celll_radioButtonList where tt.Name.Contains("modelC_" + bmuindex.ToString()) && tt.IsChecked == true select tt;
                            if (queryRadioButtonC != null && queryRadioButtonC.Count() > 0)
                            {
                                RadioButton rb = queryRadioButtonC.First() as RadioButton;
                                string name = rb.Name;//modelA_1_14
                                //解析出单体序号
                                string[] balanceArr = name.Split('_');
                                string model = balanceArr[0];
                                string bumnum = balanceArr[1];
                                ushort cellnum = ushort.Parse(balanceArr[2]);
                                byte[] cellnumbytes = System.BitConverter.GetBytes(cellnum);
                                data.Data[5] = (byte)(cellnumbytes[0] & 0x3f);
                            }

                            var queryComboBoxC = from tt in cell_comboboxList where tt.Name.Contains("handmodelCbalancecb_" + bmuindex.ToString()) select tt;
                            if (queryComboBoxC != null && queryComboBoxC.Count() > 0)
                            {
                                ComboBox cb = queryComboBoxB.First() as ComboBox;
                                ushort selectindex = (ushort)cb.SelectedIndex;
                                byte[] selectindexbytes = System.BitConverter.GetBytes(selectindex);
                                data.Data[5] = (byte)(data.Data[5] | ((selectindexbytes[0] & 0x03) << 6));
                            }

                            var queryTextBoxC = from tt in cell_textboxList where tt.Name == "handmodelCcurrentx_" + bmuindex.ToString() select tt;
                            if (queryTextBoxC != null && queryTextBoxC.Count() > 0)
                            {
                                TextBox tb = queryTextBoxC.First() as TextBox;

                                float modelCcurrent = 0;
                                if (float.TryParse(tb.Text, out modelCcurrent))
                                {
                                    data.Data[6] = (byte)((byte)((int)modelCcurrent / 0.05) & 0xff);
                                }
                                else
                                {
                                    data.Data[6] = 0x00;
                                }

                            }

                            //BMU工作模式
                            var queryComboBoxBMU = from tt in cell_comboboxList where tt.Name.Contains("handbmuworkerstatuscb_" + bmuindex.ToString()) select tt;
                            if (queryComboBoxBMU != null && queryComboBoxBMU.Count() > 0)
                            {
                                ComboBox cb = queryComboBoxBMU.First() as ComboBox;
                                ushort selectindex = (ushort)cb.SelectedIndex;
                                byte[] selectindexbytes = System.BitConverter.GetBytes(selectindex);
                                data.Data[7] = (byte)(selectindexbytes[0] & 0x01);
                            }

                            //整车工作模式
                            var queryComboBoxCAR = from tt in cell_comboboxList where tt.Name.Contains("activemvcustatusnumtx_" + bmuindex.ToString()) select tt;
                            if (queryComboBoxCAR != null && queryComboBoxCAR.Count() > 0)
                            {
                                ComboBox cb = queryComboBoxCAR.First() as ComboBox;
                                ushort selectindex = (ushort)cb.SelectedIndex;
                                byte[] selectindexbytes = System.BitConverter.GetBytes(selectindex);
                                data.Data[7] = (byte)((selectindexbytes[0] & 0x03) >> 1);
                            }
                            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                            heartbeat++;
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                });

                Thread.Sleep(1000);//延迟1秒
            }
        }

        /// <summary>
        /// zhengzhonghua
        /// 2018-11-15
        /// 接收从机模块3均衡信息 上位机解析从机均衡信息
        /// </summary>
        /// <param name="obj"></param>
        public void receiveBalanceInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                Regex rinfo = new Regex(bmubalanceactiveinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    string box = canidhex.Substring(6, 2);
                    int bumbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);//bmu箱号
                    int modelnum = int.Parse(canidhex.Substring(2, 2));//获取模块编号值
                    float setbalanceaol = (float)(obj.Data[0] - 100) * 0.05f;
                    float getbalanceaol = (float)(obj.Data[1] - 100) * 0.05f;
                    int setcellnum = obj.Data[2];
                    int getcellnum = obj.Data[3];
                    ushort cellhighabs = (ushort)(((obj.Data[5] & 0x00ff) << 8) | (obj.Data[4] & 0x00ff));
                    double cellhighabsshow = cellhighabs * 0.001;
                    ushort celllowabs = (ushort)(((obj.Data[7] & 0x00ff) << 8) | (obj.Data[6] & 0x00ff));
                    double celllowabsshow = celllowabs * 0.001;

                    //查找界面元素
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == bumbox)
                                {  //查找对应的BMU从机号
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    TabControl tabCtrl = parent.Children[1] as TabControl;
                                    TabItem tabItem = tabCtrl.Items[0] as TabItem;
                                    GroupBox gb = tabItem.Content as GroupBox; //bmu均衡信息
                                    List<TextBox> celll_textList = MyVisualTreeHelper.FindVisualChild<TextBox>(gb);
                                    if (celll_textList != null)
                                    {
                                        var query1 = from tt in celll_textList where tt.Name == "setbalanceaoltx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query1 != null && query1.Count() > 0)
                                            query1.First().Text = string.Format("{0:f3}", setbalanceaol);

                                        var query2 = from tt in celll_textList where tt.Name == "getbalanceaoltx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query2 != null && query2.Count() > 0)
                                            query2.First().Text = string.Format("{0:f3}", getbalanceaol);

                                        var query3 = from tt in celll_textList where tt.Name == "setcellnumtx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query3 != null && query3.Count() > 0)
                                            query3.First().Text = setcellnum.ToString();

                                        var query4 = from tt in celll_textList where tt.Name == "getcellnumtx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query4 != null && query4.Count() > 0)
                                            query4.First().Text = getcellnum.ToString();


                                        var query5 = from tt in celll_textList where tt.Name == "cellhighabstx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query5 != null && query5.Count() > 0)
                                            query5.First().Text = string.Format("{0:f3}", cellhighabsshow);

                                        var query6 = from tt in celll_textList where tt.Name == "celllowabstx" + modelnum.ToString() + "_" + bumbox select tt;
                                        if (query6 != null && query6.Count() > 0)
                                            query6.First().Text = string.Format("{0:f3}", celllowabsshow);
                                    }


                                }

                            }
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

            }

        }

        /// <summary>
        /// 接收从机版本号信息，一个BMU可能存在多个版本号
        /// </summary>
        /// <param name="obj"></param>
        public void receiveBMUVersionInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            uint canid = obj.ID;
            byte[] intBuff = BitConverter.GetBytes(canid);
            string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
            Regex rinfo = new Regex(bmuversioninfo);
            if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
            {
                byte[] d=obj.Data;
                //获取BMU从机号
                string index = canidhex.Substring(6, 2);
                int bmuindex = Int32.Parse(index, System.Globalization.NumberStyles.HexNumber);
                var q = from tt in receiveBmuList where tt.Bmuindex == bmuindex select tt;
                if (q != null && q.Count() > 0)
                {
                    BMUConfigModel bc= q.First();
                    String stemp= DataConverter.bytestoString(d.Skip(1).Take(6).ToArray());
                    if (stemp != null && stemp != "")
                    {
                        switch (d[0])
                        {
                            case 0x01:
                                bc.VersionA1 = stemp;
                                break;
                            case 0x02:
                                bc.VersionA2 = stemp;
                                break;
                            case 0x03:
                                bc.VersionB1 = stemp;
                                break;
                            case 0x04:
                                bc.VersionB2 = stemp;
                                break;
                            case 0x05:
                                bc.VersionC1 = stemp;
                                break;
                            case 0x06:
                                bc.VersionC2 = stemp;
                                break;
                            case 0x11:
                                bc.Monitor1 = stemp;
                                break;
                            case 0x12:
                                bc.Monitor2 = stemp;
                                break;
                            case 0x21:
                                bc.Boota1 = stemp;
                                break;
                            case 0x22:
                                bc.Boota2 = stemp;
                                break;
                        }
                    }
                   // Console.WriteLine(bc);                    
                }
            }
        }

        /// <summary>
        /// 主机发送远程帧，从机发送数据帧 上位机解析从机数据帧结果
        /// </summary>
        /// <param name="obj"></param>
        public void receiveBMUBalanceInfo(CANSDK.VCI_CAN_OBJ obj)
        {
            if (obj.DataLen == 0) { return; }
            try
            {
                uint canid = obj.ID;
                byte[] intBuff = BitConverter.GetBytes(canid);
                string canidhex = HexCommon.byteToHexStr(intBuff);//CAN ID 16进制表示方式
                Regex rinfo = new Regex(bmubalanceinfo);
                if (rinfo.IsMatch(canidhex) && obj.RemoteFlag == 0)
                {
                    string box = canidhex.Substring(6, 2);
                    int bumbox = Int32.Parse(box, System.Globalization.NumberStyles.HexNumber);//bmu箱号
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (page.grid.Children.Count > 0)
                        {
                            TabControl tc = page.grid.Children[0] as TabControl;
                            foreach (var tabitem in tc.Items)
                            {
                                TabItem item = tabitem as TabItem;
                                if ((int)item.Tag == bumbox)
                                {
                                    //查找对应的BMU从机号
                                    ScrollViewer sc = item.Content as ScrollViewer;
                                    StackPanel parent = sc.Content as StackPanel;
                                    TabControl tabCtrl = parent.Children[1] as TabControl;
                                    TabItem tabItem = tabCtrl.Items[0] as TabItem;
                                    GroupBox gb = tabItem.Content as GroupBox; //bmu均衡信息
                                    List<TextBox> celll_textList = MyVisualTreeHelper.FindVisualChild<TextBox>(gb);
                                    if (celll_textList != null)
                                    {
                                        for (int i = 1; i < 4; i++)
                                        {
                                            var query1 = from tt in celll_textList where tt.Name == "activemcellnumtx" + i.ToString() + "_" + bumbox select tt;
                                            if (query1 != null && query1.Count() > 0)
                                            {
                                                int modelcell1 = (obj.Data[1 + (i - 1) * 2] & 0x3F);
                                                query1.First().Text = modelcell1.ToString();
                                            }

                                            var query2 = from tt in celll_textList where tt.Name == "activemchargenumtx" + i.ToString() + "_" + bumbox select tt;
                                            if (query2 != null && query2.Count() > 0)
                                            {
                                                int modelaolstatus1 = (obj.Data[1 + (i - 1) * 2] & 0xC0) >> 6;
                                                switch (modelaolstatus1)
                                                {
                                                    case 0:
                                                        query2.First().Text = "不动作";
                                                        break;
                                                    case 1:
                                                        query2.First().Text = "放电";
                                                        break;
                                                    case 2:
                                                        query2.First().Text = "充电";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            var query3 = from tt in celll_textList where tt.Name == "activemchargeaolnumtx" + i.ToString() + "_" + bumbox select tt;
                                            if (query3 != null && query3.Count() > 0)
                                            {
                                                float modelaol1 = (float)(obj.Data[2 + (i - 1) * 2] * 0.05);
                                                query3.First().Text = string.Format("{0:f3}", modelaol1);
                                            }

                                            var query4 = from tt in celll_textList where tt.Name == "activembmustatusnumtx_" + bumbox select tt;
                                            if (query4 != null && query4.Count() > 0)
                                            {
                                                int bmustatus = obj.Data[7] & 0x01;
                                                switch (bmustatus)
                                                {
                                                    case 0:
                                                        query4.First().Text = "自动均衡";
                                                        break;
                                                    case 1:
                                                        query4.First().Text = "手动均衡";
                                                        break;
                                                }
                                            }

                                            var query5 = from tt in celll_textList where tt.Name == "activemvcustatusnumtx_" + bumbox select tt;
                                            if (query5 != null && query5.Count() > 0)
                                            {
                                                int vcustatus = (obj.Data[7] & 0x06) >> 1;
                                                switch (vcustatus)
                                                {
                                                    case 0:
                                                        query5.First().Text = "静置";
                                                        break;
                                                    case 1:
                                                        query5.First().Text = "放电";
                                                        break;
                                                    case 2:
                                                        query5.First().Text = "充电";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    });


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }



        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        private void runDelBMUCommand(Button button)
        {
            if (page.grid.Children.Count > 0)
            {
                page.grid.Children.Clear();
                receiveBmuList.Clear();
            }
        }




        private void sendBmuInfo(Object bmuindex)
        {

            while (true)
            {

                if (IsSendBmuInfo)
                {
                    if (receiveBmuList != null)
                    {
                        for (int i = 0; i < receiveBmuList.Count; i++)
                        {
                            //发送远程BMU总体信息
                            CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                            string s = "0C0041" + receiveBmuList[i].Bmuindex.ToString("X2");
                            data.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
                            data.RemoteFlag = 1;//远程帧
                            data.ExternFlag = 1;//扩展帧
                            data.SendType = 0;//正常发送
                            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                            Thread.Sleep(100);

                            //发送远程BMU总体信息
                            s = "0C0141" + receiveBmuList[i].Bmuindex.ToString("X2");
                            data.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
                            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                            Thread.Sleep(100);
                        }

                        if (receiveBmuList.Count() == 0)
                        {
                            Thread.Sleep(1000);
                        }

                    }


                }
                else
                {
                    break;
                }

            }
        }


        /// <summary>
        /// 模拟bms发送单体远程帧
        /// </summary>
        private void sendCellInfo()
        {
            while (true)
            {
                if (IsSendBmuCellInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                int cellcounts = receiveBmuList[i].Cellcouts;
                                int cellnums = cellcounts / 4;     //计算4的倍数
                                int cellnumsleft = cellcounts % 4; //计算4的余数
                                for (int t = 1; t < cellnums + 1; t++)
                                {
                                    CANSDK.VCI_CAN_OBJ celldata = new CANSDK.VCI_CAN_OBJ();
                                    string cellid = "18" + t.ToString("X2") + "41" + receiveBmuList[i].Bmuindex.ToString("X2");
                                    celldata.ID = uint.Parse(cellid, System.Globalization.NumberStyles.HexNumber);
                                    celldata.RemoteFlag = 1;//远程帧
                                    celldata.ExternFlag = 1;//扩展帧
                                    celldata.SendType = 0;//正常发送
                                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref celldata, 1);
                                    Thread.Sleep(100);
                                }

                                if (cellnumsleft != 0)
                                {
                                    CANSDK.VCI_CAN_OBJ celldata = new CANSDK.VCI_CAN_OBJ();
                                    string cellid = "18" + (cellnums + 1).ToString("X2") + "41" + receiveBmuList[i].Bmuindex.ToString("X2");
                                    celldata.ID = uint.Parse(cellid, System.Globalization.NumberStyles.HexNumber);
                                    celldata.RemoteFlag = 1;//远程帧
                                    celldata.ExternFlag = 1;//扩展帧
                                    celldata.SendType = 0;//正常发送
                                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref celldata, 1);
                                    Thread.Sleep(100);
                                }
                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            }
        }

        /// <summary>
        /// 温度信息
        /// </summary>
        /// <param name="bmucount"></param>
        private void sendTmpInfo()
        {
            while (true)
            {

                if (IsSendBmuTempInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                for (int t = 16; t <= 18; t++)
                                {
                                    CANSDK.VCI_CAN_OBJ tempdata = new CANSDK.VCI_CAN_OBJ();
                                    string cellid = "18" + t.ToString("X2") + "41" + receiveBmuList[i].Bmuindex.ToString("X2");
                                    tempdata.ID = uint.Parse(cellid, System.Globalization.NumberStyles.HexNumber);
                                    tempdata.RemoteFlag = 1;//远程帧
                                    tempdata.ExternFlag = 1;//扩展帧
                                    tempdata.SendType = 0;//正常发送
                                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref tempdata, 1);
                                    Thread.Sleep(100);
                                }
                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }



                }
                else
                    break;
            }



        }


        /// <summary>
        /// zhengzhonghua
        /// 2018-05-16
        /// 发送远程电压信号线状态
        /// </summary>
        /// <param name="bmucount"></param>
        private void sendSLSInfo()
        {
            while (true)
            {
                if (IsSendBmuSLSInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                                string sls = "181641" + receiveBmuList[i].Bmuindex.ToString("X2");
                                data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                                data.RemoteFlag = 1;//远程帧
                                data.ExternFlag = 1;//扩展帧
                                data.SendType = 0;//正常发送
                                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                                Thread.Sleep(100);
                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                }
                else
                    break;
            }
        }


        private void sendSLSTMPInfo()
        {

            while (true)
            {
                if (IsSendBmuSLSTMPInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                                string sls = "181541" + receiveBmuList[i].Bmuindex.ToString("X2");
                                data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                                data.RemoteFlag = 1;//远程帧
                                data.ExternFlag = 1;//扩展帧
                                data.SendType = 0;//正常发送
                                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                                Thread.Sleep(100);
                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                }
                else
                    break;
            }

        }

        private void sendTMOSInfo()
        {

            while (true)
            {
                if (IsSendBmuTMOSInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                                string sls = "181441" + receiveBmuList[i].Bmuindex.ToString("X2");
                                data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                                data.RemoteFlag = 1;//远程帧
                                data.ExternFlag = 1;//扩展帧
                                data.SendType = 0;//正常发送
                                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                                Thread.Sleep(100);

                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                }
                else
                    break;
            }

        }

        /// <summary>
        /// 发送均衡信息
        /// </summary>
        /// <param name="obj"></param>
        private void sendBalanceInfo()
        {
            while (true)
            {
                if (IsSendBalanceInfo)
                {
                    try
                    {
                        if (receiveBmuList != null)
                        {
                            for (int i = 0; i < receiveBmuList.Count; i++)
                            {
                                for (int j = 2; j <= 4; j++)
                                {
                                    if (receiveBmuList.Count() > 0)
                                    {
                                        CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                                        string sls = "0C" + j.ToString("X2") + "41" + receiveBmuList[i].Bmuindex.ToString("X2");
                                        data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                                        data.RemoteFlag = 1;//远程帧
                                        data.ExternFlag = 1;//扩展帧
                                        data.SendType = 0;//正常发送
                                        CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                                        Thread.Sleep(100);
                                    }
                                }
                            }

                            if (receiveBmuList.Count() == 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                }
                else
                    break;
            }


        }
        private void sendVersionInfo()
        {

            //while (true)
            //{

                if (IsReadVersionInfo)
                {
                    if (receiveBmuList != null)
                    {
                        for (int i = 0; i < receiveBmuList.Count; i++)
                        {
                            //发送远程BMU总体信息
                            CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                            string s = "0C0241" + receiveBmuList[i].Bmuindex.ToString("X2");
                            data.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
                            data.RemoteFlag = 1;//远程帧
                            data.ExternFlag = 1;//扩展帧
                            data.SendType = 0;//正常发送
                            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                            Thread.Sleep(100);
                        }

                        if (receiveBmuList.Count() == 0)
                        {
                            Thread.Sleep(1000);
                        }
                        //显示到界面
                        Thread.Sleep(2000);
                        String versionNum=null;
                        for (int i = 0; i < receiveBmuList.Count; i++)
                        {
                            versionNum +=("BMU"+(i+1)+":\r\n"+ receiveBmuList[i].Version);
                            BMUConfigModel bm = receiveBmuList[i];
                            Application.Current.Dispatcher.Invoke((Action)delegate {
                                if (page.grid.Children.Count > 0)
                                {
                                    TabControl tc = page.grid.Children[0] as TabControl;
                                    foreach (var tabitem in tc.Items)
                                    { 
                                    TabItem item = tabitem as TabItem;
                                    if ((int)item.Tag == bm.Bmuindex)
                                    {
                                        //查找对应的BMU从机号
                                        ScrollViewer sc = item.Content as ScrollViewer;
                                        StackPanel parent = sc.Content as StackPanel;
                                        GroupBox gb = parent.Children[3] as GroupBox;
                                        
                                        //List<TextBlock> lbList = MyVisualTreeHelper.FindVisualChild<TextBlock>(gb);

                                        TextBlock tb = (TextBlock)gb.Content;
                                        
                                        tb.Text = bm.Version;
                                        Console.WriteLine(tb);
                                       // lbList.Clear();
                                       // Label lb=new Label();
                                       // lb.Content = bm.Version;
                                       // gb.Content = tb;
                                        //Label lb = (Label)gb.Content;
                                        //lb.Content
                                        //lb.Content = bm.Version;
                                    }
                                    }
                                }
                            });
                        }
                        if (versionNum != null && versionNum != "")
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ModernDialog.ShowMessage(versionNum, (string)page.Resources["tips"], MessageBoxButton.OK);
                            });
                        }

                    }


                }
                

        }

        private string[] titles = { "BMU编号",
                                    "电压1","电压2","电压3","电压4","电压5","电压6","电压7","电压8","电压9","电压10","电压11","电压12",
                                    "电压13","电压14","电压15","电压16","电压17","电压18","电压19","电压20","电压21","电压22","电压23","电压24",
                                    "电压25","电压26","电压27","电压28","电压29","电压30","电压31","电压32","电压33","电压34","电压35","电压36",
                                    "电压37","电压38","电压39","电压40","电压41","电压42","电压43","电压44","电压45","电压46","电压47","电压48",
                                    "电压49","电压50","电压51","电压52","电压53","电压54","电压55","电压56","电压57","电压58","电压59","电压60",
                                    "温度1","温度2","时间"
                                  };
        private DataTable InitDataTabel()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BWTID", Type.GetType("System.String"));
            dt.Columns.Add("V1", Type.GetType("System.Double"));
            dt.Columns.Add("V2", Type.GetType("System.Double"));
            //dt.Columns.Add("CODEID", Type.GetType("System.String"));
            dt.Columns.Add("V3", Type.GetType("System.Double"));
            dt.Columns.Add("V4", Type.GetType("System.Double"));
            dt.Columns.Add("V5", Type.GetType("System.Double"));
            dt.Columns.Add("V6", Type.GetType("System.Double"));
            dt.Columns.Add("V7", Type.GetType("System.Double")); //最高单体电压
            dt.Columns.Add("V8", Type.GetType("System.Double")); //最高单体电压箱号
            dt.Columns.Add("V9", Type.GetType("System.Double")); //最高单体电压节号
            dt.Columns.Add("V10", Type.GetType("System.Double")); //最低单体电压
            dt.Columns.Add("V11", Type.GetType("System.Double")); //最低单体电压箱号
            dt.Columns.Add("V12", Type.GetType("System.Double")); //最低单体电压节号
            dt.Columns.Add("V13", Type.GetType("System.Double"));
            dt.Columns.Add("V14", Type.GetType("System.Double"));
            dt.Columns.Add("V15", Type.GetType("System.Double"));
            dt.Columns.Add("V16", Type.GetType("System.Double"));
            dt.Columns.Add("V17", Type.GetType("System.Double"));
            dt.Columns.Add("V18", Type.GetType("System.Double"));
            dt.Columns.Add("V19", Type.GetType("System.Double"));
            dt.Columns.Add("V20", Type.GetType("System.Double"));
            dt.Columns.Add("V21", Type.GetType("System.Double"));
            dt.Columns.Add("V22", Type.GetType("System.Double"));
            dt.Columns.Add("V23", Type.GetType("System.Double"));
            dt.Columns.Add("V24", Type.GetType("System.Double"));
            dt.Columns.Add("V25", Type.GetType("System.Double"));
            dt.Columns.Add("V26", Type.GetType("System.Double"));
            dt.Columns.Add("V27", Type.GetType("System.Double"));
            dt.Columns.Add("V28", Type.GetType("System.Double"));
            dt.Columns.Add("V29", Type.GetType("System.Double"));
            dt.Columns.Add("V30", Type.GetType("System.Double"));
            dt.Columns.Add("V31", Type.GetType("System.Double"));
            dt.Columns.Add("V32", Type.GetType("System.Double"));
            dt.Columns.Add("V33", Type.GetType("System.Double"));
            dt.Columns.Add("V34", Type.GetType("System.Double"));
            dt.Columns.Add("V35", Type.GetType("System.Double"));
            dt.Columns.Add("V36", Type.GetType("System.Double"));
            dt.Columns.Add("V37", Type.GetType("System.Double"));
            dt.Columns.Add("V38", Type.GetType("System.Double"));
            dt.Columns.Add("V39", Type.GetType("System.Double"));
            dt.Columns.Add("V40", Type.GetType("System.Double"));
            dt.Columns.Add("V41", Type.GetType("System.Double"));
            dt.Columns.Add("V42", Type.GetType("System.Double"));
            dt.Columns.Add("V43", Type.GetType("System.Double"));
            dt.Columns.Add("V44", Type.GetType("System.Double"));
            dt.Columns.Add("V45", Type.GetType("System.Double"));
            dt.Columns.Add("V46", Type.GetType("System.Double"));
            dt.Columns.Add("V47", Type.GetType("System.Double"));
            dt.Columns.Add("V48", Type.GetType("System.Double"));
            dt.Columns.Add("V49", Type.GetType("System.Double"));
            dt.Columns.Add("V50", Type.GetType("System.Double"));
            dt.Columns.Add("V51", Type.GetType("System.Double"));
            dt.Columns.Add("V52", Type.GetType("System.Double"));
            dt.Columns.Add("V53", Type.GetType("System.Double"));
            dt.Columns.Add("V54", Type.GetType("System.Double"));
            dt.Columns.Add("V55", Type.GetType("System.Double"));
            dt.Columns.Add("V56", Type.GetType("System.Double"));
            dt.Columns.Add("V57", Type.GetType("System.Double"));
            dt.Columns.Add("V58", Type.GetType("System.Double"));
            dt.Columns.Add("V59", Type.GetType("System.Double"));
            dt.Columns.Add("V60", Type.GetType("System.Double"));
            dt.Columns.Add("W1", Type.GetType("System.Double"));
            dt.Columns.Add("W2", Type.GetType("System.Double"));
            dt.Columns.Add("Time", Type.GetType("System.String"));
            return dt;

            ////使用
            //DataRow dr = dt.NewRow();
            //dr["CELl1"] = "";
            //dt.Rows.Add(dr);

            //CsvHelper export = new CsvHelper(titles);
            //export.DataToCSV("c:\\234.csv", dt);
        }
        private System.IO.FileStream fs = null;
        private System.IO.StreamWriter sw = null;
        private DataTable dt = null;
        private CsvHelper export = null;
        //保存报文
        StreamWriter debugSW = null;
       // private List<CategoryInfo> categoryI18nList = null;

        private String labelwidth;//label宽度

        public String Labelwidth
        {
            get { return labelwidth; }
            set { labelwidth = value; OnPropertyChanged("Labelwidth"); }
        }
        private SlavePassivEquilibrium spe;

        public SlavePassivEquilibrium Spe
        {
            get { return spe; }
            set { spe = value; OnPropertyChanged("Spe"); }
        }

        //public SlavePassivEquilibrium Spe
        //{
        //    get { return spe; }
        //    set { spe = value; OnPropertyChanged("Spe"); }
        //}
       
        private int slaveGeshu = 0;
        //绘制主机均衡指令控件
        private void createMasterBalance(int balanceSlaveGeshu)
        {

            if (balanceSlaveGeshu == slaveGeshu)
            {
                //从机个数与上次一致，已经完成了绘制，直接返回
                return;
            }
            slaveGeshu = balanceSlaveGeshu;
            TabControl tc = (TabControl)page.FindName("masterbalancecontrol");
            if (balanceSlaveGeshu == 0)
            {
                tc.Visibility = Visibility.Collapsed;
                tc.Items.Clear();
                slaveBalanceModelList.Clear();
                return;
            }
            slaveBalanceModelList.Clear();
            tc.Visibility = Visibility.Visible;
            tc.Items.Clear();

            for (int i = 0; i < balanceSlaveGeshu; i++)
            {
                SlaveBalanceModel sm = new SlaveBalanceModel();
                slaveBalanceModelList.Add(sm);
                TabItem ti = new TabItem();
                ti.Header = "从机" + (i + 1);
                //Style myStyle = (Style)this.FindResource("TabItemStyle");
                ti.Style = (Style)page.FindResource("ModelTabItemStyle");
                StackPanel sp1 = new StackPanel();
                StackPanel sp = null;
                tc.Items.Add(ti);
                ti.Content = sp1;
                for (int j = 0; j < 60; j++)
                {
                    //if (j % 10 == 0)
                    if (j % 8 == 0)
                    {
                        sp = new StackPanel();
                        sp.HorizontalAlignment = HorizontalAlignment.Left;
                        sp.Orientation = Orientation.Horizontal;
                        sp.VerticalAlignment = VerticalAlignment.Top;
                        sp1.Children.Add(sp);
                    }
                    Label lb = new Label();
                    lb.Content = (string)page.Resources["danti"] + (j + 1) + "：";
                    lb.VerticalContentAlignment = VerticalAlignment.Center;
                    lb.HorizontalContentAlignment = HorizontalAlignment.Right;
                    lb.Width = 70;
                    lb.Height = 32;
                   
                    CheckBox cb = new CheckBox();
                    cb.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("SlaveBalanceModelList[" + i + "].IsBalance[" + j + "]") });
                    //cb.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("Sbm2.IsBalance[" + j + "]") });
                    //Console.WriteLine("slaveBalanceModelList[" + i + "].IsBalance[" + j + "]");
                    sp.Children.Add(lb);
                    sp.Children.Add(cb);
                }

                StackPanel sp2 = new StackPanel();
                sp2.Orientation = Orientation.Horizontal;
                sp2.HorizontalAlignment = HorizontalAlignment.Left;
                Button balanceBtn = new Button();

                Command1 = new bms.startup.command.balanceCommand(ExecuteCommand1, CanExecuteCommand1,i);

                balanceBtn.Command = Command1;
                balanceBtn.Content = (string)page.Resources["balance"];

                Label alllb = new Label();
                alllb.Content = (string)page.Resources["allselect"];
                alllb.VerticalContentAlignment = VerticalAlignment.Center;
                alllb.HorizontalContentAlignment = HorizontalAlignment.Right;
                alllb.Width = 70;
                alllb.Height = 32;
                alllb.Margin = new Thickness(20, 0, 0, 0);
                CheckBox allcb = new CheckBox(); 
                BalanceAllSelectCommand = new bms.startup.command.balanceAllSelectCommand(ExecuteBalanceAllSelectCommand, CanExecuteCommand1, allcb,i);
                allcb.Command = BalanceAllSelectCommand;
                sp2.Children.Add(balanceBtn);

                sp2.Children.Add(alllb);
                sp2.Children.Add(allcb);

                sp1.Children.Add(sp2);
            }
        }
        public ICommand Command1 { get; set; }

        public ICommand BalanceAllSelectCommand { get; set; }
        public bool CanExecuteCommand1(object parameter)
        {
            return true;
        }
        public void ExecuteBalanceAllSelectCommand(CheckBox cb,int slaveNum)
        {
           // MessageBox.Show(cb.IsChecked.ToString());
            if (cb.IsChecked == true)
            {
                slaveBalanceModelList[slaveNum].setAllTrue();
            }
            else {
                slaveBalanceModelList[slaveNum].setAllFalse();
            }
        }
        //parameter表示几号从机，0为一号从机
        public void ExecuteCommand1(int parameter)
        {

           // MessageBox.Show(parameter+"");
            SlaveBalanceModel sbm = slaveBalanceModelList[parameter];
            //CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            //obj.Init();
            //obj.RemoteFlag = 0;//数据帧
            //obj.ExternFlag = 1;//扩展帧
            //obj.SendType = 0;//正常发送
            //obj.DataLen = 8;
            //obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            //obj.DataLen = 8;
            //obj.Data = sbm.getBalanceCANSDK1(parameter);
            CANSDK.VCI_CAN_OBJ obj = sbm.getBalanceCANSDK1(parameter+1);
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Thread.Sleep(100);
            obj = sbm.getBalanceCANSDK2(parameter+1);
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }
        //创建被动均衡的控件
        private void createPasEqu() {
            StackPanel sp = (StackPanel)page.FindName("slavePasEqu");
            sp.Children.Clear();
            for (int i = 1; i < 7; i++) {
                StackPanel sp2 = new StackPanel();
                sp2.Orientation = Orientation.Horizontal;
                sp2.HorizontalAlignment = HorizontalAlignment.Left;
                sp.Children.Add(sp2);
                for (int j = 0; j < 10; j++) {
                    Label l = new Label();
                    l.Width = 100;
                    l.Content = "C" + ((i-1)*10+j+1) + "均衡状态:";
                    l.Name = "slavePE_label"+((i-1)*10+j+1);
                    CheckBox cb = new CheckBox();
                    cb.Name = "slavePE_btn" + ((i - 1) * 10 + j + 1);
                    cb.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("Spe.SlavePECheck[" + ((i - 1) * 10 + j) + "]") });
                    sp2.Children.Add(l);
                    sp2.Children.Add(cb);
                }
            }
        }

        private void runSendSPECommand() {
            CANSDK.VCI_CAN_OBJ obj = spe.getSlavePEPackage(15);
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            Console.WriteLine("发送从机被动均衡控制：" + DataConverter.byteToHexStrForData(obj.Data));
            Info = "发送从机被动均衡控制";    
        }
        //private SlaveBalanceModel sbm2;

        //public SlaveBalanceModel Sbm2
        //{
        //    get { return sbm2; }
        //    set { sbm2 = value; OnPropertyChanged("Sbm2"); }
        //}

        private void Init()
        {
            ConsoleManager.Show();//打开信息打印窗口
            //Console.WriteLine(dataCache.Length);
                                               
            categoryI18nList.Add(new CategoryInfo() { Name = "English", Value = "en_US", sourceName = "English" });
            categoryI18nList.Add(new CategoryInfo() { Name = "中文", Value = "zh_CN", sourceName = "Chinese" });

            for (int i = 0; i < 9; i++) {
                SlaveVersionList.Add("");
            }

            //for (int i = 0; i < 16; i++) {
            //    BMU_Vol[i] = new double[61];
            //    for (int j = 0; j < 61; j++) {
                    
            //        BMU_Vol[i][j] = -1000;
            //    }
                    
            //}

            for (int i = 0; i < 14; i++) {
                VersionVisibility.Add("Collapsed");
                //VersionVisibility.Add("Visible");
            }

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

            //ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            page.Resources.MergedDictionaries.Add(langRd);

            //Labelwidth = (string)page.Resources["labelwidth"];

            //ResourceDictionary langRd2 = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            //page.Resources.MergedDictionaries.Add(langRd2);

            PBValue = new ObservableCollection<int>();
            Maxpb = new ObservableCollection<int>();
            for (int i = 0; i < 48; i++)
            {
                Maxpb.Add(100);
                PBValue.Add(0);
            }
            spe = new SlavePassivEquilibrium();

            //sbm2 = new SlaveBalanceModel();

            createPasEqu();//绘制从机被动均衡控件
            createMasterBalance(2);

            
            //cbLang.ItemsSource = categoryList;//绑定数据，真正的赋值
            //cbLang.DisplayMemberPath = "Name";//指定显示的内容
            //cbLang.SelectedValuePath = "Value";//指定选中后的能够获取到的内容

            //配置线程池
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);
            //初始化主机实时时钟配置界面
            WrapPanel wp = page.FindName("relay_wp") as WrapPanel;
            wp.Children.Clear();
            for (int i = 0; i < 16; i++)
            {

                TextBlock tb = new TextBlock();
                switch (i)
                {
                    case 0:
                        tb.Text = "放电正继电器:";
                        tb.Text = (string)page.Resources["dischargez"];
                        break;
                    case 1:
                        tb.Text = "放电负继电器:";
                        tb.Text = (string)page.Resources["dischargef"];
                        break;
                    case 2:
                        tb.Text = "充电正继电器:";
                        tb.Text = (string)page.Resources["chargez"];
                        break;
                    case 3:
                        tb.Text = "充电负继电器:";
                        tb.Text = (string)page.Resources["chargef"];
                        break;
                    case 4:
                        tb.Text = "预充继电器:";
                        tb.Text = (string)page.Resources["precharge"];
                        break;
                    case 5:
                        tb.Text = "加热正继电器:";
                        tb.Text = (string)page.Resources["heatz"];
                        break;
                    case 6:
                        tb.Text = "加热负继电器:";
                        tb.Text = (string)page.Resources["heatf"];
                        break;
                    case 7:
                        tb.Text = "风扇继电器:";
                        tb.Text = (string)page.Resources["fan"];
                        break;
                    case 8:
                        tb.Text = "辅助正继电器:";
                        tb.Text = (string)page.Resources["supportz"];
                        break;
                    case 9:
                        tb.Text = "辅助负继电器:";
                        tb.Text = (string)page.Resources["supportf"];
                        break;
                    case 10:
                        tb.Text = "电子锁1:";
                        tb.Text = (string)page.Resources["key1"];
                        break;
                    case 11:
                        tb.Text = "电子锁2:";
                        tb.Text = (string)page.Resources["key2"];
                        break;
                    case 12:
                        tb.Text = "负控1:";
                        tb.Text = (string)page.Resources["fukong1"];
                        break;
                    case 13:
                        tb.Text = "负控2:";
                        tb.Text = (string)page.Resources["fukong2"];
                        break;
                    case 14:
                        tb.Text = "负控3:";
                        tb.Text = (string)page.Resources["fukong3"];
                        break;
                    case 15:
                        tb.Text = "负控4:";
                        string s = (string)page.Resources["fukong4"];
                        tb.Text = s;
                        break;
                }
                //tb.Text = "继电器" + (i + 1) + ":";
                //tb.Width = 93;
                tb.TextAlignment = TextAlignment.Right;
                tb.Width = 137;
                //tb.Width = 300;
                tb.Margin = new Thickness(3, 0, 0, 0);
                CheckBox cb = new CheckBox();

                cb.Name = "relay" + i;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(tb);
                sp.Children.Add(cb);
                wp.Children.Add(sp);
            }
            //long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;
            //string debugPath = "./debuginfo/debug" + DateTime.Now.ToString("yyyy_MM_dd-hh_mm_ss-fff")+".txt";
            //Console.WriteLine(debugPath);
            //if (false == System.IO.Directory.Exists("./debuginfo"))
            //{
            //    //创建pic文件夹
            //    System.IO.Directory.CreateDirectory("./debuginfo");
            //}
            // string debugPath = "./debuginfo" + (currentTicks - dtFrom.Ticks) / 10000 + ".txt";
            //debugSW = new StreamWriter(debugPath);


            //打开电压信息和温度信息保存文件
            //dt = InitDataTabel();

            //long currentTicks = DateTime.Now.Ticks;
            //DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;

            //String fullPath = "./debug" + currentMillis + ".csv";
            //System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
            //if (!fi.Directory.Exists)
            //{
            //    fi.Directory.Create();
            //}
            //fs = new System.IO.FileStream(fullPath, System.IO.FileMode.Create,
            //System.IO.FileAccess.Write);
            //sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            //export = new CsvHelper(titles);

            dataCache[16] = new CANSDK.VCI_CAN_OBJ();
            
            //初始CAN通道
            canChannelList.Add("CAN0");
            canChannelList.Add("CAN1");

            //初始化出厂配置对象
            factoryConfig = new BMUConfigModel_gy();
            //设置发送数据线程池参数
            ThreadPool.SetMinThreads(2, 2);
            ThreadPool.SetMaxThreads(4, 4);

            //初始化波特率
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "100Kbps", Time0 = 0x04, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "125Kbps", Time0 = 0x03, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "200Kbps", Time0 = 0x81, Time1 = 0xFA });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "250Kbps", Time0 = 0x01, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "500Kbps", Time0 = 0x00, Time1 = 0x1C });
            canBaudRateList.Add(new BaudRateModel() { Baudratename = "1Mbps", Time0 = 0x00, Time1 = 0x14 });


            //初始化语言选择Combox
            //XmlDocument doc = new XmlDocument();
            //string path3 = System.IO.Directory.GetCurrentDirectory();
            //doc.Load("../../Config/Config.xml");
            //// 得到根节点bookstore
            //XmlNode xn = doc.SelectSingleNode("appSettings");
            //// 得到根节点的所有子节点
            //XmlNodeList xnl = xn.ChildNodes;
            //foreach (XmlNode x in xnl) {
            //    if (x.Name.Equals("i18n")) {
            //        selectI18n = int.Parse(x.InnerText);
            //        break;
            //    }
               
            //}

            
            //i18n注释temp
            //categoryI18nList.Add(new CategoryInfo() { Name = "English", Value = "en_US",sourceName="English" });
            //categoryI18nList.Add(new CategoryInfo() { Name = "中文", Value = "zh_CN",sourceName="Chinese" });

            //ResourceDictionary langRd = Application.LoadComponent(new Uri("i18n/" + CategoryI18nList[selectI18n].sourceName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            //page.Resources.MergedDictionaries.Add(langRd);

            //初始化均衡模式
            balanceModeList.Add("不均衡");
            balanceModeList.Add("主动均衡");
            balanceModeList.Add("被动均衡");

            isWaitting = new Dictionary<string, int>();
            isWaitting.Add("info1", 0);//初始化等待标志位字典

            this.page.sendFacConfig.IsEnabled = false;//出厂配置发送按钮不可用（获取从机编号以后才可以使用）

            All_Check = new DelegateCommand(runAll_Check);//全选
            All_UnCheck = new DelegateCommand(runAll_Uncheck);//全不选
            AddBMUConfigCommand = new DelegateCommand(runAddBMUConfigCommand);//添加配置页
            SaveConfigCommand = new DelegateCommand(runSaveConfigCommand);//保存配置文件
            ReadConfigCommand = new DelegateCommand(runReadConfigCommand);//读取配置文件
            InitConfigCommand = new DelegateCommand(runInitConfigCommand);
            GetID = new DelegateCommand(runGetID);
            ClearBMUConfigCommand = new DelegateCommand(runClearBMUConfigCommand);//清空配置页
            DelBMUCommand = new DelegateCommand<Button>(runDelBMUCommand);//删除BMU
            DeleteBMUCommand = new DelegateCommand<Grid>(runDeleteBMUCommand);//删除BMU配置
            SendConfigCommand = new DelegateCommand(runSendConfigCommand);//发送配置文件数据包
            ConnectCanCommand = new DelegateCommand<Button>(RunConnectCanCommand);
            CbI18nClickCommand = new DelegateCommand<ComboBox>(RunCbI18nClickCommand);//国际化选框
            TestBMUCommand = new DelegateCommand<Grid>(RunTestBMUCommand);
            ReadBMUCommand = new DelegateCommand(runReadBMUCommand);
            ClearBMUCommand = new DelegateCommand(runClearBMUCommand);
            SendSPECommand = new DelegateCommand(runSendSPECommand);

            ReadCfgCommand = new DelegateCommand(runReadCfgCommand);
            ReadDiaCommand = new DelegateCommand(runReadDiaCommand);
            //ReadVersion = new DelegateCommand(runReadVersion);

            ReadFactoryConfigCommand = new DelegateCommand(runReadFactoryConfigCommand);
            SaveFactoryConfigCommand = new DelegateCommand(runSaveFactoryConfigCommand);

            ReadBootLoaderUrlCommand = new DelegateCommand(runReadBootLoaderUrlCommand);
            ReadBootLoaderUrlCommand2 = new DelegateCommand(runReadBootLoaderUrlCommand2);
            ReadBootLoaderUrlCommand3 = new DelegateCommand(runReadBootLoaderUrlCommand3);
            ReadMasterBootLoaderUrlCommand = new DelegateCommand(runReadMasterBootLoaderUrlCommand);
            ReadDecryptFileCommand = new DelegateCommand(runReadDecryptFileCommand);
            DecryptCommand = new DelegateCommand(runDecryptCommand);
            StartBootLoaderCommand = new DelegateCommand<TabControl>(runStartBootLoaderCommand);
            StartMasterBootLoaderCommand = new DelegateCommand(runStartMasterBootLoaderCommand);
            //StartBootLoaderCommand2 = new DelegateCommand(runStartBootLoaderCommand2);
            //StartBootLoaderCommand3= new DelegateCommand(runStartBootLoaderCommand3);
            ReadBootLoaderFileOneCommand = new DelegateCommand(runReadBootLoaderFileOneCommand);
            ReadBootLoaderFileTwoCommand = new DelegateCommand(runReadBootLoaderFileTwoCommand);
            StartMergeCommand = new DelegateCommand(runStartMergeCommand);
            //主机握手
            HandShakeWithMaster = new DelegateCommand(runHandShakeWithMaster);
            //主机剩余容量配置
            MasterCapacityCommand = new DelegateCommand(runMasterCapacityCommand);
            //主机标称容量配置
            MasterBCCapacityCommand = new DelegateCommand(runMasterBCCapacityCommand);
            //主机总容量配置
            MasterZCapacityCommand = new DelegateCommand(runMasterZCapacityCommand);
            //主机硬件版本号配置
            MasterHardwareVersionCommand = new DelegateCommand(runMasterHardwareVersionCommand);
            SlaveHardwareVersionCommand = new DelegateCommand(runSlaveHardwareVersionCommand);
            VINConfigCommand = new DelegateCommand(runVINConfigCommand);
            //霍尔1电流偏移配置
            MasterDeviationCommand = new DelegateCommand(runMasterDeviationCommand);
            MasterDeviationCommand2 = new DelegateCommand(runMasterDeviationCommand2);
            MasterDeviationCommand3 = new DelegateCommand(runMasterDeviationCommand3);
            MasterDeviationCommand4 = new DelegateCommand(runMasterDeviationCommand4);
            //Shunt_1电流偏移配置
            Shunt1Command = new DelegateCommand(runShunt1Command);
            //Shunt_2电流偏移配置
            Shunt2Command = new DelegateCommand(runShunt2Command);
            //放电总能量配置
            FangdznlCommand = new DelegateCommand(runFangdznlCommand);
            //主机时钟实时配置
            MasterClockCommand = new DelegateCommand(runMasterClockCommand);
            //主机继电器控制
            MasterRelayCommand = new DelegateCommand(runMasterRelayCommand);
            //主机实时信息监测
            MasterShiShiInfo = new DelegateCommand(runMasterShiShiInfo);
            Tiedian = new DelegateCommand(runTiedian);
            OpenMasterRecord = new DelegateCommand(runOpenMasterRecord);
            CloseMasterRecord = new DelegateCommand(runCloseMasterRecord);

            All_Check_Factory = new DelegateCommand(runAll_Check_Factory);
            All_UnCheck_Factory = new DelegateCommand(runAll_UnCheck_Factory);

            // ClickFacInfo17 = new DelegateCommand(runClickFacInfo17);

            UserCommand = new DelegateCommand(runUserCommand);
            LogoutCommand = new DelegateCommand(runLogoutCommand);

            BmuTextChangedCommand = new DelegateCommand<TextBox>(runBmuTextChangedCommand);

            //初始化状态动画不显示
            WaitStatus = EnumStatus.Loaded;
        }

        private void runClearBMUCommand()
        {
            IsCheckAllInfo = false;
            isCreateBmuFlag = false;
            IsSendBmuInfo = false;
            IsSendBmuCellInfo = false;
            IsSendBmuTMOSInfo = false;
            IsSendBmuSLSTMPInfo = false;
            IsSendBmuSLSInfo = false;
            IsSendBmuTempInfo = false;
            IsSendBalanceInfo = false;
            Thread.Sleep(100);
            if (page.grid.Children.Count > 0)
                page.grid.Children.Clear();
            if (receiveBmuList.Count() > 0)
                receiveBmuList.Clear();
        }

        /// <summary>
        /// 获取BMU总体信息
        /// </summary>
        private void runReadBMUCommand()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }

            //for (int i = 0; i < BMU_Vol.Length; i++) {
            //    BMU_Vol[i] = 0;              
            //}
            Info = "";
                WaitStatus = EnumStatus.Loading;

            //创建发送线程
            Thread senddata = new Thread(readBMUThread);
            senddata.IsBackground = true;
            senddata.Start();

            //创建超时等待线程，超时时间为10秒等待数据接受线程获取BMU信息
            Thread WaitBMUTimeOutThread = new Thread(waitBMUThread);
            WaitBMUTimeOutThread.IsBackground = true;
            WaitBMUTimeOutThread.Start();
        }


        private void waitBMUThread()
        {
            int sleep = int.Parse(ConfigurationManager.AppSettings["SLEEP"]);
            DateTime oldTime = DateTime.Now;
            DateTime newTime = DateTime.Now;
            while (true)
            {
                if ((newTime - oldTime).TotalMilliseconds > sleep)
                {
                    if (receiveBmuList.Count() > 0)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            WaitStatus = EnumStatus.Loaded;
                            CreateElement();
                        });

                        //创建写文件线程用于后台写入单体信息
                        int iswrite = int.Parse(ConfigurationManager.AppSettings["ISWRITE"]);
                        if (iswrite == 1)
                        {
                            Thread WriteCSVThread = new Thread(RunWriteCSVThread);
                            WriteCSVThread.IsBackground = true;
                            WriteCSVThread.Start();

                        }

                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            WaitStatus = EnumStatus.Loaded;
                            string msg = String.Format("上位机在{0}秒时间内未收到从机配置信息！", sleep / 1000);
                            ModernDialog.ShowMessage(msg, "提示", MessageBoxButton.OK);
                            logger.Debug(msg);
                        });

                        break;

                    }
                };



                Thread.Sleep(500);
                newTime = DateTime.Now;
                if (isCreateBmuFlag)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        WaitStatus = EnumStatus.Loaded;
                    });
                    break;
                }
            }

        }

        CsvHelper writelog = new CsvHelper();
        /// <summary>
        /// 用户后台写入csv日志
        /// </summary>
        /// <param name="obj"></param>
        private void RunWriteCSVThread()
        {

            while (true)
            {
                try
                {
                    for (int i = 0; i < receiveBmuList.Count(); i++)
                    {
                        DataTable dt = null;
                        string filename = "BMU#" + receiveBmuList[i].Bmuindex + ".csv";
                        string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "Data\\";
                        if (!Directory.Exists(filepath))
                        {
                            Directory.CreateDirectory(filepath);
                        }
                        dt = InitDT(receiveBmuList[i].Cellcouts);

                        if (receiveBmuList[i].Cellcouts != receiveBmuList[i].CellDic.Count)
                            continue;

                        int sum = 0;
                        foreach (var item in receiveBmuList[i].CellDic)
                        {
                            sum += item.Value.Cellstatus;
                        }

                        if (sum == receiveBmuList[i].Cellcouts)
                        {
                            DataRow dr = dt.NewRow();
                            dr["BMU"] = receiveBmuList[i].Bmuindex;
                            dr["TIME"] = DateTime.Now;
                            //对字典进行数据排序
                            Dictionary<int, CellModel> dicAsc = receiveBmuList[i].CellDic.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
                            int j = 0;
                            //开始写入数据至文件
                            foreach (var tt in dicAsc)
                            {
                                tt.Value.Cellstatus = 0;
                                dr["CELL" + (j + 1).ToString()] = tt.Value.Cellvol;
                                j++;
                            }
                            dt.Rows.Add(dr);
                            writelog.DataToCSV(filepath + filename, dt);
                            dt.Rows.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                Thread.Sleep(10);
            }






        }



        private DataTable InitDT(int cellcounts)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BMU", Type.GetType("System.String"));
            dt.Columns.Add("TIME", Type.GetType("System.String"));
            for (int i = 0; i < cellcounts; i++)
            {
                dt.Columns.Add("CELL" + (i + 1).ToString(), Type.GetType("System.String"));
            }
            return dt;
        }


        /// <summary>
        /// 初始化读取配置信息过程
        /// 每个线程独立发送当前bmu配置信息远程帧
        /// </summary>
        /// <param name="obj"></param>
        private void readBMUThread()
        {
            for (int i = 0; i < BMUCNT; i++)
            {
                //发送远程帧
                CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                string readcfg = "0C217F" + (i + 1).ToString("X2");
                data.ID = uint.Parse(readcfg, System.Globalization.NumberStyles.HexNumber);
                data.RemoteFlag = 1;//远程帧
                data.ExternFlag = 1;//扩展帧
                data.SendType = 0;//正常发送
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                Thread.Sleep(10);
            }

        }



        private Int32 _index;  //光标位置
        private bool _isReentry; //标识TextChanged事件是否重入
        private int Minimum = 0;
        private int Maximum = 60;
        private int SelectionStart = 0;

        private void runBmuTextChangedCommand(TextBox tx)
        {
            if (_isReentry)
            {
                SelectionStart = _index;
                return;
            }
            _isReentry = true;
            Int32 temp = 0;
            if (Int32.TryParse(tx.Text, out temp))
            {
                if (temp > Maximum || temp < Minimum)
                {
                    temp = temp > Maximum ? Maximum : Minimum;
                    _index = SelectionStart;
                }
                tx.Text = temp.ToString();
            }
            //类型不正确或者超长会导致转换失败
            //else
            //{
            //    this.tx.Text = Int32.MaxValue.ToString();
            //}
            _isReentry = false;
        }

        //注销
        private void runLogoutCommand()
        {
            Userpower = -1;
            page.Hide();
            Login login = new Login();
            login.Show();
        }

        private void runUserCommand()
        {

            AddUser m = new AddUser(Userpower);
            m.ShowDialog();
        }

        //全选出厂配置
        private void runAll_Check_Factory()
        {
            factoryConfig.setAllTrue();
        }
        //全不选出厂配置
        private void runAll_UnCheck_Factory()
        {
            factoryConfig.setAllFalse();
        }

        //读取bootloader文件,i用来标志第几组的bootloader文件
        private void runReadBootLoaderUrlCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderUrl[1] = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取出厂配置配置成功";
        }
        private void runReadBootLoaderUrlCommand2()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderUrl[2] = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取出厂配置配置成功";
        }
        private void runReadBootLoaderUrlCommand3()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderUrl[3] = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取出厂配置配置成功";
        }

        private void decryptThread() {
            FileStream deReadStream = new FileStream(DecryptFilePath, FileMode.Open);
            StreamReader reader = new StreamReader(deReadStream);

            String s = DecryptFilePath.Substring(0, decryptFilePath.IndexOf(".csv")) + "_decrypt.csv";
            FileStream deWriteStream = new FileStream(s, FileMode.Create);
            StreamWriter writer = new StreamWriter(deWriteStream, Encoding.Default);

            String readLine;
            while ((readLine = reader.ReadLine()) != null)
            {
                writer.WriteLine(DataConverter.AESDecrypt(readLine, SECRETKEY));
                //writer.Write(DataConverter.TextDecrypt(DataConverter.StringToBytes(readLine), SECRETKEY));
            }
            reader.Close();
            deReadStream.Close();

            writer.Close();
            deWriteStream.Close();
            Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ModernDialog.ShowMessage((string)page.Resources["decryptSuc"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });
        }

        private void runDecryptCommand()
        {
            new Thread(new ThreadStart(decryptThread)).Start();            
        }

        private void runReadDecryptFileCommand() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.csv)|*.csv";
            dialog.ShowDialog();
            String path = dialog.FileName;
            DecryptFilePath = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
           // Info = "读取出厂配置配置成功";
        }

        private void runReadMasterBootLoaderUrlCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderUrl[0] = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取出厂配置配置成功";
        }

        //读取合并的文件1
        private void runReadBootLoaderFileOneCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderFileOneUrl = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取文件1成功";
        }

        //读取合并的文件2
        private void runReadBootLoaderFileTwoCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            BootLoaderFileTwoUrl = path;
            if (path == "")
            {
                return;
            }
            //FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取文件2成功";
        }

        //读取出厂配置文件
        private void runReadFactoryConfigCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            if (path == "")
            {
                return;
            }
            FactoryConfig = Serializer.DeserializeFromFileByXml<BMUConfigModel_gy>(path);
            Info = "读取出厂配置配置成功";
        }

        //保存出厂配置文件
        private void runSaveFactoryConfigCommand()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "";
            sfd.InitialDirectory = @"C:\";
            sfd.Filter = "文本文件| *.txt";
            sfd.ShowDialog();
            string path = sfd.FileName;
            // Console.WriteLine(path);
            if (path == "")
            {
                return;
            }
            List<BMUConfigModel_gy> l = new List<BMUConfigModel_gy>();
            l.Add(factoryConfig);
            //序列化
            Serializer.SerializeToFileByXml<BMUConfigModel_gy>(factoryConfig, Path.GetDirectoryName(path), Path.GetFileName(path));
            Info = "出厂配置保存完毕";
        }
        //public void runClickFacInfo17()
        //{
        //    if (factoryConfig.IsChecked17)
        //    {
        //        factoryConfig.setAllFalse();
        //        factoryConfig.IsChecked17 = true;
        //    }
        //}
        //全选
        public void runAll_Check()
        {
            List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
            for (int k = 0; k < tabItemList.Count; k++)
            {
                TabItem t = tabItemList[k];
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ScrollViewer sv = t.Content as ScrollViewer;
                    StackPanel sp = sv.Content as StackPanel;
                    BMUConfig bc = sp.Children[0] as BMUConfig;
                    BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                    bcm.BmuConfigModel_gy.setAllTrue();
                    // bcm.IsChecked2 = true;
                });
            }
        }
        //全不选
        public void runAll_Uncheck()
        {
            List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
            for (int k = 0; k < tabItemList.Count; k++)
            {
                TabItem t = tabItemList[k];
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ScrollViewer sv = t.Content as ScrollViewer;
                    StackPanel sp = sv.Content as StackPanel;
                    BMUConfig bc = sp.Children[0] as BMUConfig;
                    BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;
                    bcm.BmuConfigModel_gy.setAllFalse();
                });
            }
        }


        private int isGetID = 0;
        //获取ID
        public void runGetID()
        {
            if (!connectstate)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);
                    ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });
                return;
            }
            isGetID = 1;
            this.page.sendFacConfig.IsEnabled = true;
            //this.page.sendConfig.IsEnabled = true;


        }

        private void runReadCfgCommand()
        {
            int index = 0;
            //查找当前tabitem页
            if (page.grid.Children.Count > 0)
            {
                TabControl tc = page.grid.Children[0] as TabControl;
                index = (int)(tc.SelectedItem as TabItem).Tag; //获取tabitem索引号  
                ReadCfgForm form = new ReadCfgForm(this, index);
                Thread senddata = new Thread(new ParameterizedThreadStart(SendDataThread));
                senddata.IsBackground = true;
                senddata.Start(index);
                form.ShowDialog();
            }
        }



        public void runReadDiaCommand()
        {
            int index = 0;
            //查找当前tabitem页
            if (page.grid.Children.Count > 0)
            {
                TabControl tc = page.grid.Children[0] as TabControl;
                index = (int)(tc.SelectedItem as TabItem).Tag; //获取tabitem索引号  
                DiagnoseForm form = new DiagnoseForm(this, index);
                Thread senddata = new Thread(new ParameterizedThreadStart(SendDiaDataThread));
                senddata.IsBackground = true;
                senddata.Start(index);
                form.ShowDialog();
            }
        }

        
        private static AutoResetEvent diaEvent = new AutoResetEvent(false);
        private void SendDiaDataThread(object obj)
        {
            int index = (int)obj;
            CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
            data.RemoteFlag = 0;//远程帧
            data.ExternFlag = 1;//扩展帧
            data.SendType = 0;//正常发送
            string temp = null;
            for (int i = 1; i < 14; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    temp = i.ToString("X");
                    string sls = "182" + temp + "41" + index.ToString("X2");
                    Console.WriteLine(sls + " dia" + temp);
                    data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                    isWaitting["dia" + temp] = 1;
                    CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                    diaEvent.WaitOne(3000);
                    if (isWaitting["dia" + temp] == 0) break;
                }
                isWaitting["dia" + temp] = 0;
            }
            string s = "182F41" + index.ToString("X2");
            data.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            isWaitting["diaF"] = 1;
            CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
            diaEvent.WaitOne(3000);
        }

        private void SendDataThread(object obj)
        {
            int index = (int)obj;
            for (int i = 0; i < 5; i++)
            {
                //发送远程帧
                CANSDK.VCI_CAN_OBJ data = new CANSDK.VCI_CAN_OBJ();
                string sls = "0C217F" + index.ToString("X2");
                data.ID = uint.Parse(sls, System.Globalization.NumberStyles.HexNumber);
                data.RemoteFlag = 1;//远程帧
                data.ExternFlag = 1;//扩展帧
                data.SendType = 0;//正常发送
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref data, 1);
                autoResetEvent.WaitOne(100);
                if (!isReadCfgSendReapeat) break;
                Thread.Sleep(100);
            }
            isReadCfgSendReapeat = true;

        }




        private void RunTestBMUCommand(Grid grid)
        {
            TabControl tc = new TabControl();
            tc.TabStripPlacement = Dock.Left;
            for (int a = 1; a <= 4; a++)
            {
                TabItem ti = new TabItem();
                ti.Header = "BMU" + a;
                StackPanel parent = new StackPanel();
                for (int t = 0; t < 4; t++)
                {
                    StackPanel sp = new StackPanel();
                    sp.Margin = new Thickness(0, 0, 0, 2);
                    sp.Orientation = Orientation.Horizontal;
                    for (int j = 0; j < 16; j++)
                    {
                        TextBlock cellltb = new TextBlock();
                        cellltb.Width = 35;
                        cellltb.FontSize = 13;
                        cellltb.TextAlignment = TextAlignment.Right;
                        cellltb.HorizontalAlignment = HorizontalAlignment.Right;
                        cellltb.Text = "V" + (t * 16 + (j + 1)).ToString() + ":";
                        cellltb.VerticalAlignment = VerticalAlignment.Center;
                        sp.Children.Add(cellltb);

                        TextBlock cellval = new TextBlock();
                        cellval.Name = "V" + a + "_" + (1 + j);
                        cellval.Text = "0.00";
                        cellval.Width = 35;
                        cellval.FontSize = 13;
                        cellval.TextAlignment = TextAlignment.Left;
                        cellval.HorizontalAlignment = HorizontalAlignment.Left;
                        cellval.Margin = new Thickness(2, 0, 0, 0);

                        sp.Children.Add(cellval);
                    }
                    parent.Children.Add(sp);

                }
                ti.Content = parent;
                tc.Items.Add(ti);
            }
            grid.Children.Add(tc);
        }

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

        bool isMasterShishi = true;//表示主机“开启实时监测按键”状态，true表示为“开启实时监测按键”，否则表示“关闭实时监测按键”
        List<StackPanel> DYWDList = null;

        private FileStream masterDebug=null;//用于保存接收到的主机数据
        private StreamWriter masterSW=null;
        int masterFileLineNum = -1;
        string masterFile = null;
      
        private DataTable InitMasterDataTabel(bool iscreatenewtitle) {
            DataTable dt = new DataTable();
            export = new CsvHelper();
            //export = new CsvHelper(new string[] { "编号", "接收时间", "信息项", "细分", "值"});
            if (iscreatenewtitle)
            {
                dt.Columns.Add("编号", Type.GetType("System.String"));
                dt.Columns.Add("接收时间", Type.GetType("System.String"));
                dt.Columns.Add("信息项", Type.GetType("System.String"));
                dt.Columns.Add("细分", Type.GetType("System.String"));
                dt.Columns.Add("值", Type.GetType("System.String"));
            }
            else {
                dt.Columns.Add();
                dt.Columns.Add();
                dt.Columns.Add();
                dt.Columns.Add();
                dt.Columns.Add();
            }
            return dt;
        }



        private bool isOpenMasterRecord = false;//是否开启了主机数据记录功能

        public bool IsOpenMasterRecord
        {
            get { return isOpenMasterRecord; }
            set { isOpenMasterRecord = value; OnPropertyChanged("IsOpenMasterRecord"); }
        }
        private void runOpenMasterRecord()
        {
            isOpenMasterRecord = true;
        }

        /*
            拼铁电发送包
         * blockNum:块号，1为实时信息，2为故障信息
         * page：页号
         * type：类型，1擦除2读取
         */
        private CANSDK.VCI_CAN_OBJ getTiedianSendPackage(int blockNum,int page,int type){
            //if (blockNum < 0 || blockNum > 2||type<1||type>2) {
            //    Console.WriteLine("块号或类型有误！");
            //    Info = "块号或类型有误！";
            //    return null;
            //}
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("1801B1C1", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0] = (byte)blockNum;
            data[1] = (byte)(page / 256);
            data[2] = (byte)(page % 256);
            data[3] = (byte)type;
            data[4] = 0xff;
            data[5] = 0xff;
            data[6] = 0xff;
            data[7] = 0xff;
            obj.DataLen = 8;
            obj.Data = data;
            return obj;
        }
        private int tiedianData = 0;//是否正在等待接收铁电数据,0否1是2当前block已接收完毕，切换到下个block或者停止（suoyoublock已接收完）
        //拉取铁电数据
        private void getTeidianData()
        {
            for (int i = 1; i < 3; i++)
            {
                for (int j = 0; j < 65536; j++)
                {
                    CANSDK.VCI_CAN_OBJ obj = getTiedianSendPackage(i, j, 1);
                    uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    tiedianData = 1;
                    long starttime = DateTime.Now.Ticks;
                    for (int k = 0; k < 3; k++)
                    {
                        while (tiedianData == 1)//等待握手
                        {
                            //发出握手请求后每次等待3秒
                            if ((DateTime.Now.Ticks - starttime) / 10000 < 3000)
                            {
                                continue;
                            }
                            else
                            {
                                Console.WriteLine("接收铁电数据超时" + k);
                                break;
                            }
                        }
                        if (tiedianData == 0 || tiedianData == 2)
                        {
                            Console.WriteLine("铁电数据接收成功！");
                            break;
                        }

                    }
                    if (tiedianData == 1)
                    {
                        Console.WriteLine("接收铁电数据失败！");
                        tiedianData = 0;
                        return;
                    }
                    else if (tiedianData == 0)
                    {
                        Console.WriteLine("当前页铁电数据接收成功！");

                    }
                    else if (tiedianData == 2)
                    {
                        Console.WriteLine("当前block铁电数据接收完毕！");
                    }
                }

            }

        }

        bool isTiedianHandShake = false;//是否发送了铁电握手帧，正在等待回送
        //铁电握手
        private void SendHandShakeWithTiedianThread() {
            //握手
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("1806B1C1", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0] = (byte)'B';
            data[1] = (byte)'M';
            data[2] = (byte)'S';
            data[3] = (byte)'F';
            data[4] = (byte)'R';
            data[5] = (byte)'A';
            data[6] = (byte)'M';
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            isTiedianHandShake = true;
            long starttime = DateTime.Now.Ticks;
            for (int i = 0; i < 3; i++)
            { 
                while (isTiedianHandShake)//等待握手
                {
                    //发出握手请求后每次等待3秒
                    if (( DateTime.Now.Ticks-starttime) / 10000 < 3000)
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("铁电握手超时！"+i);
                        break;
                    }
                }
                if (!isTiedianHandShake)
                {
                    Console.WriteLine("铁电握手成功");
                    break;
                }
            }
            if (isTiedianHandShake)
            {
                Console.WriteLine("铁电握手失败！");
                return;
            }
            else { 
            //开始拉取铁电数据
                getTeidianData();
            }
        }
        private void runTiedian() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }

            new Thread(new ThreadStart(SendHandShakeWithTiedianThread)).Start();
           
        }
        
        private void writeMasterRecord()
        {
           // Console.WriteLine("!!!!!!!:" + dataRecordPerLineNum);
            if (dataRecordPerLineNum < dataRecordPerLine.Length) {
                Console.WriteLine();
            }
            if (masterSW != null)
            {
                lock (debuglocker)
                {

                    if (masterFileLineNum < 0 | masterFileLineNum >= masterDataRecordPerFileNum)
                    {
                        createNewMasterDataFile();
                    }
                    String dataLine = masterFileLineNum++ + "," + DateTime.Now.ToLocalTime().ToString();
                   // masterSW.Write(masterFileLineNum++ + ",");//序号
                   // masterSW.Write(DateTime.Now.ToLocalTime().ToString());//时间
                    for (int i = 0; i < dataRecordPerLine.Length; i++)
                    {
                        dataLine +=( "," + dataRecordPerLine[i]);
                        
                        //masterSW.Write("," + dataRecordPerLine[i]);
                    }
                    //dataLine += "\r\n";
                    //masterSW.WriteLine(dataLine);
                    masterSW.WriteLine(DataConverter.AESEncrypt(dataLine, SECRETKEY));
                    //masterSW.Write(DataConverter.bytestoString(DataConverter.TextEncrypt(dataLine+"\r\n", SECRETKEY)));
                    //masterSW.Write(dataLine);
                    //masterSW.WriteLine();


                    // masterFileLineNum++;
                    //masterSW.WriteLine((masterFileLineNum++) + "," + DateTime.Now.ToLocalTime().ToString() + "," + "单体电压," + "第" + (i + 1) + "号从机第" + dataPos + "单体电压," + temp * 0.001 + "V");
                    dataRecordPerLineNum = 0;
                    dataRecordPerLine = new string[dataRecordPerLine.Length];//每次写完清空数据
                }

            }
            
        }

        //当一行数据存储完毕或者等待2s，保存一行数据
        private void writeMasterThread()
        {
            while (isOpenMasterRecord)
            {
                if (dataRecordPerLineNum >= dataRecordPerLine.Length)
                {
                    Console.WriteLine("dataRecordPerLineNum2：" + dataRecordPerLineNum);
                    //数据已经记录完全，可以写入
                    writeMasterRecord();
                }
                else
                {
                    //数据尚未存储完毕，需要再等待
                    long startMillis = (DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000;
                    while (true)
                    {
                        long nowMillis = (DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000;
                        //while ((DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000 - startMillis < 2000)
                        //{
                        //Console.WriteLine((DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000 - startMillis);
                        //long tempmillis = nowMillis - startMillis;
                        if (nowMillis - startMillis < 2000)
                        {
                            if (dataRecordPerLineNum >= dataRecordPerLine.Length)
                            {
                                Console.WriteLine("dataRecordPerLineNum1：" + dataRecordPerLineNum);
                                writeMasterRecord();
                                break;
                            }
                        }
                        else
                        {
                            writeMasterRecord();
                            break;
                        }
                        //    if (dataRecordPerLineNum >= dataRecordPerLine.Length)
                        //    {
                        //        Console.WriteLine("dataRecordPerLineNum1：" + dataRecordPerLineNum);
                        //        writeMasterRecord();
                        //        break;
                        //    }
                        //}
                        //////等待已经超过时限，可能有数据并没有发送，直接保存
                        //writeMasterRecord();
                    }



                    //Thread.Sleep(2000);
                    //if (masterSW != null)
                    //{
                    //    lock (debuglocker)
                    //    {

                    //        if (masterFileLineNum < 0 | masterFileLineNum >= masterDataRecordPerFileNum)
                    //        {
                    //            createNewMasterDataFile();
                    //        }
                    //        masterSW.Write(masterFileLineNum++ + ",");//序号
                    //        masterSW.Write(DateTime.Now.ToLocalTime().ToString());//时间
                    //        for (int i = 0; i < dataRecordPerLine.Length; i++)
                    //        {
                    //            masterSW.Write("," + dataRecordPerLine[i]);
                    //        }
                    //        masterSW.WriteLine();
                    //        // masterFileLineNum++;
                    //        //masterSW.WriteLine((masterFileLineNum++) + "," + DateTime.Now.ToLocalTime().ToString() + "," + "单体电压," + "第" + (i + 1) + "号从机第" + dataPos + "单体电压," + temp * 0.001 + "V");
                    //    }

                    //}
                    //dataRecordPerLine = new string[dataRecordPerLine.Length];//每次写完清空数据
                }

            }
        }

        private void runCloseMasterRecord()
        {
            isOpenMasterRecord = false;
        }
        private int masterDataRecordPerFileNum = 15000;//主机信息记录保存文件每个保存多少行数据
        //主机实时信息监测
        private void runMasterShiShiInfo()
        {
            //Console.WriteLine();
            //主机接收信息前判断，需要时开启,勿删！！！
            
            //for (int i = 0, s = 0; i < chuanshu.Length; i++)
            //{
            //    s += chuanshu[i];
            //    //250*3
            //    if (s > 750)
            //    {
            //        ModernDialog.ShowMessage("从机串数信息有误，请确认后重新握手", "提示", MessageBoxButton.OK);

            //        return;
            //    }
            //    else if (s <= 0)
            //    {
            //        ModernDialog.ShowMessage("串数总和为0，请先重新上电握手", "提示", MessageBoxButton.OK);

            //        return;
            //    }

            //}
            //for (int i = 0, s = 0; i < wenganshu.Length; i++)
            //{
            //    s += wenganshu[i];
            //    //20*6
            //    if (s > 60)
            //    {
            //        ModernDialog.ShowMessage("从机温感数信息有误，请确认后重新握手", "提示", MessageBoxButton.OK);

            //        return;
            //    }
            //    else if (s <= 0)
            //    {
            //        ModernDialog.ShowMessage("请先重新上电握手", "提示", MessageBoxButton.OK);

            //        return;
            //    }
            //}

            Button btn = page.FindName("shishiBtn") as Button;
            if (!isMasterShishi)
            {
                isMasterShishi = true;
                isGetMasterInfo = 0;

                //if (sendUpSystemHeart != null)
                //{
                //    //关闭上位机发送心跳
                //    sendUpSystemHeart.Abort();
                //    sendUpSystemHeart = null;
                //}

                //关闭主机数据文件记录 717
                closeMasterDataFile(true);




                //if (masterDebug != null && masterSW != null)
                //{

                //    masterSW.Close();
                //    masterDebug.Close();
                //    masterDebug = null;
                //    masterSW = null;
                //}
                IsOpenMasterRecord = false;
                dataRecordPerLineNum = 0;
                btn.Content = "开启实时监测";
                page.isRecord.IsEnabled = true;
                btn.SetResourceReference(ContentControl.ContentProperty, "startshishi");
            }
            else
            {
                //判断当前can是否连接
                if (!connectstate)
                {
                    //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                    ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                    IsCheckAllInfo = false;
                    return;
                }
                page.isRecord.IsEnabled = false;
                isMasterShishi = false;
                if (sendUpSystemHeart == null)
                {
                    //开启上位机发送心跳
                    sendUpSystemHeart = new Thread(new ThreadStart(sendHeartThread));
                    sendUpSystemHeart.IsBackground = true;
                    sendUpSystemHeart.Start();
                }
                btn.Content = "关闭实时监测";
                btn.SetResourceReference(ContentControl.ContentProperty, "closeshishi");

                //开启文件记录 717
                closeMasterDataFile(false);

                //if (masterDebug != null && masterSW != null)
                //{
                //    masterSW.Close();
                //    masterDebug.Close();                    
                //    masterDebug = null;
                //    masterSW = null;
                //}

                if (isOpenMasterRecord)
                {
                    dataRecordIndex.Clear();
                    int wengansum = 5;
                    for (int i = 0; i < wenganshu.Length; i++)
                    {
                        if (wenganshu[i] > 0)
                        {
                            dataRecordIndex.Add("w" + (i + 1), wengansum);
                            wengansum += wenganshu[i];
                        }
                    }
                    int chuanshusum = wengansum;
                    for (int i = 0; i < chuanshu.Length; i++)
                    {
                        if (chuanshu[i] > 0)
                        {
                            dataRecordIndex.Add("y" + (i + 1), chuanshusum);
                            chuanshusum += chuanshu[i];
                        }
                    }

                    int lineSum = 5;//0:序号，1：时间，2：soc，3：总压，4：总流,5:温差，6：压差
                    for (int i = 0; i < wenganshu.Length; i++)
                    {
                        lineSum += wenganshu[i];
                    }
                    for (int i = 0; i < chuanshu.Length; i++)
                    {
                        lineSum += chuanshu[i];
                    }
                    dataRecordPerLine = new string[lineSum];

                    //读取主机实时信息记录文件的行数 717
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
                        if (x.Name.Equals("masterdataline2"))
                        {
                            masterFileLineNum = int.Parse(x.InnerText);
                            break;
                        }

                    }
                    //限定每个文件存100行 717
                    if (masterFileLineNum >= 0 && masterFileLineNum <= masterDataRecordPerFileNum)
                    {
                        //配置文件记录的上次数据符合要求
                        foreach (XmlNode x in xnl)
                        {
                            if (x.Name.Equals("masterdatafile"))
                            {
                                masterFile = x.InnerText;
                                //下面用于追加存储上一次开启实时监测存储的数据
                               // if (!File.Exists(masterFile))
                                //下面用于不追加存储上一次开启实时监测存储的数据，每次开启实时监测新建一个保存文件
                                if (true)
                                {
                                    //上次记录的文件已经不存在
                                    masterFile = "debuginfo/masterDebug" + (DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000 + ".csv";
                                    masterFileLineNum = 0;
                                    masterDebug = new FileStream(masterFile, FileMode.Append);
                                    masterSW = new StreamWriter(masterDebug, Encoding.Default);
                                    String dataLine = "序号,接收时间,SOC,总压,总流,温差,压差";
                                    //String dataLine = "No.,Receiving time,SOC,Total voltage,Total current,Temperature difference,Differential pressure";
                                    //masterSW.Write("序号,接收时间,SOC,总压,总流,温差,压差");
                                    for (int i = 0; i < wenganshu.Length; i++)
                                    {
                                        for (int j = 0; j < wenganshu[i]; j++)
                                        {
                                            dataLine += ",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度";
                                            //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell temperature";
                                            //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度");
                                        }
                                    }
                                    for (int i = 0; i < chuanshu.Length; i++)
                                    {
                                        for (int j = 0; j < chuanshu[i]; j++)
                                        {
                                            dataLine += (",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压");
                                            //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell voltage";
                                            //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压");
                                        }
                                    }
                                    //masterSW.WriteLine(dataLine);
                                    masterSW.WriteLine(DataConverter.AESEncrypt(dataLine, SECRETKEY));
                                    //masterSW.Write(DataConverter.bytestoString(DataConverter.TextEncrypt(dataLine+"\r\n", SECRETKEY)));
                                    //masterSW.WriteLine();
                                    // masterSW.WriteLine("编号,接收时间,信息项,细分,值");
                                }
                                else
                                {
                                    masterDebug = new FileStream(masterFile, FileMode.Append);
                                    masterSW = new StreamWriter(masterDebug, Encoding.Default);
                                }
                            }

                        }

                        // masterDataTable = InitMasterDataTabel(false);
                    }
                    else
                    {
                        masterFileLineNum = 0;
                        masterFile = "debuginfo/masterDebug" + (DateTime.Now.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks) / 10000 + ".csv";
                        masterDebug = new FileStream(masterFile, FileMode.Create);
                        masterSW = new StreamWriter(masterDebug, Encoding.Default);
                        //masterSW.Write("序号,接收时间,SOC,总压,总流");
                        String dataLine = "序号,接收时间,SOC,总压,总流,温差,压差";
                        //String dataLine = "No.,Receiving time,SOC,Total voltage,Total current,Temperature difference,Differential pressure";
                        //masterSW.Write("序号,接收时间,SOC,总压,总流,温差,压差");
                        for (int i = 0; i < wenganshu.Length; i++)
                        {
                            for (int j = 0; j < wenganshu[i]; j++)
                            {
                                dataLine += ",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度";
                                //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell temperature";
                                //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体温度");
                            }
                        }
                        for (int i = 0; i < chuanshu.Length; i++)
                        {
                            for (int j = 0; j < chuanshu[i]; j++)
                            {
                                dataLine += ",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压";
                                //dataLine += ",No." + (i + 1) + "Slave No." + (j + 1) + "cell voltage";
                                //masterSW.Write(",第" + (i + 1) + "号从机第" + (j + 1) + "单体电压");
                            }
                        }
                        //masterSW.WriteLine();
                        //masterSW.WriteLine(dataLine);
                        masterSW.WriteLine(DataConverter.AESEncrypt(dataLine, SECRETKEY));
                        //masterSW.Write(DataConverter.bytestoString(DataConverter.TextEncrypt(dataLine+"\r\n", SECRETKEY)));
                        // masterSW.WriteLine("编号,接收时间,信息项,细分,值");
                        // masterDataTable = InitMasterDataTabel(true);
                    }
                    reader.Close();
                    new Thread(new ThreadStart(writeMasterThread)).Start();
                    //masterDebug = new FileStream(masterFile, FileMode.Create);
                   
                }
                //masterDebug = new FileStream("masterDebug.txt", FileMode.Create);               
                //masterSW = new StreamWriter(masterDebug);

                

                StackPanel sp3 = page.FindName("masterShiShiInfo1") as StackPanel;
                sp3.Children.Clear();
                TextBlock tbtitle1_1 = new TextBlock();
                tbtitle1_1.Text = (string)page.Resources["packinfo"];
                //tbtitle1_1.Text = "电池组信息:";
                //tbtitle1_1.Text = "{DynamicResource packinfo}";
                
                //tbtitle1_1.SetResourceReference(ContentControl.ContentProperty, "packinfo");
                tbtitle1_1.FontSize = 20;
                tbtitle1_1.Visibility = Visibility.Collapsed;
                sp3.Children.Add(tbtitle1_1);
                for (int i = 0; i < 6; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 350;
                        //tb.Width = 175;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp3.Children.Add(sp_info);
                }

                StackPanel sp4 = page.FindName("masterShiShiInfo2") as StackPanel;
                sp4.Children.Clear();
                TextBlock tbtitle2_1 = new TextBlock();
                //tbtitle2_1.Text = "单体电压和电流:";
                tbtitle2_1.Text = (string)page.Resources["cellvat"];
                //tbtitle2_1.SetResourceReference(ContentControl.ContentProperty, "cellvat");
                tbtitle2_1.FontSize = 20;
                tbtitle2_1.Visibility = Visibility.Collapsed;
                sp4.Children.Add(tbtitle2_1);
                for (int i = 0; i < 450; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Name = "line" + i;
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();
                        tb.Name = "cell" + i + "_" + j;
                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        //tb.Width = 170;
                        tb.Width = 250;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp4.Children.Add(sp_info);
                }

                StackPanel sp4_2 = page.FindName("masterShiShiInfo2_1") as StackPanel;
                sp4_2.Children.Clear();
                TextBlock tbtitle2_2 = new TextBlock();
                //tbtitle2_2.Text = "单体温度:";
                //tbtitle2_2.SetResourceReference(ContentControl.ContentProperty, "celltem");
                tbtitle2_2.Text = (string)page.Resources["celltem"];
                tbtitle2_2.FontSize = 20;
                tbtitle2_2.Visibility = Visibility.Collapsed;
                sp4_2.Children.Add(tbtitle2_2);
                for (int i = 0; i < 10; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 250;
                        //tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp4_2.Children.Add(sp_info);
                }

                StackPanel sp5 = page.FindName("masterShiShiInfo3") as StackPanel;
                sp5.Children.Clear();
                TextBlock tbtitle3_1 = new TextBlock();
                //tbtitle3_1.Text = "母线高压:";
                //tbtitle3_1.SetResourceReference(ContentControl.ContentProperty, "busbarhighvol");
                tbtitle3_1.Text = (string)page.Resources["busbarhighvol"];
                tbtitle3_1.FontSize = 20;
                tbtitle3_1.Visibility = Visibility.Collapsed;
                sp5.Children.Add(tbtitle3_1);
                for (int i = 0; i < 5; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp5.Children.Add(sp_info);
                }

                StackPanel sp5_1 = page.FindName("masterShiShiInfo3_1") as StackPanel;
                sp5_1.Children.Clear();
                TextBlock tbtitle3_2 = new TextBlock();
                //tbtitle3_2.Text = "温度:";
                //tbtitle3_2.SetResourceReference(ContentControl.ContentProperty, "tem");
                tbtitle3_2.Text = (string)page.Resources["tem"];
                tbtitle3_2.FontSize = 20;
                tbtitle3_2.Visibility = Visibility.Collapsed;
                sp5_1.Children.Add(tbtitle3_2);
                for (int i = 0; i < 2; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    //温度由一帧和另一帧的最后一个字节数据组成，故有8个数据
                    for (int j = 0; j < 8; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 180;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp5_1.Children.Add(sp_info);
                }

                StackPanel sp6 = page.FindName("masterShiShiInfo4") as StackPanel;
                sp6.Children.Clear();
                TextBlock tbtitle4_1 = new TextBlock();
                //tbtitle4_1.Text = "CC信号及其他信号状态:";
                //tbtitle4_1.SetResourceReference(ContentControl.ContentProperty, "CC");
                tbtitle4_1.Text = (string)page.Resources["CC"];
                tbtitle4_1.FontSize = 20;
                tbtitle4_1.Visibility = Visibility.Collapsed;
                sp6.Children.Add(tbtitle4_1);
                for (int i = 0; i < 6; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp6.Children.Add(sp_info);
                }

                StackPanel sp4_2_2 = page.FindName("masterShiShiInfo4_2") as StackPanel;
                sp4_2_2.Children.Clear();
                TextBlock tbtitle4_2 = new TextBlock();
                //tbtitle4_2.Text = "信号采集线状态（√正常；×开路）:";
                //tbtitle4_2.SetResourceReference(ContentControl.ContentProperty, "signalstatus2");
                tbtitle4_2.Text = (string)page.Resources["signalstatus2"];
                tbtitle4_2.FontSize = 20;
                tbtitle4_2.Visibility = Visibility.Collapsed;
                sp4_2_2.Children.Add(tbtitle4_2);
                for (int i = 0; i < 242; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp4_2_2.Children.Add(sp_info);
                }
                StackPanel sp4_2_4 = page.FindName("masterShiShiInfo4_4") as StackPanel;
                sp4_2_4.Children.Clear();
                TextBlock tbtitle4_4 = new TextBlock();
                //tbtitle4_2.Text = "信号采集线状态（√正常；×开路）:";
                //tbtitle4_2.SetResourceReference(ContentControl.ContentProperty, "signalstatus2");
                tbtitle4_4.Text = (string)page.Resources["slavetemp"];
                tbtitle4_4.FontSize = 20;
                tbtitle4_4.Visibility = Visibility.Collapsed;
                sp4_2_4.Children.Add(tbtitle4_4);
                for (int i = 0; i < 120; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 4; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp4_2_4.Children.Add(sp_info);
                }
                //info25
                StackPanel sp4_2_3 = page.FindName("masterShiShiInfo4_3") as StackPanel;
                sp4_2_3.Children.Clear();
                TextBlock tbtitle4_3 = new TextBlock();
                //tbtitle4_2.Text = "信号采集线状态（√正常；×开路）:";
                //tbtitle4_2.SetResourceReference(ContentControl.ContentProperty, "signalstatus2");
                tbtitle4_3.Text = (string)page.Resources["singnalVol"];
                tbtitle4_3.FontSize = 20;
                tbtitle4_3.Visibility = Visibility.Collapsed;
                sp4_2_3.Children.Add(tbtitle4_3);
                for (int i = 0; i < 1; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 3; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp4_2_3.Children.Add(sp_info);
                }

                StackPanel sp7 = page.FindName("masterShiShiInfo5") as StackPanel;
                sp7.Children.Clear();
                TextBlock tbtitle5 = new TextBlock();
                //tbtitle5.Text = "单体均衡状态（√均衡开启；×均衡关闭）:";
                //tbtitle5.SetResourceReference(ContentControl.ContentProperty, "celljhstatus");
                tbtitle5.Text = (string)page.Resources["celljhstatus"];
                tbtitle5.FontSize = 20;
                tbtitle5.Visibility = Visibility.Collapsed;
                sp7.Children.Add(tbtitle5);
                for (int i = 0; i < 242; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 220;
                        //tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp7.Children.Add(sp_info);
                }
                StackPanel sp8 = page.FindName("masterShiShiInfo6") as StackPanel;
                sp8.Children.Clear();
                TextBlock tbtitle = new TextBlock();
                //tbtitle.Text = "从机离线故障（√正常，×失联）:";
                //tbtitle.SetResourceReference(ContentControl.ContentProperty, "bmuofflinefault");
                tbtitle.Text = (string)page.Resources["bmuofflinefault"];
                tbtitle.FontSize = 20;
                tbtitle.Visibility = Visibility.Collapsed;
                sp8.Children.Add(tbtitle);
                for (int i = 0; i < 3; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp8.Children.Add(sp_info);
                }

                StackPanel sp9 = page.FindName("masterShiShiInfo7") as StackPanel;
                sp9.Children.Clear();
                TextBlock tbtitle2 = new TextBlock();
                //tbtitle2.Text = "继电器状态（√闭合，×断开）:";
                //tbtitle2.SetResourceReference(ContentControl.ContentProperty, "relaystatus");
                tbtitle2.Text = (string)page.Resources["relaystatus"];
                tbtitle2.FontSize = 20;
                tbtitle2.Visibility = Visibility.Collapsed;
                sp9.Children.Add(tbtitle2);
                for (int i = 0; i < 2; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        // tb.Width = 170;
                        tb.Width = 300;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp9.Children.Add(sp_info);
                }

                //继电器逻辑状态
                StackPanel sp9_72 = page.FindName("masterShiShiInfo7_2") as StackPanel;
                sp9_72.Children.Clear();
                TextBlock tbtitle2_72 = new TextBlock();
                tbtitle2_72.Text = (string)page.Resources["relaylogicstatus"];
                tbtitle2_72.FontSize = 20;
                tbtitle2_72.Visibility = Visibility.Collapsed;
                sp9_72.Children.Add(tbtitle2_72);
                for (int i = 0; i < 2; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        // tb.Width = 170;
                        tb.Width = 300;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp9_72.Children.Add(sp_info);
                }

                StackPanel sp10 = page.FindName("masterShiShiInfo8") as StackPanel;
                sp10.Children.Clear();
                TextBlock tbtitle3 = new TextBlock();
                //tbtitle3.Text = "系统运行模式:";
                //tbtitle3.SetResourceReference(ContentControl.ContentProperty, "systemmode");
                tbtitle3.Text = (string)page.Resources["systemmode"];
                tbtitle3.FontSize = 20;
                tbtitle3.Visibility = Visibility.Collapsed;
                sp10.Children.Add(tbtitle3);
                for (int i = 0; i < 1; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 250;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp10.Children.Add(sp_info);
                }

                StackPanel sp11 = page.FindName("masterShiShiInfo9") as StackPanel;
                sp11.Children.Clear();
                TextBlock tbtitle4 = new TextBlock();
                //tbtitle4.Text = "BMS时钟：";
                //tbtitle4.SetResourceReference(ContentControl.ContentProperty, "bmsclock");
                tbtitle4.Text = (string)page.Resources["bmsclock"];
                tbtitle4.FontSize = 20;
                tbtitle4.Visibility = Visibility.Collapsed;
                sp11.Children.Add(tbtitle4);
                for (int i = 0; i < 1; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 7; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 210;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp11.Children.Add(sp_info);
                }

                WrapPanel sp12 = page.FindName("masterShiShiInfo10") as WrapPanel;
                sp12.Children.Clear();
                //清空异常报警缓存
                alarmInfo.Clear();

                
                StackPanel sp13 = page.FindName("masterShiShiInfo11") as StackPanel;
                sp13.Children.Clear();
                TextBlock tbtitle11 = new TextBlock();
                //tbtitle11.Text = "进出水水口温度:";
                //tbtitle11.SetResourceReference(ContentControl.ContentProperty, "watertem2");
                tbtitle11.Text = (string)page.Resources["watertem2"];
                tbtitle11.FontSize = 20;
                tbtitle11.Visibility = Visibility.Collapsed;
                sp13.Children.Add(tbtitle11);
                for (int i = 0; i < 1; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 2; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 300;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp13.Children.Add(sp_info);
                }
                //info26-31的data[2]
                StackPanel sp14 = page.FindName("masterShiShiInfo12") as StackPanel;
                sp14.Children.Clear();
                TextBlock tbtitle12 = new TextBlock();
                tbtitle12.Text = (string)page.Resources["nengliangInfo"];
                tbtitle12.FontSize = 20;
                tbtitle12.Visibility = Visibility.Collapsed;
                sp14.Children.Add(tbtitle12);
                for (int i = 0; i < 12; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 6; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 210;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp14.Children.Add(sp_info);
                }

                StackPanel sp15 = page.FindName("masterShiShiInfo13") as StackPanel;
                sp15.Children.Clear();
                TextBlock tbtitle13 = new TextBlock();
                tbtitle13.Text = (string)page.Resources["guzhanglevelInfo"];
                tbtitle13.FontSize = 20;
                tbtitle13.Visibility = Visibility.Collapsed;
                sp15.Children.Add(tbtitle13);
                for (int i = 0; i < 6; i++)
                {
                    StackPanel sp_info = new StackPanel();
                    sp_info.Orientation = Orientation.Horizontal;
                    sp_info.VerticalAlignment = VerticalAlignment.Center;
                    sp_info.IsEnabled = false;
                    sp_info.HorizontalAlignment = HorizontalAlignment.Left;
                    for (int j = 0; j < 6; j++)
                    {
                        StackPanel tb = new StackPanel();

                        tb.Visibility = Visibility.Collapsed;
                        tb.Orientation = Orientation.Horizontal;
                        tb.Width = 170;
                        tb.Height = 30;
                        tb.Margin = new Thickness(2, 0, 0, 0);
                        sp_info.Children.Add(tb);
                    }
                    sp15.Children.Add(sp_info);
                }


                StackPanel sp = page.FindName("masterShiShiInfo2") as StackPanel;
                DYWDList = MyVisualTreeHelper.FindVisualChild<StackPanel>(sp);//电压温度的stackpanel

                isGetMasterInfo = 1; //开启主机实时信息的接收
                new Thread(new ThreadStart(sendMasterShiShiInfoThread)).Start();
            }

        }

        
        private void sendMasterShiShiInfoThread()
        {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F303F0", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0] = 0x52;
            data[1] = 0x45;
            data[2] = 0x41;
            data[3] = 0x44;
            data[4] = 0x00;
            data[5] = 0x00;
            data[6] = 0x00;
            data[7] = DataConverter.getAllBytesSum(data);

            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);

        }
        private void runMasterRelayCommand()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0] = 0x61;
            WrapPanel wp = page.FindName("relay_wp") as WrapPanel; //bmu均衡信息

            List<CheckBox> relayList = MyVisualTreeHelper.FindVisualChild<CheckBox>(wp);

            if (relayList == null) { return; }
            for (int j = 0; j < 2; j++)
            {
                int b = 0;
                for (int i = 0; i < 8; i++)
                {
                    var query1 = from tt in relayList where tt.Name == "relay" + (i + 8 * j) select tt;
                    //String name = "relay" + (i + 8 * j);
                    if (query1 != null && query1.Count() > 0)
                    {
                        //CheckBox cb = page.FindName(name) as CheckBox;

                        int k = query1.First().IsChecked == true ? 1 : 0;
                        // b = b | k << (7 - i);
                        b = b | k << i;

                    }
                }
                data[j + 1] = (byte)b;
            }
            data[3] = 0x00;
            data[4] = 0x00;
            data[5] = 0x00;
            data[6] = 0x00;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }
        private void runMasterClockCommand()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            byte[] data = new byte[8];
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            int year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int second = currentTime.Second;

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x41;
            data[1] = DataConverter.strToHexByte((year-2000).ToString())[0];
            data[2] = DataConverter.strToHexByte(month.ToString())[0];
            data[3] = DataConverter.strToHexByte(day.ToString())[0];
            data[4] = DataConverter.strToHexByte(hour.ToString())[0];
            data[5] = DataConverter.strToHexByte(minute.ToString())[0];
            data[6] = DataConverter.strToHexByte(second.ToString())[0];
            //data[1] = DataConverter.strToHexByte(((year % 1000 % 100 / 10) << 4 | (year % 1000 % 100 % 10)).ToString())[0];
            //data[1] = (byte)((year % 1000 % 100 / 10) << 4 | (year % 1000 % 100 % 10));
            //data[2] = (byte)((month / 10) << 4 | (month % 10));
            //data[3] = (byte)((day / 10) << 4 | (day % 10));
            //data[4] = (byte)((hour / 10) << 4 | (hour % 10));
            //data[5] = (byte)((minute / 10) << 4 | (minute % 10));
            //data[6] = (byte)((second / 10) << 4 | (second % 10));
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);

        }
        //主机握手
        private void runHandShakeWithMaster()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            //初始化版本号显示界面
            for (int i = 0; i < VersionVisibility.Count; i++)
            {
                VersionVisibility[i] = "Collapsed";
            }
            //初始化主机串数信息界面
            StackPanel sp = page.FindName("masterChuanShuInfo") as StackPanel;
            sp.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                StackPanel sp_info = new StackPanel();
                sp_info.Orientation = Orientation.Horizontal;
                sp_info.HorizontalAlignment = HorizontalAlignment.Center;
                for (int j = 0; j < 6; j++)
                {
                    TextBox tb = new TextBox();

                    tb.Visibility = Visibility.Hidden;
                    tb.Width = 150;
                    sp_info.Children.Add(tb);
                }
                sp.Children.Add(sp_info);
            }
            //初始化主机温感信息界面
            StackPanel sp2 = page.FindName("masterWenGanInfo") as StackPanel;
            sp2.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                StackPanel sp_info = new StackPanel();
                sp_info.Orientation = Orientation.Horizontal;
                sp_info.HorizontalAlignment = HorizontalAlignment.Center;
                for (int j = 0; j < 6; j++)
                {
                    TextBox tb = new TextBox();

                    tb.Visibility = Visibility.Hidden;
                    tb.Width = 150;
                    sp_info.Children.Add(tb);
                }
                sp2.Children.Add(sp_info);
            }

            //正在监听数据则关闭并清空
            if (!isMasterShishi)
            {                
                runMasterShiShiInfo();
            }

            clearMasterInfo();

            new Thread(new ThreadStart(SendHandShakeWithMasterThread)).Start();
            if (sendUpSystemHeart == null)
            {
                //开启上位机发送心跳
                sendUpSystemHeart = new Thread(new ThreadStart(sendHeartThread));
                sendUpSystemHeart.IsBackground = true;
                sendUpSystemHeart.Start();
            }

        }

        //清空主机信息显示页
        private void clearMasterInfo()
        {
            StackPanel sp3 = page.FindName("masterShiShiInfo1") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo2") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo2_1") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo3") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo3_1") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo4") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo4_2") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo4_4") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo4_3") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo5") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo6") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo7") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo7_2") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo8") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo9") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo12") as StackPanel;
            sp3.Children.Clear();
            sp3 = page.FindName("masterShiShiInfo13") as StackPanel;
            sp3.Children.Clear();
            WrapPanel sp12 = page.FindName("masterShiShiInfo10") as WrapPanel;
            sp12.Children.Clear();
            //WrapPanel sp4 = page.FindName("mastershishiinfo10") as WrapPanel;
            //sp4.Children.Clear();
        }

        //VIN配置
        private void runVINConfigCommand() {
            byte[] b = DataConverter.str2ASCII(VIN);
            for (int i = 0; i < 3; i++)
            {
                CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
                obj.Init();
                obj.RemoteFlag = 0;//数据帧
                obj.ExternFlag = 1;//扩展帧
                obj.SendType = 0;//正常发送
                obj.DataLen = 8;
                obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
                byte[] data = new byte[8];
                data[0] = (byte)(0x4E + i);
                for (int j = 0; j < 6; j++)
                {
                    if ((i * 6 + j) < b.Length)
                    {
                        data[j + 1] = b[i * 6 + j];
                    }
                    else
                    {
                        data[j + 1] = 0x00;
                    }
                }
                data[7] = DataConverter.getAllBytesSum(data);
                obj.Data = data;
                Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                Thread.Sleep(100);
            }
        }

        //从机硬件版本号配置
        private void runSlaveHardwareVersionCommand() {
            byte[] b = DataConverter.str2ASCII(SlaveHardwareVersion);
            for (int i = 0; i < 3; i++)
            {
                CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
                obj.Init();
                obj.RemoteFlag = 0;//数据帧
                obj.ExternFlag = 1;//扩展帧
                obj.SendType = 0;//正常发送
                obj.DataLen = 8;
                obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
                byte[] data = new byte[8];
                data[0] = (byte)(0x47 + i);
                for (int j = 0; j < 6; j++)
                {
                    if ((i * 6 + j) < b.Length)
                    {
                        data[j + 1] = b[i * 6 + j];
                    }
                    else
                    {
                        data[j + 1] = 0x00;
                    }
                }
                data[7] = DataConverter.getAllBytesSum(data);
                obj.Data = data;
                Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                Thread.Sleep(100);
            }
        }

        //主机硬件版本号配置
        private void runMasterHardwareVersionCommand()
        {
            byte[] b = DataConverter.str2ASCII(MasterHardwareVersion);
            for (int i = 0; i < 3; i++) {
                CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
                obj.Init();
                obj.RemoteFlag = 0;//数据帧
                obj.ExternFlag = 1;//扩展帧
                obj.SendType = 0;//正常发送
                obj.DataLen = 8;
                obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
                byte[] data = new byte[8];
                data[0]=(byte)(0x44+i);
                for (int j = 0; j < 6; j++) {
                    if ((i * 6 + j) < b.Length)
                    {
                        data[j + 1] = b[i * 6 + j];
                    }
                    else {
                        data[j + 1] = 0x00;
                    }
                }
                data[7] = DataConverter.getAllBytesSum(data);
                obj.Data = data;
                Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
                 CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                 Thread.Sleep(100);
            }
            
        }

        //主机总容量配置
        private void runMasterZCapacityCommand() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            byte[] data = new byte[8];

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x42;
            ushort d = (ushort)(zCapacity * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = t[0];
            data[6] = t[1];
            data[7] = DataConverter.getAllBytesSum(data);
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }


        //主机标称容量配置
        private void runMasterBCCapacityCommand() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            byte[] data = new byte[8];

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x42;
            ushort d = (ushort)(bCCapacity * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = t[0];
            data[4] = t[1];
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //主机剩余容量配置
        private void runMasterCapacityCommand()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            byte[] data = new byte[8];


            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x42;
            ushort d = (ushort)(leftCapacity * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = t[0];
            data[2] = t[1];
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //放电总能量配置
        private void runFangdznlCommand() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }

            byte[] data = new byte[8];
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x4B;
            int d = (int)fangdznl * 100;
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = t[0];
            data[2] = t[1];
            data[3] = t[2];
            data[4] = t[3];
            data[5] = 0x00;
            data[6] = 0x00;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }


        //Shunt_2电流偏移配置
        private void runShunt2Command() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (Shunt_2Cur > 100 | Shunt_2Cur < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x43;
            short d = (short)(Shunt_2Cur * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = t[0];
            data[6] = t[1];
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //Shunt_1电流偏移配置
        private void runShunt1Command() {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (Shunt_1Cur > 100 | Shunt_1Cur < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x43;
            short d = (short)(Shunt_1Cur * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = t[0];
            data[4] = t[1];
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }
        //霍尔1电流偏移配置
        private void runMasterDeviationCommand()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (deviation > 100 | deviation < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];


            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x43;
            short d = (short)(deviation * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = t[0];
            data[2] = t[1];
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }
        //霍尔2电流偏移配置
        private void runMasterDeviationCommand2()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (deviation2 > 100 | deviation2 < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];


            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x4A;
            short d = (short)(deviation2 * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = t[0];
            data[2] = t[1];
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //霍尔3电流偏移配置
        private void runMasterDeviationCommand3()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (deviation3 > 100 | deviation3 < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];


            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x4A;
            short d = (short)(deviation3 * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = t[0];
            data[4] = t[1];
            data[5] = 0xFF;
            data[6] = 0xFF;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        //霍尔4电流偏移配置
        private void runMasterDeviationCommand4()
        {
            //判断当前can是否连接
            if (!connectstate)
            {
                //ModernDialog.ShowMessage("请连接设备！", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);
                IsCheckAllInfo = false;
                return;
            }
            if (deviation4 > 100 | deviation4 < -100)
            {
                //ModernDialog.ShowMessage("电流偏移范围在-100到100之间", "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["currlimit"], (string)page.Resources["tips"], MessageBoxButton.OK);
                return;
            }
            byte[] data = new byte[8];


            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            data[0] = 0x4A;
            short d = (short)(deviation4 * 10 + 1000);
            //ushort d = (ushort)(deviation * 10);
            byte[] t = System.BitConverter.GetBytes(d);
            data[1] = 0xFF;
            data[2] = 0xFF;
            data[3] = 0xFF;
            data[4] = 0xFF;
            data[5] = t[0];
            data[6] = t[1];
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
        }

        private void SendHandShakeWithMasterThread()
        {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//远程帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F103F0", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0] = 0x47;
            data[1] = 0x58;
            data[2] = 0x42;
            data[3] = 0x4D;
            data[4] = 0x53;
            data[5] = data[6] = 0x00;
            data[7] = DataConverter.getAllBytesSum(data);
            obj.DataLen = 8;
            obj.Data = data;
            // canBootNext = 0;//如果握手成功再赋值为1
            sendMasterHSData(new sendInfo("masterHS", obj));//发送握手指令
        }

        private void sendMasterHSData(Object o)
        {
            sendData(o);
            Console.WriteLine("发送了主机握手信息");
        }

        Thread sendFactoryThread;//发送出厂配置线程
        //发送初始配置
        public void runInitConfigCommand()
        {
            sendFactoryThread = new Thread(new ThreadStart(SendFactoryDataThread));
            sendFactoryThread.Start();
        }

        //已经改为低字节在前，高字节在后
        private void SendFactoryDataThread()
        {
            //try
            //{
            if (FactoryConfig.if17andOther())
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //ModernDialog.ShowMessage("请勿将info17与其他属性同时配置！", "提示", MessageBoxButton.OK);
                    ModernDialog.ShowMessage((string)page.Resources["dontinfo17"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });

                return;
            }

            if (FactoryConfig.IsChecked20 && !new Regex("^[0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef]$").IsMatch(FactoryConfig.SerialNum))
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //ModernDialog.ShowMessage("info20格式有误，应为6位16进制字符串！", "提示", MessageBoxButton.OK);
                    ModernDialog.ShowMessage((string)page.Resources["info20formatwrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });

                return;
            }


            FactoryConfig.ifHasNull(false);
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            // obj.flag = "info1";//配置信息info1
            if (SlaveNumTarget != null && SlaveNumTarget.Length == 1) { SlaveNumTarget = "0" + SlaveNumTarget; }
            string s = "0C21" + int.Parse(SlaveNumTarget).ToString("X2") + "7F";
            //string s = "0C21017F";//调试
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            //data[0] = 0x51;
            byte byted;
            ushort d;
            byte[] t;
            int temp = FactoryConfig.ifHasNull(false);
            if (temp != -1)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //ModernDialog.ShowMessage("请完整填写info" + temp + "的内容！", "提示", MessageBoxButton.OK);
                    //please fill in The ** completely
                    ModernDialog.ShowMessage((string)page.Resources["plswrite"] + temp + (string)page.Resources["scontent"], (string)page.Resources["tips"], MessageBoxButton.OK);
                });

                return;
            }
            //info17单独配置
            if (FactoryConfig.IsChecked17)
            {
                //info17
                // Thread.Sleep(5);
                data[0] = 0x51;
                byted = (byte)((double.Parse(FactoryConfig.SlaveNum) - ResolutionRatioModel.slaveNum_offset) / ResolutionRatioModel.slaveNum_rr);
                data[1] = byted;
                if (FactoryConfig.CellBalanMode != null)
                    byted = (byte)((double.Parse(FactoryConfig.CellBalanMode) - ResolutionRatioModel.cellBalanMode_offset) / ResolutionRatioModel.cellBalanMode_rr);
                data[2] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ChildModuleMonCellNumber) - ResolutionRatioModel.childModuleMonCellNumber_offset) / ResolutionRatioModel.childModuleMonCellNumber_rr);
                data[3] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ChildMonModuleTemperatureNumber) - ResolutionRatioModel.childMonModuleTemperatureNumber_offset) / ResolutionRatioModel.childMonModuleTemperatureNumber_rr);
                data[4] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleAMonCellNum) - ResolutionRatioModel.moduleAMonCellNum_offset) / ResolutionRatioModel.moduleAMonCellNum_rr);
                data[5] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleAMonTemperatureNum) - ResolutionRatioModel.moduleAMonTemperatureNum_offset) / ResolutionRatioModel.moduleAMonTemperatureNum_rr);
                data[6] = byted;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                // this.page.sendFacConfig.IsEnabled = false;
                sendData(new sendInfo("fac17", obj));
            }
            if (FactoryConfig.IsChecked1)
            {
                //info1
                // Thread.Sleep(5);
                data[0] = 0x41;
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmFirst) - ResolutionRatioModel.cellVolHighAlarmFirst_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmSecond) - ResolutionRatioModel.cellVolHighAlarmSecond_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmThird) - ResolutionRatioModel.cellVolHighAlarmThird_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;

                sendData(new sendInfo("fac1", obj));
            }

            if (FactoryConfig.IsChecked2)
            {
                //info2
                // Thread.Sleep(5);
                data[0] = 0x42;
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmRemoveFirst) - ResolutionRatioModel.cellVolHighAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmRemoveSecond) - ResolutionRatioModel.cellVolHighAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolHighAlarmRemoveThird) - ResolutionRatioModel.cellVolHighAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac2", obj));
            }
            if (FactoryConfig.IsChecked3)
            {
                //info3
                //Thread.Sleep(5);
                data[0] = 0x43;
                double mytest = (double.Parse(FactoryConfig.CellVolLowAlarmFirst) - ResolutionRatioModel.cellVolLowAlarmFirst_offset) * ResolutionRatioModel.cellVolLowAlarmFirst_rr * 1000000;
                //d = (ushort)((float.Parse(FactoryConfig.CellVolLowAlarmFirst) - ResolutionRatioModel.cellVolLowAlarmFirst_offset) / ResolutionRatioModel.cellVolLowAlarmFirst_rr);
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmFirst) - ResolutionRatioModel.cellVolLowAlarmFirst_offset) * ResolutionRatioModel.cellVolLowAlarmFirst_rr * 1000000);

                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmSecond) - ResolutionRatioModel.cellVolLowAlarmSecond_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmThird) - ResolutionRatioModel.cellVolLowAlarmThird_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmThird_rr);
                // d = (ushort)5000;
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac3", obj));
            }
            if (FactoryConfig.IsChecked4)
            {
                //info4
                // Thread.Sleep(5);
                data[0] = 0x44;
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmRemoveFirst) - ResolutionRatioModel.cellVolLowAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmRemoveSecond) - ResolutionRatioModel.cellVolLowAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.CellVolLowAlarmRemoveThird) - ResolutionRatioModel.cellVolLowAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac4", obj));
            }
            if (FactoryConfig.IsChecked5)
            {
                //info5

                // Thread.Sleep(5);
                data[0] = 0x45;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmFirst) - ResolutionRatioModel.cellTemperatureHighAlarmFirst_offset) * (1 / ResolutionRatioModel.cellTemperatureHighAlarmFirst_rr));
                data[1] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmSecond) - ResolutionRatioModel.cellTemperatureHighAlarmSecond_offset) / ResolutionRatioModel.cellTemperatureHighAlarmSecond_rr);
                data[2] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmThird) - ResolutionRatioModel.cellTemperatureHighAlarmThird_offset) / ResolutionRatioModel.cellTemperatureHighAlarmThird_rr);
                data[3] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmRemoveFirst) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_rr);
                data[4] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmRemoveSecond) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_rr);
                data[5] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureHighAlarmRemoveThird) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_rr);
                data[6] = byted;
                // Console.WriteLine("data[6]:"+data[6]);
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac5", obj));
            }
            if (FactoryConfig.IsChecked6)
            {
                //info6
                // Thread.Sleep(5);
                data[0] = 0x46;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmFirst) - ResolutionRatioModel.cellTemperatureLowAlarmFirst_offset) / ResolutionRatioModel.cellTemperatureLowAlarmFirst_rr);
                data[1] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmSecond) - ResolutionRatioModel.cellTemperatureLowAlarmSecond_offset) / ResolutionRatioModel.cellTemperatureLowAlarmSecond_rr);
                data[2] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmThird) - ResolutionRatioModel.cellTemperatureLowAlarmThird_offset) / ResolutionRatioModel.cellTemperatureLowAlarmThird_rr);
                data[3] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmRemoveFirst) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_rr);
                data[4] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmRemoveSecond) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_rr);
                data[5] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellTemperatureLowAlarmRemoveThird) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_rr);
                data[6] = byted;
                //Console.WriteLine("data[6]:" + data[6]);
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac6", obj));
            }
            if (FactoryConfig.IsChecked7)
            {
                //info7
                // Thread.Sleep(5);
                data[0] = 0x47;
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmFirst) - ResolutionRatioModel.balanCurrentHighAlarmFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmSecond) - ResolutionRatioModel.balanCurrentHighAlarmSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmThird) - ResolutionRatioModel.balanCurrentHighAlarmThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac7", obj));
            }
            if (FactoryConfig.IsChecked8)
            {
                //info8
                //  Thread.Sleep(5);
                data[0] = 0x48;
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmRemoveFirst) - ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmRemoveSecond) - ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentHighAlarmRemoveThird) - ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac8", obj));
            }
            if (FactoryConfig.IsChecked9)
            {
                //info9
                // Thread.Sleep(5);
                data[0] = 0x49;
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmFirst) - ResolutionRatioModel.balanCurrentLowAlarmFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmSecond) - ResolutionRatioModel.balanCurrentLowAlarmSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmThird) - ResolutionRatioModel.balanCurrentLowAlarmThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac9", obj));
            }
            if (FactoryConfig.IsChecked10)
            {
                //info10
                // Thread.Sleep(5);
                data[0] = 0x4A;
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmRemoveFirst) - ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmRemoveSecond) - ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentLowAlarmRemoveThird) - ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac10", obj));
            }
            if (FactoryConfig.IsChecked11)
            {
                //info11
                // Thread.Sleep(5);
                data[0] = 0x4B;
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentSetValueFirst) - ResolutionRatioModel.balanCurrentSetValueFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueFirst_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];//低字节
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentSetValueSecond) - ResolutionRatioModel.balanCurrentSetValueSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueSecond_rr);
                t = System.BitConverter.GetBytes(d);
                data[4] = t[1];
                data[3] = t[0];
                d = (ushort)((double.Parse(FactoryConfig.BalanCurrentSetValueThird) - ResolutionRatioModel.balanCurrentSetValueThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueThird_rr);
                t = System.BitConverter.GetBytes(d);
                data[6] = t[1];
                data[5] = t[0];
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac11", obj));
            }
            if (FactoryConfig.IsChecked12)
            {
                //info12
                //Thread.Sleep(5);
                data[0] = 0x4C;
                d = (ushort)((double.Parse(FactoryConfig.BalanVolOpenValue) - ResolutionRatioModel.balanVolOpenValue_offset) * 1000000 * ResolutionRatioModel.balanVolOpenValue_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                data[3] = 0xff;
                data[4] = 0xff;
                data[5] = 0xff;
                data[6] = 0xff;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac12", obj));
            }
            if (FactoryConfig.IsChecked13)
            {
                //info13
                //  Thread.Sleep(5);
                data[0] = 0x4D;
                d = (ushort)((double.Parse(FactoryConfig.BalanVolCloseValue) - ResolutionRatioModel.balanVolCloseValue_offset) * 1000000 * ResolutionRatioModel.balanVolCloseValue_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                data[3] = 0xff;
                data[4] = 0xff;
                data[5] = 0xff;
                data[6] = 0xff;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac13", obj));
            }
            if (FactoryConfig.IsChecked14)
            {
                //info14
                // Thread.Sleep(5);
                data[0] = 0x4E;
                d = (ushort)((double.Parse(FactoryConfig.BalanVolDifOpenValue) - ResolutionRatioModel.balanVolDifOpenValue_offset) * 1000000 * ResolutionRatioModel.balanVolDifOpenValue_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                data[3] = 0xff;
                data[4] = 0xff;
                data[5] = 0xff;
                data[6] = 0xff;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac14", obj));
            }
            if (FactoryConfig.IsChecked15)
            {
                //info15
                //Thread.Sleep(5);
                data[0] = 0x4F;
                d = (ushort)((double.Parse(FactoryConfig.BalanVolDifCloseValue) - ResolutionRatioModel.balanVolDifCloseValue_offset) * 1000000 * ResolutionRatioModel.balanVolDifCloseValue_rr);
                t = System.BitConverter.GetBytes(d);
                data[2] = t[1];
                data[1] = t[0];
                data[3] = 0xff;
                data[4] = 0xff;
                data[5] = 0xff;
                data[6] = 0xff;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac15", obj));
            }
            if (FactoryConfig.IsChecked16)
            {
                //info16
                // Thread.Sleep(5);
                data[0] = 0x50;
                byted = (byte)((double.Parse(FactoryConfig.CellBalanTemperatureOpenValue) - ResolutionRatioModel.cellBalanTemperatureOpenValue_offset) / ResolutionRatioModel.cellBalanTemperatureOpenValue_rr);
                data[1] = byted;
                byted = (byte)((double.Parse(FactoryConfig.CellBalanTemperatureCloseValue) - ResolutionRatioModel.cellBalanTemperatureCloseValue_offset) / ResolutionRatioModel.cellBalanTemperatureCloseValue_rr);
                data[2] = byted;
                data[3] = 0xff;
                data[4] = 0xff;
                data[5] = 0xff;
                data[6] = 0xff;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac16", obj));
            }

            if (FactoryConfig.IsChecked18)
            {
                //info18
                // Thread.Sleep(5);
                data[0] = 0x52;
                byted = (byte)((double.Parse(FactoryConfig.ModuleBMonCellNum) - ResolutionRatioModel.moduleBMonCellNum_offset) / ResolutionRatioModel.moduleBMonCellNum_rr);
                data[1] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleBMonTemperatureNum) - ResolutionRatioModel.moduleBMonTemperatureNum_offset) / ResolutionRatioModel.moduleBMonTemperatureNum_rr);
                data[2] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleCMonCellNum) - ResolutionRatioModel.moduleCMonCellNum_offset) / ResolutionRatioModel.moduleCMonCellNum_rr);
                data[3] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleCMonTemperatureNum) - ResolutionRatioModel.moduleCMonTemperatureNum_offset) / ResolutionRatioModel.moduleCMonTemperatureNum_rr);
                data[4] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleDMonCellNum) - ResolutionRatioModel.moduleDMonCellNum_offset) / ResolutionRatioModel.moduleDMonCellNum_rr);
                data[5] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleDMonTemperatureNum) - ResolutionRatioModel.moduleDMonTemperatureNum_offset) / ResolutionRatioModel.moduleDMonTemperatureNum_rr);
                data[6] = byted;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac18", obj));
            }
            if (FactoryConfig.IsChecked19)
            {
                //info19
                //  Thread.Sleep(5);
                data[0] = 0x53;
                byted = (byte)((double.Parse(FactoryConfig.ModuleEMonCellNum) - ResolutionRatioModel.moduleEMonCellNum_offset) / ResolutionRatioModel.moduleEMonCellNum_rr);
                data[1] = byted;
                byted = (byte)((double.Parse(FactoryConfig.ModuleEMonTemperatureNum) - ResolutionRatioModel.moduleEMonTemperatureNum_offset) / ResolutionRatioModel.moduleEMonTemperatureNum_rr);
                data[2] = byted;
                d = (ushort)((double.Parse(FactoryConfig.PackProYear) - ResolutionRatioModel.packProYear_offset) / ResolutionRatioModel.packProYear_rr);
                t = System.BitConverter.GetBytes(d);
                data[3] = t[1];
                data[4] = t[0];
                byted = (byte)((double.Parse(FactoryConfig.PackProMonth) - ResolutionRatioModel.packProMonth_offset) / ResolutionRatioModel.packProMonth_rr);
                data[5] = byted;
                byted = (byte)((double.Parse(FactoryConfig.PackProDay) - ResolutionRatioModel.packProDay_offset) / ResolutionRatioModel.packProDay_rr);
                data[6] = byted;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac19", obj));
            }
            if (FactoryConfig.IsChecked20)
            {
                //info20
                //  Thread.Sleep(5);
                data[0] = 0x54;
                String sn = FactoryConfig.SerialNum;
                byted = (byte)((Convert.ToInt32(sn[0].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                // byted = (byte)((double.Parse(FactoryConfig.PackBatchNumberData1) - ResolutionRatioModel.packBatchNumberData1_offset) / ResolutionRatioModel.packBatchNumberData1_rr);
                data[1] = byted;
                byted = (byte)((Convert.ToInt32(sn[1].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                data[2] = byted;
                byted = (byte)((Convert.ToInt32(sn[2].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                data[3] = byted;
                byted = (byte)((Convert.ToInt32(sn[3].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                data[4] = byted;
                byted = (byte)((Convert.ToInt32(sn[4].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                data[5] = byted;
                byted = (byte)((Convert.ToInt32(sn[5].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                data[6] = byted;
                data[7] = checkSum(data);//校验和
                obj.Data = data;
                sendData(new sendInfo("fac20", obj));
            }
            //}
            //catch (Exception e) {
            //    logger.Error(e.Message);
            //}
        }

        //public void sendT2()
        //{
        //    uint id;
        //    byte[] data = new byte[8];
        //    int i = 0;
        //    //如果接收线程没开启则打开
        //    if (receiveThread == null)
        //    {
        //        receiveThread = new Thread(new ThreadStart(ReceiveDataThread));
        //        receiveThread.IsBackground = true;
        //        receiveThread.Start();
        //    }
        //    saveBmuConfigList.Clear();
        //    List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
        //    for (int k = 0; k < tabItemList.Count; k++)
        //    {
        //        TabItem t = tabItemList[k];

        //        //ScrollViewer sv = t.Content as ScrollViewer;
        //        //StackPanel sp = sv.Content as StackPanel;
        //        //BMUConfig bc = sp.Children[0] as BMUConfig;
        //        //BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;

        //        Application.Current.Dispatcher.Invoke((Action)delegate
        //        {
        //            ScrollViewer sv = t.Content as ScrollViewer;
        //            StackPanel sp = sv.Content as StackPanel;
        //            BMUConfig bc = sp.Children[0] as BMUConfig;
        //            BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;


        //            if (bcm.BmuConfigModel_gy.if17andOther())
        //            {
        //                Application.Current.Dispatcher.Invoke((Action)delegate
        //                {
        //                    //ModernDialog.ShowMessage("请勿将info17与其他属性同时配置！", "提示", MessageBoxButton.OK);
        //                    ModernDialog.ShowMessage((string)page.Resources["dontinfo17"], (string)page.Resources["tips"], MessageBoxButton.OK);
        //                });

        //                return;
        //            }
        //            if (bcm.BmuConfigModel_gy.IsChecked20 && !new Regex("^[0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef]$").IsMatch(bcm.BmuConfigModel_gy.SerialNum))
        //            {
        //                Application.Current.Dispatcher.Invoke((Action)delegate
        //                {
        //                    //ModernDialog.ShowMessage("info20格式有误，应为6位16进制字符串！", "提示", MessageBoxButton.OK);
        //                    ModernDialog.ShowMessage((string)page.Resources["info20formatwrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
        //                });

        //                return;
        //            }
        //            int temp = bcm.BmuConfigModel_gy.ifHasNull(true);
        //            if (temp != -1)
        //            {
        //                if (temp == 0)
        //                {
        //                    Application.Current.Dispatcher.Invoke((Action)delegate
        //                    {
        //                        //ModernDialog.ShowMessage("请填写从机编号", "提示", MessageBoxButton.OK);
        //                        ModernDialog.ShowMessage((string)page.Resources["plsbmuid"], (string)page.Resources["tips"], MessageBoxButton.OK);

        //                    });
        //                }
        //                else
        //                {
        //                    Application.Current.Dispatcher.Invoke((Action)delegate
        //                    {
        //                        //ModernDialog.ShowMessage("请完整填写info" + temp + "的内容！", "提示", MessageBoxButton.OK);
        //                        ModernDialog.ShowMessage((string)page.Resources["plswrite"] + temp + (string)page.Resources["scontent"], (string)page.Resources["tips"], MessageBoxButton.OK);

        //                    });
        //                }
        //                return;
        //            }
        //            saveBmuConfigList.Add(bcm.BmuConfigModel_gy);
        //        });

        //        //saveBmuConfigList.Add(bcm.BmuConfigModel_gy);
        //    }
        //    for (int k = 0; k < saveBmuConfigList.Count; k++)
        //    {
        //        BMUConfigModel_gy b = saveBmuConfigList[k];
        //        ushort d;
        //        byte[] t;
        //        byte byted;
        //        bmuConfigListNum = k;//正式使用时这里k改成i
        //        //  i++;
        //        i = 1;//正式使用时注释这里打开上面
        //        //string s = "0C21" + i.ToString("X2") + "7F";
        //        Console.WriteLine("slaveNum:" + b.Slave);
        //        if (b.Slave.Length == 1)
        //        {
        //            b.Slave = "0" + b.Slave;
        //        }
        //        string s = "0C21" + int.Parse(b.Slave).ToString("X2") + "7F";
        //        // string s = "0C21" + b.Slave + "7F";
        //        id = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
        //        CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
        //        obj.Init();
        //        obj.RemoteFlag = 0;//数据帧
        //        obj.ExternFlag = 1;//扩展帧
        //        obj.SendType = 0;//正常发送
        //        obj.DataLen = 8;
        //        obj.ID = id;
        //        if (b.IsChecked17)
        //        {
        //            //info17
        //            // Thread.Sleep(5);
        //            data[0] = 0x51;
        //            byted = (byte)((double.Parse(b.SlaveNum) - ResolutionRatioModel.slaveNum_offset) / ResolutionRatioModel.slaveNum_rr);
        //            data[1] = byted;
        //            byted = (byte)((double.Parse(b.CellBalanMode) - ResolutionRatioModel.cellBalanMode_offset) / ResolutionRatioModel.cellBalanMode_rr);
        //            data[2] = byted;
        //            byted = (byte)((double.Parse(b.ChildModuleMonCellNumber) - ResolutionRatioModel.childModuleMonCellNumber_offset) / ResolutionRatioModel.childModuleMonCellNumber_rr);
        //            data[3] = byted;
        //            byted = (byte)((double.Parse(b.ChildMonModuleTemperatureNumber) - ResolutionRatioModel.childMonModuleTemperatureNumber_offset) / ResolutionRatioModel.childMonModuleTemperatureNumber_rr);
        //            data[4] = byted;
        //            byted = (byte)((double.Parse(b.ModuleAMonCellNum) - ResolutionRatioModel.moduleAMonCellNum_offset) / ResolutionRatioModel.moduleAMonCellNum_rr);
        //            data[5] = byted;
        //            byted = (byte)((double.Parse(b.ModuleAMonTemperatureNum) - ResolutionRatioModel.moduleAMonTemperatureNum_offset) / ResolutionRatioModel.moduleAMonTemperatureNum_rr);
        //            data[6] = byted;
        //            data[7] = checkSum(data);//校验和
        //            obj.Data = data;
        //            sendData(new sendInfo("info17", obj));
        //        }
        //    }
        //}

        public void sendT()
        {

            uint id;
            byte[] data = new byte[8];
            int i = 0;
            //如果接收线程没开启则打开
            if (receiveThread == null)
            {
                receiveThread = new Thread(new ThreadStart(ReceiveDataThread));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            saveBmuConfigList.Clear();
            List<TabItem> tabItemList = bmuConfigList.ToList<TabItem>();
            for (int k = 0; k < tabItemList.Count; k++)
            {
                TabItem t = tabItemList[k];

                //ScrollViewer sv = t.Content as ScrollViewer;
                //StackPanel sp = sv.Content as StackPanel;
                //BMUConfig bc = sp.Children[0] as BMUConfig;
                //BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ScrollViewer sv = t.Content as ScrollViewer;
                    StackPanel sp = sv.Content as StackPanel;
                    BMUConfig bc = sp.Children[0] as BMUConfig;
                    BMUConfigViewModel bcm = bc.DataContext as BMUConfigViewModel;

                    
                        if (bcm.BmuConfigModel_gy.if17andOther())
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //ModernDialog.ShowMessage("请勿将info17与其他属性同时配置！", "提示", MessageBoxButton.OK);
                            ModernDialog.ShowMessage((string)page.Resources["dontinfo17"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });

                        return;
                    }
                    if (bcm.BmuConfigModel_gy.IsChecked20 && !new Regex("^[0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef][0123456789ABCDEFabcdef]$").IsMatch(bcm.BmuConfigModel_gy.SerialNum))
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //ModernDialog.ShowMessage("info20格式有误，应为6位16进制字符串！", "提示", MessageBoxButton.OK);
                            ModernDialog.ShowMessage((string)page.Resources["info20formatwrong"], (string)page.Resources["tips"], MessageBoxButton.OK);
                        });

                        return;
                    }
                    int temp = bcm.BmuConfigModel_gy.ifHasNull(true);
                    if (temp != -1)
                    {
                        if (temp == 0)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                //ModernDialog.ShowMessage("请填写从机编号", "提示", MessageBoxButton.OK);
                                ModernDialog.ShowMessage((string)page.Resources["plsbmuid"], (string)page.Resources["tips"], MessageBoxButton.OK);

                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                //ModernDialog.ShowMessage("请完整填写info" + temp + "的内容！", "提示", MessageBoxButton.OK);
                                ModernDialog.ShowMessage((string)page.Resources["plswrite"] + temp + (string)page.Resources["scontent"], (string)page.Resources["tips"], MessageBoxButton.OK);

                            });
                        }
                        return;
                    }
                    saveBmuConfigList.Add(bcm.BmuConfigModel_gy);
                });

                //saveBmuConfigList.Add(bcm.BmuConfigModel_gy);
            }

            for (int k = 0; k < saveBmuConfigList.Count; k++)
            {
                BMUConfigModel_gy b = saveBmuConfigList[k];
                ushort d;
                byte[] t;
                byte byted;
                bmuConfigListNum = k;//正式使用时这里k改成i
                //  i++;
                i = 1;//正式使用时注释这里打开上面
                //string s = "0C21" + i.ToString("X2") + "7F";
                Console.WriteLine("slaveNum:" + b.Slave);
                if (b.Slave.Length == 1)
                {
                    b.Slave = "0" + b.Slave;
                }
                string s = "0C21" + int.Parse(b.Slave).ToString("X2") + "7F";
                // string s = "0C21" + b.Slave + "7F";
                id = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
                CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
                obj.Init();
                obj.RemoteFlag = 0;//数据帧
                obj.ExternFlag = 1;//扩展帧
                obj.SendType = 0;//正常发送
                obj.DataLen = 8;
                obj.ID = id;
                if (b.IsChecked1)
                {
                    //info1
                    // Thread.Sleep(5);
                    data[0] = 0x41;
                    d = (ushort)((double.Parse(b.CellVolHighAlarmFirst) - ResolutionRatioModel.cellVolHighAlarmFirst_offset) * ResolutionRatioModel.cellVolHighAlarmFirst_rr * 1000000);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.CellVolHighAlarmSecond) - ResolutionRatioModel.cellVolHighAlarmSecond_offset) * ResolutionRatioModel.cellVolHighAlarmSecond_rr * 1000000);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.CellVolHighAlarmThird) - ResolutionRatioModel.cellVolHighAlarmThird_offset) * ResolutionRatioModel.cellVolHighAlarmThird_rr * 1000000);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和

                    obj.Data = data;
                    sendData(new sendInfo("info1", obj));
                }
                if (b.IsChecked2)
                {
                    //info2
                    //  Thread.Sleep(5);
                    data[0] = 0x42;
                    d = (ushort)((double.Parse(b.CellVolHighAlarmRemoveFirst) - ResolutionRatioModel.cellVolHighAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.CellVolHighAlarmRemoveSecond) - ResolutionRatioModel.cellVolHighAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.CellVolHighAlarmRemoveThird) - ResolutionRatioModel.cellVolHighAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.cellVolHighAlarmRemoveThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info2", obj));
                }
                if (b.IsChecked3)
                {
                    //info3
                    //  Thread.Sleep(5);
                    data[0] = 0x43;
                    d = (ushort)((double.Parse(b.CellVolLowAlarmFirst) - ResolutionRatioModel.cellVolLowAlarmFirst_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.CellVolLowAlarmSecond) - ResolutionRatioModel.cellVolLowAlarmSecond_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.CellVolLowAlarmThird) - ResolutionRatioModel.cellVolLowAlarmThird_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmThird_rr);
                    // d = (ushort)5000;
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info3", obj));
                }
                if (b.IsChecked4)
                {
                    //info4
                    // Thread.Sleep(5);
                    data[0] = 0x44;
                    d = (ushort)((double.Parse(b.CellVolLowAlarmRemoveFirst) - ResolutionRatioModel.cellVolLowAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.CellVolLowAlarmRemoveSecond) - ResolutionRatioModel.cellVolLowAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.CellVolLowAlarmRemoveThird) - ResolutionRatioModel.cellVolLowAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.cellVolLowAlarmRemoveThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info4", obj));
                }
                if (b.IsChecked5)
                {
                    //info5

                    // Thread.Sleep(5);
                    data[0] = 0x45;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmFirst) - ResolutionRatioModel.cellTemperatureHighAlarmFirst_offset) / ResolutionRatioModel.cellTemperatureHighAlarmFirst_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmSecond) - ResolutionRatioModel.cellTemperatureHighAlarmSecond_offset) / ResolutionRatioModel.cellTemperatureHighAlarmSecond_rr);
                    data[2] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmThird) - ResolutionRatioModel.cellTemperatureHighAlarmThird_offset) / ResolutionRatioModel.cellTemperatureHighAlarmThird_rr);
                    data[3] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmRemoveFirst) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_rr);
                    data[4] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmRemoveSecond) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_rr);
                    data[5] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureHighAlarmRemoveThird) - ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_offset) / ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_rr);
                    data[6] = byted;
                    // Console.WriteLine("data[6]:"+data[6]);
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info5", obj));
                }
                if (b.IsChecked6)
                {
                    //info6
                    // Thread.Sleep(5);
                    data[0] = 0x46;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmFirst) - ResolutionRatioModel.cellTemperatureLowAlarmFirst_offset) / ResolutionRatioModel.cellTemperatureLowAlarmFirst_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmSecond) - ResolutionRatioModel.cellTemperatureLowAlarmSecond_offset) / ResolutionRatioModel.cellTemperatureLowAlarmSecond_rr);
                    data[2] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmThird) - ResolutionRatioModel.cellTemperatureLowAlarmThird_offset) / ResolutionRatioModel.cellTemperatureLowAlarmThird_rr);
                    data[3] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmRemoveFirst) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_rr);
                    data[4] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmRemoveSecond) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_rr);
                    data[5] = byted;
                    byted = (byte)((double.Parse(b.CellTemperatureLowAlarmRemoveThird) - ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_offset) / ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_rr);
                    data[6] = byted;
                    //Console.WriteLine("data[6]:" + data[6]);
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info6", obj));
                }
                if (b.IsChecked7)
                {
                    //info7
                    // Thread.Sleep(5);
                    data[0] = 0x47;
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmFirst) - ResolutionRatioModel.balanCurrentHighAlarmFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmSecond) - ResolutionRatioModel.balanCurrentHighAlarmSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmThird) - ResolutionRatioModel.balanCurrentHighAlarmThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info7", obj));
                }
                if (b.IsChecked8)
                {
                    //info8
                    //  Thread.Sleep(5);
                    data[0] = 0x48;
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmRemoveFirst) - ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmRemoveSecond) - ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentHighAlarmRemoveThird) - ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info8", obj));
                }
                if (b.IsChecked9)
                {
                    //info9
                    //Thread.Sleep(5);
                    data[0] = 0x49;
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmFirst) - ResolutionRatioModel.balanCurrentLowAlarmFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmSecond) - ResolutionRatioModel.balanCurrentLowAlarmSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmThird) - ResolutionRatioModel.balanCurrentLowAlarmThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info9", obj));
                }
                if (b.IsChecked10)
                {
                    //info10
                    // Thread.Sleep(5);
                    data[0] = 0x4A;
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmRemoveFirst) - ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmRemoveSecond) - ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentLowAlarmRemoveThird) - ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info10", obj));
                }
                if (b.IsChecked11)
                {
                    //info11
                    //  Thread.Sleep(5);
                    data[0] = 0x4B;
                    d = (ushort)((double.Parse(b.BalanCurrentSetValueFirst) - ResolutionRatioModel.balanCurrentSetValueFirst_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueFirst_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentSetValueSecond) - ResolutionRatioModel.balanCurrentSetValueSecond_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueSecond_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[4] = t[1];
                    data[3] = t[0];
                    d = (ushort)((double.Parse(b.BalanCurrentSetValueThird) - ResolutionRatioModel.balanCurrentSetValueThird_offset) * 1000000 * ResolutionRatioModel.balanCurrentSetValueThird_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[6] = t[1];
                    data[5] = t[0];
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info11", obj));
                }
                if (b.IsChecked12)
                {
                    //info12
                    // Thread.Sleep(5);
                    data[0] = 0x4C;
                    d = (ushort)((double.Parse(b.BalanVolOpenValue) - ResolutionRatioModel.balanVolOpenValue_offset) * 1000000 * ResolutionRatioModel.balanVolOpenValue_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    data[3] = 0xff;
                    data[4] = 0xff;
                    data[5] = 0xff;
                    data[6] = 0xff;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info12", obj));
                }
                if (b.IsChecked13)
                {
                    //info13
                    // Thread.Sleep(5);
                    data[0] = 0x4D;
                    d = (ushort)((double.Parse(b.BalanVolCloseValue) - ResolutionRatioModel.balanVolCloseValue_offset) * 1000000 * ResolutionRatioModel.balanVolCloseValue_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    data[3] = 0xff;
                    data[4] = 0xff;
                    data[5] = 0xff;
                    data[6] = 0xff;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info13", obj));
                }
                if (b.IsChecked14)
                {
                    //info14
                    // Thread.Sleep(5);
                    data[0] = 0x4E;
                    d = (ushort)((double.Parse(b.BalanVolDifOpenValue) - ResolutionRatioModel.balanVolDifOpenValue_offset) * 1000000 * ResolutionRatioModel.balanVolDifOpenValue_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    data[3] = 0xff;
                    data[4] = 0xff;
                    data[5] = 0xff;
                    data[6] = 0xff;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info14", obj));
                }
                if (b.IsChecked15)
                {
                    //info15
                    // Thread.Sleep(5);
                    data[0] = 0x4F;
                    d = (ushort)((double.Parse(b.BalanVolDifCloseValue) - ResolutionRatioModel.balanVolDifCloseValue_offset) * 1000000 * ResolutionRatioModel.balanVolDifCloseValue_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[2] = t[1];
                    data[1] = t[0];
                    data[3] = 0xff;
                    data[4] = 0xff;
                    data[5] = 0xff;
                    data[6] = 0xff;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info15", obj));
                }
                if (b.IsChecked16)
                {
                    //info16
                    //  Thread.Sleep(5);
                    data[0] = 0x50;
                    byted = (byte)((double.Parse(b.CellBalanTemperatureOpenValue) - ResolutionRatioModel.cellBalanTemperatureOpenValue_offset) / ResolutionRatioModel.cellBalanTemperatureOpenValue_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.CellBalanTemperatureCloseValue) - ResolutionRatioModel.cellBalanTemperatureCloseValue_offset) / ResolutionRatioModel.cellBalanTemperatureCloseValue_rr);
                    data[2] = byted;
                    data[3] = 0xff;
                    data[4] = 0xff;
                    data[5] = 0xff;
                    data[6] = 0xff;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info16", obj));
                }
                //info17单独配置
                if (b.IsChecked17)
                {
                    //info17
                    // Thread.Sleep(5);
                    data[0] = 0x51;
                    byted = (byte)((double.Parse(b.SlaveNum) - ResolutionRatioModel.slaveNum_offset) / ResolutionRatioModel.slaveNum_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.CellBalanMode) - ResolutionRatioModel.cellBalanMode_offset) / ResolutionRatioModel.cellBalanMode_rr);
                    data[2] = byted;
                    byted = (byte)((double.Parse(b.ChildModuleMonCellNumber) - ResolutionRatioModel.childModuleMonCellNumber_offset) / ResolutionRatioModel.childModuleMonCellNumber_rr);
                    data[3] = byted;
                    byted = (byte)((double.Parse(b.ChildMonModuleTemperatureNumber) - ResolutionRatioModel.childMonModuleTemperatureNumber_offset) / ResolutionRatioModel.childMonModuleTemperatureNumber_rr);
                    data[4] = byted;
                    byted = (byte)((double.Parse(b.ModuleAMonCellNum) - ResolutionRatioModel.moduleAMonCellNum_offset) / ResolutionRatioModel.moduleAMonCellNum_rr);
                    data[5] = byted;
                    byted = (byte)((double.Parse(b.ModuleAMonTemperatureNum) - ResolutionRatioModel.moduleAMonTemperatureNum_offset) / ResolutionRatioModel.moduleAMonTemperatureNum_rr);
                    data[6] = byted;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info17", obj));
                }
                if (b.IsChecked18)
                {
                    //info18
                    //  Thread.Sleep(5);
                    data[0] = 0x52;
                    byted = (byte)((double.Parse(b.ModuleBMonCellNum) - ResolutionRatioModel.moduleBMonCellNum_offset) / ResolutionRatioModel.moduleBMonCellNum_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.ModuleBMonTemperatureNum) - ResolutionRatioModel.moduleBMonTemperatureNum_offset) / ResolutionRatioModel.moduleBMonTemperatureNum_rr);
                    data[2] = byted;
                    byted = (byte)((double.Parse(b.ModuleCMonCellNum) - ResolutionRatioModel.moduleCMonCellNum_offset) / ResolutionRatioModel.moduleCMonCellNum_rr);
                    data[3] = byted;
                    byted = (byte)((double.Parse(b.ModuleCMonTemperatureNum) - ResolutionRatioModel.moduleCMonTemperatureNum_offset) / ResolutionRatioModel.moduleCMonTemperatureNum_rr);
                    data[4] = byted;
                    byted = (byte)((double.Parse(b.ModuleDMonCellNum) - ResolutionRatioModel.moduleDMonCellNum_offset) / ResolutionRatioModel.moduleDMonCellNum_rr);
                    data[5] = byted;
                    byted = (byte)((double.Parse(b.ModuleDMonTemperatureNum) - ResolutionRatioModel.moduleDMonTemperatureNum_offset) / ResolutionRatioModel.moduleDMonTemperatureNum_rr);
                    data[6] = byted;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info18", obj));
                }
                if (b.IsChecked19)
                {
                    //info19
                    // Thread.Sleep(5);
                    data[0] = 0x53;
                    byted = (byte)((double.Parse(b.ModuleEMonCellNum) - ResolutionRatioModel.moduleEMonCellNum_offset) / ResolutionRatioModel.moduleEMonCellNum_rr);
                    data[1] = byted;
                    byted = (byte)((double.Parse(b.ModuleEMonTemperatureNum) - ResolutionRatioModel.moduleEMonTemperatureNum_offset) / ResolutionRatioModel.moduleEMonTemperatureNum_rr);
                    data[2] = byted;
                    d = (ushort)((double.Parse(b.PackProYear) - ResolutionRatioModel.packProYear_offset) / ResolutionRatioModel.packProYear_rr);
                    t = System.BitConverter.GetBytes(d);
                    data[3] = t[1];
                    data[4] = t[0];
                    byted = (byte)((double.Parse(b.PackProMonth) - ResolutionRatioModel.packProMonth_offset) / ResolutionRatioModel.packProMonth_rr);
                    data[5] = byted;
                    byted = (byte)((double.Parse(b.PackProDay) - ResolutionRatioModel.packProDay_offset) / ResolutionRatioModel.packProDay_rr);
                    data[6] = byted;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    sendData(new sendInfo("info19", obj));
                }
                if (b.IsChecked20)
                {
                    //info20
                    // Thread.Sleep(5);
                    data[0] = 0x54;

                    String sn = b.SerialNum;
                    byted = (byte)((Convert.ToInt32(sn[0].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    // byted = (byte)((double.Parse(FactoryConfig.PackBatchNumberData1) - ResolutionRatioModel.packBatchNumberData1_offset) / ResolutionRatioModel.packBatchNumberData1_rr);
                    data[1] = byted;
                    byted = (byte)((Convert.ToInt32(sn[1].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    data[2] = byted;
                    byted = (byte)((Convert.ToInt32(sn[2].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    data[3] = byted;
                    byted = (byte)((Convert.ToInt32(sn[3].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    data[4] = byted;
                    byted = (byte)((Convert.ToInt32(sn[4].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    data[5] = byted;
                    byted = (byte)((Convert.ToInt32(sn[5].ToString(), 16) - ResolutionRatioModel.serialNum_offset) / ResolutionRatioModel.serialNum_rr);
                    data[6] = byted;
                    data[7] = checkSum(data);//校验和
                    obj.Data = data;
                    //sendData(new sendInfo("fac20", obj));

                    //byted = (byte)((double.Parse(b.PackBatchNumberData1) - ResolutionRatioModel.packBatchNumberData1_offset) / ResolutionRatioModel.packBatchNumberData1_rr);
                    //data[1] = byted;
                    //byted = (byte)((double.Parse(b.PackBatchNumberData2) - ResolutionRatioModel.packBatchNumberData2_offset) / ResolutionRatioModel.packBatchNumberData2_rr);
                    //data[2] = byted;
                    //byted = (byte)((double.Parse(b.PackBatchNumberData3) - ResolutionRatioModel.packBatchNumberData3_offset) / ResolutionRatioModel.packBatchNumberData3_rr);
                    //data[3] = byted;
                    //byted = (byte)((double.Parse(b.PackBatchNumberData4) - ResolutionRatioModel.packBatchNumberData4_offset) / ResolutionRatioModel.packBatchNumberData4_rr);
                    //data[4] = byted;
                    //byted = (byte)((double.Parse(b.PackBatchNumberData5) - ResolutionRatioModel.packBatchNumberData5_offset) / ResolutionRatioModel.packBatchNumberData5_rr);
                    //data[5] = byted;
                    //byted = (byte)((double.Parse(b.PackBatchNumberData6) - ResolutionRatioModel.packBatchNumberData6_offset) / ResolutionRatioModel.packBatchNumberData6_rr);
                    //data[6] = byted;
                    //data[7] = checkSum(data);//校验和
                    //obj.Data = data;
                    sendData(new sendInfo("info20", obj));
                }
            }

            //停止线程
            Application.Current.Dispatcher.Invoke((Action)delegate
                  {
                      this.page.sendConfig.IsEnabled = true;
                  });
            Console.WriteLine("停止线程");
            isWaitting.Clear();
            saveBmuConfigList.Clear();
            // receiveThread.Abort();
            //receiveThread = null;
            sendThread.Abort();
        }



        //发送配置信息包
        public void runSendConfigCommand()
        {

            if (!connectstate)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    //ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);
                    ModernDialog.ShowMessage((string)page.Resources["plsconnectdevice"], (string)page.Resources["tips"], MessageBoxButton.OK);

                });
                return;
            }
            this.page.sendConfig.IsEnabled = false;
            sendThread = new Thread(new ThreadStart(sendT));
            sendThread.Start();

        }

        //清空配置页按钮响应
        public void runClearBMUConfigCommand()
        {
            bmuConfigList.Clear();
            bmuConfigListNum = 0;
            Info = "配置页清空成功";
        }

        //读取配置文件按钮响应
        public void runReadConfigCommand()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            dialog.ShowDialog();
            String path = dialog.FileName;
            if (path == "")
            {
                return;
            }
            saveBmuConfigList = Serializer.DeserializeFromFileByXml<List<BMUConfigModel_gy>>(path);
            int i = 0;
            bmuConfigList.Clear();
            foreach (BMUConfigModel_gy b in saveBmuConfigList)
            {
                i++;

                StackPanel p = new StackPanel();
                ScrollViewer sv = new ScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                BMUConfig bmuC = new BMUConfig();
                //BMUConfigViewModel vm = bmuC.DataContext as BMUConfigViewModel;
                //((BMUConfigViewModel)bmuC.DataContext).CellVolHighAlarmFirst = b.CellVolHighAlarmFirst;
                ((BMUConfigViewModel)bmuC.DataContext).BmuConfigModel_gy = b;
                //BMUConfigModel_gy test = new BMUConfigModel_gy();

                //test.CellVolHighAlarmFirst = "1000";
                //((BMUConfigViewModel)bmuC.DataContext).BmuConfigModel_gy_rec = test;
                // vm.BmuConfigModel_gy = b;
                // Console.WriteLine(((BMUConfigViewModel)bmuC.DataContext).BmuConfigModel_gy.CellVolHighAlarmFirst + "@@@@");
                // ((BMUConfigViewModel)bmuC.DataContext).CellVolHighAlarmFirst = b.CellVolHighAlarmFirst;
                p.Children.Add(bmuC);
                sv.Content = p;

                //u = new BMUConfig();
                // p.Children.Add(u); 

                bmuConfigList.Add(new TabItem() { Header = "BMU" + i, Margin = new Thickness(0, 0, 0, 0), Content = sv });
            }
            Info = "读取配置成功";
        }

        //保存配置文件按钮响应
        public void runSaveConfigCommand()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "";
            sfd.InitialDirectory = @"C:\";
            sfd.Filter = "文本文件| *.txt";
            sfd.ShowDialog();
            string path = sfd.FileName;
            // Console.WriteLine(path);
            if (path == "")
            {
                return;
            }
            saveBmuConfigList.Clear();
            List<TabItem> list = bmuConfigList.ToList<TabItem>();
            foreach (TabItem t in list)
            {
                ScrollViewer s = (ScrollViewer)t.Content;
                StackPanel p = s.Content as StackPanel;
                BMUConfig b = p.Children[0] as BMUConfig;
                BMUConfigModel_gy bmuConfigModel_gy = new BMUConfigModel_gy();
                BMUConfigViewModel bmu = b.DataContext as BMUConfigViewModel;
                BMUConfigModel_gy config = bmu.BmuConfigModel_gy as BMUConfigModel_gy;
                saveBmuConfigList.Add(config);
                //Console.WriteLine(config.CellVolHighAlarmFirst);
            }

            //序列化
            Serializer.SerializeToFileByXml<List<BMUConfigModel_gy>>(saveBmuConfigList, Path.GetDirectoryName(path), Path.GetFileName(path));
            Info = "配置保存完毕";
        }

        private void runDeleteBMUCommand(Grid grid)
        {
            if (grid.Children.Count > 0)
                grid.Children.Clear();


        }

        public void runAddBMUConfigCommand()
        {
            bmuConfigList.Clear();
            BMUConfig u;
            for (int i = 0; i < AddBMUConfigNum; i++)
            {
                StackPanel p = new StackPanel();
                ScrollViewer sv = new ScrollViewer();
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                u = new BMUConfig();
                p.Children.Add(u);
                sv.Content = p;

                bmuConfigList.Add(new TabItem() { Header = "BMU" + (i + 1), Margin = new Thickness(0, 0, 0, 0), Content = sv });
            }
            Info = "配置页添加成功";
        }




        private Thread mergeThread = null;
        //文件合并
        private void runStartMergeCommand()
        {
            mergeThread = new Thread(new ThreadStart(startMergeThread));
            mergeThread.Start();
        }
        private void startMergeThread()
        {
            Info = "正在合并请稍等......";
            String newFileName = Path.GetFileNameWithoutExtension(BootLoaderFileOneUrl) + "_" + Path.GetFileNameWithoutExtension(BootLoaderFileTwoUrl) + "_" + DateTime.Now.Ticks / 10000 + ".srec";
            String newFilePath = Path.GetDirectoryName(BootLoaderFileOneUrl) + "\\" + newFileName;

            FileStream fs = new FileStream(BootLoaderFileOneUrl, FileMode.Open);
            StreamWriter sw = new StreamWriter(newFilePath, false);
            StreamReader reader = new StreamReader(fs);
            //Encoding r = FileUtil.GetType(reader);
            //Encoding r=FileUtil.GetType(fs);
            // Console.WriteLine(r);
            String readLine;
            String endLine = "";//记录最后一行，最后一行为第一个文件的最后一行
            while ((readLine = reader.ReadLine()) != null)
            {
                if (readLine.Substring(0, 2) != "S8" && readLine.Substring(0, 2) != "S9")
                {
                    sw.WriteLine(readLine);
                }
                else
                {
                    endLine = readLine;
                }
            }
            reader.Close();
            fs.Close();
            fs = new FileStream(BootLoaderFileTwoUrl, FileMode.Open);
            reader = new StreamReader(fs);

            int line = 0;
            while ((readLine = reader.ReadLine()) != null)
            {
                if (++line != 2 && readLine.Substring(0, 2) != "S8" && readLine.Substring(0, 2) != "S9")
                {
                    sw.WriteLine(readLine);
                }
            }
            sw.WriteLine(endLine);
            reader.Close();
            fs.Close();
            sw.Close();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                //ModernDialog.ShowMessage("合并成功，新文件为" + newFilePath, "提示", MessageBoxButton.OK);
                ModernDialog.ShowMessage((string)page.Resources["mergesuccess"] + newFilePath, (string)page.Resources["tips"], MessageBoxButton.OK);

            });
            Info = "合并完成";
            Info = (string)page.Resources["mergefinish"];
            // File.Copy(BootLoaderFileOneUrl, newFilePath);
        }
        //bootloader恢复代码
        private void runStartBootLoaderCommand(TabControl tb)
        {
            int index = tb.SelectedIndex - 1;//获取当前是第几组bmu，从0开始
            //if (!connectstate)
            //{
            //    Application.Current.Dispatcher.Invoke((Action)delegate
            //    {
            //        ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);

            //    });
            //    return;
            //}
            sendThread = new Thread(new ParameterizedThreadStart(startBootLoaderThread));
            sendThread.Start(index);
        }

        //主机bootloader刷写，主机的id为0
        private void runStartMasterBootLoaderCommand()
        {
            //if (!connectstate)
            //{
            //    Application.Current.Dispatcher.Invoke((Action)delegate
            //    {StartMasterBootLoaderCommand
            //        ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);

            //    });
            //    return;
            //}
            isReceive = true;//开启接收
            int[] serial = new int[] { 0, 0 };
            Masterbootisenable = "False";
           // button.SetResourceReference(ContentControl.ContentProperty, "disconnect");
           // button.IsEnabled = false;
            sendThread = new Thread(new ParameterizedThreadStart(runStartBootLoaderCommandThread2));
            sendThread.Start(serial);
        }
        //private void runStartBootLoaderCommand2()
        //{
        //    if (!connectstate)
        //    {
        //        Application.Current.Dispatcher.Invoke((Action)delegate
        //        {
        //            ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);

        //        });
        //        return;
        //    }
        //    sendThread = new Thread(new ThreadStart(startBootLoaderThread2));
        //    sendThread.Start();
        //}

        //private void runStartBootLoaderCommand3()
        //{
        //    if (!connectstate)
        //    {
        //        Application.Current.Dispatcher.Invoke((Action)delegate
        //        {
        //            ModernDialog.ShowMessage("请连接设备", "提示", MessageBoxButton.OK);

        //        });
        //        return;
        //    }
        //    sendThread = new Thread(new ThreadStart(startBootLoaderThread3));
        //    sendThread.Start();
        //}

        //从机bootloader发送
        private void startBootLoaderThread(Object obj)
        {
            isReceive = true;
            int group = (int)obj;//获取这是第几组的bootloader，从0开始
            for (int t = 16 * group; t < (16 * group + 16); t++)
            {
                if (CBArray[t])
                {
                    int[] serial = new int[] { group, t - 16 * group + 1 };
                    runStartBootLoaderCommandThread(serial);
                }
            }
        }

        private String decryptFilePath = "";//待解密的文件路径

        public String DecryptFilePath
        {
            get { return decryptFilePath; }
            set { decryptFilePath = value; OnPropertyChanged("DecryptFilePath"); }
        }

        // private int bootCheck = 0;

        //bootloader文件地址
        // private string[] bootLoaderUrl = new string[3];
        private ObservableCollection<string> bootLoaderUrl = new ObservableCollection<string>() { "", "", "", "" };//BMU配置列表

        public ObservableCollection<string> BootLoaderUrl
        {
            get { return bootLoaderUrl; }
            set { bootLoaderUrl = value; OnPropertyChanged("BootLoaderUrl"); }
        }

        //public String[] BootLoaderUrl
        //{
        //    get { return bootLoaderUrl; }
        //    set { bootLoaderUrl = value; OnPropertyChanged("BootLoaderUrl"); }
        //}

        //主机硬件版本号配置值
        private string masterHardwareVersion;
        public string MasterHardwareVersion
        {
            get { return masterHardwareVersion; }
            set { masterHardwareVersion = value; OnPropertyChanged("MasterHardwareVersion"); }
        }

        private string masterHardwareVersion1="";//主机硬件版本号1
        private string masterHardwareVersion2 = "";//主机硬件版本号2
        private string masterHardwareVersion3 = "";//主机硬件版本号3
        //主机硬件版本号返回值
        private string masterHardwareVersion_rec = "";//masterHardwareVersion_rec=masterHardwareVersion1+masterHardwareVersion2+masterHardwareVersion3
        public string MasterHardwareVersion_rec
        {
            get { return masterHardwareVersion_rec; }
            set { masterHardwareVersion_rec = value; OnPropertyChanged("MasterHardwareVersion_rec"); }
        }

        //存储索引0：主机软件版本号，索引1-4：对应索引号的从机软件版本号，索引5-8：对应索引号-4的从机boot版本号
        private ObservableCollection<string> slaveVersionList = new ObservableCollection<string>();

        public ObservableCollection<string> SlaveVersionList
        {
            get { return slaveVersionList; }
            set { slaveVersionList = value; OnPropertyChanged("SlaveVersionList"); }
        }

        //private string[] slaveVersionList=new string[4];//从机软件版本号

        //public string[] SlaveVersionList
        //{
        //    get { return slaveVersionList; }
        //    set { slaveVersionList = value; OnPropertyChanged("SlaveVersionList"); }
        //}

        //VIN配置
        private string vIN;

        public string VIN
        {
            get { return vIN; }
            set { vIN = value; OnPropertyChanged("VIN"); }
        }
        private string VIN1 = "";
        private string VIN2 = "";
        private string VIN3 = "";

        private string vIN_rec;

        public string VIN_rec
        {
            get { return vIN_rec; }
            set { vIN_rec = value; OnPropertyChanged("VIN_rec"); }
        }     

        //从机硬件版本号配置值
        private string slaveHardwareVersion;
        public string SlaveHardwareVersion
        {
            get { return slaveHardwareVersion; }
            set { slaveHardwareVersion = value; OnPropertyChanged("SlaveHardwareVersion"); }
        }

        private string slaveHardwareVersion1 = "";//从机硬件版本号1
        private string slaveHardwareVersion2 = "";//从机硬件版本号2
        private string slaveHardwareVersion3 = "";//从机硬件版本号3
        //从机硬件版本号返回值
        private string slaveHardwareVersion_rec="";
        public string SlaveHardwareVersion_rec
        {
            get { return slaveHardwareVersion_rec; }
            set { slaveHardwareVersion_rec = value; OnPropertyChanged("SlaveHardwareVersion_rec"); }
        }

        //主机剩余容量
        private double leftCapacity;

        public double LeftCapacity
        {
            get { return leftCapacity; }
            set { leftCapacity = value; OnPropertyChanged("LeftCapacity"); }
        }

        //主机电池组标称容量
        private double bCCapacity;

        public double BCCapacity
        {
            get { return bCCapacity; }
            set { bCCapacity = value; OnPropertyChanged("BCCapacity"); }
        }


        //主机总容量配置
        private double zCapacity;

        public double ZCapacity
        {
            get { return zCapacity; }
            set { zCapacity = value; OnPropertyChanged("ZCapacity"); }
        }
        //霍尔1
        private double deviation;
        public double Deviation
        {
            get { return deviation; }
            set { deviation = value; OnPropertyChanged("Deviation"); }
        }

        private double deviation2;
        public double Deviation2
        {
            get { return deviation2; }
            set { deviation2 = value; OnPropertyChanged("Deviation2"); }
        }
        private double deviation3;
        public double Deviation3
        {
            get { return deviation3; }
            set { deviation3 = value; OnPropertyChanged("Deviation3"); }
        }
        private double deviation4;
        public double Deviation4
        {
            get { return deviation4; }
            set { deviation4 = value; OnPropertyChanged("Deviation4"); }
        }

        //Shunt_1电流
        private double shunt_1Cur;
        public double Shunt_1Cur
        {
            get { return shunt_1Cur; }
            set { shunt_1Cur = value; OnPropertyChanged("Shunt_1Cur"); }
        }

        private double shunt_2Cur;
        public double Shunt_2Cur
        {
            get { return shunt_2Cur; }
            set { shunt_2Cur = value; OnPropertyChanged("Shunt_2Cur"); }
        }
        //放电总能量
        private double fangdznl;

        public double Fangdznl
        {
            get { return fangdznl; }
            set { fangdznl = value; OnPropertyChanged("Fangdznl"); }
        }

        //合并的文件1
        private string bootLoaderFileOneUrl = "";

        public string BootLoaderFileOneUrl
        {
            get { return bootLoaderFileOneUrl; }
            set { bootLoaderFileOneUrl = value; OnPropertyChanged("BootLoaderFileOneUrl"); }
        }

        //合并的文件2
        private string bootLoaderFileTwoUrl = "";

        public string BootLoaderFileTwoUrl
        {
            get { return bootLoaderFileTwoUrl; }
            set { bootLoaderFileTwoUrl = value; OnPropertyChanged("BootLoaderFileTwoUrl"); }
        }




        int sendNum = 0;//记录当次已经发送了多少次（[0,0F]）
        CANSDK.VCI_CAN_OBJ[] dataCache = new CANSDK.VCI_CAN_OBJ[17];//缓存当次发送的数据以防重发
        private int canBootNext = 0;

        //主机bootloader的进度条
        private int pBValueMaster = 0;
        public int PBValueMaster
        {
            get { return pBValueMaster; }
            set { pBValueMaster = value; OnPropertyChanged("PBValueMaster"); }
        }

        private int maxpbMaster = 100;
        public int MaxpbMaster
        {
            get { return maxpbMaster; }
            set { maxpbMaster = value; OnPropertyChanged("MaxpbMaster"); }
        }

        private string masterbootisenable = "True";

        public string Masterbootisenable
        {
            get { return masterbootisenable; }
            set { masterbootisenable = value; OnPropertyChanged("Masterbootisenable"); }
        }


        //bootloader从机刷写进度条
        private ObservableCollection<int> pBValue;

        public ObservableCollection<int> PBValue
        {
            get { return pBValue; }
            set { pBValue = value; OnPropertyChanged("PBValue"); }
        }

        //从机进度条的最大值
        private ObservableCollection<int> maxpb;
        public ObservableCollection<int> Maxpb
        {
            get { return maxpb; }
            set { maxpb = value; OnPropertyChanged("Maxpb"); }
        }

        private int bootloaderSum = 0;//bootloader每次传输的校验和
        private int dataCacheIndex = 0;//记录bootloader缓存的游标
        private int dataCount = 0;//记录一轮发送了多少数据

        //数据重发
        private void reSend(CANSDK.VCI_CAN_OBJ obj)
        {
            Console.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));
            swDebug.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));

            swDebug.Flush();
            if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 2 && isGetReceived == 0)
            {
                //这次发送是重发缓存里的数据，所以不用再次修改缓存，直接发送就好
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
        }

        //发送bootloader数据
        private void sendBootData(CANSDK.VCI_CAN_OBJ obj)
        {
            dataCount += (obj.DataLen - 1);
            byte[] b = obj.Data;
            Console.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));
            swDebug.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));

            swDebug.Flush();
            if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 2 && isGetReceived == 0)
            {
                //这次发送是重发缓存里的数据，所以不用再次修改缓存，直接发送就好
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                dataCount = 0;//重传数据不需要再计算数据字节数，因为上一帧为0所以维持0就好
            }
            else
            {
                CANSDK.VCI_CAN_OBJ tempData = (CANSDK.VCI_CAN_OBJ)DataConverter.CloneObject(obj);

                // dataCache[b[0]] = tempData;//加入缓存防止需要重发
                dataCache[dataCacheIndex++] = tempData;
                for (int i = 0; i < obj.DataLen; i++)
                {
                    bootloaderSum += b[i];
                }
                // Console.WriteLine("校验和：" + bootloaderSum);
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
        }
        // private byte[] masterCRC=new byte[117];
        //适用于主机bootload
        private void sendBootData2(CANSDK.VCI_CAN_OBJ obj)
        {
            dataCount += (obj.DataLen - 1);
            byte[] b = obj.Data;
            Console.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));
            swDebug.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));

            swDebug.Flush();
            if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 2 && isGetReceived == 0)
            {
                //这次发送是重发缓存里的数据，所以不用再次修改缓存，直接发送就好
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                dataCount = 0;//重传数据不需要计算数据字节个数，故保持上一帧FE的0不变
            }
            else
            {
                CANSDK.VCI_CAN_OBJ tempData = (CANSDK.VCI_CAN_OBJ)DataConverter.CloneObject(obj);

                // dataCache[b[0]] = tempData;//加入缓存防止需要重发
                dataCache[dataCacheIndex++] = tempData;
                //for (int i = 0; i < obj.DataLen; i++)
                //{
                //    bootloaderSum += b[i];
                //}
                // Console.WriteLine("校验和：" + bootloaderSum);
                CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
            }
        }
        //发送FE帧表示bootloader本次传输结束
        private void sendFE(CANSDK.VCI_CAN_OBJ obj, int addLen, byte[] addData)
        {
            Console.WriteLine("发送FE帧");
            canBootNext = 0;
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
            dataCache[dataCacheIndex] = tempData;

            bootloaderSum = 0;//发送完一轮数据后，校验和归0


            sendData(new sendInfo("bootloaderFE", obj));
            reSendTimes = 0;//重发次数归零
        }
        //适用于主机bootloader
        private void sendFE2(CANSDK.VCI_CAN_OBJ obj, int addLen, byte[] addData)
        {
            //if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 1) {
            //    Console.WriteLine("跳出这次FE");
            //    return;
            //}
            Console.WriteLine("发送FE帧");
            canBootNext = 0;
            sendNum = 0;
            obj.DataLen = (byte)(4 + addLen);
            obj.Data[0] = 0xFE;
            //bootloaderSum += 0xFE;
            for (int i = 0; i < addLen; i++)
            {
                obj.Data[1 + i] = addData[i];
                //bootloaderSum += addData[i];
            }
            //obj.Data[1 + addLen] = (byte)bootloaderSum;
            obj.Data[1 + addLen] = (byte)dataCount;
            byte[] btemp = new byte[dataCount + addLen + 1];
            int cursor = 0;
            for (int i = 0; i < dataCacheIndex + 1; i++)
            {
                if (dataCache[i].Data == null)
                {
                    break;
                }

                byte[] b = dataCache[i].Data;
                for (int j = 1; j < dataCache[i].DataLen; j++)
                {
                    btemp[cursor++] = b[j];
                }
            }

            for (int i = 0; i < addLen; i++)
            {
                btemp[cursor++] = addData[i];
            }
            btemp[cursor] = (byte)dataCount;
            dataCount = 0;
            byte[] CRCResult = DataConverter.CRC16(btemp, btemp.Length);
            //低位在前高位在后
            obj.Data[2 + addLen] = CRCResult[1];
            obj.Data[3 + addLen] = CRCResult[0];
            //dataCount = 0;
            CANSDK.VCI_CAN_OBJ tempData = (CANSDK.VCI_CAN_OBJ)DataConverter.CloneObject(obj);
            // dataCache[15] = tempData;
            dataCache[dataCacheIndex] = tempData;

            //bootloaderSum = 0;//发送完一轮数据后，校验和归0
            swDebug.WriteLine(DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen));
            swDebug.Flush();
           // Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2"));
            sendData(new sendInfo("bootloaderFE", obj));
            reSendTimes = 0;//重发次数归零
        }
        //发送握手
        // sendBootHSData(new sendInfo("bootHS", obj));
        private void sendBootHSData(Object o)
        {

            sendData(o);
            Console.WriteLine("发送了握手信息");
            //sendInfo s = (sendInfo)o;
            //CANSDK.VCI_CAN_OBJ canObj = s.Obj;

        }
        private FileStream fsDebug;//用于调试保存发送的数据
        private StreamWriter swDebug;
        //private void runStartBootLoaderCommandThread(int group,int t)
        private void runStartBootLoaderCommandThread(Object serialObj)
        {
            int[] serial = (int[])serialObj;
            int group = serial[0];//组号,从机从0开始，主机为0
            int t = serial[1];//id号，表示第几个bmu,取值[1,16]

            dataCount = 0;
            String bmuId = t.ToString("X2");
            if (swDebug != null && fsDebug != null)
            {
                swDebug.Close();
                fsDebug.Close();
            }

            fsDebug = new FileStream("debug.txt", FileMode.Create);
            swDebug = new StreamWriter(fsDebug);
            //bootCheck = 0;

            if (BootLoaderUrl[group + 1] == null | BootLoaderUrl[group + 1] == "") { return; }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            string s = "18A" + group + "26" + bmuId;
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);

            // obj.ID = 0x18A02601;
            byte[] data = new byte[8];


            FileStream fs = new FileStream(BootLoaderUrl[group + 1], FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            int lineSum = 0;
            //统计总行数
            while (read.ReadLine() != null)
            {
                lineSum++;
            }
            if (t == 0)
            {
                MaxpbMaster = lineSum;
                PBValueMaster = 0;
            }
            else
            {
                Maxpb[t - 1] = lineSum;
                PBValue[t - 1] = 0;
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
            dataCache = new CANSDK.VCI_CAN_OBJ[17];
            canBootNext = 1;
            while ((strReadline = read.ReadLine()) != null && canBootNext == 1)
            {
                //测试用
                //if (i >= 40) { return; }
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if ((strReadline.Substring(0, 2).Equals("S8") | strReadline.Substring(0, 2).Equals("S9")) && canBootNext == 1)
                {
                    //结束行
                    //最后一轮发送的数据可能不满16次，不是8的倍数

                    if (temp != null)
                    {
                        int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                        //if (fillCount != 0)
                        //{
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
                            //}
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
                    //sendBootData(obj);
                    if (t == 0)
                    {
                        PBValueMaster = lineSum;
                    }
                    else
                    {
                        PBValue[t - 1] = lineSum;
                    }
                    swDebug.Close();
                    fsDebug.Close();
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
                    canBootNext = 0;//如果握手成功再赋值为1
                    sendBootHSData(new sendInfo("bootHS", obj));//发送握手指令
                }

                if (i != 0 && canBootNext == 1)
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
                                    //i++;//新加
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


                if (temp != null & canBootNext == 1)
                {
                    if (isBreak == 1)
                    {
                        //地址断了，此时将temp单独发出去而不与这一行拼接
                        if (sendNum != 0)
                        {
                            int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                            //if (fillCount != 0)
                            //{
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
                            //}
                        }
                        else
                        {
                            //如果上一次发送的是0x0F,temp从0开始并且单独一帧发送
                            data[0] = 0x00;
                            // addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
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
                        //i!=1
                        // data[0] = (byte)((i - 1) % 16);
                        data[0] = (byte)sendNum;//注释了这里******

                        {
                            // Console.WriteLine("data[0]" + data[0]);
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
                    //if (sendNum != 0 & isBreak == 1)
                    //{
                    //没有temp，且地址断了，且上一次没有发送FE，则这次补上一个FE
                    sendFE(obj, addLen, addData);

                    //}
                }

                //if ((i - 1) % 16 == 0&sendNum!=16&lineLen>=7&canBootNext==1)
                //if ((i - 1) % 16 == 0 &!( i!=0&sendNum == 16 & lineLen >= 7 & canBootNext == 1))
                if ((i - 1) % 16 == 0 & lineLen == int.Parse(strReadline.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - addLen - 1)
                {

                    //当前是00号帧，且完整（没有与上一行拼接），则获取地址
                    addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                }

                while (i != 0 & lineLen >= 7 & canBootNext == 1)
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
                if (lineLen != 0 && canBootNext == 1)
                {
                    dataTemp = strReadline.Substring(4 + addLen * 2 + pos * 2, lineLen * 2);
                    temp = DataConverter.strToHexByte(dataTemp);
                }
                if (t == 0)
                {
                    PBValueMaster++;
                }
                else
                {
                    PBValue[t - 1]++;
                }
            }
            fs.Close();
            read.Close();
        }

        //主机bootloader刷写
        private void runStartBootLoaderCommandThread2(Object serialObj)
        {
            int[] serial = (int[])serialObj;
            int group = serial[0];//组号,从机从0开始，主机为0
            int t = serial[1];//id号，表示第几个bmu,取值[1,16]
            sendNum = 0;
            dataCount = 0;
            dataCache = new CANSDK.VCI_CAN_OBJ[17];
            canBootNext = 1;
            String bmuId = t.ToString("X2");
            if (swDebug != null && fsDebug != null)
            {
                swDebug.Close();
                fsDebug.Close();
            }

            fsDebug = new FileStream("debug.txt", FileMode.Create);
            swDebug = new StreamWriter(fsDebug);
            //bootCheck = 0;

            if (BootLoaderUrl[0] == null | BootLoaderUrl[0] == "") { return; }
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            string s = "18A02601";
            //string s = "18A" + group + "26" + bmuId;
            obj.ID = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);

            // obj.ID = 0x18A02601;
            byte[] data = new byte[8];


            FileStream fs = new FileStream(BootLoaderUrl[0], FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            int lineSum = 0;
            //统计总行数
            while (read.ReadLine() != null)
            {
                lineSum++;
            }
            if (t == 0)
            {
                MaxpbMaster = lineSum;
                PBValueMaster = 0;
            }
            else
            {
                Maxpb[t - 1] = lineSum;
                PBValue[t - 1] = 0;
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

            while ((strReadline = read.ReadLine()) != null && canBootNext == 1)
            {
                if (i > 1)
                {
                    if (!DataConverter.examine(strReadline.Substring(2)))
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //ModernDialog.ShowMessage("文件第" + (PBValueMaster + 2) + "行有误", "提示", MessageBoxButton.OK);
                            //Error in line 2 of the document
                            ModernDialog.ShowMessage((string)page.Resources["fileno"] + (PBValueMaster + 2) + (string)page.Resources["linewrong"], (string)page.Resources["tips"], MessageBoxButton.OK);

                        });
                        Info = "文件第" + (PBValueMaster + 2) + "行有误，停止运行";
                        Info = (string)page.Resources["fileno"] + (PBValueMaster + 2) + (string)page.Resources["linewrongsoprun"];
                        return;
                    }

                }
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if ((strReadline.Substring(0, 2).Equals("S8") | strReadline.Substring(0, 2).Equals("S9") | strReadline.Substring(0, 2).Equals("S5") | strReadline.Substring(0, 2).Equals("S7")) && canBootNext == 1)
                {
                    //结束行
                    //最后一轮发送的数据可能不满16次，不是8的倍数

                    if (temp != null)
                    {
                        int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                        //if (fillCount != 0)
                        //{
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
                            sendBootData2(obj);
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

                                sendBootData2(obj);
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

                            sendBootData2(obj);
                            temp = null;
                            sendNum++;
                            //}
                        }
                        //发送FE帧
                        Console.WriteLine("masterFE1");
                        sendFE2(obj, addLen, addData);

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
                            sendBootData2(obj);
                            sendNum++;
                        }
                        if (sendNum <= 16)
                        {
                            Console.WriteLine("masterFE2");
                            sendFE2(obj, addLen, addData);

                        }
                    }
                    //中止程序,主机的结束帧和从机不同，为FDFDFD
                    data[0] = 0xFD;
                    data[1] = 0xFD;
                    data[2] = 0xFD;
                    obj.DataLen = (byte)3;
                    Masterbootisenable = "True";//使主机bootloader下载按钮可用
                    sendData(new sendInfo("over", obj));
                   
                    //sendBootData(obj);
                    if (t == 0)
                    {
                        PBValueMaster = lineSum;
                    }
                    else
                    {
                        PBValue[t - 1] = lineSum;
                    }
                    swDebug.Close();
                    fsDebug.Close();
                }//结束行


                pos = 0;

                if (i == 1)
                {
                    //主机的握手与从机不同，为FFFFFF
                    data[0] = 0xFF;
                    data[1] = 0xFF;
                    data[2] = 0xFF;
                    //data[1] = (byte)'S';
                    switch (strReadline.Substring(0, 2))
                    {
                        case "S1":
                            //data[2] = (byte)'2';
                            addLen = 2;
                            break;
                        case "S2":
                            // data[2] = (byte)'3';
                            addLen = 3;
                            break;
                        case "S3":
                            //data[2] = (byte)'4';
                            addLen = 4;
                            break;
                    }


                    data[3] = data[4] = data[5] = data[6] = data[7] = 0x00;
                    obj.Data = data;
                    obj.DataLen = 3;
                    canBootNext = 0;//如果握手成功再赋值为1
                    sendBootHSData(new sendInfo("bmasterHS", obj));//发送握手指令
                }

                //int isError = 0;
                if (i != 0 && canBootNext == 1)
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

                                    sendBootData2(obj);
                                    //i++;//新加
                                }
                                //发送FE帧
                                Console.WriteLine("masterFE3");
                              //  isError = 1;
                                sendFE2(obj, addLen, addData);
                               // Thread.Sleep(10);
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
                //else if (isError == 1) {
                //    isError = 0;
                //}

                //temp有数据
                if (temp != null & canBootNext == 1)
                {
                    if (isBreak == 1)
                    {
                        //地址断了，此时将temp单独发出去而不与这一行拼接
                        if (sendNum != 0)
                        {
                            int fillCount = (8 - (7 * sendNum + temp.Length) % 8) % 8;//计算需要补几个0xFF
                            //if (fillCount != 0)
                            //{
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
                                sendBootData2(obj);
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

                                    sendBootData2(obj);
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

                                sendBootData2(obj);
                                temp = null;
                                sendNum++;
                            }
                            //}
                        }
                        else
                        {
                            //如果上一次发送的是0x0F,temp从0开始并且单独一帧发送
                            data[0] = 0x00;
                            // addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
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
                            sendBootData2(obj);

                            data[0] = 0x01;
                            data[1] = 0xFF;
                            obj.Data = data;
                            obj.DataLen = 2;
                            sendBootData2(obj);
                        }
                        //发送FE帧
                        Console.WriteLine("masterFE4");
                        sendFE2(obj, addLen, addData);

                    }
                    else
                    {
                        //i!=1
                        // data[0] = (byte)((i - 1) % 16);
                        data[0] = (byte)sendNum;//注释了这里******

                        {
                            // Console.WriteLine("data[0]" + data[0]);
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
                                sendBootData2(obj);
                                sendNum++;
                                temp = null;

                            }
                            //如果当前发送的是第15帧，发送FE，偏移地址

                            if (data[0] == 0x0F)
                            {
                                Console.WriteLine("masterFE5");
                                sendFE2(obj, addLen, addData);
                                //偏移
                                addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                                byte[] bb = DataConverter.strToHexByte((DataConverter.byteArrayToInt(addData) + L).ToString("X6"));
                                addData = bb;
                            }

                        }
                        i++;
                    }
                }//temp有数据
                else if (sendNum != 0 & isBreak == 1)//temp无数据
                {
                    //if (sendNum != 0 & isBreak == 1)
                    //{
                    //没有temp，且地址断了，且上一次没有发送FE，则这次补上一个FE
                    Console.WriteLine("masterFE6");
                    sendFE2(obj, addLen, addData);

                    //}
                }

                if ((i - 1) % 16 == 0 & lineLen == int.Parse(strReadline.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - addLen - 1)
                {

                    //当前是00号帧，且完整（没有与上一行拼接），则获取地址
                    addData = DataConverter.strToHexByte(strReadline.Substring(4, addLen * 2));
                }

                while (i != 0 & lineLen >= 7 & canBootNext == 1)
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
                        sendBootData2(obj);
                        sendNum++;

                        if (data[0] == 0x0F)
                        {
                            //发送本次传输结束命令
                            Console.WriteLine("masterFE7");
                            sendFE2(obj, addLen, addData);

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
                }//while
                if (lineLen != 0 && canBootNext == 1)
                {
                    dataTemp = strReadline.Substring(4 + addLen * 2 + pos * 2, lineLen * 2);
                    temp = DataConverter.strToHexByte(dataTemp);
                }
                if (t == 0)
                {
                    PBValueMaster++;
                }
                else
                {
                    PBValue[t - 1]++;
                }
            }
            fs.Close();
            read.Close();
        }
        private int reSendTimes = 0;//下位机回复F1时，记录重传数据的次数
        //发送数据
        public void sendData(Object o)
        {
            isGaosProfram = 1;
            isGetReceived = 0;
            canBootNext = 0;
            // int i = 1;
            sendInfo send = (sendInfo)o;
            CANSDK.VCI_CAN_OBJ obj = send.Obj;
            int coefficient = 10;
            int waitTimes = 0;
            if (send.Flag.Equals("bootHS"))
            {
                waitTimes = 500;
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

            if (send.Flag == "bootFE")
            {
                Console.WriteLine("---------------------");
            }
            int time = 0;
            int bmasterhstime = 0;
            while (true)
            {

                //Thread.Sleep(5);
                if (retryTimes < waitTimes && isGetReceived == 0)
                {
                    //如果未收到回应且还有重发次数
                    Console.WriteLine("发送：" + DataConverter.byteToHexStrForData(obj.Data).Substring(0, 2 * obj.DataLen) + ",ID:" + obj.ID.ToString("X2") + "," + (time++));
                    uint frameCount = CANSDK.VCI_Transmit(CANSDK.m_devtype, CANSDK.m_devind, CANSDK.m_canind, ref obj, 1);
                    isWaitting[send.Flag] = 1;//开始等待接收回送数据包                   
                    retryTimes++;
                    long sendTicks = DateTime.Now.Ticks; //记录数据发出的时间

                    while (true)
                    {
                        //如果正在等待接收回送数据包
                        if (isWaitting.ContainsKey(send.Flag) && isWaitting[send.Flag] == 1 && isGetReceived == 0)
                        {
                            // if ((DateTime.Now.Ticks - sendTicks) / 10000 < (OVERTIME * 1000))//没有超时
                            if ((DateTime.Now.Ticks - sendTicks) / 10000 < (OVERTIME * coefficient))//没有超时
                            {
                                continue;
                            }
                            else
                            { //超时
                                Console.WriteLine("超时");
                                isWaitting[send.Flag] = 0;
                                if (++bmasterhstime > 499)
                                {
                                    Masterbootisenable = "True";
                                }
                                break;
                            }
                        }
                        else if (isWaitting.ContainsKey("bootloaderFE") && isWaitting["bootloaderFE"] == 2 && isGetReceived == 0)
                        {
                            //重传数据
                            if (reSendTimes >= RETRYTIMES)
                            {
                                Info = "重传数据五次，接收超时退出！";
                                Console.WriteLine("重传数据五次，接收超时退出！");
                                Masterbootisenable = "True";
                                return;
                            }
                            else
                            {
                                Console.WriteLine("重传数据！！！");
                                reSendTimes++;
                                for (int i = 0; i < dataCacheIndex + 1; i++)
                                {
                                    Console.WriteLine(i+":"+DataConverter.byteToHexStrForData( dataCache[i].Data));
                                    if (dataCache[i].Data != null)
                                    {
                                        reSend(dataCache[i]);
                                        // sendBootData(dataCache[i]);
                                    }

                                }
                                retryTimes = 0;
                                Thread.Sleep(50);
                            }
                        }
                        else { break; }
                    }
                }
                else
                {
                    if (retryTimes >= RETRYTIMES) { Info = "第" + (bmuConfigListNum + 1) + "号配置的" + send.Flag + "超时"; }
                    //Console.WriteLine("复位");
                    //复位
                    reSendTimes = 0;
                    retryTimes = 0;
                    isWaitting[send.Flag] = 0;
                    isGetReceived = 1;
                    isGaosProfram = 0;
                    //saveBmuConfigList.Clear();
                    return;
                }
            }



        }


        public ObservableCollection<string> CanChannelList
        {
            get { return canChannelList; }
            set { canChannelList = value; OnPropertyChanged("CanChannelList"); }
        }

        public ObservableCollection<BaudRateModel> CanBaudRateList
        {
            get { return canBaudRateList; }
            set { canBaudRateList = value; OnPropertyChanged("CanBaudRateList"); }
        }
        public ObservableCollection<TabItem> BmuConfigList
        {
            get { return bmuConfigList; }
            set { bmuConfigList = value; OnPropertyChanged("BmuConfigList"); }
        }

        public ObservableCollection<int> CanIndexList
        {
            get { return canIndexList; }
            set { canIndexList = value; }
        }

        public ObservableCollection<string> VersionVisibility
        {
            get
            {
                return versionVisibility;
            }

            set
            {
                versionVisibility = value;
                OnPropertyChanged("VersionVisibility");
            }
        }

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
