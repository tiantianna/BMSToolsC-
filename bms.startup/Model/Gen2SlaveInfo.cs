using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    class Gen2SlaveInfo : INotifyPropertyChanged
    {
        public Gen2SlaveInfo() {
            for (int i = 0; i < 60; i++) {
                Vol.Add("");
                Signal.Add("");
                Balance.Add("");
            }
            Signal.Add("");
            for (int i = 0; i < 10; i++)
            {
                TemSensor.Add("");
            }
        }
        //实时信息
        private string bCNT;//监控电池数目
        private string sOC;
        private string vTotal;//总电压
        private string cVMax;//单体最高电压
        private string cellInfo;//电池情况，0电池电压正常，1单体电池一致性差，2电池过压，3电池欠压
        private string temInfo;//温度状态，0正常，1异常
        private string cVmin;//单体最低电压
        private string bmuHardwareFault;//BMU硬件故障，0正常，1异常
        private string signalInfo;//信号线连接状态，0正常，1异常
        private string balanceInfo;//均衡状态，0关闭，1开启

        private ObservableCollection<string> vol = new ObservableCollection<string>();
        private ObservableCollection<string> temSensor = new ObservableCollection<string>();//温感

        
        private string tb;//电路板温度
        private string can_life;

        private ObservableCollection<string> signal = new ObservableCollection<string>();//信号采集线状态，0正常1开路
        private ObservableCollection<string> balance = new ObservableCollection<string>();//电池均衡状态，0关闭1开启

        private string pacMaxTemp;//单箱最高温度       
        private string pacMinTemp;//单箱最低温度        
        private string slaveVersion;//从机版本号
        
        //配置信息
        private string bcnt_2;//监控电池总数目
        private string sid;//从机编号
        private string covth;//过压报警电压
        private string cuvth;//欠压报警电压
        private string foth;//风扇开启温度
        private string fcth;//风扇关闭温度
        private string traceCode;//追溯码
        private string bcnt_A;//子模块A监控电池数目
        private string bcnt_B;//子模块B监控电池数目
        private string bcnt_C;//子模块C监控电池数目
        private string bcnt_D;//子模块D监控电池数目
        private string bcnt_E;//子模块E监控电池数目
        private string bcnt_F;//子模块F监控电池数目
        private string maxChargeCur;//最大充电电流

        public string Bcnt_2
        {
            get { return bcnt_2; }
            set { bcnt_2 = value; OnPropertyChanged("Bcnt_2"); }
        }
        
        public string Sid
        {
            get { return sid; }
            set { sid = value; OnPropertyChanged("Sid"); }
        }
        
        public string Covth
        {
            get { return covth; }
            set { covth = value; OnPropertyChanged("Covth"); }
        }
        
        public string Cuvth
        {
            get { return cuvth; }
            set { cuvth = value; OnPropertyChanged("Cuvth"); }
        }
        
        public string Foth
        {
            get { return foth; }
            set { foth = value; OnPropertyChanged("Foth"); }
        }
        
        public string Fcth
        {
            get { return fcth; }
            set { fcth = value; OnPropertyChanged("Fcth"); }
        }
        
        public string TraceCode
        {
            get { return traceCode; }
            set { traceCode = value; OnPropertyChanged("TraceCode"); }
        }
        
        public string Bcnt_A
        {
            get { return bcnt_A; }
            set { bcnt_A = value; OnPropertyChanged("Bcnt_A"); }
        }
        
        public string Bcnt_B
        {
            get { return bcnt_B; }
            set { bcnt_B = value; OnPropertyChanged("Bcnt_B"); }
        }
        
        public string Bcnt_C
        {
            get { return bcnt_C; }
            set { bcnt_C = value; OnPropertyChanged("Bcnt_C"); }
        }
        
        public string Bcnt_D
        {
            get { return bcnt_D; }
            set { bcnt_D = value; OnPropertyChanged("Bcnt_D"); }
        }
        
        public string Bcnt_E
        {
            get { return bcnt_E; }
            set { bcnt_E = value; OnPropertyChanged("Bcnt_E"); }
        }
        
        public string Bcnt_F
        {
            get { return bcnt_F; }
            set { bcnt_F = value; OnPropertyChanged("Bcnt_F"); }
        }
        

        public string MaxChargeCur
        {
            get { return maxChargeCur; }
            set { maxChargeCur = value; OnPropertyChanged("MaxChargeCur"); }
        }

        public ObservableCollection<string> Balance
        {
            get { return balance; }
            set { balance = value; OnPropertyChanged("Balance"); }
        }
        public ObservableCollection<string> Signal
        {
            get { return signal; }
            set { signal = value; OnPropertyChanged("Signal"); }
        }
        public ObservableCollection<string> TemSensor
        {
            get { return temSensor; }
            set { temSensor = value; OnPropertyChanged("TemSensor"); }
        }
        public string SlaveVersion
        {
            get { return slaveVersion; }
            set { slaveVersion = value; OnPropertyChanged("SlaveVersion"); }
        }
        public string PacMaxTemp
        {
            get { return pacMaxTemp; }
            set { pacMaxTemp = value; OnPropertyChanged("PacMaxTemp"); }
        }
        public string PacMinTemp
        {
            get { return pacMinTemp; }
            set { pacMinTemp = value; OnPropertyChanged("PacMinTemp"); }
        }
        public ObservableCollection<string> Vol
        {
            get { return vol; }
            set { vol = value; OnPropertyChanged("Vol"); }
        }
        
        
        public string Can_life
        {
            get { return can_life; }
            set { can_life = value; OnPropertyChanged("Can_life"); }
        }

        public string Tb
        {
            get { return tb; }
            set { tb = value; OnPropertyChanged("Tb"); }
        }
        

        public string BalanceInfo
        {
            get { return balanceInfo; }
            set { balanceInfo = value; OnPropertyChanged("BalanceInfo"); }
        }
        public string SignalInfo
        {
            get { return signalInfo; }
            set { signalInfo = value; OnPropertyChanged("SignalInfo"); }
        }
        public string BmuHardwareFault
        {
            get { return bmuHardwareFault; }
            set { bmuHardwareFault = value; OnPropertyChanged("BmuHardwareFault"); }
        }
        public string CVmin
        {
            get { return cVmin; }
            set { cVmin = value; OnPropertyChanged("CVmin"); }
        }
        public string TemInfo
        {
            get { return temInfo; }
            set { temInfo = value; OnPropertyChanged("TemInfo"); }
        }
        public string CellInfo
        {
            get
            {

                return cellInfo;

            }
            set
            {

                cellInfo = value; OnPropertyChanged("CellInfo");
            }
        }
        public string CVMax
        {
            get { return cVMax; }
            set { cVMax = value; OnPropertyChanged("CVMax"); }
        }


        public string VTotal
        {
            get { return vTotal; }
            set { vTotal = value; OnPropertyChanged("VTotal"); }
        }

        public string SOC
        {
            get { return sOC; }
            set { sOC = value; OnPropertyChanged("SOC"); }
        }

        public string BCNT
        {
            get { return bCNT; }
            set { bCNT = value; OnPropertyChanged("BCNT"); }
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
