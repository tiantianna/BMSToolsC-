using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    public class ResolutionRatioModel
    {

        public static double cellVolHighAlarmFirst_offset = 0;//单体过高一级
        public static double cellVolHighAlarmSecond_offset = 0;//单体过高二级
        public static double cellVolHighAlarmThird_offset = 0;//单体过高三级

        public static double cellVolHighAlarmRemoveFirst_offset = 0;//单体过高解除一级
        public static double cellVolHighAlarmRemoveSecond_offset = 0;//单体过高解除二级
        public static double cellVolHighAlarmRemoveThird_offset = 0;//单体过高解除三级

        public static double cellVolLowAlarmFirst_offset = 0;//单体过低一级
        public static double cellVolLowAlarmSecond_offset = 0;//单体过低二级
        public static double cellVolLowAlarmThird_offset = 0;//单体过低三级

        public static double cellVolLowAlarmRemoveFirst_offset = 0;//单体过低解除一级
        public static double cellVolLowAlarmRemoveSecond_offset = 0;//单体过低解除二级
        public static double cellVolLowAlarmRemoveThird_offset = 0;//单体过低解除三级

        public static double cellTemperatureHighAlarmFirst_offset = -40;//温度过高一级
        public static double cellTemperatureHighAlarmSecond_offset = -40;//温度过高二级
        public static double cellTemperatureHighAlarmThird_offset = -40;//温度过高三级
        public static double cellTemperatureHighAlarmRemoveFirst_offset = -40;//温度过高解除一级
        public static double cellTemperatureHighAlarmRemoveSecond_offset = -40;//温度过高解除二级
        public static double cellTemperatureHighAlarmRemoveThird_offset = -40;//温度过高解除三级

        public static double cellTemperatureLowAlarmFirst_offset = -40;//温度过低一级
        public static double cellTemperatureLowAlarmSecond_offset = -40;//温度过低二级
        public static double cellTemperatureLowAlarmThird_offset = -40;//温度过低三级
        public static double cellTemperatureLowAlarmRemoveFirst_offset = -40;//温度过低解除一级
        public static double cellTemperatureLowAlarmRemoveSecond_offset = -40;//温度过低解除二级
        public static double cellTemperatureLowAlarmRemoveThird_offset = -40;//温度过低解除三级
        //info7
        public static double balanCurrentHighAlarmFirst_offset = 0;//均衡电流过高一级
        public static double balanCurrentHighAlarmSecond_offset = 0;//均衡电流过高二级
        public static double balanCurrentHighAlarmThird_offset = 0;//均衡电流过高三级
        //info8
        public static double balanCurrentHighAlarmRemoveFirst_offset = 0;//均衡电流过高解除一级
        public static double balanCurrentHighAlarmRemoveSecond_offset = 0;//均衡电流过高解除二级
        public static double balanCurrentHighAlarmRemoveThird_offset = 0;//均衡电流过高解除三级

        //info9
        public static double balanCurrentLowAlarmFirst_offset = 0;//均衡电流过低一级
        public static double balanCurrentLowAlarmSecond_offset = 0;//均衡电流过低二级
        public static double balanCurrentLowAlarmThird_offset = 0;//均衡电流过低三级
        //info10
        public static double balanCurrentLowAlarmRemoveFirst_offset = 0;//均衡电流过低解除一级
        public static double balanCurrentLowAlarmRemoveSecond_offset = 0;//均衡电流过低解除二级
        public static double balanCurrentLowAlarmRemoveThird_offset = 0;//均衡电流过低解除三级


        //info11
        public static double balanCurrentSetValueFirst_offset = 0;//均衡电流大小一级
        public static double balanCurrentSetValueSecond_offset = 0;//均衡电流大小二级
        public static double balanCurrentSetValueThird_offset = 0;//均衡电流大小三级

        //info12
        public static double balanVolOpenValue_offset = 0;//均衡开启电压
        //info13
        public static double balanVolCloseValue_offset = 0;//均衡截止电压
        //info14
        public static double balanVolDifOpenValue_offset = 0;//均衡开启压差
        //info15
        public static double balanVolDifCloseValue_offset = 0;//均衡截止压差

        //info16
        public static double cellBalanTemperatureOpenValue_offset = -40;//均衡电池开启温度
        public static double cellBalanTemperatureCloseValue_offset = -40;//均衡电池截止温度
        //info17(单独配置)
        public static double slaveNum_offset = 0;//从机编号
        public static double cellBalanMode_offset = 0;//均衡模式
        public static double childModuleMonCellNumber_offset = 0;//从机监控单体总数
        public static double childMonModuleTemperatureNumber_offset = 0;//从机监控温感总数
        public static double moduleAMonCellNum_offset = 0;//子模块A监控电池数目
        public static double moduleAMonTemperatureNum_offset = 0;//子模块A监控温感数目
        //info18
        public static double moduleBMonCellNum_offset = 0;//子模块B监控电池数目
        public static double moduleBMonTemperatureNum_offset = 0;//子模块B监控温感数目
        public static double moduleCMonCellNum_offset = 0;//子模块C监控电池数目
        public static double moduleCMonTemperatureNum_offset = 0;//子模块C监控温感数目
        public static double moduleDMonCellNum_offset = 0;//子模块D监控电池数目
        public static double moduleDMonTemperatureNum_offset = 0;//子模块D监控温感数目
        //info19
        public static double moduleEMonCellNum_offset = 0;//子模块E监控电池数目
        public static double moduleEMonTemperatureNum_offset = 0;//子模块E监控温感数目
        public static double packProYear_offset = 0;//电池组生产年份
        public static double packProMonth_offset = 0;//电池组生产月份
        public static double packProDay_offset = 0;//电池组生产日期
        //info20
        public static double serialNum_offset = 0;
        public static double packBatchNumberData1_offset = 0;//电池组项目批量号1
        public static double packBatchNumberData2_offset = 0;//电池组项目批量号2
        public static double packBatchNumberData3_offset = 0;//电池组项目批量号3
        public static double packBatchNumberData4_offset = 0;//电池组项目批量号4
        public static double packBatchNumberData5_offset = 0;//电池组项目批量号5
        public static double packBatchNumberData6_offset = 0;//电池组项目批量号6
        //Dia1
        public static double mON_PWM_SWP_offset = 0;
        public static double mON_PWM_SWP_Fre_offset = 0;
        public static double mON_VB_24V_offset = 0;
        public static double mON_VS_24V_offset = 0;
        public static double mON_EN_VB_24V_offset = 0;

        //Dia2
        public static double mON_EN_POWER_offset = 0;
        public static double mON_12VL_offset = 0;
        public static double mON_Vref_25VL_offset = 0;
        public static double mON_Vref_147VL_offset = 0;

        //Dia3
        public static double mON_Vref_353VL_offset = 0;
        public static double mON_T_AMB_offset = -40;
        public static double mON_ABC_PRI_0_offset = -10000;
        public static double mON_T_PRI_0_offset = -40;

        //Dia4
        public static double mON_5VH_1428_offset = 0;
        public static double mON_12VH_offset = 0;
        public static double mON_Vref_03VH_offset = 0;
        public static double mON_Vref_47VH_offset = 0;

        //Dia5
        public static double mON_ABC_SEC_offset = -10000;
        public static double mON_ABV_offset = 0;
        public static double mON_VREF2_offset = 0;
        public static double mON_VSET_offset = 0;

        //Dia6
        public static double mON_T_SEC_offset = -40;
        public static double mON_TV_Cell_offset = 0;
        public static double mON_FAULT_INT_1428_offset = 0;
        public static double mON_GATE_LS_0_offset = 0;
        public static double mON_GATE_LS_0_Fre_offset = 0;

        //Dia7
        public static double mON_ABC_PRI_1_offset = -10000;
        public static double mON_T_PRI_1_offset = -40;
        public static double mON_5VH_1428_1_offset = 0;
        public static double mON_12VH1_offset = 0;

        //Dia8
        public static double mON_Vref_03VH1_offset = 0;
        public static double mON_Vref_47VH1_offset = 0;
        public static double mON_ABC_SEC_1_offset = -10000;
        public static double mON_ABV_1_offset = 0;

        //Dia9
        public static double mON_VREF2_1_offset = 0;
        public static double mON_VSET_1_offset = 0;
        public static double mON_T_SEC_1_offset = -40;
        public static double mON_TV_Cell_1_offset = 0;

        //Dia10
        public static double mON_FAULT_INT_1428_1_offset = 0;
        public static double mON_GATE_LS_1_offset = 0;
        public static double mON_GATE_LS_1_Fre_offset = 0;
        public static double mON_ABC_PRI_2_offset = -10000;
        public static double mON_T_PRI_2_offset = -40;

        //Dia11
        public static double mON_5VH_1428_2_offset = 0;
        public static double mON_12VH2_offset = 0;
        public static double mON_Vref_03VH2_offset = 0;
        public static double mON_Vref_47VH2_offset = 0;

        //Dia12
        public static double mON_ABC_SEC_2_offset = -10000;
        public static double mON_ABV_2_offset = 0;
        public static double mON_VREF2_2_offset = 0;
        public static double mON_VSET_2_offset = 0;

        //Dia13
        public static double mON_T_SEC_2_offset = -40;
        public static double mON_TV_Cell_2_offset = 0;
        public static double mON_FAULT_INT_1428_2_offset = 0;
        public static double mON_GATE_LS_2_offset = 0;
        public static double mON_GATE_LS_2_Fre_offset = 0;


        //二代从机
        public static double bCNT_offset = 0;
        public static double sOC_offset = 0;
        public static double vTotal_offset = 0;
        public static double cVMax_offset = 0;
        public static double cVmin_offset = 0;

        public static double vol_offset = 0;
        public static double temSensor_offset = -40;
        public static double tb_offset = -40;
        public static double can_life_offset = 0;

        public static double pacMaxTemp_offset = -40;
        public static double pacMinTemp_offset = -40;
        //-----------------------------------------------------------------------------------------------
        //info1
        public static double cellVolHighAlarmFirst_rr = 0.001;//单体过高一级
        public static double cellVolHighAlarmSecond_rr = 0.001;//单体过高二级
        public static double cellVolHighAlarmThird_rr = 0.001;//单体过高三级

        public static double cellVolHighAlarmRemoveFirst_rr = 0.001;//单体过高解除一级
        public static double cellVolHighAlarmRemoveSecond_rr = 0.001;//单体过高解除二级
        public static double cellVolHighAlarmRemoveThird_rr = 0.001;//单体过高解除三级

        public static double cellVolLowAlarmFirst_rr = 0.001;//单体过低一级
        public static double cellVolLowAlarmSecond_rr = 0.001;//单体过低二级
        public static double cellVolLowAlarmThird_rr = 0.001;//单体过低三级
        //info4
        public static double cellVolLowAlarmRemoveFirst_rr = 0.001;//单体过低解除一级
        public static double cellVolLowAlarmRemoveSecond_rr = 0.001;//单体过低解除二级
        public static double cellVolLowAlarmRemoveThird_rr = 0.001;//单体过低解除三级

        public static double cellTemperatureHighAlarmFirst_rr = 1;//温度过高一级
        public static double cellTemperatureHighAlarmSecond_rr = 1;//温度过高二级
        public static double cellTemperatureHighAlarmThird_rr = 1;//温度过高三级
        public static double cellTemperatureHighAlarmRemoveFirst_rr = 1;//温度过高解除一级
        public static double cellTemperatureHighAlarmRemoveSecond_rr = 1;//温度过高解除二级
        public static double cellTemperatureHighAlarmRemoveThird_rr = 1;//温度过高解除三级

        public static double cellTemperatureLowAlarmFirst_rr = 1;//温度过低一级
        public static double cellTemperatureLowAlarmSecond_rr = 1;//温度过低二级
        public static double cellTemperatureLowAlarmThird_rr = 1;//温度过低三级
        public static double cellTemperatureLowAlarmRemoveFirst_rr = 1;//温度过低解除一级
        public static double cellTemperatureLowAlarmRemoveSecond_rr = 1;//温度过低解除二级
        public static double cellTemperatureLowAlarmRemoveThird_rr = 1;//温度过低解除三级
        //info7
        public static double balanCurrentHighAlarmFirst_rr = 0.001;//均衡电流过高一级
        public static double balanCurrentHighAlarmSecond_rr = 0.001;//均衡电流过高二级
        public static double balanCurrentHighAlarmThird_rr = 0.001;//均衡电流过高三级

        //info8
        public static double balanCurrentHighAlarmRemoveFirst_rr = 0.001;//均衡电流过高解除一级
        public static double balanCurrentHighAlarmRemoveSecond_rr = 0.001;//均衡电流过高解除二级
        public static double balanCurrentHighAlarmRemoveThird_rr = 0.001;//均衡电流过高解除三级

        //info9
        public static double balanCurrentLowAlarmFirst_rr = 0.001;//均衡电流过低一级
        public static double balanCurrentLowAlarmSecond_rr = 0.001;//均衡电流过低二级
        public static double balanCurrentLowAlarmThird_rr = 0.001;//均衡电流过低三级
        //info10
        public static double balanCurrentLowAlarmRemoveFirst_rr = 0.001;//均衡电流过低解除一级
        public static double balanCurrentLowAlarmRemoveSecond_rr = 0.001;//均衡电流过低解除二级
        public static double balanCurrentLowAlarmRemoveThird_rr = 0.001;//均衡电流过低解除三级
        //info11
        public static double balanCurrentSetValueFirst_rr = 0.001;//均衡电流大小一级
        public static double balanCurrentSetValueSecond_rr = 0.001;//均衡电流大小二级
        public static double balanCurrentSetValueThird_rr = 0.001;//均衡电流大小三级
        //info12
        public static double balanVolOpenValue_rr = 0.001;//均衡开启电压
        //info13
        public static double balanVolCloseValue_rr = 0.001;//均衡截止电压
        //info14
        public static double balanVolDifOpenValue_rr = 0.001;//均衡开启压差
        //info15
        public static double balanVolDifCloseValue_rr = 0.001;//均衡截止压差


        //info16
        public static double cellBalanTemperatureOpenValue_rr = 1;//均衡电池开启温度
        public static double cellBalanTemperatureCloseValue_rr = 1;//均衡电池截止温度
        //info17(单独配置)
        public static double slaveNum_rr = 1;//从机编号
        public static double cellBalanMode_rr = 1;//均衡模式
        public static double childModuleMonCellNumber_rr = 1;//从机监控单体总数
        public static double childMonModuleTemperatureNumber_rr = 1;//从机监控温感总数
        public static double moduleAMonCellNum_rr = 1;//子模块A监控电池数目
        public static double moduleAMonTemperatureNum_rr = 1;//子模块A监控温感数目
        //info18
        public static double moduleBMonCellNum_rr = 1;//子模块B监控电池数目
        public static double moduleBMonTemperatureNum_rr = 1;//子模块B监控温感数目
        public static double moduleCMonCellNum_rr = 1;//子模块C监控电池数目
        public static double moduleCMonTemperatureNum_rr = 1;//子模块C监控温感数目
        public static double moduleDMonCellNum_rr = 1;//子模块D监控电池数目
        public static double moduleDMonTemperatureNum_rr = 1;//子模块D监控温感数目
        //info19
        public static double moduleEMonCellNum_rr = 1;//子模块E监控电池数目
        public static double moduleEMonTemperatureNum_rr = 1;//子模块E监控温感数目
        public static double packProYear_rr = 1;//电池组生产年份
        public static double packProMonth_rr = 1;//电池组生产月份
        public static double packProDay_rr = 1;//电池组生产日期
        //info20
        public static double serialNum_rr = 1;
        public static double packBatchNumberData1_rr = 1;//电池组项目批量号1
        public static double packBatchNumberData2_rr = 1;//电池组项目批量号2
        public static double packBatchNumberData3_rr = 1;//电池组项目批量号3
        public static double packBatchNumberData4_rr = 1;//电池组项目批量号4
        public static double packBatchNumberData5_rr = 1;//电池组项目批量号5
        public static double packBatchNumberData6_rr = 1;//电池组项目批量号6
        //Dia1
        public static double mON_PWM_SWP_rr = 1;
        public static double mON_PWM_SWP_Fre_rr = 0.1;//单位kHZ
        public static double mON_VB_24V_rr = 0.001;
        public static double mON_VS_24V_rr = 0.001;
        public static double mON_EN_VB_24V_rr = 0.001;
        //Dia2
        public static double mON_EN_POWER_rr = 0.001;
        public static double mON_12VL_rr = 0.001;
        public static double mON_Vref_25VL_rr = 0.001;
        public static double mON_Vref_147VL_rr = 0.001;
        //Dia3
        public static double mON_Vref_353VL_rr = 0.001;
        public static double mON_T_AMB_rr = 1;
        public static double mON_ABC_PRI_0_rr = 0.001;
        public static double mON_T_PRI_0_rr = 1;
        //Dia4
        public static double mON_5VH_1428_rr = 0.001;
        public static double mON_12VH_rr = 0.001;
        public static double mON_Vref_03VH_rr = 0.001;
        public static double mON_Vref_47VH_rr = 0.001;
        //Dia5
        public static double mON_ABC_SEC_rr = 0.001;
        public static double mON_ABV_rr = 0.001;
        public static double mON_VREF2_rr = 0.001;
        public static double mON_VSET_rr = 0.001;

        //Dia6
        public static double mON_T_SEC_rr = 1;
        public static double mON_TV_Cell_rr = 0.001;
        public static double mON_FAULT_INT_1428_rr = 1;
        public static double mON_GATE_LS_0_rr = 1;
        public static double mON_GATE_LS_0_Fre_rr = 0.1;
        //Dia7
        public static double mON_ABC_PRI_1_rr = 0.001;
        public static double mON_T_PRI_1_rr = 1;
        public static double mON_5VH_1428_1_rr = 0.001;
        public static double mON_12VH1_rr = 0.001;
        //Dia8
        public static double mON_Vref_03VH1_rr = 0.001;
        public static double mON_Vref_47VH1_rr = 0.001;
        public static double mON_ABC_SEC_1_rr = 0.001;
        public static double mON_ABV_1_rr = 0.001;

        //Dia9
        public static double mON_VREF2_1_rr = 0.001;
        public static double mON_VSET_1_rr = 0.001;
        public static double mON_T_SEC_1_rr = 1;
        public static double mON_TV_Cell_1_rr = 0.001;

        //Dia10
        public static double mON_FAULT_INT_1428_1_rr = 1;
        public static double mON_GATE_LS_1_rr = 1;
        public static double mON_GATE_LS_1_Fre_rr = 0.1;
        public static double mON_ABC_PRI_2_rr = 0.001;
        public static double mON_T_PRI_2_rr = 1;

        //Dia11
        public static double mON_5VH_1428_2_rr = 0.001;
        public static double mON_12VH2_rr = 0.001;
        public static double mON_Vref_03VH2_rr = 0.001;
        public static double mON_Vref_47VH2_rr = 0.001;

        //Dia12
        public static double mON_ABC_SEC_2_rr = 0.001;
        public static double mON_ABV_2_rr = 0.001;
        public static double mON_VREF2_2_rr = 0.001;
        public static double mON_VSET_2_rr = 0.001;

        //Dia13
        public static double mON_T_SEC_2_rr = 1;
        public static double mON_TV_Cell_2_rr = 0.001;
        public static double mON_FAULT_INT_1428_2_rr = 1;
        public static double mON_GATE_LS_2_rr = 1;
        public static double mON_GATE_LS_2_Fre_rr = 0.1;

        //二代从机
        public static double bCNT_rr = 1;
        public static double sOC_rr = 0.004;
        public static double vTotal_rr = 10;
        public static double cVMax_rr = 1;
        public static double cVmin_rr = 1;

        public static double vol_rr = 1;
        public static double temSensor_rr = 1;
        public static double tb_rr = 1;
        public static double can_life_rr = 1;

        public static double pacMaxTemp_rr = 1;
        public static double pacMinTemp_rr = 1;
    }
}

