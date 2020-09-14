using bms.startup.SDK;
using slaveUpperComputer.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    [Serializable]
   public class Gen2SlaveConfig : INotifyPropertyChanged
    {
        public Gen2SlaveConfig() {
            for (int i = 0; i < 60; i++) {
                balance.Add(false);
            }
        }

        private string sid;
        private string covth ;
        private string cuvth ;
        private string foth ;
        private string fcth;
        private string traceCode;
        private string bcnt_a;
        private string bcnt_b ;
        private string bcnt_c ;
        private string bcnt_d ;
        private string bcnt_e;
        private string bcnt_f ;
        private string maxCharge ;
        private bool realy2;//true开启(1),false关闭(0)
        private string canLife;
        private ObservableCollection<bool> balance = new ObservableCollection<bool>();

        public ObservableCollection<bool> Balance
        {
            get { return balance; }
            set { balance = value; OnPropertyChanged("Balance"); }
        }

        public string CanLife
        {
            get { return canLife; }
            set { canLife = value; OnPropertyChanged("CanLife"); }
        }

        public bool Realy2
        {
            get { return realy2; }
            set { realy2 = value; OnPropertyChanged("Realy2"); }
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
        
        public string Bcnt_a
        {
            get { return bcnt_a; }
            set { bcnt_a = value; OnPropertyChanged("Bcnt_a"); }
        }
        
        public string Bcnt_b
        {
            get { return bcnt_b; }
            set { bcnt_b = value; OnPropertyChanged("Bcnt_b"); }
        }
        
        public string Bcnt_c
        {
            get { return bcnt_c; }
            set { bcnt_c = value; OnPropertyChanged("Bcnt_c"); }
        }
        
        public string Bcnt_d
        {
            get { return bcnt_d; }
            set { bcnt_d = value; OnPropertyChanged("Bcnt_d"); }
        }
        
        public string Bcnt_e
        {
            get { return bcnt_e; }
            set { bcnt_e = value; OnPropertyChanged("Bcnt_e"); }
        }
        
        public string Bcnt_f
        {
            get { return bcnt_f; }
            set { bcnt_f = value; OnPropertyChanged("Bcnt_f"); }
        }
        

        public string MaxCharge
        {
            get { return maxCharge; }
            set { maxCharge = value; OnPropertyChanged("MaxCharge"); }
        }

        //判断配置1的数据输入是否有误
        public bool checkConfig1()
        {
            return DataConverter.canStirng2int(Sid) && DataConverter.canStirng2int(Covth) && DataConverter.canStirng2int(Cuvth)
                && DataConverter.canStirng2int(Foth) && DataConverter.canStirng2int(Fcth);
        }

        //均衡控制全选或者全不选
        public void setAll(bool b) {
            for (int i = 0; i < Balance.Count; i++) {
                Balance[i] = b;
            }
        }

        //获取均衡控制包
        public byte[] getBalanceData()
        {
            byte[] data = new byte[8];
            //data[0] = 0x4C;
            //data[1] = (byte)bmuNum;

            for (int i = 0; i < 56; i++)
            {
                data[i / 8 + 1] = (byte)(data[i / 8 + 1] | (Balance[i] == true ? 1 : 0) << (i % 8));
            }
            for (int i = 56; i < 60; i++) {
                data[0] = (byte)(data[0] | (Balance[i] == true ? 1 : 0) << (i % 8));
            }

            return data;
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
