using bms.startup.Model;
using bms.startup.SDK;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace bms.startup.Windows
{
    class DiagnoseViewModel : INotifyPropertyChanged
    {
        private DiagnoseModel diagnose;

        public DiagnoseModel Diagnose
        {
            get { return diagnose; }
            set { diagnose = value; OnPropertyChanged("Diagnose"); }
        }

        private SlaveViewModel parent;
        private int bmuindex;
        private string titleText;

        public DelegateCommand DiagnoseClosedCommand { get; set; }

        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; OnPropertyChanged("TitleText"); }
        }
        DiagnoseForm dfGlobal;
        public DiagnoseViewModel(SlaveViewModel svm, int bmuindex,DiagnoseForm df)
        {
            diagnose = new DiagnoseModel();
            parent = svm;
            this.bmuindex = bmuindex;
            if (bmuindex != 0)
                TitleText = "BUM" + bmuindex;
            parent.DiagnoseEvent += parent_DiagnoseEvent;
            DiagnoseClosedCommand = new DelegateCommand(runDiagnoseClosedCommand);
            dfGlobal=df;
            //parent.ReadCfgEvent += parent_ReadCfgEvent;
            //ReadCfgClosedCommand = new DelegateCommand(runReadCfgClosedCommand);
        }

        
        private void runDiagnoseClosedCommand()
        {
            parent.DiagnoseEvent -= parent_DiagnoseEvent;
        }

        private void parent_DiagnoseEvent(object sender, ReadCfgArgs e)
        {
            Random rd = new Random();
            CANSDK.VCI_CAN_OBJ obj = e.Args;
            string tag = obj.ID.ToString("X2").Substring(3,1);
            switch (tag) { 
                case "1":
                    diagnose.MON_PWM_SWP = (int)(obj.Data[0] * ResolutionRatioModel.mON_PWM_SWP_rr + ResolutionRatioModel.mON_PWM_SWP_offset);
                    diagnose.MON_PWM_SWP_Fre = (double)(obj.Data[1] * ResolutionRatioModel.mON_PWM_SWP_Fre_rr + ResolutionRatioModel.mON_PWM_SWP_Fre_offset);
                    //低字节在前，高字节在后
                    diagnose.MON_VB_24V = 
                        (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_VB_24V_rr + ResolutionRatioModel.mON_VB_24V_offset;
                    //显示小数点后3位
                    diagnose.MON_VB_24V = double.Parse(string.Format("{0:f3}", diagnose.MON_VB_24V));

                    diagnose.MON_VS_24V = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_VS_24V_rr + ResolutionRatioModel.mON_VS_24V_offset;
                    diagnose.MON_VS_24V = double.Parse(string.Format("{0:f3}", diagnose.MON_VS_24V));

                    diagnose.MON_EN_VB_24V = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_EN_VB_24V_rr + ResolutionRatioModel.mON_EN_VB_24V_offset;
                    diagnose.MON_EN_VB_24V = double.Parse(string.Format("{0:f3}", diagnose.MON_EN_VB_24V));

                    break;
                case "2":
                    diagnose.MON_EN_POWER = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_EN_POWER_rr + ResolutionRatioModel.mON_EN_POWER_offset;
                    diagnose.MON_EN_POWER = double.Parse(string.Format("{0:f3}", diagnose.MON_EN_POWER));

                    diagnose.MON_12VL = ((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_12VL_rr + ResolutionRatioModel.mON_12VL_offset;
                    diagnose.MON_12VL = double.Parse(string.Format("{0:f3}", diagnose.MON_12VL));


                    diagnose.MON_Vref_25VL = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_Vref_25VL_rr + ResolutionRatioModel.mON_Vref_25VL_offset;
                    diagnose.MON_Vref_25VL = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_25VL));

                    diagnose.MON_Vref_147VL = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_Vref_147VL_rr + ResolutionRatioModel.mON_Vref_147VL_offset;
                    diagnose.MON_Vref_147VL = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_147VL));

                    break;
                case "3":
                    diagnose.MON_Vref_353VL = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_Vref_353VL_rr + ResolutionRatioModel.mON_Vref_353VL_offset;
                    diagnose.MON_Vref_353VL = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_353VL));

                    diagnose.MON_T_AMB = (int)(((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_T_AMB_rr + ResolutionRatioModel.mON_T_AMB_offset);
                    diagnose.MON_T_AMB = double.Parse(string.Format("{0:f3}", diagnose.MON_T_AMB));

                    diagnose.MON_ABC_PRI_0 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_ABC_PRI_0_rr + ResolutionRatioModel.mON_ABC_PRI_0_offset * ResolutionRatioModel.mON_ABC_PRI_0_rr;
                    diagnose.MON_ABC_PRI_0 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_PRI_0));

                    diagnose.MON_T_PRI_0 = (int)(((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_T_PRI_0_rr + ResolutionRatioModel.mON_T_PRI_0_offset * ResolutionRatioModel.mON_T_PRI_0_rr);
                    diagnose.MON_T_PRI_0 = double.Parse(string.Format("{0:f3}", diagnose.MON_T_PRI_0));

                    break;
                case "4":
                    diagnose.MON_5VH_1428 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_5VH_1428_rr + ResolutionRatioModel.mON_5VH_1428_offset;
                    diagnose.MON_5VH_1428 = double.Parse(string.Format("{0:f3}", diagnose.MON_5VH_1428));

                    diagnose.MON_12VH = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_12VH_rr + ResolutionRatioModel.mON_12VH_offset;
                    diagnose.MON_12VH = double.Parse(string.Format("{0:f3}", diagnose.MON_12VH));

                    diagnose.MON_Vref_03VH = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_Vref_03VH_rr + ResolutionRatioModel.mON_Vref_03VH_offset;
                    diagnose.MON_Vref_03VH = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_03VH));

                    diagnose.MON_Vref_47VH = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_Vref_47VH_rr + ResolutionRatioModel.mON_Vref_47VH_offset;
                    diagnose.MON_Vref_47VH = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_47VH));

                    break;
                case "5":
                    diagnose.MON_ABC_SEC = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_ABC_SEC_rr + ResolutionRatioModel.mON_ABC_SEC_offset * ResolutionRatioModel.mON_ABC_SEC_rr;
                    diagnose.MON_ABC_SEC = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_SEC));

                    diagnose.MON_ABV = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_ABV_rr + ResolutionRatioModel.mON_ABV_offset;
                    diagnose.MON_ABV = double.Parse(string.Format("{0:f3}", diagnose.MON_ABV));

                    diagnose.MON_VREF2 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_VREF2_rr + ResolutionRatioModel.mON_VREF2_offset;
                    diagnose.MON_VREF2 = double.Parse(string.Format("{0:f3}", diagnose.MON_VREF2));

                    diagnose.MON_VSET = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_VSET_rr + ResolutionRatioModel.mON_VSET_offset;
                    diagnose.MON_VSET = double.Parse(string.Format("{0:f3}", diagnose.MON_VSET));

                    break;
                case "6":
                    diagnose.MON_T_SEC = (int)(((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_T_SEC_rr + ResolutionRatioModel.mON_T_SEC_offset);
                    diagnose.MON_T_SEC = double.Parse(string.Format("{0:f3}", diagnose.MON_T_SEC));

                    diagnose.MON_TV_Cell = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_TV_Cell_rr + ResolutionRatioModel.mON_TV_Cell_offset;
                    diagnose.MON_TV_Cell = double.Parse(string.Format("{0:f3}", diagnose.MON_TV_Cell));

                    diagnose.MON_FAULT_INT_1428 = (int)(((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_FAULT_INT_1428_rr + ResolutionRatioModel.mON_FAULT_INT_1428_offset);
                    diagnose.MON_FAULT_INT_1428 = double.Parse(string.Format("{0:f3}", diagnose.MON_FAULT_INT_1428));

                    diagnose.MON_GATE_LS_0 = (int)(obj.Data[6] * ResolutionRatioModel.mON_GATE_LS_0_rr + ResolutionRatioModel.mON_GATE_LS_0_offset);
                    diagnose.MON_GATE_LS_0 = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_0));

                    diagnose.MON_GATE_LS_0_Fre = (double)(obj.Data[7] * ResolutionRatioModel.mON_GATE_LS_0_Fre_rr + ResolutionRatioModel.mON_GATE_LS_0_Fre_offset);
                    diagnose.MON_GATE_LS_0_Fre = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_0_Fre));
                    break;
                case "7":
                    diagnose.MON_ABC_PRI_1 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_ABC_PRI_1_rr + ResolutionRatioModel.mON_ABC_PRI_1_offset * ResolutionRatioModel.mON_ABC_PRI_1_rr;
                    diagnose.MON_ABC_PRI_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_PRI_1));

                    diagnose.MON_T_PRI_1 = (int)(((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_T_PRI_1_rr + ResolutionRatioModel.mON_T_PRI_1_offset);
                    diagnose.MON_T_PRI_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_T_PRI_1));

                    diagnose.MON_5VH_1428_1 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_5VH_1428_1_rr + ResolutionRatioModel.mON_5VH_1428_1_offset;
                    diagnose.MON_5VH_1428_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_5VH_1428_1));

                    diagnose.MON_12VH1 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_12VH1_rr + ResolutionRatioModel.mON_12VH1_offset;
                    diagnose.MON_12VH1 = double.Parse(string.Format("{0:f3}", diagnose.MON_12VH1));
                    break;
                case "8":
                    diagnose.MON_Vref_03VH1 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_Vref_03VH1_rr + ResolutionRatioModel.mON_Vref_03VH1_offset;
                    diagnose.MON_Vref_03VH1 = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_03VH1));

                    diagnose.MON_Vref_47VH1 = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_Vref_47VH1_rr + ResolutionRatioModel.mON_Vref_47VH1_offset;
                    diagnose.MON_Vref_47VH1 = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_47VH1));

                    diagnose.MON_ABC_SEC_1 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_ABC_SEC_1_rr + ResolutionRatioModel.mON_ABC_SEC_1_offset * ResolutionRatioModel.mON_ABC_SEC_1_rr;
                    diagnose.MON_ABC_SEC_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_SEC_1));

                    diagnose.MON_ABV_1 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_ABV_1_rr + ResolutionRatioModel.mON_ABV_1_offset;
                    diagnose.MON_ABV_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABV_1));

                    break;
                case "9":
                    diagnose.MON_VREF2_1 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_VREF2_1_rr + ResolutionRatioModel.mON_VREF2_1_offset;
                    diagnose.MON_VREF2_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_VREF2_1));

                    diagnose.MON_VSET_1 = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_VSET_1_rr + ResolutionRatioModel.mON_VSET_1_offset;
                    diagnose.MON_VSET_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_VSET_1));

                    diagnose.MON_T_SEC_1 = (int)(((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_T_SEC_1_rr + ResolutionRatioModel.mON_T_SEC_1_offset);
                    diagnose.MON_T_SEC_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_T_SEC_1));

                    diagnose.MON_TV_Cell_1 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_TV_Cell_1_rr + ResolutionRatioModel.mON_TV_Cell_1_offset;
                    diagnose.MON_TV_Cell_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_TV_Cell_1));

                    break;
                case "A":
                    diagnose.MON_FAULT_INT_1428_1 = (int)(((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_FAULT_INT_1428_1_rr + ResolutionRatioModel.mON_FAULT_INT_1428_1_offset);
                    diagnose.MON_FAULT_INT_1428_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_FAULT_INT_1428_1));

                    diagnose.MON_GATE_LS_1 = (int)(obj.Data[2] * ResolutionRatioModel.mON_GATE_LS_1_rr + ResolutionRatioModel.mON_GATE_LS_1_offset);
                    diagnose.MON_GATE_LS_1 = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_1));

                    diagnose.MON_GATE_LS_1_Fre = (double)(obj.Data[3] * ResolutionRatioModel.mON_GATE_LS_1_Fre_rr + ResolutionRatioModel.mON_GATE_LS_1_Fre_offset);
                    diagnose.MON_GATE_LS_1_Fre = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_1_Fre));

                    diagnose.MON_ABC_PRI_2 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_ABC_PRI_2_rr + ResolutionRatioModel.mON_ABC_PRI_2_offset * ResolutionRatioModel.mON_ABC_PRI_2_rr;
                    diagnose.MON_ABC_PRI_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_PRI_2));

                    diagnose.MON_T_PRI_2 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_T_PRI_2_rr + ResolutionRatioModel.mON_T_PRI_2_offset;
                    diagnose.MON_T_PRI_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_T_PRI_2));

                    break;
                case "B":
                    diagnose.MON_5VH_1428_2 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_5VH_1428_2_rr + ResolutionRatioModel.mON_5VH_1428_2_offset;
                    diagnose.MON_5VH_1428_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_5VH_1428_2));

                    diagnose.MON_12VH2 = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_12VH2_rr + ResolutionRatioModel.mON_12VH2_offset;
                    diagnose.MON_12VH2 = double.Parse(string.Format("{0:f3}", diagnose.MON_12VH2));

                    diagnose.MON_Vref_03VH2 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_Vref_03VH2_rr + ResolutionRatioModel.mON_Vref_03VH2_offset;
                    diagnose.MON_Vref_03VH2 = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_03VH2));

                    diagnose.MON_Vref_47VH2 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_Vref_47VH2_rr + ResolutionRatioModel.mON_Vref_47VH2_offset;
                    diagnose.MON_Vref_47VH2 = double.Parse(string.Format("{0:f3}", diagnose.MON_Vref_47VH2));
                    break;
                case "C":
                    diagnose.MON_ABC_SEC_2 = (double)((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_ABC_SEC_2_rr + ResolutionRatioModel.mON_ABC_SEC_2_offset * ResolutionRatioModel.mON_ABC_SEC_2_rr;
                    diagnose.MON_ABC_SEC_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABC_SEC_2));

                    diagnose.MON_ABV_2 = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_ABV_2_rr + ResolutionRatioModel.mON_ABV_2_offset;
                    diagnose.MON_ABV_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_ABV_2));

                    diagnose.MON_VREF2_2 = (double)((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_VREF2_2_rr + ResolutionRatioModel.mON_VREF2_2_offset;
                    diagnose.MON_VREF2_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_VREF2_2));

                    diagnose.MON_VSET_2 = (double)((obj.Data[7] & 0x00FF) << 8 | obj.Data[6]) * ResolutionRatioModel.mON_VSET_2_rr + ResolutionRatioModel.mON_VSET_2_offset;
                    diagnose.MON_VSET_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_VSET_2));
                    break;
                case "D":
                    diagnose.MON_T_SEC_2 = (int)(((obj.Data[1] & 0x00FF) << 8 | obj.Data[0]) * ResolutionRatioModel.mON_T_SEC_2_rr + ResolutionRatioModel.mON_T_SEC_2_offset);
                    diagnose.MON_T_SEC_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_T_SEC_2));

                    diagnose.MON_TV_Cell_2 = (double)((obj.Data[3] & 0x00FF) << 8 | obj.Data[2]) * ResolutionRatioModel.mON_TV_Cell_2_rr + ResolutionRatioModel.mON_TV_Cell_2_offset;
                    diagnose.MON_TV_Cell_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_TV_Cell_2));

                    diagnose.MON_FAULT_INT_1428_2 = (int)(((obj.Data[5] & 0x00FF) << 8 | obj.Data[4]) * ResolutionRatioModel.mON_FAULT_INT_1428_2_rr + ResolutionRatioModel.mON_FAULT_INT_1428_2_offset);
                    diagnose.MON_FAULT_INT_1428_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_FAULT_INT_1428_2));

                    diagnose.MON_GATE_LS_2 = (int)(obj.Data[6] * ResolutionRatioModel.mON_GATE_LS_2_rr + ResolutionRatioModel.mON_GATE_LS_2_offset);
                    diagnose.MON_GATE_LS_2 = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_2));

                    diagnose.MON_GATE_LS_2_Fre = (double)(obj.Data[7] * ResolutionRatioModel.mON_GATE_LS_2_Fre_rr + ResolutionRatioModel.mON_GATE_LS_2_Fre_offset);
                    diagnose.MON_GATE_LS_2_Fre = double.Parse(string.Format("{0:f3}", diagnose.MON_GATE_LS_2_Fre));
                    break;
                case "F":
                   byte[] b= obj.Data;
                    int[] resultArray = new int[56];
                    for (int i = 0; i < 56; i++) {
                        resultArray[i]=obj.Data[i/8] >> (i%8) & 0x01;                       
                    }
                    int[] q=resultArray;
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        int j = -1;
                        foreach (UIElement element in dfGlobal.parent.Children)
                        {
                            if (element is StackPanel)
                            {
                                StackPanel elm = element as StackPanel;
                                foreach (var item in elm.Children)
                                {
                                    
                                    Button btn = item as Button;
                                    if (btn != null)
                                    {
                                        j++;
                                        Console.Write(j+",");
                                        if (resultArray[j] == 0) {
                                            btn.SetValue(Button.StyleProperty, Application.Current.Resources["SignalEnableButton"]);
                                        }

                                    }
                                  
                                }
                                
                            }
                           
                        }

                    });
                 

                    break;
                  
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
