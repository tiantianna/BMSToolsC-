using bms.startup.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.userControl
{



    public class BMUConfigViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> balanceModeListInBMU = new ObservableCollection<string>(); //均衡模式

        private int? selectBalanceMode2;
        public int? SelectBalanceMode2
        {
            get {
                switch (CellBalanMode)
                {
                    case "80":
                        return 3;
                    case "65":
                        return 1;
                    case "78":
                        return 2;
                    default:
                        return selectBalanceMode2;
                }

               // return selectBalanceMode2;
            }
            set { selectBalanceMode2 = value;
            if (selectBalanceMode2 != null) {
                switch (selectBalanceMode2) { 
                    case 0:
                        CellBalanMode = ((int)'N').ToString();
                        break;
                    case 1:
                        CellBalanMode = ((int)'A').ToString();
                        break;
                    case 2:
                        CellBalanMode = ((int)'P').ToString();
                        break;
                }
            }
                
                OnPropertyChanged("SelectBalanceMode2"); }
        }


        public ObservableCollection<string> BalanceModeListInBMU
        {
            get { return balanceModeListInBMU; }
            set { balanceModeListInBMU = value; OnPropertyChanged("BalanceModeListInBMU"); }
        }
        private int selectBalanceModeInBMU;//均衡模式索引 



        public int SelectBalanceModeInBMU
        {
            get {
                switch (selectBalanceModeInBMU)
                {
                    case 0:
                        bmuConfigModel_gy.CellBalanMode = ((int)'N').ToString();
                        break;

                    case 1:
                        bmuConfigModel_gy.CellBalanMode = ((int)'A').ToString();
                        break;
                    case 2:
                        bmuConfigModel_gy.CellBalanMode = ((int)'P').ToString();
                        break;
                };
                return selectBalanceModeInBMU; }
            set {               
                selectBalanceModeInBMU = value; OnPropertyChanged("SelectBalanceModeInBMU"); }
        }
        

        //从机编号
        private string slave;

        public string Slave
        {
            get { return bmuConfigModel_gy.Slave; }
            set
            {
                slave = value;
                OnPropertyChanged("Slave");
                bmuConfigModel_gy.Slave = slave;
            }
        }

        ////标志位，记录各info是否需要发送
        //private bool isChecked1;
        //private bool isChecked2;
        //private bool isChecked3;
        //private bool isChecked4;
        //private bool isChecked5;
        //private bool isChecked6;
        //private bool isChecked7;
        //private bool isChecked8;
        //private bool isChecked9;
        //private bool isChecked10;
        //private bool isChecked11;
        //private bool isChecked12;
        //private bool isChecked13;
        //private bool isChecked14;
        //private bool isChecked15;
        //private bool isChecked16;
        //private bool isChecked17;
        //private bool isChecked18;
        //private bool isChecked19;
        //private bool isChecked20;

        //public bool IsChecked1
        //{
        //    get { return isChecked1; }
        //    set
        //    {
        //        isChecked1 = value;
        //        OnPropertyChanged("IsChecked1");
        //        bmuConfigModel_gy.IsChecked1 = isChecked1;
        //    }
        //}
        //public bool IsChecked2
        //{
        //    get { return bmuConfigModel_gy.IsChecked2; }
        //    set
        //    {
        //        isChecked2 = value;
        //        OnPropertyChanged("IsChecked2");
        //        bmuConfigModel_gy.IsChecked2 = isChecked2;
        //    }
        //}
        //public bool IsChecked3
        //{
        //    get { return bmuConfigModel_gy.IsChecked3; }
        //    set
        //    {
        //        isChecked3 = value;
        //        OnPropertyChanged("IsChecked3");
        //        bmuConfigModel_gy.IsChecked3 = isChecked3;
        //    }
        //}
        //public bool IsChecked4
        //{
        //    get { return bmuConfigModel_gy.IsChecked4; }
        //    set
        //    {
        //        isChecked4 = value;
        //        OnPropertyChanged("IsChecked4");
        //        bmuConfigModel_gy.IsChecked4 = isChecked4;
        //    }
        //}
        //public bool IsChecked5
        //{
        //    get { return bmuConfigModel_gy.IsChecked5; }
        //    set
        //    {
        //        isChecked5 = value;
        //        OnPropertyChanged("IsChecked5");
        //        bmuConfigModel_gy.IsChecked5 = isChecked5;
        //    }
        //}
        //public bool IsChecked6
        //{
        //    get { return bmuConfigModel_gy.IsChecked6; }
        //    set
        //    {
        //        isChecked6 = value;
        //        OnPropertyChanged("IsChecked6");
        //        bmuConfigModel_gy.IsChecked6 = isChecked6;
        //    }
        //}
        //public bool IsChecked7
        //{
        //    get { return bmuConfigModel_gy.IsChecked7; }
        //    set
        //    {
        //        isChecked7 = value;
        //        OnPropertyChanged("IsChecked7");
        //        bmuConfigModel_gy.IsChecked7 = isChecked7;
        //    }
        //}
        //public bool IsChecked8
        //{
        //    get { return bmuConfigModel_gy.IsChecked8; }
        //    set
        //    {
        //        isChecked8 = value;
        //        OnPropertyChanged("IsChecked8");
        //        bmuConfigModel_gy.IsChecked8 = isChecked8;
        //    }
        //}
        //public bool IsChecked9
        //{
        //    get { return bmuConfigModel_gy.IsChecked9; }
        //    set
        //    {
        //        isChecked9 = value;
        //        OnPropertyChanged("IsChecked9");
        //        bmuConfigModel_gy.IsChecked9 = isChecked9;
        //    }
        //}
        //public bool IsChecked10
        //{
        //    get { return bmuConfigModel_gy.IsChecked10; }
        //    set
        //    {
        //        isChecked10 = value;
        //        OnPropertyChanged("IsChecked10");
        //        bmuConfigModel_gy.IsChecked10 = isChecked10;
        //    }
        //}
        //public bool IsChecked11
        //{
        //    get { return bmuConfigModel_gy.IsChecked11; }
        //    set
        //    {
        //        isChecked11 = value;
        //        OnPropertyChanged("IsChecked11");
        //        bmuConfigModel_gy.IsChecked11 = isChecked11;
        //    }
        //}
        //public bool IsChecked12
        //{
        //    get { return bmuConfigModel_gy.IsChecked12; }
        //    set
        //    {
        //        isChecked12 = value;
        //        OnPropertyChanged("IsChecked12");
        //        bmuConfigModel_gy.IsChecked12 = isChecked12;
        //    }
        //}
        //public bool IsChecked13
        //{
        //    get { return bmuConfigModel_gy.IsChecked13; }
        //    set
        //    {
        //        isChecked13 = value;
        //        OnPropertyChanged("IsChecked13");
        //        bmuConfigModel_gy.IsChecked13 = isChecked13;
        //    }
        //}
        //public bool IsChecked14
        //{
        //    get { return bmuConfigModel_gy.IsChecked14; }
        //    set
        //    {
        //        isChecked14 = value;
        //        OnPropertyChanged("IsChecked14");
        //        bmuConfigModel_gy.IsChecked14 = isChecked14;
        //    }
        //}
        //public bool IsChecked15
        //{
        //    get { return bmuConfigModel_gy.IsChecked15; }
        //    set
        //    {
        //        isChecked15 = value;
        //        OnPropertyChanged("IsChecked15");
        //        bmuConfigModel_gy.IsChecked15 = isChecked15;
        //    }
        //}
        //public bool IsChecked16
        //{
        //    get { return bmuConfigModel_gy.IsChecked16; }
        //    set
        //    {
        //        isChecked16 = value;
        //        OnPropertyChanged("IsChecked16");
        //        bmuConfigModel_gy.IsChecked16 = isChecked16;
        //    }
        //}
        //public bool IsChecked17
        //{
        //    get { return bmuConfigModel_gy.IsChecked17; }
        //    set
        //    {
        //        isChecked17 = value;
        //        OnPropertyChanged("IsChecked17");
        //        bmuConfigModel_gy.IsChecked17 = isChecked17;
        //    }
        //}
        //public bool IsChecked18
        //{
        //    get { return bmuConfigModel_gy.IsChecked18; }
        //    set
        //    {
        //        isChecked18 = value;
        //        OnPropertyChanged("IsChecked18");
        //        bmuConfigModel_gy.IsChecked18 = isChecked18;
        //    }
        //}
        //public bool IsChecked19
        //{
        //    get { return bmuConfigModel_gy.IsChecked19; }
        //    set
        //    {
        //        isChecked19 = value;
        //        OnPropertyChanged("IsChecked19");
        //        bmuConfigModel_gy.IsChecked19 = isChecked19;
        //    }
        //}
        //public bool IsChecked20
        //{
        //    get { return bmuConfigModel_gy.IsChecked20; }
        //    set
        //    {
        //        isChecked20 = value;
        //        OnPropertyChanged("IsChecked20");
        //        bmuConfigModel_gy.IsChecked20 = isChecked20;
        //    }
        //}

        //info1
        private string cellVolHighAlarmFirst;//单体过高一级
        private string cellVolHighAlarmSecond;//单体过高二级
        private string cellVolHighAlarmThird;//单体过高三级
        private string cellVolHighAlarmFirst_rec;
        private string cellVolHighAlarmSecond_rec;//单体过高二级
        private string cellVolHighAlarmThird_rec;//单体过高三级

        public string CellVolHighAlarmFirst
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmFirst; }
            set
            {
                //cellVolHighAlarmFirst = (value == "" ? "0" : value);
                //cellVolHighAlarmFirst = value ?? "0";
                cellVolHighAlarmFirst = value;
                OnPropertyChanged("CellVolHighAlarmFirst");
                bmuConfigModel_gy.CellVolHighAlarmFirst = cellVolHighAlarmFirst;
            }
        }

        public string CellVolHighAlarmFirst_rec
        {
            get
            {
                return cellVolHighAlarmFirst_rec;
            }

            set
            {
                cellVolHighAlarmFirst_rec = value;
                OnPropertyChanged("CellVolHighAlarmFirst_rec");
            }
        }

        public string CellVolHighAlarmSecond
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmSecond; }
            set
            {
                cellVolHighAlarmSecond = value;
                OnPropertyChanged("CellVolHighAlarmSecond");
                bmuConfigModel_gy.CellVolHighAlarmSecond = cellVolHighAlarmSecond;
            }
        }
        public string CellVolHighAlarmSecond_rec
        {
            get
            {
                return cellVolHighAlarmSecond_rec;
            }

            set
            {
                cellVolHighAlarmSecond_rec = value;
                OnPropertyChanged("CellVolHighAlarmSecond_rec");
            }
        }
        public string CellVolHighAlarmThird
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmThird; }
            set
            {
                cellVolHighAlarmThird = value; OnPropertyChanged("CellVolHighAlarmThird");
                bmuConfigModel_gy.CellVolHighAlarmThird = cellVolHighAlarmThird;
            }
        }
        public string CellVolHighAlarmThird_rec
        {
            get
            {
                return cellVolHighAlarmThird_rec;
            }

            set
            {
                cellVolHighAlarmThird_rec = value;
                OnPropertyChanged("CellVolHighAlarmThird_rec");
            }
        }

        //info2
        private string cellVolHighAlarmRemoveFirst;//单体过高解除一级
        private string cellVolHighAlarmRemoveSecond;//单体过高解除二级
        private string cellVolHighAlarmRemoveThird;//单体过高解除三级
        private string cellVolHighAlarmRemoveFirst_rec;//单体过高解除一级
        private string cellVolHighAlarmRemoveSecond_rec;//单体过高解除二级
        private string cellVolHighAlarmRemoveThird_rec;//单体过高解除三级

        public string CellVolHighAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmRemoveFirst; }
            set
            {
                cellVolHighAlarmRemoveFirst = value;
                OnPropertyChanged("CellVolHighAlarmRemoveFirst");
                bmuConfigModel_gy.CellVolHighAlarmRemoveFirst = cellVolHighAlarmRemoveFirst;
            }
        }
        public string CellVolHighAlarmRemoveFirst_rec
        {
            get
            {
                return cellVolHighAlarmRemoveFirst_rec;
            }

            set
            {
                cellVolHighAlarmRemoveFirst_rec = value;
                OnPropertyChanged("CellVolHighAlarmRemoveFirst_rec");

            }
        }

        public string CellVolHighAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmRemoveSecond; }
            set
            {
                cellVolHighAlarmRemoveSecond = value;
                OnPropertyChanged("CellVolHighAlarmRemoveSecond");
                bmuConfigModel_gy.CellVolHighAlarmRemoveSecond = cellVolHighAlarmRemoveSecond;
            }
        }
        public string CellVolHighAlarmRemoveSecond_rec
        {
            get
            {
                return cellVolHighAlarmRemoveSecond_rec;
            }

            set
            {
                cellVolHighAlarmRemoveSecond_rec = value;
                OnPropertyChanged("CellVolHighAlarmRemoveSecond_rec");

            }
        }

        public string CellVolHighAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.CellVolHighAlarmRemoveThird; }
            set
            {
                cellVolHighAlarmRemoveThird = value;
                OnPropertyChanged("CellVolHighAlarmRemoveThird");
                bmuConfigModel_gy.CellVolHighAlarmRemoveThird = cellVolHighAlarmRemoveThird;
            }
        }
        public string CellVolHighAlarmRemoveThird_rec
        {
            get
            {
                return cellVolHighAlarmRemoveThird_rec;
            }

            set
            {
                cellVolHighAlarmRemoveThird_rec = value;
                OnPropertyChanged("CellVolHighAlarmRemoveThird_rec");

            }
        }
        //info3
        private string cellVolLowAlarmFirst;//单体过低一级
        private string cellVolLowAlarmSecond;//单体过低二级
        private string cellVolLowAlarmThird;//单体过低三级
        private string cellVolLowAlarmFirst_rec;//单体过低一级
        private string cellVolLowAlarmSecond_rec;//单体过低二级
        private string cellVolLowAlarmThird_rec;//单体过低三级

        public string CellVolLowAlarmFirst
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmFirst; }
            set
            {
                cellVolLowAlarmFirst = value;
                OnPropertyChanged("CellVolLowAlarmFirst");
                bmuConfigModel_gy.CellVolLowAlarmFirst = cellVolLowAlarmFirst;
            }
        }
        public string CellVolLowAlarmFirst_rec
        {
            get
            {
                return cellVolLowAlarmFirst_rec;
            }

            set
            {
                cellVolLowAlarmFirst_rec = value;
                OnPropertyChanged("CellVolLowAlarmFirst_rec");

            }
        }
        public string CellVolLowAlarmSecond
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmSecond; }
            set
            {
                cellVolLowAlarmSecond = value;
                OnPropertyChanged("CellVolLowAlarmSecond");
                bmuConfigModel_gy.CellVolLowAlarmSecond = cellVolLowAlarmSecond;
            }
        }
        public string CellVolLowAlarmSecond_rec
        {
            get
            {
                return cellVolLowAlarmSecond_rec;
            }

            set
            {
                cellVolLowAlarmSecond_rec = value;
                OnPropertyChanged("CellVolLowAlarmSecond_rec");

            }
        }
        public string CellVolLowAlarmThird
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmThird; }
            set
            {
                cellVolLowAlarmThird = value;
                OnPropertyChanged("CellVolLowAlarmThird");
                bmuConfigModel_gy.CellVolLowAlarmThird = cellVolLowAlarmThird;
            }
        }
        public string CellVolLowAlarmThird_rec
        {
            get
            {
                return cellVolLowAlarmThird_rec;
            }

            set
            {
                cellVolLowAlarmThird_rec = value;
                OnPropertyChanged("CellVolLowAlarmThird_rec");

            }
        }

        //info4
        private string cellVolLowAlarmRemoveFirst;//单体过低解除一级
        private string cellVolLowAlarmRemoveSecond;//单体过低解除二级
        private string cellVolLowAlarmRemoveThird;//单体过低解除三级
        private string cellVolLowAlarmRemoveFirst_rec;//单体过低解除一级
        private string cellVolLowAlarmRemoveSecond_rec;//单体过低解除二级
        private string cellVolLowAlarmRemoveThird_rec;//单体过低解除三级

        public string CellVolLowAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmRemoveFirst; }
            set
            {
                cellVolLowAlarmRemoveFirst = value;
                OnPropertyChanged("CellVolLowAlarmRemoveFirst");
                bmuConfigModel_gy.CellVolLowAlarmRemoveFirst = cellVolLowAlarmRemoveFirst;
            }
        }
        public string CellVolLowAlarmRemoveFirst_rec
        {
            get
            {
                return cellVolLowAlarmRemoveFirst_rec;
            }

            set
            {
                cellVolLowAlarmRemoveFirst_rec = value;
                OnPropertyChanged("CellVolLowAlarmRemoveFirst_rec");
            }
        }
        public string CellVolLowAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmRemoveSecond; }
            set
            {
                cellVolLowAlarmRemoveSecond = value;
                OnPropertyChanged("CellVolLowAlarmRemoveSecond");
                bmuConfigModel_gy.CellVolLowAlarmRemoveSecond = cellVolLowAlarmRemoveSecond;
            }
        }
        public string CellVolLowAlarmRemoveSecond_rec
        {
            get
            {
                return cellVolLowAlarmRemoveSecond_rec;
            }

            set
            {
                cellVolLowAlarmRemoveSecond_rec = value;
                OnPropertyChanged("CellVolLowAlarmRemoveSecond_rec");
            }
        }
        public string CellVolLowAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.CellVolLowAlarmRemoveThird; }
            set
            {
                cellVolLowAlarmRemoveThird = value;
                OnPropertyChanged("CellVolLowAlarmRemoveThird");
                bmuConfigModel_gy.CellVolLowAlarmRemoveThird = cellVolLowAlarmRemoveThird;
            }
        }
        public string CellVolLowAlarmRemoveThird_rec
        {
            get
            {
                return cellVolLowAlarmRemoveThird_rec;
            }

            set
            {
                cellVolLowAlarmRemoveThird_rec = value;
                OnPropertyChanged("CellVolLowAlarmRemoveThird_rec");
            }
        }
        //info5
        private string cellTemperatureHighAlarmFirst;//温度过高一级
        private string cellTemperatureHighAlarmSecond;//温度过高二级
        private string cellTemperatureHighAlarmThird;//温度过高三级
        private string cellTemperatureHighAlarmRemoveFirst;//温度过高解除一级
        private string cellTemperatureHighAlarmRemoveSecond;//温度过高解除二级
        private string cellTemperatureHighAlarmRemoveThird;//温度过高解除三级
        private string cellTemperatureHighAlarmFirst_rec;//温度过高一级
        private string cellTemperatureHighAlarmSecond_rec;//温度过高二级
        private string cellTemperatureHighAlarmThird_rec;//温度过高三级
        private string cellTemperatureHighAlarmRemoveFirst_rec;//温度过高解除一级
        private string cellTemperatureHighAlarmRemoveSecond_rec;//温度过高解除二级
        private string cellTemperatureHighAlarmRemoveThird_rec;//温度过高解除三级

        public string CellTemperatureHighAlarmFirst
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmFirst; }
            set
            {
                cellTemperatureHighAlarmFirst = value;
                OnPropertyChanged("CellTemperatureHighAlarmFirst");
                bmuConfigModel_gy.CellTemperatureHighAlarmFirst = cellTemperatureHighAlarmFirst;
            }
        }

        public string CellTemperatureHighAlarmFirst_rec
        {
            get
            {
                return cellTemperatureHighAlarmFirst_rec;
            }

            set
            {
                cellTemperatureHighAlarmFirst_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmFirst_rec");
            }
        }
        public string CellTemperatureHighAlarmSecond
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmSecond; }
            set
            {
                cellTemperatureHighAlarmSecond = value;
                OnPropertyChanged("CellTemperatureHighAlarmSecond");
                bmuConfigModel_gy.CellTemperatureHighAlarmSecond = cellTemperatureHighAlarmSecond;
            }
        }
        public string CellTemperatureHighAlarmSecond_rec
        {
            get
            {
                return cellTemperatureHighAlarmSecond_rec;
            }

            set
            {
                cellTemperatureHighAlarmSecond_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmSecond_rec");
            }
        }
        public string CellTemperatureHighAlarmThird
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmThird; }
            set
            {
                cellTemperatureHighAlarmThird = value;
                OnPropertyChanged("CellTemperatureHighAlarmThird");
                bmuConfigModel_gy.CellTemperatureHighAlarmThird = cellTemperatureHighAlarmThird;
            }
        }
        public string CellTemperatureHighAlarmThird_rec
        {
            get
            {
                return cellTemperatureHighAlarmThird_rec;
            }

            set
            {
                cellTemperatureHighAlarmThird_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmThird_rec");
            }
        }
        public string CellTemperatureHighAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmRemoveFirst; }
            set
            {
                cellTemperatureHighAlarmRemoveFirst = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveFirst");
                bmuConfigModel_gy.CellTemperatureHighAlarmRemoveFirst = cellTemperatureHighAlarmRemoveFirst;
            }
        }
        public string CellTemperatureHighAlarmRemoveFirst_rec
        {
            get
            {
                return cellTemperatureHighAlarmRemoveFirst_rec;
            }

            set
            {
                cellTemperatureHighAlarmRemoveFirst_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveFirst_rec");
            }
        }
        public string CellTemperatureHighAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmRemoveSecond; }
            set
            {
                cellTemperatureHighAlarmRemoveSecond = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveSecond");
                bmuConfigModel_gy.CellTemperatureHighAlarmRemoveSecond = cellTemperatureHighAlarmRemoveSecond;
            }
        }
        public string CellTemperatureHighAlarmRemoveSecond_rec
        {
            get
            {
                return cellTemperatureHighAlarmRemoveSecond_rec;
            }

            set
            {
                cellTemperatureHighAlarmRemoveSecond_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveSecond_rec");
            }
        }
        public string CellTemperatureHighAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.CellTemperatureHighAlarmRemoveThird; }
            set
            {
                cellTemperatureHighAlarmRemoveThird = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveThird");
                bmuConfigModel_gy.CellTemperatureHighAlarmRemoveThird = cellTemperatureHighAlarmRemoveThird;
            }
        }
        public string CellTemperatureHighAlarmRemoveThird_rec
        {
            get
            {
                return cellTemperatureHighAlarmRemoveThird_rec;
            }

            set
            {
                cellTemperatureHighAlarmRemoveThird_rec = value;
                OnPropertyChanged("CellTemperatureHighAlarmRemoveThird_rec");
            }
        }

        //info6
        private string cellTemperatureLowAlarmFirst;//温度过低一级
        private string cellTemperatureLowAlarmSecond;//温度过低二级
        private string cellTemperatureLowAlarmThird;//温度过低三级
        private string cellTemperatureLowAlarmRemoveFirst;//温度过低解除一级
        private string cellTemperatureLowAlarmRemoveSecond;//温度过低解除二级
        private string cellTemperatureLowAlarmRemoveThird;//温度过低解除三级
        private string cellTemperatureLowAlarmFirst_rec;//温度过低一级
        private string cellTemperatureLowAlarmSecond_rec;//温度过低二级
        private string cellTemperatureLowAlarmThird_rec;//温度过低三级
        private string cellTemperatureLowAlarmRemoveFirst_rec;//温度过低解除一级
        private string cellTemperatureLowAlarmRemoveSecond_rec;//温度过低解除二级
        private string cellTemperatureLowAlarmRemoveThird_rec;//温度过低解除三级

        public string CellTemperatureLowAlarmFirst
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmFirst; }
            set
            {
                cellTemperatureLowAlarmFirst = value;
                OnPropertyChanged("CellTemperatureLowAlarmFirst");
                bmuConfigModel_gy.CellTemperatureLowAlarmFirst = cellTemperatureLowAlarmFirst;
            }
        }
        public string CellTemperatureLowAlarmFirst_rec
        {
            get
            {
                return cellTemperatureLowAlarmFirst_rec;
            }

            set
            {
                cellTemperatureLowAlarmFirst_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmFirst_rec");
            }
        }

        public string CellTemperatureLowAlarmSecond
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmSecond; }
            set
            {
                cellTemperatureLowAlarmSecond = value;
                OnPropertyChanged("CellTemperatureLowAlarmSecond");
                bmuConfigModel_gy.CellTemperatureLowAlarmSecond = cellTemperatureLowAlarmSecond;
            }
        }
        public string CellTemperatureLowAlarmSecond_rec
        {
            get
            {
                return cellTemperatureLowAlarmSecond_rec;
            }

            set
            {
                cellTemperatureLowAlarmSecond_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmSecond_rec");
            }

        }

        public string CellTemperatureLowAlarmThird
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmThird; }
            set
            {
                cellTemperatureLowAlarmThird = value;
                OnPropertyChanged("CellTemperatureLowAlarmThird");
                bmuConfigModel_gy.CellTemperatureLowAlarmThird = cellTemperatureLowAlarmThird;
            }
        }
        public string CellTemperatureLowAlarmThird_rec
        {
            get
            {
                return cellTemperatureLowAlarmThird_rec;
            }

            set
            {
                cellTemperatureLowAlarmThird_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmThird_rec");
            }
        }
        public string CellTemperatureLowAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmRemoveFirst; }
            set
            {
                cellTemperatureLowAlarmRemoveFirst = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveFirst");
                bmuConfigModel_gy.CellTemperatureLowAlarmRemoveFirst = cellTemperatureLowAlarmRemoveFirst;
            }
        }
        public string CellTemperatureLowAlarmRemoveFirst_rec
        {
            get
            {
                return cellTemperatureLowAlarmRemoveFirst_rec;
            }

            set
            {
                cellTemperatureLowAlarmRemoveFirst_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveFirst_rec");
            }
        }
       
        public string CellTemperatureLowAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmRemoveSecond; }
            set
            {
                cellTemperatureLowAlarmRemoveSecond = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveSecond");
                bmuConfigModel_gy.CellTemperatureLowAlarmRemoveSecond = cellTemperatureLowAlarmRemoveSecond;
            }
        }
        public string CellTemperatureLowAlarmRemoveSecond_rec
        {
            get
            {
                return cellTemperatureLowAlarmRemoveSecond_rec;
            }

            set
            {
                cellTemperatureLowAlarmRemoveSecond_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveSecond_rec");
            }
        }
        public string CellTemperatureLowAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.CellTemperatureLowAlarmRemoveThird; }
            set
            {
                cellTemperatureLowAlarmRemoveThird = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveThird");
                bmuConfigModel_gy.CellTemperatureLowAlarmRemoveThird = cellTemperatureLowAlarmRemoveThird;
            }
        }
        public string CellTemperatureLowAlarmRemoveThird_rec
        {
            get
            {
                return cellTemperatureLowAlarmRemoveThird_rec;
            }

            set
            {
                cellTemperatureLowAlarmRemoveThird_rec = value;
                OnPropertyChanged("CellTemperatureLowAlarmRemoveThird_rec");
            }
        }

        //info7
        private string balanCurrentHighAlarmFirst;//均衡电流过高一级
        private string balanCurrentHighAlarmSecond;//均衡电流过高二级
        private string balanCurrentHighAlarmThird;//均衡电流过高三级
        private string balanCurrentHighAlarmFirst_rec;//均衡电流过高一级
        private string balanCurrentHighAlarmSecond_rec;//均衡电流过高二级
        private string balanCurrentHighAlarmThird_rec;//均衡电流过高三级

        public string BalanCurrentHighAlarmFirst
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmFirst; }
            set
            {
                balanCurrentHighAlarmFirst = value;
                OnPropertyChanged("BalanCurrentHighAlarmFirst");
                bmuConfigModel_gy.BalanCurrentHighAlarmFirst = balanCurrentHighAlarmFirst;
            }
        }
        public string BalanCurrentHighAlarmFirst_rec
        {
            get
            {
                return balanCurrentHighAlarmFirst_rec;
            }

            set
            {
                balanCurrentHighAlarmFirst_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmFirst_rec");
            }
        }
        public string BalanCurrentHighAlarmSecond
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmSecond; }
            set
            {
                balanCurrentHighAlarmSecond = value;
                OnPropertyChanged("BalanCurrentHighAlarmSecond");
                bmuConfigModel_gy.BalanCurrentHighAlarmSecond = balanCurrentHighAlarmSecond;
            }
        }
        public string BalanCurrentHighAlarmSecond_rec
        {
            get
            {
                return balanCurrentHighAlarmSecond_rec;
            }

            set
            {
                balanCurrentHighAlarmSecond_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmSecond_rec");
            }
        }
        public string BalanCurrentHighAlarmThird
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmThird; }
            set
            {
                balanCurrentHighAlarmThird = value;
                OnPropertyChanged("BalanCurrentHighAlarmThird");
                bmuConfigModel_gy.BalanCurrentHighAlarmThird = balanCurrentHighAlarmThird;
            }
        }
        public string BalanCurrentHighAlarmThird_rec
        {
            get
            {
                return balanCurrentHighAlarmThird_rec;
            }

            set
            {
                balanCurrentHighAlarmThird_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmThird_rec");
            }
        }

        //info8
        private string balanCurrentHighAlarmRemoveFirst;//均衡电流过高解除一级
        private string balanCurrentHighAlarmRemoveSecond;//均衡电流过高解除二级
        private string balanCurrentHighAlarmRemoveThird;//均衡电流过高解除三级
        private string balanCurrentHighAlarmRemoveFirst_rec;//均衡电流过高解除一级
        private string balanCurrentHighAlarmRemoveSecond_rec;//均衡电流过高解除二级
        private string balanCurrentHighAlarmRemoveThird_rec;//均衡电流过高解除三级

        public string BalanCurrentHighAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmRemoveFirst; }
            set
            {
                balanCurrentHighAlarmRemoveFirst = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveFirst");
                bmuConfigModel_gy.BalanCurrentHighAlarmRemoveFirst = balanCurrentHighAlarmRemoveFirst;
            }
        }
        public string BalanCurrentHighAlarmRemoveFirst_rec
        {
            get
            {
                return balanCurrentHighAlarmRemoveFirst_rec;
            }

            set
            {
                balanCurrentHighAlarmRemoveFirst_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveFirst_rec");
            }
        }
        public string BalanCurrentHighAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmRemoveSecond; }
            set
            {
                balanCurrentHighAlarmRemoveSecond = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveSecond");
                bmuConfigModel_gy.BalanCurrentHighAlarmRemoveSecond = balanCurrentHighAlarmRemoveSecond;
            }
        }
        public string BalanCurrentHighAlarmRemoveSecond_rec
        {
            get
            {
                return balanCurrentHighAlarmRemoveSecond_rec;
            }

            set
            {
                balanCurrentHighAlarmRemoveSecond_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveSecond_rec");
            }
        }
        public string BalanCurrentHighAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.BalanCurrentHighAlarmRemoveThird; }
            set
            {
                balanCurrentHighAlarmRemoveThird = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveThird");
                bmuConfigModel_gy.BalanCurrentHighAlarmRemoveThird = balanCurrentHighAlarmRemoveThird;
            }
        }
        public string BalanCurrentHighAlarmRemoveThird_rec
        {
            get
            {
                return balanCurrentHighAlarmRemoveThird_rec;
            }

            set
            {
                balanCurrentHighAlarmRemoveThird_rec = value;
                OnPropertyChanged("BalanCurrentHighAlarmRemoveThird_rec");
            }
        }

        //info9
        private string balanCurrentLowAlarmFirst;//均衡电流过低一级
        private string balanCurrentLowAlarmSecond;//均衡电流过低二级
        private string balanCurrentLowAlarmThird;//均衡电流过低三级
        private string balanCurrentLowAlarmFirst_rec;//均衡电流过低一级
        private string balanCurrentLowAlarmSecond_rec;//均衡电流过低二级
        private string balanCurrentLowAlarmThird_rec;//均衡电流过低三级

        public string BalanCurrentLowAlarmFirst
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmFirst; }
            set
            {
                balanCurrentLowAlarmFirst = value;
                OnPropertyChanged("BalanCurrentLowAlarmFirst");
                bmuConfigModel_gy.BalanCurrentLowAlarmFirst = balanCurrentLowAlarmFirst;
            }
        }
        public string BalanCurrentLowAlarmFirst_rec
        {
            get
            {
                return balanCurrentLowAlarmFirst_rec;
            }

            set
            {
                balanCurrentLowAlarmFirst_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmFirst_rec");
            }
        }
        public string BalanCurrentLowAlarmSecond
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmSecond; }
            set
            {
                balanCurrentLowAlarmSecond = value;
                OnPropertyChanged("BalanCurrentLowAlarmSecond");
                bmuConfigModel_gy.BalanCurrentLowAlarmSecond = balanCurrentLowAlarmSecond;
            }
        }
        public string BalanCurrentLowAlarmSecond_rec
        {
            get
            {
                return balanCurrentLowAlarmSecond_rec;
            }

            set
            {
                balanCurrentLowAlarmSecond_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmSecond_rec");
            }
        }
        public string BalanCurrentLowAlarmThird
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmThird; }
            set
            {
                balanCurrentLowAlarmSecond = value;
                OnPropertyChanged("BalanCurrentLowAlarmThird");
                bmuConfigModel_gy.BalanCurrentLowAlarmThird = balanCurrentLowAlarmSecond;
            }
        }
        public string BalanCurrentLowAlarmThird_rec
        {
            get
            {
                return balanCurrentLowAlarmThird_rec;
            }

            set
            {
                balanCurrentLowAlarmThird_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmThird_rec");
            }
        }


        //info10
        private string balanCurrentLowAlarmRemoveFirst;//均衡电流过低解除一级
        private string balanCurrentLowAlarmRemoveSecond;//均衡电流过低解除二级
        private string balanCurrentLowAlarmRemoveThird;//均衡电流过低解除三级
        private string balanCurrentLowAlarmRemoveFirst_rec;//均衡电流过低解除一级
        private string balanCurrentLowAlarmRemoveSecond_rec;//均衡电流过低解除二级
        private string balanCurrentLowAlarmRemoveThird_rec;//均衡电流过低解除三级

        public string BalanCurrentLowAlarmRemoveFirst
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmRemoveFirst; }
            set
            {
                balanCurrentLowAlarmRemoveFirst = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveFirst");
                bmuConfigModel_gy.BalanCurrentLowAlarmRemoveFirst = balanCurrentLowAlarmRemoveFirst;
            }
        }
        public string BalanCurrentLowAlarmRemoveFirst_rec
        {
            get
            {
                return balanCurrentLowAlarmRemoveFirst_rec;
            }

            set
            {
                balanCurrentLowAlarmRemoveFirst_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveFirst_rec");
            }
        }
        public string BalanCurrentLowAlarmRemoveSecond
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmRemoveSecond; }
            set
            {
                balanCurrentLowAlarmRemoveSecond = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveSecond");
                bmuConfigModel_gy.BalanCurrentLowAlarmRemoveSecond = balanCurrentLowAlarmRemoveSecond;
            }
        }
        public string BalanCurrentLowAlarmRemoveSecond_rec
        {
            get
            {
                return balanCurrentLowAlarmRemoveSecond_rec;
            }

            set
            {
                balanCurrentLowAlarmRemoveSecond_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveSecond_rec");
            }
        }
        public string BalanCurrentLowAlarmRemoveThird
        {
            get { return bmuConfigModel_gy.BalanCurrentLowAlarmRemoveThird; }
            set
            {
                balanCurrentLowAlarmRemoveThird = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveThird");
                bmuConfigModel_gy.BalanCurrentLowAlarmRemoveThird = balanCurrentLowAlarmRemoveThird;
            }
        }
        public string BalanCurrentLowAlarmRemoveThird_rec
        {
            get
            {
                return balanCurrentLowAlarmRemoveThird_rec;
            }

            set
            {
                balanCurrentLowAlarmRemoveThird_rec = value;
                OnPropertyChanged("BalanCurrentLowAlarmRemoveThird_rec");
            }
        }

        //info11
        private string balanCurrentSetValueFirst;//均衡电流大小一级 
        private string balanCurrentSetValueSecond;//均衡电流大小二级
        private string balanCurrentSetValueThird;//均衡电流大小三级
        private string balanCurrentSetValueFirst_rec;//均衡电流大小一级 
        private string balanCurrentSetValueSecond_rec;//均衡电流大小二级
        private string balanCurrentSetValueThird_rec;//均衡电流大小三级

        public string BalanCurrentSetValueFirst
        {
            get { return bmuConfigModel_gy.BalanCurrentSetValueFirst; }
            set
            {
                balanCurrentSetValueFirst = value;
                OnPropertyChanged("BalanCurrentSetValueFirst");
                bmuConfigModel_gy.BalanCurrentSetValueFirst = balanCurrentSetValueFirst;
            }
        }
        public string BalanCurrentSetValueFirst_rec
        {
            get
            {
                return balanCurrentSetValueFirst_rec;
            }

            set
            {
                balanCurrentSetValueFirst_rec = value;
                OnPropertyChanged("BalanCurrentSetValueFirst_rec");
            }
        }
        public string BalanCurrentSetValueSecond
        {
            get { return bmuConfigModel_gy.BalanCurrentSetValueSecond; }
            set
            {
                balanCurrentSetValueSecond = value;
                OnPropertyChanged("BalanCurrentSetValueSecond");
                bmuConfigModel_gy.BalanCurrentSetValueSecond = balanCurrentSetValueSecond;
            }
        }
        public string BalanCurrentSetValueSecond_rec
        {
            get
            {
                return balanCurrentSetValueSecond_rec;
            }

            set
            {
                balanCurrentSetValueSecond_rec = value;
                OnPropertyChanged("BalanCurrentSetValueSecond_rec");
            }
        }
        public string BalanCurrentSetValueThird
        {
            get { return bmuConfigModel_gy.BalanCurrentSetValueThird; }
            set
            {
                balanCurrentSetValueThird = value;
                OnPropertyChanged("BalanCurrentSetValueThird");
                bmuConfigModel_gy.BalanCurrentSetValueThird = balanCurrentSetValueThird;
            }
        }
        public string BalanCurrentSetValueThird_rec
        {
            get
            {
                return balanCurrentSetValueThird_rec;
            }

            set
            {
                balanCurrentSetValueThird_rec = value;
                OnPropertyChanged("BalanCurrentSetValueThird_rec");
            }
        }


        //info12
        private string balanVolOpenValue;//均衡开启电压
        private string balanVolOpenValue_rec;//均衡开启电压

        public string BalanVolOpenValue
        {
            get { return bmuConfigModel_gy.BalanVolOpenValue; }
            set
            {
                balanVolOpenValue = value;
                OnPropertyChanged("BalanVolOpenValue");
                bmuConfigModel_gy.BalanVolOpenValue = balanVolOpenValue;
            }
        }
        public string BalanVolOpenValue_rec
        {
            get
            {
                return balanVolOpenValue_rec;
            }

            set
            {
                balanVolOpenValue_rec = value;
                OnPropertyChanged("BalanVolOpenValue_rec");
            }
        }


        //info13
        private string balanVolCloseValue;//均衡截止电压
        private string balanVolCloseValue_rec;//均衡截止电压

        public string BalanVolCloseValue
        {
            get { return bmuConfigModel_gy.BalanVolCloseValue; }
            set
            {
                balanVolCloseValue = value;
                OnPropertyChanged("BalanVolCloseValue");
                bmuConfigModel_gy.BalanVolCloseValue = balanVolCloseValue;
            }
        }
        public string BalanVolCloseValue_rec
        {
            get
            {
                return balanVolCloseValue_rec;
            }

            set
            {
                balanVolCloseValue_rec = value;
                OnPropertyChanged("BalanVolCloseValue_rec");
            }
        }

        //info14
        private string balanVolDifOpenValue;//均衡开启压差
        private string balanVolDifOpenValue_rec;//均衡开启压差

        public string BalanVolDifOpenValue
        {
            get { return bmuConfigModel_gy.BalanVolDifOpenValue; }
            set
            {
                balanVolDifOpenValue = value;
                OnPropertyChanged("BalanVolDifOpenValue");
                bmuConfigModel_gy.BalanVolDifOpenValue = balanVolDifOpenValue;
            }
        }
        public string BalanVolDifOpenValue_rec
        {
            get
            {
                return balanVolDifOpenValue_rec;
            }

            set
            {
                balanVolDifOpenValue_rec = value;
                OnPropertyChanged("BalanVolDifOpenValue_rec");
            }
        }

        //info15
        private string balanVolDifCloseValue;//均衡截止压差
        private string balanVolDifCloseValue_rec;//均衡截止压差

        public string BalanVolDifCloseValue
        {
            get { return bmuConfigModel_gy.BalanVolDifCloseValue; }
            set
            {
                balanVolDifCloseValue = value;
                OnPropertyChanged("BalanVolDifCloseValue");
                bmuConfigModel_gy.BalanVolDifCloseValue = balanVolDifCloseValue;
            }
        }
        public string BalanVolDifCloseValue_rec
        {
            get
            {
                return balanVolDifCloseValue_rec;
            }

            set
            {
                balanVolDifCloseValue_rec = value;
                OnPropertyChanged("BalanVolDifCloseValue_rec");
            }
        }

        //info16
        private string cellBalanTemperatureOpenValue;//均衡电池开启温度
        private string cellBalanTemperatureCloseValue;//均衡电池截止温度
        private string cellBalanTemperatureOpenValue_rec;//均衡电池开启温度
        private string cellBalanTemperatureCloseValue_rec;//均衡电池截止温度

        public string CellBalanTemperatureOpenValue
        {
            get { return bmuConfigModel_gy.CellBalanTemperatureOpenValue; }
            set
            {
                cellBalanTemperatureOpenValue = value;
                OnPropertyChanged("CellBalanTemperatureOpenValue");
                bmuConfigModel_gy.CellBalanTemperatureOpenValue = cellBalanTemperatureOpenValue;
            }
        }
        public string CellBalanTemperatureOpenValue_rec
        {
            get
            {
                return cellBalanTemperatureOpenValue_rec;
            }

            set
            {
                cellBalanTemperatureOpenValue_rec = value;
                OnPropertyChanged("CellBalanTemperatureOpenValue_rec");
            }
        }
        public string CellBalanTemperatureCloseValue
        {
            get { return bmuConfigModel_gy.CellBalanTemperatureCloseValue; }
            set
            {
                cellBalanTemperatureCloseValue = value;
                OnPropertyChanged("CellBalanTemperatureCloseValue");
                bmuConfigModel_gy.CellBalanTemperatureCloseValue = cellBalanTemperatureCloseValue;
            }
        }
        public string CellBalanTemperatureCloseValue_rec
        {
            get
            {
                return cellBalanTemperatureCloseValue_rec;
            }

            set
            {
                cellBalanTemperatureCloseValue_rec = value;
                OnPropertyChanged("CellBalanTemperatureCloseValue_rec");
            }
        }

        //info17（单独配置）
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

        public string SlaveNum
        {
            get { return bmuConfigModel_gy.SlaveNum; }
            set
            {
                slaveNum = value;
                OnPropertyChanged("SlaveNum");
                bmuConfigModel_gy.SlaveNum = slaveNum;
            }
        }
        public string SlaveNum_rec
        {
            get
            {
                return slaveNum_rec;
            }

            set
            {
                slaveNum_rec = value;
                OnPropertyChanged("SlaveNum_rec");
            }
        }


        //public string CellBalanMode_rec
        //{
        //    get
        //    {
        //        if (CellBalanMode == null)
        //        {
        //            return null;
        //        }
        //        int temp = Convert.ToInt32(CellBalanMode);

        //        switch (temp)
        //        {
        //            case (int)'N':
        //                return "不均衡";
        //            case (int)'A':
        //                return "主动均衡";
        //            case (int)'P':
        //                return "被动均衡";
        //        }
        //        return cellBalanMode_rec;
        //    }
        //    set { cellBalanMode_rec = value; OnPropertyChanged("CellBalanMode_rec"); }
        //}

        public string CellBalanMode
        {
            get
            {
                if (selectBalanceMode2 == null)
                {
                    return null;
                }
                switch (selectBalanceMode2)
                {
                    case 0:
                        return ((int)'N').ToString();
                    case 1:
                        return ((int)'A').ToString();
                    case 2:
                        return ((int)'P').ToString();
                };
                return bmuConfigModel_gy.CellBalanMode;
            }
            set { cellBalanMode = value; OnPropertyChanged("CellBalanMode");
            bmuConfigModel_gy.SelectBalanceMode = SelectBalanceMode2;
            bmuConfigModel_gy.CellBalanMode = cellBalanMode;
            }
        }

        //public string CellBalanMode
        //{
        //    get { return bmuConfigModel_gy.CellBalanMode; }
        //    set
        //    {
        //        cellBalanMode = value;
        //        OnPropertyChanged("CellBalanMode");
        //        bmuConfigModel_gy.CellBalanMode = cellBalanMode;
        //    }
        //}

        private string cellBalanModeTemp="";
        public string CellBalanMode_rec
        {
            get
            {
                if (CellBalanMode == null)
                {
                    return null;
                }
                int temp = 0;
                if (!cellBalanModeTemp.Equals(""))
                {
                    temp = Convert.ToInt32(cellBalanModeTemp);
                    cellBalanModeTemp = "";
                }
                else
                {
                    temp = Convert.ToInt32(CellBalanMode);
                }
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
                
                //return cellBalanMode_rec;
            }
            set { cellBalanMode_rec = value; cellBalanModeTemp = value; OnPropertyChanged("CellBalanMode_rec"); }
        }

        //public string CellBalanMode_rec
        //{
        //    get
        //    {
        //        int temp = Convert.ToInt32(cellBalanMode_rec);
        //        switch (temp)
        //        {
        //            case (int)'N':
        //                return "不均衡";
        //            case (int)'A':
        //                return "主动均衡";
        //            case (int)'P':
        //                return "被动均衡";
        //        }
        //        return cellBalanMode_rec;
        //    }

        //    set
        //    {
        //        cellBalanMode_rec = value;
        //        OnPropertyChanged("CellBalanMode_rec");
        //    }
        //}
        public string ChildModuleMonCellNumber
        {
            get { return bmuConfigModel_gy.ChildModuleMonCellNumber; }
            set
            {
                childModuleMonCellNumber = value;
                OnPropertyChanged("ChildModuleMonCellNumber");
                bmuConfigModel_gy.ChildModuleMonCellNumber = childModuleMonCellNumber;
            }
        }
        public string ChildModuleMonCellNumber_rec
        {
            get
            {
                return childModuleMonCellNumber_rec;
            }

            set
            {
                childModuleMonCellNumber_rec = value;
                OnPropertyChanged("ChildModuleMonCellNumber_rec");
            }
        }
        public string ChildMonModuleTemperatureNumber
        {
            get { return bmuConfigModel_gy.ChildMonModuleTemperatureNumber; }
            set
            {
                childMonModuleTemperatureNumber = value;
                OnPropertyChanged("ChildMonModuleTemperatureNumber");
                bmuConfigModel_gy.ChildMonModuleTemperatureNumber = childMonModuleTemperatureNumber;
            }
        }
        public string ChildMonModuleTemperatureNumber_rec
        {
            get
            {
                return childMonModuleTemperatureNumber_rec;
            }

            set
            {
                childMonModuleTemperatureNumber_rec = value;
                OnPropertyChanged("ChildMonModuleTemperatureNumber_rec");
            }
        }
        public string ModuleAMonCellNum
        {
            get { return bmuConfigModel_gy.ModuleAMonCellNum; }
            set
            {
                moduleAMonCellNum = value;
                OnPropertyChanged("ModuleAMonCellNum");
                bmuConfigModel_gy.ModuleAMonCellNum = moduleAMonCellNum;
            }
        }
        public string ModuleAMonCellNum_rec
        {
            get
            {
                return moduleAMonCellNum_rec;
            }

            set
            {
                moduleAMonCellNum_rec = value;
                OnPropertyChanged("ModuleAMonCellNum_rec");
            }
        }
        public string ModuleAMonTemperatureNum
        {
            get { return bmuConfigModel_gy.ModuleAMonTemperatureNum; }
            set
            {
                moduleAMonCellNum = value;
                OnPropertyChanged("ModuleAMonTemperatureNum");
                bmuConfigModel_gy.ModuleAMonTemperatureNum = moduleAMonCellNum;
            }
        }
        public string ModuleAMonTemperatureNum_rec
        {
            get
            {
                return moduleAMonTemperatureNum_rec;
            }

            set
            {
                moduleAMonTemperatureNum_rec = value;
                OnPropertyChanged("ModuleAMonTemperatureNum_rec");
            }
        }

        //info18
        private string moduleBMonCellNum;//子模块B监控电池数目
        private string moduleBMonTemperatureNum;//子模块B监控温感数目
        private string moduleCMonCellNum;//子模块C监控电池数目
        private string moduleCMonTemperatureNum;//子模块C监控温感数目
        private string moduleDMonCellNum;//子模块D监控电池数目
        private string moduleDMonTemperatureNum;//子模块D监控温感数目
        private string moduleBMonCellNum_rec;//子模块B监控电池数目
        private string moduleBMonTemperatureNum_rec;//子模块B监控温感数目
        private string moduleCMonCellNum_rec;//子模块C监控电池数目
        private string moduleCMonTemperatureNum_rec;//子模块C监控温感数目
        private string moduleDMonCellNum_rec;//子模块D监控电池数目
        private string moduleDMonTemperatureNum_rec;//子模块D监控温感数目

        public string ModuleBMonCellNum
        {
            get { return bmuConfigModel_gy.ModuleBMonCellNum; }
            set
            {
                moduleBMonCellNum = value;
                OnPropertyChanged("ModuleBMonCellNum");
                bmuConfigModel_gy.ModuleBMonCellNum = moduleBMonCellNum;
            }
        }
        public string ModuleBMonCellNum_rec
        {
            get
            {
                return moduleBMonCellNum_rec;
            }

            set
            {
                moduleBMonCellNum_rec = value;
                OnPropertyChanged("ModuleBMonCellNum_rec");
            }
        }
        public string ModuleBMonTemperatureNum
        {
            get { return bmuConfigModel_gy.ModuleBMonTemperatureNum; }
            set
            {
                moduleBMonTemperatureNum = value;
                OnPropertyChanged("ModuleBMonTemperatureNum");
                bmuConfigModel_gy.ModuleBMonTemperatureNum = moduleBMonTemperatureNum;
            }
        }
        public string ModuleBMonTemperatureNum_rec
        {
            get
            {
                return moduleBMonTemperatureNum_rec;
            }

            set
            {
                moduleBMonTemperatureNum_rec = value;
                OnPropertyChanged("ModuleBMonTemperatureNum_rec");
            }
        }
        public string ModuleCMonCellNum
        {
            get { return bmuConfigModel_gy.ModuleCMonCellNum; }
            set
            {
                moduleCMonCellNum = value;
                OnPropertyChanged("ModuleCMonCellNum");
                bmuConfigModel_gy.ModuleCMonCellNum = moduleCMonCellNum;
            }
        }
        public string ModuleCMonCellNum_rec
        {
            get
            {
                return moduleCMonCellNum_rec;
            }

            set
            {
                moduleCMonCellNum_rec = value;
                OnPropertyChanged("ModuleCMonCellNum_rec");
            }
        }
        public string ModuleCMonTemperatureNum
        {
            get { return bmuConfigModel_gy.ModuleCMonTemperatureNum; }
            set
            {
                moduleCMonTemperatureNum = value;
                OnPropertyChanged("ModuleCMonTemperatureNum");
                bmuConfigModel_gy.ModuleCMonTemperatureNum = moduleCMonTemperatureNum;
            }
        }
        public string ModuleCMonTemperatureNum_rec
        {
            get
            {
                return moduleCMonTemperatureNum_rec;
            }

            set
            {
                moduleCMonTemperatureNum_rec = value;
                OnPropertyChanged("ModuleCMonTemperatureNum_rec");
            }
        }
        public string ModuleDMonCellNum
        {
            get { return bmuConfigModel_gy.ModuleDMonCellNum; }
            set
            {
                moduleDMonCellNum = value;
                OnPropertyChanged("ModuleDMonCellNum");
                bmuConfigModel_gy.ModuleDMonCellNum = moduleDMonCellNum;
            }
        }
        public string ModuleDMonCellNum_rec
        {
            get
            {
                return moduleDMonCellNum_rec;
            }

            set
            {
                moduleDMonCellNum_rec = value;
                OnPropertyChanged("ModuleDMonCellNum_rec");
            }
        }
        public string ModuleDMonTemperatureNum
        {
            get { return bmuConfigModel_gy.ModuleDMonTemperatureNum; }
            set
            {
                moduleDMonTemperatureNum = value;
                OnPropertyChanged("ModuleDMonTemperatureNum");
                bmuConfigModel_gy.ModuleDMonTemperatureNum = moduleDMonTemperatureNum;
            }
        }
        public string ModuleDMonTemperatureNum_rec
        {
            get
            {
                return moduleDMonTemperatureNum_rec;
            }

            set
            {
                moduleDMonTemperatureNum_rec = value;
                OnPropertyChanged("ModuleDMonTemperatureNum_rec");
            }
        }

        //info19
        private string moduleEMonCellNum;//子模块E监控电池数目
        private string moduleEMonTemperatureNum;//子模块E监控温感数目
        private string packProYear;//电池组生产年份
        private string packProMonth;//电池组生产月份
        private string packProDay;//电池组生产日期

        private string moduleEMonCellNum_rec;//子模块E监控电池数目
        private string moduleEMonTemperatureNum_rec;//子模块E监控温感数目
        private string packProYear_rec;//电池组生产年份
        private string packProMonth_rec;//电池组生产月份
        private string packProDay_rec;//电池组生产日期

        public string ModuleEMonCellNum
        {
            get { return bmuConfigModel_gy.ModuleEMonCellNum; }
            set
            {
                moduleEMonCellNum = value;
                OnPropertyChanged("ModuleEMonCellNum");
                bmuConfigModel_gy.ModuleEMonCellNum = moduleEMonCellNum;
            }
        }
        public string ModuleEMonCellNum_rec
        {
            get
            {
                return moduleEMonCellNum_rec;
            }

            set
            {
                moduleEMonCellNum_rec = value;
                OnPropertyChanged("ModuleEMonCellNum_rec");
            }
        }
        public string ModuleEMonTemperatureNum
        {
            get { return bmuConfigModel_gy.ModuleEMonTemperatureNum; }
            set
            {
                moduleEMonTemperatureNum = value;
                OnPropertyChanged("ModuleEMonTemperatureNum");
                bmuConfigModel_gy.ModuleEMonTemperatureNum = moduleEMonTemperatureNum;
            }
        }
        public string ModuleEMonTemperatureNum_rec
        {
            get
            {
                return moduleEMonTemperatureNum_rec;
            }

            set
            {
                moduleEMonTemperatureNum_rec = value;
                OnPropertyChanged("ModuleEMonTemperatureNum_rec");
            }
        }
        public string PackProYear
        {
            get { return bmuConfigModel_gy.PackProYear; }
            set
            {
                packProYear = value;
                OnPropertyChanged("PackProYear");
                bmuConfigModel_gy.PackProYear = packProYear;
            }
        }
        public string PackProYear_rec
        {
            get
            {
                return packProYear_rec;
            }

            set
            {
                packProYear_rec = value;
                OnPropertyChanged("PackProYear_rec");
            }
        }
        public string PackProMonth
        {
            get { return bmuConfigModel_gy.PackProMonth; }
            set
            {
                packProMonth = value;
                OnPropertyChanged("PackProMonth");
                bmuConfigModel_gy.PackProMonth = packProMonth;
            }
        }
        public string PackProMonth_rec
        {
            get
            {
                return packProMonth_rec;
            }

            set
            {
                packProMonth_rec = value;
                OnPropertyChanged("PackProMonth_rec");
            }
        }
        public string PackProDay
        {
            get { return bmuConfigModel_gy.PackProDay; }
            set
            {
                packProDay = value;
                OnPropertyChanged("PackProDay");
                bmuConfigModel_gy.PackProDay = packProDay;
            }
        }
        public string PackProDay_rec
        {
            get
            {
                return packProDay_rec;
            }

            set
            {
                packProDay_rec = value;
                OnPropertyChanged("PackProDay_rec");
            }
        }


        //info20
        private string serialNum;      
        private string packBatchNumberData1;//电池组项目批量号1
        private string packBatchNumberData2;//电池组项目批量号2
        private string packBatchNumberData3;//电池组项目批量号3
        private string packBatchNumberData4;//电池组项目批量号4
        private string packBatchNumberData5;//电池组项目批量号5
        private string packBatchNumberData6;//电池组项目批量号6

        private string serialNum_rec;

        
        private string packBatchNumberData1_rec;//电池组项目批量号1
        private string packBatchNumberData2_rec;//电池组项目批量号2
        private string packBatchNumberData3_rec;//电池组项目批量号3
        private string packBatchNumberData4_rec;//电池组项目批量号4
        private string packBatchNumberData5_rec;//电池组项目批量号5
        private string packBatchNumberData6_rec;//电池组项目批量号6

        public string SerialNum
        {
            get { return bmuConfigModel_gy.SerialNum; }
            set
            {
                serialNum = value;
                OnPropertyChanged("PackBatchNumberData1");
                bmuConfigModel_gy.SerialNum = serialNum;
            }
        }
        public string SerialNum_rec
        {
            get { return serialNum_rec; }
            set { serialNum_rec = value; OnPropertyChanged("SerialNum_rec"); }
        }
        public string PackBatchNumberData1
        {
            get { return bmuConfigModel_gy.PackBatchNumberData1; }
            set
            {
                packBatchNumberData1 = value;
                OnPropertyChanged("PackBatchNumberData1");
                bmuConfigModel_gy.PackBatchNumberData1 = packBatchNumberData1;
            }
        }
        public string PackBatchNumberData1_rec
        {
            get
            {
                return packBatchNumberData1_rec;
            }

            set
            {
                packBatchNumberData1_rec = value;
                OnPropertyChanged("PackBatchNumberData1_rec");
            }
        }
        public string PackBatchNumberData2
        {
            get { return bmuConfigModel_gy.PackBatchNumberData2; }
            set
            {
                packBatchNumberData2 = value;
                OnPropertyChanged("PackBatchNumberData2");
                bmuConfigModel_gy.PackBatchNumberData2 = packBatchNumberData2;
            }
        }
        public string PackBatchNumberData2_rec
        {
            get
            {
                return packBatchNumberData2_rec;
            }

            set
            {
                packBatchNumberData2_rec = value;
                OnPropertyChanged("PackBatchNumberData2_rec");
            }
        }
        public string PackBatchNumberData3
        {
            get { return bmuConfigModel_gy.PackBatchNumberData3; }
            set
            {
                packBatchNumberData3 = value;
                OnPropertyChanged("PackBatchNumberData3");
                bmuConfigModel_gy.PackBatchNumberData3 = packBatchNumberData3;
            }
        }
        public string PackBatchNumberData3_rec
        {
            get
            {
                return packBatchNumberData3_rec;
            }

            set
            {
                packBatchNumberData3_rec = value;
                OnPropertyChanged("PackBatchNumberData3_rec");
            }
        }
        public string PackBatchNumberData4
        {
            get { return bmuConfigModel_gy.PackBatchNumberData4; }
            set
            {
                packBatchNumberData4 = value;
                OnPropertyChanged("PackBatchNumberData4");
                bmuConfigModel_gy.PackBatchNumberData4 = packBatchNumberData4;
            }
        }
        public string PackBatchNumberData4_rec
        {
            get
            {
                return packBatchNumberData4_rec;
            }

            set
            {
                packBatchNumberData4_rec = value;
                OnPropertyChanged("PackBatchNumberData4_rec");
            }
        }
        public string PackBatchNumberData5
        {
            get { return bmuConfigModel_gy.PackBatchNumberData5; }
            set
            {
                packBatchNumberData5 = value;
                OnPropertyChanged("PackBatchNumberData5");
                bmuConfigModel_gy.PackBatchNumberData5 = packBatchNumberData5;
            }
        }
        public string PackBatchNumberData5_rec
        {
            get
            {
                return packBatchNumberData5_rec;
            }

            set
            {
                packBatchNumberData5_rec = value;
                OnPropertyChanged("PackBatchNumberData5_rec");
            }
        }
        public string PackBatchNumberData6
        {
            get { return bmuConfigModel_gy.PackBatchNumberData6; }
            set
            {
                packBatchNumberData6 = value;
                OnPropertyChanged("PackBatchNumberData6");
                bmuConfigModel_gy.PackBatchNumberData6 = packBatchNumberData6;
            }
        }
        public string PackBatchNumberData6_rec
        {
            get
            {
                return packBatchNumberData6_rec;
            }

            set
            {
                packBatchNumberData6_rec = value;
                OnPropertyChanged("PackBatchNumberData6_rec");
            }
        }


        private BMUConfigModel_gy bmuConfigModel_gy;

        public BMUConfigModel_gy BmuConfigModel_gy
        {
            get { return bmuConfigModel_gy; }
            set
            {
                bmuConfigModel_gy = value;
                OnPropertyChanged("BmuConfigModel_gy");
                // CellVolHighAlarmFirst=bmuConfigModel_gy.CellVolHighAlarmFirst;
            }
        }
        private BMUConfigModel_gy bmuConfigModel_gy_rec;

        public BMUConfigModel_gy BmuConfigModel_gy_rec
        {
            get { return bmuConfigModel_gy_rec; }
            set
            {
                bmuConfigModel_gy_rec = value; OnPropertyChanged("BmuConfigModel_gy_rec");
                //cellVolHighAlarmFirst_rec = bmuConfigModel_gy_rec.CellVolHighAlarmFirst;
                //cellVolHighAlarmSecond_rec = bmuConfigModel_gy_rec.CellVolHighAlarmSecond;
                //cellVolHighAlarmThird_rec = bmuConfigModel_gy_rec.CellVolHighAlarmThird;
            }
        }

        

        public BMUConfigViewModel()
        {
            bmuConfigModel_gy = new BMUConfigModel_gy();
            bmuConfigModel_gy_rec = new BMUConfigModel_gy();
            balanceModeListInBMU.Add("不均衡");
            balanceModeListInBMU.Add("主动均衡");
            balanceModeListInBMU.Add("被动均衡");
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
