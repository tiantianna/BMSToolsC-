using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

///配置参数项
namespace bms.startup.Model
{
    [Serializable]
    public class BMUConfigModel_gy : INotifyPropertyChanged
    {
        //从机编号
        private string slave;

        public string Slave
        {
            get { return slave; }
            set { slave = value; }
        }

        //标志位，记录各info是否需要发送
        private bool isChecked1;
        private bool isChecked2;
        private bool isChecked3;
        private bool isChecked4;
        private bool isChecked5;
        private bool isChecked6;
        private bool isChecked7;
        private bool isChecked8;
        private bool isChecked9;
        private bool isChecked10;
        private bool isChecked11;
        private bool isChecked12;
        private bool isChecked13;
        private bool isChecked14;
        private bool isChecked15;
        private bool isChecked16;
        private bool isChecked17;
        private bool isChecked18;
        private bool isChecked19;
        private bool isChecked20;

        public bool IsChecked1
        {
            get { return isChecked1; }
            set { isChecked1 = value; OnPropertyChanged("IsChecked1"); }
        }

        public bool IsChecked2
        {
            get { return isChecked2; }
            set { isChecked2 = value; OnPropertyChanged("IsChecked2"); }
        }

        public bool IsChecked3
        {
            get { return isChecked3; }
            set { isChecked3 = value; OnPropertyChanged("IsChecked3"); }
        }

        public bool IsChecked4
        {
            get { return isChecked4; }
            set { isChecked4 = value; OnPropertyChanged("IsChecked4"); }
        }

        public bool IsChecked5
        {
            get { return isChecked5; }
            set { isChecked5 = value; OnPropertyChanged("IsChecked5"); }
        }

        public bool IsChecked6
        {
            get { return isChecked6; }
            set { isChecked6 = value; OnPropertyChanged("IsChecked6"); }
        }

        public bool IsChecked7
        {
            get { return isChecked7; }
            set { isChecked7 = value; OnPropertyChanged("IsChecked7"); }
        }

        public bool IsChecked8
        {
            get { return isChecked8; }
            set { isChecked8 = value; OnPropertyChanged("IsChecked8"); }
        }

        public bool IsChecked9
        {
            get { return isChecked9; }
            set { isChecked9 = value; OnPropertyChanged("IsChecked9"); }
        }

        public bool IsChecked10
        {
            get { return isChecked10; }
            set { isChecked10 = value; OnPropertyChanged("IsChecked10"); }
        }

        public bool IsChecked11
        {
            get { return isChecked11; }
            set { isChecked11 = value; OnPropertyChanged("IsChecked11"); }
        }

        public bool IsChecked12
        {
            get { return isChecked12; }
            set { isChecked12 = value; OnPropertyChanged("IsChecked12"); }
        }

        public bool IsChecked13
        {
            get { return isChecked13; }
            set { isChecked13 = value; OnPropertyChanged("IsChecked13"); }
        }

        public bool IsChecked14
        {
            get { return isChecked14; }
            set { isChecked14 = value; OnPropertyChanged("IsChecked14"); }
        }

        public bool IsChecked15
        {
            get { return isChecked15; }
            set { isChecked15 = value; OnPropertyChanged("IsChecked15"); }
        }

        public bool IsChecked16
        {
            get { return isChecked16; }
            set { isChecked16 = value; OnPropertyChanged("IsChecked16"); }
        }

        public bool IsChecked17
        {
            get { return isChecked17; }
            set { isChecked17 = value; OnPropertyChanged("IsChecked17"); }
        }

        public bool IsChecked18
        {
            get { return isChecked18; }
            set { isChecked18 = value; OnPropertyChanged("IsChecked18"); }
        }

        public bool IsChecked19
        {
            get { return isChecked19; }
            set { isChecked19 = value; OnPropertyChanged("IsChecked19"); }
        }

        public bool IsChecked20
        {
            get { return isChecked20; }
            set { isChecked20 = value; OnPropertyChanged("IsChecked20"); }
        }

        //info1
        private string cellVolHighAlarmFirst = DefaultValue.cellVolHighAlarmFirst_dv;//单体过高一级
        private string cellVolHighAlarmSecond = DefaultValue.cellVolHighAlarmSecond_dv;//单体过高二级
        private string cellVolHighAlarmThird = DefaultValue.cellVolHighAlarmThird_dv;//单体过高三级

        private string cellVolHighAlarmFirst_rec;//单体过高一级
        private string cellVolHighAlarmSecond_rec;//单体过高二级
        private string cellVolHighAlarmThird_rec;//单体过高三级

        public string CellVolHighAlarmFirst_rec
        {
            get { return cellVolHighAlarmFirst_rec; }
            set { cellVolHighAlarmFirst_rec = value; OnPropertyChanged("cellVolHighAlarmFirst_rec"); }
        }

        public string CellVolHighAlarmSecond_rec
        {
            get { return cellVolHighAlarmSecond_rec; }
            set { cellVolHighAlarmSecond_rec = value; OnPropertyChanged("CellVolHighAlarmSecond_rec"); }
        }

        public string CellVolHighAlarmThird_rec
        {
            get { return cellVolHighAlarmThird_rec; }
            set { cellVolHighAlarmThird_rec = value; OnPropertyChanged("CellVolHighAlarmThird_rec"); }
        }

        public string CellVolHighAlarmFirst
        {
            get { return cellVolHighAlarmFirst; }
            set { cellVolHighAlarmFirst = value; OnPropertyChanged("CellVolHighAlarmFirst"); }
        }
        public string CellVolHighAlarmSecond
        {
            get { return cellVolHighAlarmSecond; }
            set { cellVolHighAlarmSecond = value; OnPropertyChanged("CellVolHighAlarmSecond"); }
        }
        public string CellVolHighAlarmThird
        {
            get { return cellVolHighAlarmThird; }
            set { cellVolHighAlarmThird = value; OnPropertyChanged("CellVolHighAlarmThird"); }
        }
        //info2
        private string cellVolHighAlarmRemoveFirst = DefaultValue.cellVolHighAlarmRemoveFirst_dv;//单体过高解除一级
        private string cellVolHighAlarmRemoveSecond = DefaultValue.cellVolHighAlarmRemoveSecond_dv;//单体过高解除二级
        private string cellVolHighAlarmRemoveThird = DefaultValue.cellVolHighAlarmRemoveThird_dv;//单体过高解除三级

        private string cellVolHighAlarmRemoveFirst_rec;//单体过高解除一级
        private string cellVolHighAlarmRemoveSecond_rec;//单体过高解除二级
        private string cellVolHighAlarmRemoveThird_rec;//单体过高解除三级

        public string CellVolHighAlarmRemoveFirst_rec
        {
            get { return cellVolHighAlarmRemoveFirst_rec; }
            set { cellVolHighAlarmRemoveFirst_rec = value; OnPropertyChanged("CellVolHighAlarmRemoveFirst_rec"); }
        }

        public string CellVolHighAlarmRemoveSecond_rec
        {
            get { return cellVolHighAlarmRemoveSecond_rec; }
            set { cellVolHighAlarmRemoveSecond_rec = value; OnPropertyChanged("CellVolHighAlarmRemoveSecond_rec"); }
        }

        public string CellVolHighAlarmRemoveThird_rec
        {
            get { return cellVolHighAlarmRemoveThird_rec; }
            set { cellVolHighAlarmRemoveThird_rec = value; OnPropertyChanged("CellVolHighAlarmRemoveThird_rec"); }
        }

        public string CellVolHighAlarmRemoveFirst
        {
            get { return cellVolHighAlarmRemoveFirst; }
            set { cellVolHighAlarmRemoveFirst = value; OnPropertyChanged("CellVolHighAlarmRemoveFirst"); }
        }

        public string CellVolHighAlarmRemoveSecond
        {
            get { return cellVolHighAlarmRemoveSecond; }
            set { cellVolHighAlarmRemoveSecond = value; OnPropertyChanged("CellVolHighAlarmRemoveSecond"); }
        }

        public string CellVolHighAlarmRemoveThird
        {
            get { return cellVolHighAlarmRemoveThird; }
            set { cellVolHighAlarmRemoveThird = value; OnPropertyChanged("CellVolHighAlarmRemoveThird"); }
        }
        //info3
        private string cellVolLowAlarmFirst = DefaultValue.cellVolLowAlarmFirst_dv;//单体过低一级
        private string cellVolLowAlarmSecond = DefaultValue.cellVolLowAlarmSecond_dv;//单体过低二级
        private string cellVolLowAlarmThird = DefaultValue.cellVolLowAlarmThird_dv;//单体过低三级

        private string cellVolLowAlarmFirst_rec;//单体过低一级
        private string cellVolLowAlarmSecond_rec;//单体过低二级
        private string cellVolLowAlarmThird_rec;//单体过低三级

        public string CellVolLowAlarmFirst_rec
        {
            get { return cellVolLowAlarmFirst_rec; }
            set { cellVolLowAlarmFirst_rec = value; OnPropertyChanged("CellVolLowAlarmFirst_rec"); }
        }

        public string CellVolLowAlarmSecond_rec
        {
            get { return cellVolLowAlarmSecond_rec; }
            set { cellVolLowAlarmSecond_rec = value; OnPropertyChanged("CellVolLowAlarmSecond_rec"); }
        }


        public string CellVolLowAlarmThird_rec
        {
            get { return cellVolLowAlarmThird_rec; }
            set { cellVolLowAlarmThird_rec = value; OnPropertyChanged("CellVolLowAlarmThird_rec"); }
        }

        public string CellVolLowAlarmFirst
        {
            get { return cellVolLowAlarmFirst; }
            set { cellVolLowAlarmFirst = value; OnPropertyChanged("CellVolLowAlarmFirst"); }
        }

        public string CellVolLowAlarmSecond
        {
            get { return cellVolLowAlarmSecond; }
            set { cellVolLowAlarmSecond = value; OnPropertyChanged("CellVolLowAlarmSecond"); }
        }

        public string CellVolLowAlarmThird
        {
            get { return cellVolLowAlarmThird; }
            set { cellVolLowAlarmThird = value; OnPropertyChanged("CellVolLowAlarmThird"); }
        }
        //info4
        private string cellVolLowAlarmRemoveFirst = DefaultValue.cellVolLowAlarmRemoveFirst_dv;//单体过低解除一级
        private string cellVolLowAlarmRemoveSecond = DefaultValue.cellVolLowAlarmRemoveSecond_dv;//单体过低解除二级
        private string cellVolLowAlarmRemoveThird = DefaultValue.cellVolLowAlarmRemoveThird_dv;//单体过低解除三级

        private string cellVolLowAlarmRemoveFirst_rec;//单体过低解除一级
        private string cellVolLowAlarmRemoveSecond_rec;//单体过低解除二级
        private string cellVolLowAlarmRemoveThird_rec;//单体过低解除三级

        public string CellVolLowAlarmRemoveFirst_rec
        {
            get { return cellVolLowAlarmRemoveFirst_rec; }
            set { cellVolLowAlarmRemoveFirst_rec = value; OnPropertyChanged("CellVolLowAlarmRemoveFirst_rec"); }
        }

        public string CellVolLowAlarmRemoveSecond_rec
        {
            get { return cellVolLowAlarmRemoveSecond_rec; }
            set { cellVolLowAlarmRemoveSecond_rec = value; OnPropertyChanged("CellVolLowAlarmRemoveSecond_rec"); }
        }


        public string CellVolLowAlarmRemoveThird_rec
        {
            get { return cellVolLowAlarmRemoveThird_rec; }
            set { cellVolLowAlarmRemoveThird_rec = value; OnPropertyChanged("CellVolLowAlarmRemoveThird_rec"); }
        }

        public string CellVolLowAlarmRemoveFirst
        {
            get { return cellVolLowAlarmRemoveFirst; }
            set { cellVolLowAlarmRemoveFirst = value; OnPropertyChanged("CellVolLowAlarmRemoveFirst"); }
        }

        public string CellVolLowAlarmRemoveSecond
        {
            get { return cellVolLowAlarmRemoveSecond; }
            set { cellVolLowAlarmRemoveSecond = value; OnPropertyChanged("CellVolLowAlarmRemoveSecond"); }
        }

        public string CellVolLowAlarmRemoveThird
        {
            get { return cellVolLowAlarmRemoveThird; }
            set { cellVolLowAlarmRemoveThird = value; OnPropertyChanged("CellVolLowAlarmRemoveThird"); }
        }
        //info5
        private string cellTemperatureHighAlarmFirst = DefaultValue.cellTemperatureHighAlarmFirst_dv;//温度过高一级
        private string cellTemperatureHighAlarmSecond = DefaultValue.cellTemperatureHighAlarmSecond_dv;//温度过高二级
        private string cellTemperatureHighAlarmThird = DefaultValue.cellTemperatureHighAlarmThird_dv;//温度过高三级
        private string cellTemperatureHighAlarmRemoveFirst = DefaultValue.cellTemperatureHighAlarmRemoveFirst_dv;//温度过高解除一级
        private string cellTemperatureHighAlarmRemoveSecond = DefaultValue.cellTemperatureHighAlarmRemoveSecond_dv;//温度过高解除二级
        private string cellTemperatureHighAlarmRemoveThird = DefaultValue.cellTemperatureHighAlarmRemoveThird_dv;//温度过高解除三级

        private string cellTemperatureHighAlarmFirst_rec;//温度过高一级
        private string cellTemperatureHighAlarmSecond_rec;//温度过高二级
        private string cellTemperatureHighAlarmThird_rec;//温度过高三级
        private string cellTemperatureHighAlarmRemoveFirst_rec;//温度过高解除一级
        private string cellTemperatureHighAlarmRemoveSecond_rec;//温度过高解除二级
        private string cellTemperatureHighAlarmRemoveThird_rec;//温度过高解除三级

        public string CellTemperatureHighAlarmFirst_rec
        {
            get { return cellTemperatureHighAlarmFirst_rec; }
            set { cellTemperatureHighAlarmFirst_rec = value; OnPropertyChanged("CellTemperatureHighAlarmFirst_rec"); }
        }

        public string CellTemperatureHighAlarmSecond_rec
        {
            get { return cellTemperatureHighAlarmSecond_rec; }
            set { cellTemperatureHighAlarmSecond_rec = value; OnPropertyChanged("CellTemperatureHighAlarmSecond_rec"); }
        }

        public string CellTemperatureHighAlarmThird_rec
        {
            get { return cellTemperatureHighAlarmThird_rec; }
            set { cellTemperatureHighAlarmThird_rec = value; OnPropertyChanged("CellTemperatureHighAlarmThird_rec"); }
        }

        public string CellTemperatureHighAlarmRemoveFirst_rec
        {
            get { return cellTemperatureHighAlarmRemoveFirst_rec; }
            set { cellTemperatureHighAlarmRemoveFirst_rec = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveFirst_rec"); }
        }

        public string CellTemperatureHighAlarmRemoveSecond_rec
        {
            get { return cellTemperatureHighAlarmRemoveSecond_rec; }
            set { cellTemperatureHighAlarmRemoveSecond_rec = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveSecond_rec"); }
        }


        public string CellTemperatureHighAlarmRemoveThird_rec
        {
            get { return cellTemperatureHighAlarmRemoveThird_rec; }
            set { cellTemperatureHighAlarmRemoveThird_rec = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveThird_rec"); }
        }

        public string CellTemperatureHighAlarmFirst
        {
            get { return cellTemperatureHighAlarmFirst; }
            set { cellTemperatureHighAlarmFirst = value; OnPropertyChanged("CellTemperatureHighAlarmFirst"); }
        }

        public string CellTemperatureHighAlarmSecond
        {
            get { return cellTemperatureHighAlarmSecond; }
            set { cellTemperatureHighAlarmSecond = value; OnPropertyChanged("CellTemperatureHighAlarmSecond"); }
        }

        public string CellTemperatureHighAlarmThird
        {
            get { return cellTemperatureHighAlarmThird; }
            set { cellTemperatureHighAlarmThird = value; OnPropertyChanged("CellTemperatureHighAlarmThird"); }
        }

        public string CellTemperatureHighAlarmRemoveFirst
        {
            get { return cellTemperatureHighAlarmRemoveFirst; }
            set { cellTemperatureHighAlarmRemoveFirst = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveFirst"); }
        }

        public string CellTemperatureHighAlarmRemoveSecond
        {
            get { return cellTemperatureHighAlarmRemoveSecond; }
            set { cellTemperatureHighAlarmRemoveSecond = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveSecond"); }
        }

        public string CellTemperatureHighAlarmRemoveThird
        {
            get { return cellTemperatureHighAlarmRemoveThird; }
            set { cellTemperatureHighAlarmRemoveThird = value; OnPropertyChanged("CellTemperatureHighAlarmRemoveThird"); }
        }


        //info6
        private string cellTemperatureLowAlarmFirst = DefaultValue.cellTemperatureLowAlarmFirst_dv;//温度过低一级
        private string cellTemperatureLowAlarmSecond = DefaultValue.cellTemperatureLowAlarmSecond_dv;//温度过低二级
        private string cellTemperatureLowAlarmThird = DefaultValue.cellTemperatureLowAlarmThird_dv;//温度过低三级
        private string cellTemperatureLowAlarmRemoveFirst = DefaultValue.cellTemperatureLowAlarmRemoveFirst_dv;//温度过低解除一级
        private string cellTemperatureLowAlarmRemoveSecond = DefaultValue.cellTemperatureLowAlarmRemoveSecond_dv;//温度过低解除二级
        private string cellTemperatureLowAlarmRemoveThird = DefaultValue.cellTemperatureLowAlarmRemoveThird_dv;//温度过低解除三级

        private string cellTemperatureLowAlarmFirst_rec;//温度过低一级
        private string cellTemperatureLowAlarmSecond_rec;//温度过低二级
        private string cellTemperatureLowAlarmThird_rec;//温度过低三级
        private string cellTemperatureLowAlarmRemoveFirst_rec;//温度过低解除一级
        private string cellTemperatureLowAlarmRemoveSecond_rec;//温度过低解除二级
        private string cellTemperatureLowAlarmRemoveThird_rec;//温度过低解除三级

        public string CellTemperatureLowAlarmFirst_rec
        {
            get { return cellTemperatureLowAlarmFirst_rec; }
            set { cellTemperatureLowAlarmFirst_rec = value; OnPropertyChanged("CellTemperatureLowAlarmFirst_rec"); }
        }

        public string CellTemperatureLowAlarmSecond_rec
        {
            get { return cellTemperatureLowAlarmSecond_rec; }
            set { cellTemperatureLowAlarmSecond_rec = value; OnPropertyChanged("CellTemperatureLowAlarmSecond_rec"); }
        }

        public string CellTemperatureLowAlarmThird_rec
        {
            get { return cellTemperatureLowAlarmThird_rec; }
            set { cellTemperatureLowAlarmThird_rec = value; OnPropertyChanged("CellTemperatureLowAlarmThird_rec"); }
        }

        public string CellTemperatureLowAlarmRemoveFirst_rec
        {
            get { return cellTemperatureLowAlarmRemoveFirst_rec; }
            set { cellTemperatureLowAlarmRemoveFirst_rec = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveFirst_rec"); }
        }

        public string CellTemperatureLowAlarmRemoveSecond_rec
        {
            get { return cellTemperatureLowAlarmRemoveSecond_rec; }
            set { cellTemperatureLowAlarmRemoveSecond_rec = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveSecond_rec"); }
        }


        public string CellTemperatureLowAlarmRemoveThird_rec
        {
            get { return cellTemperatureLowAlarmRemoveThird_rec; }
            set { cellTemperatureLowAlarmRemoveThird_rec = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveThird_rec"); }
        }

        public string CellTemperatureLowAlarmFirst
        {
            get { return cellTemperatureLowAlarmFirst; }
            set { cellTemperatureLowAlarmFirst = value; OnPropertyChanged("CellTemperatureLowAlarmFirst"); }
        }

        public string CellTemperatureLowAlarmSecond
        {
            get { return cellTemperatureLowAlarmSecond; }
            set { cellTemperatureLowAlarmSecond = value; OnPropertyChanged("CellTemperatureLowAlarmSecond"); }
        }

        public string CellTemperatureLowAlarmThird
        {
            get { return cellTemperatureLowAlarmThird; }
            set { cellTemperatureLowAlarmThird = value; OnPropertyChanged("CellTemperatureLowAlarmThird"); }
        }

        public string CellTemperatureLowAlarmRemoveFirst
        {
            get { return cellTemperatureLowAlarmRemoveFirst; }
            set { cellTemperatureLowAlarmRemoveFirst = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveFirst"); }
        }

        public string CellTemperatureLowAlarmRemoveSecond
        {
            get { return cellTemperatureLowAlarmRemoveSecond; }
            set { cellTemperatureLowAlarmRemoveSecond = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveSecond"); }
        }

        public string CellTemperatureLowAlarmRemoveThird
        {
            get { return cellTemperatureLowAlarmRemoveThird; }
            set { cellTemperatureLowAlarmRemoveThird = value; OnPropertyChanged("CellTemperatureLowAlarmRemoveThird"); }
        }


        //info7
        private string balanCurrentHighAlarmFirst = DefaultValue.balanCurrentHighAlarmFirst_dv;//均衡电流过高一级
        private string balanCurrentHighAlarmSecond = DefaultValue.balanCurrentHighAlarmSecond_dv;//均衡电流过高二级
        private string balanCurrentHighAlarmThird = DefaultValue.balanCurrentHighAlarmThird_dv;//均衡电流过高三级

        private string balanCurrentHighAlarmFirst_rec;//均衡电流过高一级
        private string balanCurrentHighAlarmSecond_rec;//均衡电流过高二级
        private string balanCurrentHighAlarmThird_rec;//均衡电流过高三级

        public string BalanCurrentHighAlarmFirst_rec
        {
            get { return balanCurrentHighAlarmFirst_rec; }
            set { balanCurrentHighAlarmFirst_rec = value; OnPropertyChanged("BalanCurrentHighAlarmFirst_rec"); }
        }

        public string BalanCurrentHighAlarmSecond_rec
        {
            get { return balanCurrentHighAlarmSecond_rec; }
            set { balanCurrentHighAlarmSecond_rec = value; OnPropertyChanged("BalanCurrentHighAlarmSecond_rec"); }
        }


        public string BalanCurrentHighAlarmThird_rec
        {
            get { return balanCurrentHighAlarmThird_rec; }
            set { balanCurrentHighAlarmThird_rec = value; OnPropertyChanged("BalanCurrentHighAlarmThird_rec"); }
        }

        public string BalanCurrentHighAlarmFirst
        {
            get { return balanCurrentHighAlarmFirst; }
            set { balanCurrentHighAlarmFirst = value; OnPropertyChanged("BalanCurrentHighAlarmFirst"); }
        }

        public string BalanCurrentHighAlarmSecond
        {
            get { return balanCurrentHighAlarmSecond; }
            set { balanCurrentHighAlarmSecond = value; OnPropertyChanged("BalanCurrentHighAlarmSecond"); }
        }

        public string BalanCurrentHighAlarmThird
        {
            get { return balanCurrentHighAlarmThird; }
            set { balanCurrentHighAlarmThird = value; OnPropertyChanged("BalanCurrentHighAlarmThird"); }
        }


        //info8
        private string balanCurrentHighAlarmRemoveFirst = DefaultValue.balanCurrentHighAlarmRemoveFirst_dv;//均衡电流过高解除一级
        private string balanCurrentHighAlarmRemoveSecond = DefaultValue.balanCurrentHighAlarmRemoveSecond_dv;//均衡电流过高解除二级
        private string balanCurrentHighAlarmRemoveThird = DefaultValue.balanCurrentHighAlarmRemoveThird_dv;//均衡电流过高解除三级

        private string balanCurrentHighAlarmRemoveFirst_rec;//均衡电流过高解除一级
        private string balanCurrentHighAlarmRemoveSecond_rec;//均衡电流过高解除二级
        private string balanCurrentHighAlarmRemoveThird_rec;//均衡电流过高解除三级

        public string BalanCurrentHighAlarmRemoveFirst_rec
        {
            get { return balanCurrentHighAlarmRemoveFirst_rec; }
            set { balanCurrentHighAlarmRemoveFirst_rec = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveFirst_rec"); }
        }

        public string BalanCurrentHighAlarmRemoveSecond_rec
        {
            get { return balanCurrentHighAlarmRemoveSecond_rec; }
            set { balanCurrentHighAlarmRemoveSecond_rec = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveSecond_rec"); }
        }

        public string BalanCurrentHighAlarmRemoveThird_rec
        {
            get { return balanCurrentHighAlarmRemoveThird_rec; }
            set { balanCurrentHighAlarmRemoveThird_rec = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveThird_rec"); }
        }

        public string BalanCurrentHighAlarmRemoveFirst
        {
            get { return balanCurrentHighAlarmRemoveFirst; }
            set { balanCurrentHighAlarmRemoveFirst = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveFirst"); }
        }

        public string BalanCurrentHighAlarmRemoveSecond
        {
            get { return balanCurrentHighAlarmRemoveSecond; }
            set { balanCurrentHighAlarmRemoveSecond = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveSecond"); }
        }

        public string BalanCurrentHighAlarmRemoveThird
        {
            get { return balanCurrentHighAlarmRemoveThird; }
            set { balanCurrentHighAlarmRemoveThird = value; OnPropertyChanged("BalanCurrentHighAlarmRemoveThird"); }
        }


        //info9
        private string balanCurrentLowAlarmFirst = DefaultValue.balanCurrentLowAlarmFirst_dv;//均衡电流过低一级
        private string balanCurrentLowAlarmSecond = DefaultValue.balanCurrentLowAlarmSecond_dv;//均衡电流过低二级
        private string balanCurrentLowAlarmThird = DefaultValue.balanCurrentLowAlarmThird_dv;//均衡电流过低三级

        private string balanCurrentLowAlarmFirst_rec;//均衡电流过低一级
        private string balanCurrentLowAlarmSecond_rec;//均衡电流过低二级
        private string balanCurrentLowAlarmThird_rec;//均衡电流过低三级

        public string BalanCurrentLowAlarmFirst_rec
        {
            get { return balanCurrentLowAlarmFirst_rec; }
            set { balanCurrentLowAlarmFirst_rec = value; OnPropertyChanged("BalanCurrentLowAlarmFirst_rec"); }
        }

        public string BalanCurrentLowAlarmSecond_rec
        {
            get { return balanCurrentLowAlarmSecond_rec; }
            set { balanCurrentLowAlarmSecond_rec = value; OnPropertyChanged("BalanCurrentLowAlarmSecond_rec"); }
        }


        public string BalanCurrentLowAlarmThird_rec
        {
            get { return balanCurrentLowAlarmThird_rec; }
            set { balanCurrentLowAlarmThird_rec = value; OnPropertyChanged("BalanCurrentLowAlarmThird_rec"); }
        }

        public string BalanCurrentLowAlarmFirst
        {
            get { return balanCurrentLowAlarmFirst; }
            set { balanCurrentLowAlarmFirst = value; OnPropertyChanged("BalanCurrentLowAlarmFirst"); }
        }

        public string BalanCurrentLowAlarmSecond
        {
            get { return balanCurrentLowAlarmSecond; }
            set { balanCurrentLowAlarmSecond = value; OnPropertyChanged("BalanCurrentLowAlarmSecond"); }
        }

        public string BalanCurrentLowAlarmThird
        {
            get { return balanCurrentLowAlarmThird; }
            set { balanCurrentLowAlarmThird = value; OnPropertyChanged("BalanCurrentLowAlarmThird"); }
        }



        //info10
        private string balanCurrentLowAlarmRemoveFirst = DefaultValue.balanCurrentLowAlarmRemoveFirst_dv;//均衡电流过低解除一级
        private string balanCurrentLowAlarmRemoveSecond = DefaultValue.balanCurrentLowAlarmRemoveSecond_dv;//均衡电流过低解除二级
        private string balanCurrentLowAlarmRemoveThird = DefaultValue.balanCurrentLowAlarmRemoveThird_dv;//均衡电流过低解除三级

        private string balanCurrentLowAlarmRemoveFirst_rec;//均衡电流过低解除一级
        private string balanCurrentLowAlarmRemoveSecond_rec;//均衡电流过低解除二级
        private string balanCurrentLowAlarmRemoveThird_rec;//均衡电流过低解除三级

        public string BalanCurrentLowAlarmRemoveFirst_rec
        {
            get { return balanCurrentLowAlarmRemoveFirst_rec; }
            set { balanCurrentLowAlarmRemoveFirst_rec = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveFirst_rec"); }
        }

        public string BalanCurrentLowAlarmRemoveSecond_rec
        {
            get { return balanCurrentLowAlarmRemoveSecond_rec; }
            set { balanCurrentLowAlarmRemoveSecond_rec = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveSecond_rec"); }
        }


        public string BalanCurrentLowAlarmRemoveThird_rec
        {
            get { return balanCurrentLowAlarmRemoveThird_rec; }
            set { balanCurrentLowAlarmRemoveThird_rec = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveThird_rec"); }
        }

        public string BalanCurrentLowAlarmRemoveFirst
        {
            get { return balanCurrentLowAlarmRemoveFirst; }
            set { balanCurrentLowAlarmRemoveFirst = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveFirst"); }
        }

        public string BalanCurrentLowAlarmRemoveSecond
        {
            get { return balanCurrentLowAlarmRemoveSecond; }
            set { balanCurrentLowAlarmRemoveSecond = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveSecond"); }
        }

        public string BalanCurrentLowAlarmRemoveThird
        {
            get { return balanCurrentLowAlarmRemoveThird; }
            set { balanCurrentLowAlarmRemoveThird = value; OnPropertyChanged("BalanCurrentLowAlarmRemoveThird"); }
        }



        //info11
        private string balanCurrentSetValueFirst = DefaultValue.balanCurrentSetValueFirst_dv;//均衡电流大小一级 
        private string balanCurrentSetValueSecond = DefaultValue.balanCurrentSetValueSecond_dv;//均衡电流大小二级
        private string balanCurrentSetValueThird = DefaultValue.balanCurrentSetValueThird_dv;//均衡电流大小三级

        private string balanCurrentSetValueFirst_rec;//均衡电流大小一级 
        private string balanCurrentSetValueSecond_rec;//均衡电流大小二级
        private string balanCurrentSetValueThird_rec;//均衡电流大小三级

        public string BalanCurrentSetValueFirst_rec
        {
            get { return balanCurrentSetValueFirst_rec; }
            set { balanCurrentSetValueFirst_rec = value; OnPropertyChanged("BalanCurrentSetValueFirst_rec"); }
        }

        public string BalanCurrentSetValueSecond_rec
        {
            get { return balanCurrentSetValueSecond_rec; }
            set { balanCurrentSetValueSecond_rec = value; OnPropertyChanged("BalanCurrentSetValueSecond_rec"); }
        }

        public string BalanCurrentSetValueThird_rec
        {
            get { return balanCurrentSetValueThird_rec; }
            set { balanCurrentSetValueThird_rec = value; OnPropertyChanged("BalanCurrentSetValueThird_rec"); }
        }

        public string BalanCurrentSetValueFirst
        {
            get { return balanCurrentSetValueFirst; }
            set { balanCurrentSetValueFirst = value; OnPropertyChanged("BalanCurrentSetValueFirst"); }
        }

        public string BalanCurrentSetValueSecond
        {
            get { return balanCurrentSetValueSecond; }
            set { balanCurrentSetValueSecond = value; OnPropertyChanged("BalanCurrentSetValueSecond"); }
        }

        public string BalanCurrentSetValueThird
        {
            get { return balanCurrentSetValueThird; }
            set { balanCurrentSetValueThird = value; OnPropertyChanged("BalanCurrentSetValueThird"); }
        }



        //info12
        private string balanVolOpenValue = DefaultValue.balanVolOpenValue_dv;//均衡开启电压

        private string balanVolOpenValue_rec;//均衡开启电压

        public string BalanVolOpenValue_rec
        {
            get { return balanVolOpenValue_rec; }
            set { balanVolOpenValue_rec = value; OnPropertyChanged("BalanVolOpenValue_rec"); }
        }

        public string BalanVolOpenValue
        {
            get { return balanVolOpenValue; }
            set { balanVolOpenValue = value; OnPropertyChanged("BalanVolOpenValue"); }
        }


        //info13
        private string balanVolCloseValue = DefaultValue.balanVolCloseValue_dv;//均衡截止电压

        private string balanVolCloseValue_rec;//均衡截止电压

        public string BalanVolCloseValue_rec
        {
            get { return balanVolCloseValue_rec; }
            set { balanVolCloseValue_rec = value; OnPropertyChanged("BalanVolCloseValue_rec"); }
        }

        public string BalanVolCloseValue
        {
            get { return balanVolCloseValue; }
            set { balanVolCloseValue = value; OnPropertyChanged("BalanVolCloseValue"); }
        }



        //info14
        private string balanVolDifOpenValue = DefaultValue.balanVolDifOpenValue_dv;//均衡开启压差

        private string balanVolDifOpenValue_rec;//均衡开启压差

        public string BalanVolDifOpenValue_rec
        {
            get { return balanVolDifOpenValue_rec; }
            set { balanVolDifOpenValue_rec = value; OnPropertyChanged("BalanVolDifOpenValue_rec"); }
        }

        public string BalanVolDifOpenValue
        {
            get { return balanVolDifOpenValue; }
            set { balanVolDifOpenValue = value; OnPropertyChanged("BalanVolDifOpenValue"); }
        }

        //info15
        private string balanVolDifCloseValue = DefaultValue.balanVolDifCloseValue_dv;//均衡截止压差

        private string balanVolDifCloseValue_rec;//均衡截止压差

        public string BalanVolDifCloseValue_rec
        {
            get { return balanVolDifCloseValue_rec; }
            set { balanVolDifCloseValue_rec = value; OnPropertyChanged("BalanVolDifCloseValue_rec"); }
        }

        public string BalanVolDifCloseValue
        {
            get { return balanVolDifCloseValue; }
            set { balanVolDifCloseValue = value; OnPropertyChanged("BalanVolDifCloseValue"); }
        }

        //info16
        private string cellBalanTemperatureOpenValue = DefaultValue.cellBalanTemperatureOpenValue_dv;//均衡电池开启温度
        private string cellBalanTemperatureCloseValue = DefaultValue.cellBalanTemperatureCloseValue_dv;//均衡电池截止温度

        private string cellBalanTemperatureOpenValue_rec;//均衡电池开启温度
        private string cellBalanTemperatureCloseValue_rec;//均衡电池截止温度

        public string CellBalanTemperatureOpenValue_rec
        {
            get { return cellBalanTemperatureOpenValue_rec; }
            set { cellBalanTemperatureOpenValue_rec = value; OnPropertyChanged("CellBalanTemperatureOpenValue_rec"); }
        }

        public string CellBalanTemperatureCloseValue_rec
        {
            get { return cellBalanTemperatureCloseValue_rec; }
            set { cellBalanTemperatureCloseValue_rec = value; OnPropertyChanged("CellBalanTemperatureCloseValue_rec"); }
        }

        public string CellBalanTemperatureOpenValue
        {
            get { return cellBalanTemperatureOpenValue; }
            set { cellBalanTemperatureOpenValue = value; OnPropertyChanged("CellBalanTemperatureOpenValue"); }
        }


        public string CellBalanTemperatureCloseValue
        {
            get { return cellBalanTemperatureCloseValue; }
            set { cellBalanTemperatureCloseValue = value; OnPropertyChanged("CellBalanTemperatureCloseValue"); }
        }


        //info17（单独配置）
        private string slaveNum;//从机编号
        private string cellBalanMode;//均衡模式
        private string childModuleMonCellNumber;//从机监控单体总数
        private string childMonModuleTemperatureNumber;//从机监控温感总数
        private string moduleAMonCellNum;//子模块A监控电池数目
        private string moduleAMonTemperatureNum;//子模块A监控温感数目
        private int? selectBalanceMode;

        private string slaveNum_rec;//从机编号
        private string cellBalanMode_rec;//均衡模式
        private string childModuleMonCellNumber_rec;//从机监控单体总数
        private string childMonModuleTemperatureNumber_rec;//从机监控温感总数
        private string moduleAMonCellNum_rec;//子模块A监控电池数目
        private string moduleAMonTemperatureNum_rec;//子模块A监控温感数目

        public int? SelectBalanceMode
        {
            get { return selectBalanceMode; }
            set { selectBalanceMode = value; OnPropertyChanged("SelectBalanceMode"); }
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

                //  int temp = Convert.ToInt32(CellBalanMode);

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

        public string SlaveNum
        {
            get { return slaveNum; }
            set { slaveNum = value; OnPropertyChanged("SlaveNum"); }
        }

        public string CellBalanMode
        {
            get
            {
                if (selectBalanceMode == null)
                {
                    return null;
                }
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
            set { cellBalanMode = value; OnPropertyChanged("CellBalanMode"); }
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

        //info18
        private string moduleBMonCellNum = DefaultValue.moduleBMonCellNum_dv;//子模块B监控电池数目
        private string moduleBMonTemperatureNum = DefaultValue.moduleBMonTemperatureNum_dv;//子模块B监控温感数目
        private string moduleCMonCellNum = DefaultValue.moduleCMonCellNum_dv;//子模块C监控电池数目
        private string moduleCMonTemperatureNum = DefaultValue.moduleCMonTemperatureNum_dv;//子模块C监控温感数目
        private string moduleDMonCellNum = DefaultValue.moduleDMonCellNum_dv;//子模块D监控电池数目
        private string moduleDMonTemperatureNum = DefaultValue.moduleDMonTemperatureNum_dv;//子模块D监控温感数目

        private string moduleBMonCellNum_rec;//子模块B监控电池数目
        private string moduleBMonTemperatureNum_rec;//子模块B监控温感数目
        private string moduleCMonCellNum_rec;//子模块C监控电池数目
        private string moduleCMonTemperatureNum_rec;//子模块C监控温感数目
        private string moduleDMonCellNum_rec;//子模块D监控电池数目
        private string moduleDMonTemperatureNum_rec;//子模块D监控温感数目

        public string ModuleBMonCellNum_rec
        {
            get { return moduleBMonCellNum_rec; }
            set { moduleBMonCellNum_rec = value; OnPropertyChanged("ModuleBMonCellNum_rec"); }
        }

        public string ModuleBMonTemperatureNum_rec
        {
            get { return moduleBMonTemperatureNum_rec; }
            set { moduleBMonTemperatureNum_rec = value; OnPropertyChanged("ModuleBMonTemperatureNum_rec"); }
        }

        public string ModuleCMonCellNum_rec
        {
            get { return moduleCMonCellNum_rec; }
            set { moduleCMonCellNum_rec = value; OnPropertyChanged("ModuleCMonCellNum_rec"); }
        }

        public string ModuleCMonTemperatureNum_rec
        {
            get { return moduleCMonTemperatureNum_rec; }
            set { moduleCMonTemperatureNum_rec = value; OnPropertyChanged("ModuleCMonTemperatureNum_rec"); }
        }

        public string ModuleDMonCellNum_rec
        {
            get { return moduleDMonCellNum_rec; }
            set { moduleDMonCellNum_rec = value; OnPropertyChanged("ModuleDMonCellNum_rec"); }
        }

        public string ModuleDMonTemperatureNum_rec
        {
            get { return moduleDMonTemperatureNum_rec; }
            set { moduleDMonTemperatureNum_rec = value; OnPropertyChanged("ModuleDMonTemperatureNum_rec"); }
        }

        public string ModuleBMonCellNum
        {
            get { return moduleBMonCellNum; }
            set { moduleBMonCellNum = value; OnPropertyChanged("ModuleBMonCellNum"); }
        }

        public string ModuleBMonTemperatureNum
        {
            get { return moduleBMonTemperatureNum; }
            set { moduleBMonTemperatureNum = value; OnPropertyChanged("ModuleBMonTemperatureNum"); }
        }

        public string ModuleCMonCellNum
        {
            get { return moduleCMonCellNum; }
            set { moduleCMonCellNum = value; OnPropertyChanged("ModuleCMonCellNum"); }
        }

        public string ModuleCMonTemperatureNum
        {
            get { return moduleCMonTemperatureNum; }
            set { moduleCMonTemperatureNum = value; OnPropertyChanged("ModuleCMonTemperatureNum"); }
        }

        public string ModuleDMonCellNum
        {
            get { return moduleDMonCellNum; }
            set { moduleDMonCellNum = value; OnPropertyChanged("ModuleDMonCellNum"); }
        }

        public string ModuleDMonTemperatureNum
        {
            get { return moduleDMonTemperatureNum; }
            set { moduleDMonTemperatureNum = value; OnPropertyChanged("ModuleDMonTemperatureNum"); }
        }



        //info19
        private string moduleEMonCellNum = DefaultValue.moduleEMonCellNum_dv;//子模块E监控电池数目
        private string moduleEMonTemperatureNum = DefaultValue.moduleEMonTemperatureNum_dv;//子模块E监控温感数目
        private string packProYear = DefaultValue.packProYear_dv;//电池组生产年份
        private string packProMonth = DefaultValue.packProMonth_dv;//电池组生产月份
        private string packProDay = DefaultValue.packProDay_dv;//电池组生产日期

        private string moduleEMonCellNum_rec;//子模块E监控电池数目
        private string moduleEMonTemperatureNum_rec;//子模块E监控温感数目
        private string packProYear_rec;//电池组生产年份
        private string packProMonth_rec;//电池组生产月份
        private string packProDay_rec;//电池组生产日期

        public string ModuleEMonCellNum_rec
        {
            get { return moduleEMonCellNum_rec; }
            set { moduleEMonCellNum_rec = value; OnPropertyChanged("ModuleEMonCellNum_rec"); }
        }

        public string ModuleEMonTemperatureNum_rec
        {
            get { return moduleEMonTemperatureNum_rec; }
            set { moduleEMonTemperatureNum_rec = value; OnPropertyChanged("ModuleEMonTemperatureNum_rec"); }
        }

        public string PackProYear_rec
        {
            get { return packProYear_rec; }
            set { packProYear_rec = value; OnPropertyChanged("PackProYear_rec"); }
        }

        public string PackProMonth_rec
        {
            get { return packProMonth_rec; }
            set { packProMonth_rec = value; OnPropertyChanged("PackProMonth_rec"); }
        }


        public string PackProDay_rec
        {
            get { return packProDay_rec; }
            set { packProDay_rec = value; OnPropertyChanged("PackProDay_rec"); }
        }

        public string ModuleEMonCellNum
        {
            get { return moduleEMonCellNum; }
            set { moduleEMonCellNum = value; OnPropertyChanged("ModuleEMonCellNum"); }
        }

        public string ModuleEMonTemperatureNum
        {
            get { return moduleEMonTemperatureNum; }
            set { moduleEMonTemperatureNum = value; OnPropertyChanged("ModuleEMonTemperatureNum"); }
        }

        public string PackProYear
        {
            get { return packProYear; }
            set { packProYear = value; OnPropertyChanged("PackProYear"); }
        }

        public string PackProMonth
        {
            get { return packProMonth; }
            set { packProMonth = value; OnPropertyChanged("PackProMonth"); }
        }

        public string PackProDay
        {
            get { return packProDay; }
            set { packProDay = value; OnPropertyChanged("PackProDay"); }
        }



        //info20
        private string serialNum = DefaultValue.serialNum_dv;
        private string packBatchNumberData1 = DefaultValue.packBatchNumberData1_dv;//电池组项目批量号1
        private string packBatchNumberData2 = DefaultValue.packBatchNumberData2_dv;//电池组项目批量号2
        private string packBatchNumberData3 = DefaultValue.packBatchNumberData3_dv;//电池组项目批量号3
        private string packBatchNumberData4 = DefaultValue.packBatchNumberData4_dv;//电池组项目批量号4
        private string packBatchNumberData5 = DefaultValue.packBatchNumberData5_dv;//电池组项目批量号5
        private string packBatchNumberData6 = DefaultValue.packBatchNumberData6_dv;//电池组项目批量号6

        private string serialNum_rec;
        private string packBatchNumberData1_rec;//电池组项目批量号1
        private string packBatchNumberData2_rec;//电池组项目批量号2
        private string packBatchNumberData3_rec;//电池组项目批量号3
        private string packBatchNumberData4_rec;//电池组项目批量号4
        private string packBatchNumberData5_rec;//电池组项目批量号5
        private string packBatchNumberData6_rec;//电池组项目批量号6

        public string SerialNum_rec
        {
            get { return serialNum_rec; }
            set
            {
                serialNum_rec = value;

                OnPropertyChanged("SerialNum_rec");
            }
        }
        public string PackBatchNumberData1_rec
        {
            get { return packBatchNumberData1_rec; }
            set { packBatchNumberData1_rec = value; OnPropertyChanged("PackBatchNumberData1_rec"); }
        }

        public string PackBatchNumberData2_rec
        {
            get { return packBatchNumberData2_rec; }
            set { packBatchNumberData2_rec = value; OnPropertyChanged("PackBatchNumberData2_rec"); }
        }

        public string PackBatchNumberData3_rec
        {
            get { return packBatchNumberData3_rec; }
            set { packBatchNumberData3_rec = value; OnPropertyChanged("PackBatchNumberData3_rec"); }
        }

        public string PackBatchNumberData4_rec
        {
            get { return packBatchNumberData4_rec; }
            set { packBatchNumberData4_rec = value; OnPropertyChanged("PackBatchNumberData4_rec"); }
        }

        public string PackBatchNumberData5_rec
        {
            get { return packBatchNumberData5_rec; }
            set { packBatchNumberData5_rec = value; OnPropertyChanged("PackBatchNumberData5_rec"); }
        }


        public string PackBatchNumberData6_rec
        {
            get { return packBatchNumberData6_rec; }
            set { packBatchNumberData6_rec = value; OnPropertyChanged("PackBatchNumberData6_rec"); }
        }

        public string SerialNum
        {
            get { return serialNum; }
            set
            {
                serialNum = value;
                //char[] c = value.ToCharArray();
                //PackBatchNumberData1 = Convert.ToInt32(c[0].ToString(),16).ToString();
                //PackBatchNumberData2 = Convert.ToInt32(c[1].ToString(),16).ToString();
                //PackBatchNumberData3 = Convert.ToInt32(c[2].ToString(),16).ToString();
                //PackBatchNumberData4 = Convert.ToInt32(c[3].ToString(),16).ToString();
                //PackBatchNumberData5 = Convert.ToInt32(c[4].ToString(),16).ToString();
                //PackBatchNumberData6 = Convert.ToInt32(c[5].ToString(),16).ToString();
                OnPropertyChanged("SerialNum");
            }
        }
        public string PackBatchNumberData1
        {
            get { return packBatchNumberData1; }
            set { packBatchNumberData1 = value; OnPropertyChanged("PackBatchNumberData1"); }
        }

        public string PackBatchNumberData2
        {
            get { return packBatchNumberData2; }
            set { packBatchNumberData2 = value; OnPropertyChanged("PackBatchNumberData2"); }
        }

        public string PackBatchNumberData3
        {
            get { return packBatchNumberData3; }
            set { packBatchNumberData3 = value; OnPropertyChanged("PackBatchNumberData3"); }
        }

        public string PackBatchNumberData4
        {
            get { return packBatchNumberData4; }
            set { packBatchNumberData4 = value; OnPropertyChanged("PackBatchNumberData4"); }
        }

        public string PackBatchNumberData5
        {
            get { return packBatchNumberData5; }
            set { packBatchNumberData5 = value; OnPropertyChanged("PackBatchNumberData5"); }
        }

        public string PackBatchNumberData6
        {
            get { return packBatchNumberData6; }
            set { packBatchNumberData6 = value; OnPropertyChanged("PackBatchNumberData6"); }
        }

        //判断info17是否和其他属性同时选中，true表示同时选中
        public bool if17andOther()
        {
            if (IsChecked17)
            {
                if (IsChecked1 || IsChecked2 || IsChecked3 || IsChecked4 || IsChecked5 || IsChecked6 || IsChecked7 || IsChecked8 || IsChecked9 || IsChecked10 ||
                    IsChecked11 || IsChecked12 || IsChecked13 || IsChecked14 || IsChecked15 || IsChecked16 || IsChecked18 || IsChecked19 || IsChecked20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {

                return false;
            }
        }

        //返回是否存在未填的空值，true表示存在
        //参数b表示是否需要对Slave进行判断，true表示需要
        public int ifHasNull(bool b)
        {
            if (b)
            {
                if (Slave == "" || Slave == null) { return 0; }
            }
            if (IsChecked1) { if (CellVolHighAlarmFirst == "" || CellVolHighAlarmSecond == "" || CellVolHighAlarmThird == "") { return 1; } }
            if (IsChecked2) { if (CellVolHighAlarmRemoveFirst == "" || CellVolHighAlarmRemoveSecond == "" || CellVolHighAlarmRemoveThird == "") { return 2; } }
            if (IsChecked3) { if (CellVolLowAlarmFirst == "" || CellVolLowAlarmSecond == "" || CellVolLowAlarmThird == "") { return 3; } }
            if (IsChecked4) { if (CellVolLowAlarmRemoveFirst == "" || CellVolLowAlarmRemoveSecond == "" || CellVolLowAlarmRemoveThird == "") { return 4; } }
            if (IsChecked5)
            {
                if (CellTemperatureHighAlarmFirst == "" || CellTemperatureHighAlarmSecond == "" || CellTemperatureHighAlarmThird == "" || CellTemperatureHighAlarmRemoveFirst == ""
                    || CellTemperatureHighAlarmRemoveSecond == "" || CellTemperatureHighAlarmRemoveThird == "") { return 5; }
            }
            if (IsChecked6)
            {
                if (CellTemperatureLowAlarmFirst == "" || CellTemperatureLowAlarmSecond == ""
                    || CellTemperatureLowAlarmThird == "" || CellTemperatureLowAlarmRemoveFirst == "" || CellTemperatureLowAlarmRemoveSecond == "" || CellTemperatureLowAlarmRemoveThird == "") { return 6; }
            }
            if (IsChecked7) { if (BalanCurrentHighAlarmFirst == "" || BalanCurrentHighAlarmSecond == "" || BalanCurrentHighAlarmThird == "") { return 7; } }
            if (IsChecked8) { if (BalanCurrentHighAlarmRemoveFirst == "" || BalanCurrentHighAlarmRemoveSecond == "" || BalanCurrentHighAlarmRemoveThird == "") { return 8; } }
            if (IsChecked9) { if (BalanCurrentLowAlarmFirst == "" || BalanCurrentLowAlarmSecond == "" || BalanCurrentLowAlarmThird == "") { return 9; } }
            if (IsChecked10) { if (BalanCurrentLowAlarmRemoveFirst == "" || BalanCurrentLowAlarmRemoveSecond == "" || BalanCurrentLowAlarmRemoveThird == "") { return 10; } }
            if (IsChecked11) { if (BalanCurrentSetValueFirst == "" || BalanCurrentSetValueSecond == "" || BalanCurrentSetValueThird == "") { return 11; } }
            if (IsChecked12) { if (BalanVolOpenValue == "") { return 12; } }
            if (IsChecked13) { if (BalanVolCloseValue == "") { return 13; } }
            if (IsChecked14) { if (BalanVolDifOpenValue == "") { return 14; } }
            if (IsChecked15) { if (BalanVolDifCloseValue == "") { return 15; } }
            if (IsChecked16) { if (CellBalanTemperatureOpenValue == "" || CellBalanTemperatureCloseValue == "") { return 16; } }
            if (IsChecked18)
            {
                if (ModuleBMonCellNum == "" || ModuleBMonTemperatureNum == "" || ModuleCMonCellNum == "" || ModuleCMonTemperatureNum == "" ||
                    ModuleDMonCellNum == "" || ModuleDMonTemperatureNum == "") { return 18; }
            }
            if (IsChecked19) { if (ModuleEMonCellNum == "" || ModuleEMonTemperatureNum == "" || PackProYear == "" || PackProMonth == "" || PackProDay == "") { return 19; } }
            if (IsChecked20)
            {
                if (SerialNum == "") { return 20; }
                //if (PackBatchNumberData1 == "" || PackBatchNumberData2 == "" || PackBatchNumberData3 == ""
                //    || PackBatchNumberData4 == "" || PackBatchNumberData5 == "" || PackBatchNumberData6 == "") { return 20; }
            }
            return -1;
        }

        //全选
        public void setAllTrue()
        {
            IsChecked1 = IsChecked2 = IsChecked3 = IsChecked4 = IsChecked5 = IsChecked6 = IsChecked7 = IsChecked8 = IsChecked9 = IsChecked10 = IsChecked11
                = IsChecked12 = IsChecked13 = IsChecked14 = IsChecked15 = IsChecked16 = IsChecked17 = IsChecked18 = IsChecked19 = IsChecked20 = true;
        }
        //全不选
        public void setAllFalse()
        {
            IsChecked1 = IsChecked2 = IsChecked3 = IsChecked4 = IsChecked5 = IsChecked6 = IsChecked7 = IsChecked8 = IsChecked9 = IsChecked10 = IsChecked11
                = IsChecked12 = IsChecked13 = IsChecked14 = IsChecked15 = IsChecked16 = IsChecked17 = IsChecked18 = IsChecked19 = IsChecked20 = false;
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
