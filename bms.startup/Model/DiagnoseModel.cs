using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    public class DiagnoseModel : INotifyPropertyChanged
    {
         //Dia1
         private int mON_PWM_SWP;

        public int MON_PWM_SWP
        {
            get { return mON_PWM_SWP; }
            set { mON_PWM_SWP = value; OnPropertyChanged("MON_PWM_SWP"); }
        }

        private double? mON_PWM_SWP_Fre;

        public double? MON_PWM_SWP_Fre
        {
            get { return mON_PWM_SWP_Fre; }
            set { mON_PWM_SWP_Fre = value; OnPropertyChanged("MON_PWM_SWP_Fre"); }
        }
        private double? mON_VB_24V;

        public double? MON_VB_24V
        {
            get { return mON_VB_24V; }
            set { mON_VB_24V = value; OnPropertyChanged("MON_VB_24V"); }
        }
        private double? mON_VS_24V;

        public double? MON_VS_24V
        {
            get { return mON_VS_24V; }
            set { mON_VS_24V = value; OnPropertyChanged("MON_VS_24V"); }
        }
        private double? mON_EN_VB_24V;

        public double? MON_EN_VB_24V
        {
            get { return mON_EN_VB_24V; }
            set { mON_EN_VB_24V = value; OnPropertyChanged("MON_EN_VB_24V"); }
        }

      
         //Dia2
        private double? mON_EN_POWER;

        public double? MON_EN_POWER
        {
            get { return mON_EN_POWER; }
            set { mON_EN_POWER = value; OnPropertyChanged("MON_EN_POWER"); }
        }
        private double? mON_12VL;

        public double? MON_12VL
        {
            get { return mON_12VL; }
            set { mON_12VL = value; OnPropertyChanged("MON_12VL"); }
        }
        private double? mON_Vref_25VL;

        public double? MON_Vref_25VL
        {
            get { return mON_Vref_25VL; }
            set { mON_Vref_25VL = value; OnPropertyChanged("MON_Vref_25VL"); }
        }
        private double? mON_Vref_147VL;

        public double? MON_Vref_147VL
        {
            get { return mON_Vref_147VL; }
            set { mON_Vref_147VL = value; OnPropertyChanged("MON_Vref_147VL"); }
        }

       

         //Dia3
        private double? mON_Vref_353VL;

        public double? MON_Vref_353VL
        {
            get { return mON_Vref_353VL; }
            set { mON_Vref_353VL = value; OnPropertyChanged("MON_Vref_353VL"); }
        }
        private double? mON_T_AMB;

        public double? MON_T_AMB
        {
            get { return mON_T_AMB; }
            set { mON_T_AMB = value; OnPropertyChanged("MON_T_AMB"); }
        }
        private double? mON_ABC_PRI_0;

        public double? MON_ABC_PRI_0
        {
            get { return mON_ABC_PRI_0; }
            set { mON_ABC_PRI_0 = value; OnPropertyChanged("MON_ABC_PRI_0"); }
        }
        private double? mON_T_PRI_0;

        public double? MON_T_PRI_0
        {
            get { return mON_T_PRI_0; }
            set { mON_T_PRI_0 = value; OnPropertyChanged("MON_T_PRI_0"); }
        }

        

         //Dia4
        private double? mON_5VH_1428;
        private double? mON_12VH;
        private double? mON_Vref_03VH;
        private double? mON_Vref_47VH;

        public double? MON_5VH_1428
        {
            get { return mON_5VH_1428; }
            set { mON_5VH_1428 = value; OnPropertyChanged("MON_5VH_1428"); }
        }
        
        public double? MON_12VH
        {
            get { return mON_12VH; }
            set { mON_12VH = value; OnPropertyChanged("MON_12VH"); }
        }
        
        public double? MON_Vref_03VH
        {
            get { return mON_Vref_03VH; }
            set { mON_Vref_03VH = value; OnPropertyChanged("MON_Vref_03VH"); }
        }
        
        public double? MON_Vref_47VH
        {
            get { return mON_Vref_47VH; }
            set { mON_Vref_47VH = value; OnPropertyChanged("MON_Vref_47VH"); }
        }
       
         //Dia5
        private double? mON_ABC_SEC;
        private double? mON_ABV;
        private double? mON_VREF2;
        private double? mON_VSET;

        public double? MON_ABC_SEC
        {
            get { return mON_ABC_SEC; }
            set { mON_ABC_SEC = value; OnPropertyChanged("MON_ABC_SEC"); }
        }
       
        public double? MON_ABV
        {
            get { return mON_ABV; }
            set { mON_ABV = value; OnPropertyChanged("MON_ABV"); }
        }
        
        public double? MON_VREF2
        {
            get { return mON_VREF2; }
            set { mON_VREF2 = value; OnPropertyChanged("MON_VREF2"); }
        }
        
        public double? MON_VSET
        {
            get { return mON_VSET; }
            set { mON_VSET = value; OnPropertyChanged("MON_VSET"); }
        }

         //Dia6
        private double? mON_T_SEC;
        private double? mON_TV_Cell;
        private double? mON_FAULT_INT_1428;
        private double? mON_GATE_LS_0;
        private double? mON_GATE_LS_0_Fre;

        public double? MON_T_SEC
        {
            get { return mON_T_SEC; }
            set { mON_T_SEC = value; OnPropertyChanged("MON_T_SEC"); }
        }
        
        public double? MON_TV_Cell
        {
            get { return mON_TV_Cell; }
            set { mON_TV_Cell = value; OnPropertyChanged("MON_TV_Cell"); }
        }
        
        public double? MON_FAULT_INT_1428
        {
            get { return mON_FAULT_INT_1428; }
            set { mON_FAULT_INT_1428 = value; OnPropertyChanged("MON_FAULT_INT_1428"); }
        }
        
        public double? MON_GATE_LS_0
        {
            get { return mON_GATE_LS_0; }
            set { mON_GATE_LS_0 = value; OnPropertyChanged("MON_GATE_LS_0"); }
        }
        
        public double? MON_GATE_LS_0_Fre
        {
            get { return mON_GATE_LS_0_Fre; }
            set { mON_GATE_LS_0_Fre = value; OnPropertyChanged("MON_GATE_LS_0_Fre"); }
        }
         //Dia7
        private double? mON_ABC_PRI_1;
        private double? mON_T_PRI_1;
        private double? mON_5VH_1428_1;
        private double? mON_12VH1;

        public double? MON_ABC_PRI_1
        {
            get { return mON_ABC_PRI_1; }
            set { mON_ABC_PRI_1 = value; OnPropertyChanged("MON_ABC_PRI_1"); }
        }
        
        public double? MON_T_PRI_1
        {
            get { return mON_T_PRI_1; }
            set { mON_T_PRI_1 = value; OnPropertyChanged("MON_T_PRI_1"); }
        }
        
        public double? MON_5VH_1428_1
        {
            get { return mON_5VH_1428_1; }
            set { mON_5VH_1428_1 = value; OnPropertyChanged("MON_5VH_1428_1"); }
        }
        
        public double? MON_12VH1
        {
            get { return mON_12VH1; }
            set { mON_12VH1 = value; OnPropertyChanged("MON_12VH1"); }
        }

         //Dia8
        private double? mON_Vref_03VH1;
        private double? mON_Vref_47VH1;
        private double? mON_ABC_SEC_1;
        private double? mON_ABV_1;

        public double? MON_Vref_03VH1
        {
            get { return mON_Vref_03VH1; }
            set { mON_Vref_03VH1 = value; OnPropertyChanged("MON_Vref_03VH1"); }
        }
        
        public double? MON_Vref_47VH1
        {
            get { return mON_Vref_47VH1; }
            set { mON_Vref_47VH1 = value; OnPropertyChanged("MON_Vref_47VH1"); }
        }
        
        public double? MON_ABC_SEC_1
        {
            get { return mON_ABC_SEC_1; }
            set { mON_ABC_SEC_1 = value; OnPropertyChanged("MON_ABC_SEC_1"); }
        }
        
        public double? MON_ABV_1
        {
            get { return mON_ABV_1; }
            set { mON_ABV_1 = value; OnPropertyChanged("MON_ABV_1"); }
        }

         //Dia9
        private double? mON_VREF2_1;
        private double? mON_VSET_1;
        private double? mON_T_SEC_1;
        private double? mON_TV_Cell_1;

        public double? MON_VREF2_1
        {
            get { return mON_VREF2_1; }
            set { mON_VREF2_1 = value; OnPropertyChanged("MON_VREF2_1"); }
        }
        
        public double? MON_VSET_1
        {
            get { return mON_VSET_1; }
            set { mON_VSET_1 = value; OnPropertyChanged("MON_VSET_1"); }
        }
        
        public double? MON_T_SEC_1
        {
            get { return mON_T_SEC_1; }
            set { mON_T_SEC_1 = value; OnPropertyChanged("MON_T_SEC_1"); }
        }
        
        public double? MON_TV_Cell_1
        {
            get { return mON_TV_Cell_1; }
            set { mON_TV_Cell_1 = value; OnPropertyChanged("MON_TV_Cell_1"); }
        }

         //Dia10
        private double? mON_FAULT_INT_1428_1;
        private double? mON_GATE_LS_1;
        private double? mON_GATE_LS_1_Fre;
        private double? mON_ABC_PRI_2;
        private double? mON_T_PRI_2;

        public double? MON_FAULT_INT_1428_1
        {
            get { return mON_FAULT_INT_1428_1; }
            set { mON_FAULT_INT_1428_1 = value; OnPropertyChanged("MON_FAULT_INT_1428_1"); }
        }
        
        public double? MON_GATE_LS_1
        {
            get { return mON_GATE_LS_1; }
            set { mON_GATE_LS_1 = value; OnPropertyChanged("MON_GATE_LS_1"); }
        }
       
        public double? MON_GATE_LS_1_Fre
        {
            get { return mON_GATE_LS_1_Fre; }
            set { mON_GATE_LS_1_Fre = value; OnPropertyChanged("MON_GATE_LS_1_Fre"); }
        }
        
        public double? MON_ABC_PRI_2
        {
            get { return mON_ABC_PRI_2; }
            set { mON_ABC_PRI_2 = value; OnPropertyChanged("MON_ABC_PRI_2"); }
        }
        
        public double? MON_T_PRI_2
        {
            get { return mON_T_PRI_2; }
            set { mON_T_PRI_2 = value; OnPropertyChanged("MON_T_PRI_2"); }
        }

         //Dia11
        private double? mON_5VH_1428_2;
        private double? mON_12VH2;
        private double? mON_Vref_03VH2;
        private double? mON_Vref_47VH2;

        public double? MON_5VH_1428_2
        {
            get { return mON_5VH_1428_2; }
            set { mON_5VH_1428_2 = value; OnPropertyChanged("MON_5VH_1428_2"); }
        }
        
        public double? MON_12VH2
        {
            get { return mON_12VH2; }
            set { mON_12VH2 = value; OnPropertyChanged("MON_12VH2"); }
        }
        
        public double? MON_Vref_03VH2
        {
            get { return mON_Vref_03VH2; }
            set { mON_Vref_03VH2 = value; OnPropertyChanged("MON_Vref_03VH2"); }
        }
        
        public double? MON_Vref_47VH2
        {
            get { return mON_Vref_47VH2; }
            set { mON_Vref_47VH2 = value; OnPropertyChanged("MON_Vref_47VH2"); }
        }

         //Dia12
        private double? mON_ABC_SEC_2;
        private double? mON_ABV_2;
        private double? mON_VREF2_2;
        private double? mON_VSET_2;

        public double? MON_ABC_SEC_2
        {
            get { return mON_ABC_SEC_2; }
            set { mON_ABC_SEC_2 = value; OnPropertyChanged("MON_ABC_SEC_2"); }
        }
        
        public double? MON_ABV_2
        {
            get { return mON_ABV_2; }
            set { mON_ABV_2 = value; OnPropertyChanged("MON_ABV_2"); }
        }
        
        public double? MON_VREF2_2
        {
            get { return mON_VREF2_2; }
            set { mON_VREF2_2 = value; OnPropertyChanged("MON_VREF2_2"); }
        }       

        public double? MON_VSET_2
        {
            get { return mON_VSET_2; }
            set { mON_VSET_2 = value; OnPropertyChanged("MON_VSET_2"); }
        }

         //Dia13
        private double? mON_T_SEC_2;
        private double? mON_TV_Cell_2;
        private double? mON_FAULT_INT_1428_2;
        private double? mON_GATE_LS_2;
        private double? mON_GATE_LS_2_Fre;

        public double? MON_GATE_LS_2
        {
            get { return mON_GATE_LS_2; }
            set { mON_GATE_LS_2 = value; OnPropertyChanged("MON_GATE_LS_2"); }
        }
        
        public double? MON_GATE_LS_2_Fre
        {
            get { return mON_GATE_LS_2_Fre; }
            set { mON_GATE_LS_2_Fre = value; OnPropertyChanged("MON_GATE_LS_2_Fre"); }
        }

        public double? MON_T_SEC_2
        {
            get { return mON_T_SEC_2; }
            set { mON_T_SEC_2 = value; OnPropertyChanged("MON_T_SEC_2"); }
        }
        
        public double? MON_TV_Cell_2
        {
            get { return mON_TV_Cell_2; }
            set { mON_TV_Cell_2 = value; OnPropertyChanged("MON_TV_Cell_2"); }
        }
        
        public double? MON_FAULT_INT_1428_2
        {
            get { return mON_FAULT_INT_1428_2; }
            set { mON_FAULT_INT_1428_2 = value; OnPropertyChanged("MON_FAULT_INT_1428_2"); }
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
