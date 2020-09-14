using bms.startup.Model;
using bms.startup.SDK;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Windows
{
   
    public class ReadCfgViewModel : INotifyPropertyChanged
    {
        private SlaveViewModel parent;
        private int bmuindex;
        private string titleText;

        public DelegateCommand ReadCfgClosedCommand { get; set; }

        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; OnPropertyChanged("TitleText"); }
        }

        public ReadCfgViewModel(SlaveViewModel svm,int bmuindex) {
            parent = svm;
            this.bmuindex = bmuindex;
            if(bmuindex !=0)
                TitleText = "BUM" + bmuindex;

            parent.ReadCfgEvent += parent_ReadCfgEvent;
            ReadCfgClosedCommand = new DelegateCommand(runReadCfgClosedCommand);
        }

        private void runReadCfgClosedCommand()
        {
            parent.ReadCfgEvent -= parent_ReadCfgEvent;
        }




        void parent_ReadCfgEvent(object sender, ReadCfgArgs e)
        {
            CANSDK.VCI_CAN_OBJ obj = e.Args;
            byte id = obj.Data[0];
            switch(id){
                case 0x41:
                    double cell1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.cellVolHighAlarmFirst_rr;
                    CellVolHighAlarmFirst = cell1.ToString();
                    double cell2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.cellVolHighAlarmSecond_rr;
                    CellVolHighAlarmSecond = cell2.ToString();
                    double cell3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.cellVolHighAlarmThird_rr;
                    CellVolHighAlarmThird = cell3.ToString();
                    break;
                case 0x42:
                    double cellremove1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.cellVolHighAlarmRemoveFirst_rr;
                    CellVolHighAlarmRemoveFirst = cellremove1.ToString();
                    double cellremove12 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.cellVolHighAlarmRemoveSecond_rr;
                    CellVolHighAlarmRemoveSecond = cellremove12.ToString();
                    double cellremove13 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.cellVolHighAlarmRemoveThird_rr;
                    CellVolHighAlarmRemoveThird = cellremove13.ToString();
                    break;
                case 0x43:
                     double cellalarm1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.cellVolLowAlarmFirst_rr;
                     CellVolLowAlarmFirst = cellalarm1.ToString();
                     double cellalarm2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.cellVolLowAlarmSecond_rr;
                     CellVolLowAlarmSecond = cellalarm2.ToString();
                     double cellalarm3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.cellVolLowAlarmThird_rr;
                     CellVolLowAlarmThird = cellalarm3.ToString();
                    break;
                case 0x44:
                     double c1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr;
                     CellVolLowAlarmRemoveFirst = c1.ToString();
                     double c2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.cellVolLowAlarmRemoveFirst_rr;
                     CellVolLowAlarmRemoveSecond = c2.ToString();
                     double c3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.cellVolLowAlarmRemoveThird_rr;
                     CellVolLowAlarmRemoveThird = c3.ToString();
                    break;
                case 0x45:
                    int t1 = (int)(obj.Data[1] + ResolutionRatioModel.cellTemperatureHighAlarmFirst_offset);
                    CellTemperatureHighAlarmFirst = t1.ToString();
                    int t2 = (int)(obj.Data[2] + ResolutionRatioModel.cellTemperatureHighAlarmSecond_offset);
                    CellTemperatureHighAlarmSecond = t2.ToString();
                    int t3 = (int)(obj.Data[3] + ResolutionRatioModel.cellTemperatureHighAlarmThird_offset);
                    CellTemperatureHighAlarmThird = t3.ToString();

                    int tr1 = (int)(obj.Data[4] + ResolutionRatioModel.cellTemperatureHighAlarmRemoveFirst_offset);
                    CellTemperatureHighAlarmRemoveFirst = tr1.ToString();
                    int tr2 = (int)(obj.Data[5] + ResolutionRatioModel.cellTemperatureHighAlarmRemoveSecond_offset);
                    CellTemperatureHighAlarmRemoveSecond = tr2.ToString();
                    int tr3 = (int)(obj.Data[6] + ResolutionRatioModel.cellTemperatureHighAlarmRemoveThird_offset);
                    CellTemperatureHighAlarmRemoveThird = tr3.ToString();
                    break;

                case 0x46:
                    int tl1 = (int)(obj.Data[1] + ResolutionRatioModel.cellTemperatureLowAlarmFirst_offset);
                    CellTemperatureLowAlarmFirst = tl1.ToString();
                    int tl2 = (int)(obj.Data[2] + ResolutionRatioModel.cellTemperatureLowAlarmSecond_offset);
                    CellTemperatureLowAlarmSecond = tl2.ToString();
                    int tl3 = (int)(obj.Data[3] + ResolutionRatioModel.cellTemperatureLowAlarmThird_offset);
                    CellTemperatureLowAlarmThird = tl3.ToString();

                    int tlr1 = (int)(obj.Data[4] + ResolutionRatioModel.cellTemperatureLowAlarmRemoveFirst_offset);
                    CellTemperatureLowAlarmRemoveFirst = tlr1.ToString();
                    int tlr2 = (int)(obj.Data[5] + ResolutionRatioModel.cellTemperatureLowAlarmRemoveSecond_offset);
                    CellTemperatureLowAlarmRemoveSecond = tlr2.ToString();
                    int tlr3 = (int)(obj.Data[6] + ResolutionRatioModel.cellTemperatureLowAlarmRemoveThird_offset);
                    CellTemperatureLowAlarmRemoveThird = tlr3.ToString();
                    break;

                case 0x47:
                    double ba1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanCurrentHighAlarmFirst_rr;
                    BalanCurrentHighAlarmFirst = ba1.ToString();
                    double ba2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.balanCurrentHighAlarmSecond_rr;
                    BalanCurrentHighAlarmSecond = ba2.ToString();
                    double ba3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.balanCurrentHighAlarmThird_rr;
                    BalanCurrentHighAlarmThird = ba3.ToString();
                    break;

                case 0x48:
                    double bra1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanCurrentHighAlarmRemoveFirst_rr;
                    BalanCurrentHighAlarmRemoveFirst = bra1.ToString();
                    double bra2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.balanCurrentHighAlarmRemoveSecond_rr;
                    BalanCurrentHighAlarmRemoveSecond = bra2.ToString();
                    double bra3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.balanCurrentHighAlarmRemoveThird_rr;
                    BalanCurrentHighAlarmRemoveThird = bra3.ToString();
                    break;

                case 0x49:
                    double bla1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanCurrentLowAlarmFirst_rr;
                    BalanCurrentLowAlarmFirst = bla1.ToString();
                    double bla2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.balanCurrentLowAlarmSecond_rr;
                    BalanCurrentLowAlarmSecond = bla2.ToString();
                    double bla3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.balanCurrentLowAlarmThird_rr;
                    BalanCurrentLowAlarmThird = bla3.ToString();
                    break;

                case 0x4A:
                    double bb1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanCurrentLowAlarmRemoveFirst_rr;
                    BalanCurrentLowAlarmRemoveFirst = bb1.ToString();
                    double bb2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.balanCurrentLowAlarmRemoveSecond_rr;
                    BalanCurrentLowAlarmRemoveSecond = bb2.ToString();
                    double bb3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.balanCurrentLowAlarmRemoveThird_rr;
                    BalanCurrentLowAlarmRemoveThird = bb3.ToString();
                    break;

                case 0x4B:
                    double cc1 = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanCurrentSetValueFirst_rr;
                    BalanCurrentSetValueFirst = cc1.ToString();
                    double cc2 = (double)(((obj.Data[4] & 0x00FF) << 8) | obj.Data[3]) * ResolutionRatioModel.balanCurrentSetValueSecond_rr;
                    BalanCurrentSetValueSecond = cc2.ToString();
                    double cc3 = (double)(((obj.Data[6] & 0x00FF) << 8) | obj.Data[5]) * ResolutionRatioModel.balanCurrentSetValueThird_rr;
                    BalanCurrentSetValueThird = cc3.ToString();
                    break;

                case 0x4C:
                    double bvv = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanVolOpenValue_rr;
                    BalanVolOpenValue = bvv.ToString();
                    break;
                case 0x4D:
                    double bcv = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanVolCloseValue_rr;
                    BalanVolCloseValue = bcv.ToString();
                    break;
                case 0x4E:
                    double bdov = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanVolDifOpenValue_rr;
                    BalanVolDifOpenValue = bdov.ToString();
                    break;
                case 0x4F:
                    double bvdcv = (double)(((obj.Data[2] & 0x00FF) << 8) | obj.Data[1]) * ResolutionRatioModel.balanVolDifCloseValue_rr;
                    BalanVolDifCloseValue = bvdcv.ToString();
                    break;
                case 0x50:
                    int cbto = (int)(obj.Data[1] + ResolutionRatioModel.cellBalanTemperatureOpenValue_offset);
                    CellBalanTemperatureOpenValue = cbto.ToString();
                    int ctcv = (int)(obj.Data[2] + ResolutionRatioModel.cellBalanTemperatureCloseValue_offset);
                    CellBalanTemperatureCloseValue = ctcv.ToString();
                    break;
                case 0x51:
                    int sn = (int)(obj.Data[1] + ResolutionRatioModel.slaveNum_offset);
                    SlaveNum = sn.ToString();
                    int mode = (int)(obj.Data[2] + ResolutionRatioModel.cellBalanMode_offset);
                    switch (mode) 
                    { 
                        case 'N':
                            CellBalanMode = "不均衡";
                            break;
                        case 'A':
                            CellBalanMode = "主动均衡";
                            break;
                        case 'P':
                            CellBalanMode = "被动均衡";
                            break;
                        default:
                            CellBalanMode = "获取错误";
                            break;
                    }
                    int cmmcn = (int)(obj.Data[3] + ResolutionRatioModel.childModuleMonCellNumber_offset);
                    ChildModuleMonCellNumber = cmmcn.ToString();
                    int cmmtn = (int)(obj.Data[4] + ResolutionRatioModel.childMonModuleTemperatureNumber_offset);
                    ChildMonModuleTemperatureNumber = cmmtn.ToString();
                    int mamcn = (int)(obj.Data[5] + ResolutionRatioModel.moduleAMonCellNum_offset);
                    ModuleAMonCellNum = mamcn.ToString();
                    int matn = (int)(obj.Data[6] + ResolutionRatioModel.moduleAMonTemperatureNum_offset);
                    ModuleAMonTemperatureNum = matn.ToString();
                    break;
                case 0x52:
                    int mbcn = (int)(obj.Data[1] + ResolutionRatioModel.moduleBMonCellNum_offset);
                    ModuleBMonCellNum = mbcn.ToString();
                    int mbtn = (int)(obj.Data[2] + ResolutionRatioModel.moduleBMonTemperatureNum_offset);
                    ModuleBMonTemperatureNum = mbtn.ToString();
                    int mccn = (int)(obj.Data[3] + ResolutionRatioModel.moduleCMonCellNum_offset);
                    ModuleCMonCellNum = mccn.ToString();
                    int mctn = (int)(obj.Data[4] + ResolutionRatioModel.moduleCMonTemperatureNum_offset);
                    ModuleCMonTemperatureNum = mctn.ToString();
                    int mdcn = (int)(obj.Data[5] + ResolutionRatioModel.moduleDMonCellNum_offset);
                    ModuleDMonCellNum = mdcn.ToString();
                    int mdtn = (int)(obj.Data[6] + ResolutionRatioModel.moduleDMonTemperatureNum_offset);
                    ModuleDMonTemperatureNum = mdtn.ToString();

                    break;

                case 0x53:
                    int mecn = (int)(obj.Data[1] + ResolutionRatioModel.moduleEMonCellNum_offset);
                    ModuleEMonCellNum = mecn.ToString();
                    int metn = (int)(obj.Data[2] + ResolutionRatioModel.moduleEMonTemperatureNum_offset);
                    ModuleEMonTemperatureNum = metn.ToString();
                    int ppy = (int)((((obj.Data[3] & 0x00FF) << 8) | obj.Data[4]) + ResolutionRatioModel.packProYear_offset);
                    PackProYear = ppy.ToString();
                    int ppm = (int)(obj.Data[5] + ResolutionRatioModel.packProMonth_offset);
                    PackProMonth = ppm.ToString();
                    int ppd = (int)(obj.Data[6] + ResolutionRatioModel.packProDay_offset);
                    PackProDay = ppd.ToString();

                    break;

                case 0x54:
                    int p1 = obj.Data[1];
                    PackBatchNumberData1 = p1.ToString();
                    double p2 = obj.Data[2];
                    PackBatchNumberData2 = p2.ToString();
                      int p3 = obj.Data[3];
                      PackBatchNumberData3 = p3.ToString();
                    double p4 = obj.Data[4];
                    PackBatchNumberData4 = p4.ToString();
                      int p5 = obj.Data[5];
                    PackBatchNumberData5 = p5.ToString();
                    double p6 = obj.Data[6];
                    PackBatchNumberData6 = p6.ToString();

                    break;


            }

        }

        

        //info1
        private string cellVolHighAlarmFirst;//单体过高一级
        private string cellVolHighAlarmSecond;//单体过高二级
        private string cellVolHighAlarmThird;//单体过高三级

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
        private string cellVolHighAlarmRemoveFirst;//单体过高解除一级
        private string cellVolHighAlarmRemoveSecond;//单体过高解除二级
        private string cellVolHighAlarmRemoveThird;//单体过高解除三级

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
        private string cellVolLowAlarmFirst;//单体过低一级
        private string cellVolLowAlarmSecond;//单体过低二级
        private string cellVolLowAlarmThird;//单体过低三级
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
        private string cellVolLowAlarmRemoveFirst;//单体过低解除一级
        private string cellVolLowAlarmRemoveSecond;//单体过低解除二级
        private string cellVolLowAlarmRemoveThird;//单体过低解除三级

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
        private string cellTemperatureHighAlarmFirst;//温度过高一级
        private string cellTemperatureHighAlarmSecond;//温度过高二级
        private string cellTemperatureHighAlarmThird;//温度过高三级
        private string cellTemperatureHighAlarmRemoveFirst;//温度过高解除一级
        private string cellTemperatureHighAlarmRemoveSecond;//温度过高解除二级
        private string cellTemperatureHighAlarmRemoveThird;//温度过高解除三级

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
        private string cellTemperatureLowAlarmFirst;//温度过低一级
        private string cellTemperatureLowAlarmSecond;//温度过低二级
        private string cellTemperatureLowAlarmThird;//温度过低三级
        private string cellTemperatureLowAlarmRemoveFirst;//温度过低解除一级
        private string cellTemperatureLowAlarmRemoveSecond;//温度过低解除二级
        private string cellTemperatureLowAlarmRemoveThird;//温度过低解除三级

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
        private string balanCurrentHighAlarmFirst;//均衡电流过高一级
        private string balanCurrentHighAlarmSecond;//均衡电流过高二级
        private string balanCurrentHighAlarmThird;//均衡电流过高三级
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
        private string balanCurrentHighAlarmRemoveFirst;//均衡电流过高解除一级
        private string balanCurrentHighAlarmRemoveSecond;//均衡电流过高解除二级
        private string balanCurrentHighAlarmRemoveThird;//均衡电流过高解除三级

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
        private string balanCurrentLowAlarmFirst;//均衡电流过低一级
        private string balanCurrentLowAlarmSecond;//均衡电流过低二级
        private string balanCurrentLowAlarmThird;//均衡电流过低三级

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
        private string balanCurrentLowAlarmRemoveFirst;//均衡电流过低解除一级
        private string balanCurrentLowAlarmRemoveSecond;//均衡电流过低解除二级
        private string balanCurrentLowAlarmRemoveThird;//均衡电流过低解除三级

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
        private string balanCurrentSetValueFirst;//均衡电流大小一级 
        private string balanCurrentSetValueSecond;//均衡电流大小二级
        private string balanCurrentSetValueThird;//均衡电流大小三级

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
        private string balanVolOpenValue;//均衡开启电压

        public string BalanVolOpenValue
        {
            get { return balanVolOpenValue; }
            set { balanVolOpenValue = value; OnPropertyChanged("BalanVolOpenValue"); }
        }


        //info13
        private string balanVolCloseValue;//均衡截止电压

        public string BalanVolCloseValue
        {
            get { return balanVolCloseValue; }
            set { balanVolCloseValue = value; OnPropertyChanged("BalanVolCloseValue"); }
        }



        //info14
        private string balanVolDifOpenValue;//均衡开启压差

        public string BalanVolDifOpenValue
        {
            get { return balanVolDifOpenValue; }
            set { balanVolDifOpenValue = value; OnPropertyChanged("BalanVolDifOpenValue"); }
        }

        //info15
        private string balanVolDifCloseValue;//均衡截止压差

        public string BalanVolDifCloseValue
        {
            get { return balanVolDifCloseValue; }
            set { balanVolDifCloseValue = value; OnPropertyChanged("BalanVolDifCloseValue"); }
        }

        //info16
        private string cellBalanTemperatureOpenValue;//均衡电池开启温度
        private string cellBalanTemperatureCloseValue;//均衡电池截止温度

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

      

        public string SlaveNum
        {
            get { return slaveNum; }
            set { slaveNum = value; OnPropertyChanged("SlaveNum"); }
        }

        public string CellBalanMode
        {
            get { return cellBalanMode; }
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
        private string moduleBMonCellNum;//子模块B监控电池数目
        private string moduleBMonTemperatureNum;//子模块B监控温感数目
        private string moduleCMonCellNum;//子模块C监控电池数目
        private string moduleCMonTemperatureNum;//子模块C监控温感数目
        private string moduleDMonCellNum;//子模块D监控电池数目
        private string moduleDMonTemperatureNum;//子模块D监控温感数目

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
        private string moduleEMonCellNum;//子模块E监控电池数目
        private string moduleEMonTemperatureNum;//子模块E监控温感数目
        private string packProYear;//电池组生产年份
        private string packProMonth;//电池组生产月份
        private string packProDay;//电池组生产日期

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
        private string packBatchNumberData1;//电池组项目批量号1
        private string packBatchNumberData2;//电池组项目批量号2
        private string packBatchNumberData3;//电池组项目批量号3
        private string packBatchNumberData4;//电池组项目批量号4
        private string packBatchNumberData5;//电池组项目批量号5
        private string packBatchNumberData6;//电池组项目批量号6

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
