using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    class DefaultValue
    {
        public static string cellVolHighAlarmFirst_dv = "3.7";//单体过高一级
        public static string cellVolHighAlarmSecond_dv = "3.8";//单体过高二级
        public static string cellVolHighAlarmThird_dv = "3.9";//单体过高三级

        public static string cellVolHighAlarmRemoveFirst_dv = "3.7";//单体过高解除一级
        public static string cellVolHighAlarmRemoveSecond_dv = "3.8";//单体过高解除二级
        public static string cellVolHighAlarmRemoveThird_dv = "3.9";//单体过高解除三级

        public static string cellVolLowAlarmFirst_dv = "2.8";//单体过低一级
        public static string cellVolLowAlarmSecond_dv = "2.5";//单体过低二级
        public static string cellVolLowAlarmThird_dv = "2.3";//单体过低三级

        public static string cellVolLowAlarmRemoveFirst_dv = "3.0";//单体过低解除一级
        public static string cellVolLowAlarmRemoveSecond_dv = "2.7";//单体过低解除二级
        public static string cellVolLowAlarmRemoveThird_dv = "2.4";//单体过低解除三级

        public static string cellTemperatureHighAlarmFirst_dv = "50";//温度过高一级
        public static string cellTemperatureHighAlarmSecond_dv = "55";//温度过高二级
        public static string cellTemperatureHighAlarmThird_dv = "60";//温度过高三级
        public static string cellTemperatureHighAlarmRemoveFirst_dv = "46";//温度过高解除一级
        public static string cellTemperatureHighAlarmRemoveSecond_dv = "51";//温度过高解除二级
        public static string cellTemperatureHighAlarmRemoveThird_dv = "56";//温度过高解除三级

        public static string cellTemperatureLowAlarmFirst_dv = "0";//温度过低一级
        public static string cellTemperatureLowAlarmSecond_dv = "-10";//温度过低二级
        public static string cellTemperatureLowAlarmThird_dv = "-15";//温度过低三级
        public static string cellTemperatureLowAlarmRemoveFirst_dv = "5";//温度过低解除一级
        public static string cellTemperatureLowAlarmRemoveSecond_dv = "-5";//温度过低解除二级
        public static string cellTemperatureLowAlarmRemoveThird_dv = "-11";//温度过低解除三级
        //info7
        public static string balanCurrentHighAlarmFirst_dv ="";//均衡电流过高一级
        public static string balanCurrentHighAlarmSecond_dv ="";//均衡电流过高二级
        public static string balanCurrentHighAlarmThird_dv ="";//均衡电流过高三级
        //info8
        public static string balanCurrentHighAlarmRemoveFirst_dv ="" ;//均衡电流过高解除一级
        public static string balanCurrentHighAlarmRemoveSecond_dv ="";//均衡电流过高解除二级
        public static string balanCurrentHighAlarmRemoveThird_dv ="";//均衡电流过高解除三级

        //info9
        public static string balanCurrentLowAlarmFirst_dv =""  ;//均衡电流过低一级
        public static string balanCurrentLowAlarmSecond_dv =""  ;//均衡电流过低二级
        public static string balanCurrentLowAlarmThird_dv =""  ;//均衡电流过低三级
        //info1 
        public static string balanCurrentLowAlarmRemoveFirst_dv =""  ;//均衡电流过低解除一级
        public static string balanCurrentLowAlarmRemoveSecond_dv =""  ;//均衡电流过低解除二级
        public static string balanCurrentLowAlarmRemoveThird_dv =""  ;//均衡电流过低解除三级


        //info11
        public static string balanCurrentSetValueFirst_dv ="";//均衡电流大小一级
        public static string balanCurrentSetValueSecond_dv ="";//均衡电流大小二级
        public static string balanCurrentSetValueThird_dv ="";//均衡电流大小三级

        //info12
        public static string balanVolOpenValue_dv ="";//均衡开启电压
        //info13
        public static string balanVolCloseValue_dv =""  ;//均衡截止电压
        //info14
        public static string balanVolDifOpenValue_dv =""  ;//均衡开启压差
        //info15
        public static string balanVolDifCloseValue_dv =""  ;//均衡截止压差

        //info16
        public static string cellBalanTemperatureOpenValue_dv =""   ;//均衡电池开启温度
        public static string cellBalanTemperatureCloseValue_dv =""   ;//均衡电池截止温度
        //info17(单独配置)
        public static string slaveNum_dv =""  ;//从机编号
        public static string cellBalanMode_dv =""  ;//均衡模式
        public static string childModuleMonCellNumber_dv =""  ;//从机监控单体总数
        public static string childMonModuleTemperatureNumber_dv =""  ;//从机监控温感总数
        public static string moduleAMonCellNum_dv =""  ;//子模块A监控电池数目
        public static string moduleAMonTemperatureNum_dv =""  ;//子模块A监控温感数目
        //info18
        public static string moduleBMonCellNum_dv =""  ;//子模块B监控电池数目
        public static string moduleBMonTemperatureNum_dv =""  ;//子模块B监控温感数目
        public static string moduleCMonCellNum_dv =""  ;//子模块C监控电池数目
        public static string moduleCMonTemperatureNum_dv =""  ;//子模块C监控温感数目
        public static string moduleDMonCellNum_dv =""  ;//子模块D监控电池数目
        public static string moduleDMonTemperatureNum_dv =""  ;//子模块D监控温感数目
        //info19
        public static string moduleEMonCellNum_dv =""  ;//子模块E监控电池数目
        public static string moduleEMonTemperatureNum_dv =""  ;//子模块E监控温感数目
        public static string packProYear_dv =""  ;//电池组生产年份
        public static string packProMonth_dv =""  ;//电池组生产月份
        public static string packProDay_dv =""  ;//电池组生产日期
        //info2 
        public static string packBatchNumberData1_dv =""  ;//电池组项目批量号1
        public static string packBatchNumberData2_dv =""  ;//电池组项目批量号2
        public static string packBatchNumberData3_dv =""  ;//电池组项目批量号3
        public static string packBatchNumberData4_dv =""  ;//电池组项目批量号4
        public static string packBatchNumberData5_dv =""  ;//电池组项目批量号5
        public static string packBatchNumberData6_dv =""  ;//电池组项目批量号6
        public static string serialNum_dv = "";//16进制6位字符表示的批量号
    }
}
