using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    class Test
    {

        private string[] titles = { "终端编号",
                                    "车牌号",
                                    "总电压",
                                    "总电流",
                                    "获取时间", 
                                    "SOC", 
                                    "最高单体电压", 
                                    "最高单体电压箱号",
                                    "最高单体电压节号",
                                    "最低单体电压",  
                                    "最低单体电压箱号",
                                    "最低单体电压节号",
                                    "平均电压",
                                    "压差",
                                    "最高温度", 
                                    "最高温度箱号",
                                    "最高温度节号", 
                                    "最低温度",
                                    "最低温度箱号",
                                    "最低温度节号",
                                    "温差"
                                  };


        private DataTable InitDataTabel()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BWTID", Type.GetType("System.String"));
            dt.Columns.Add("CARNUMBER", Type.GetType("System.String"));
            //dt.Columns.Add("CODEID", Type.GetType("System.String"));
            dt.Columns.Add("TOTALVOLTAGE", Type.GetType("System.String"));
            dt.Columns.Add("TOTALCURRENT", Type.GetType("System.String"));
            dt.Columns.Add("RECEIVETIME", Type.GetType("System.String"));
            dt.Columns.Add("SOC", Type.GetType("System.String"));
            dt.Columns.Add("MAXVOLTAGE", Type.GetType("System.String")); //最高单体电压
            dt.Columns.Add("MAXVOLTAGEBOXNUM", Type.GetType("System.String")); //最高单体电压箱号
            dt.Columns.Add("MAXVOLTAGEBATTERYNUM", Type.GetType("System.String")); //最高单体电压节号
            dt.Columns.Add("MINVOLTAGE", Type.GetType("System.String")); //最低单体电压
            dt.Columns.Add("MINVOLTAGEBOXNUM", Type.GetType("System.String")); //最低单体电压箱号
            dt.Columns.Add("MINVOLTAGEBATTERYNUM", Type.GetType("System.String")); //最低单体电压节号
            dt.Columns.Add("NORMALVOLTAGE", Type.GetType("System.String"));
            dt.Columns.Add("ABVOLTAGE", Type.GetType("System.String"));
            dt.Columns.Add("MAXTEMPERATURE", Type.GetType("System.String"));
            dt.Columns.Add("MAXTEMPERATUREBOXNUM", Type.GetType("System.String"));
            dt.Columns.Add("MAXTEMPERATUREBATTERYNUM", Type.GetType("System.String"));
            dt.Columns.Add("MINTEMPERATURE", Type.GetType("System.String"));
            dt.Columns.Add("MINTEMPERATUREBOXNUM", Type.GetType("System.String"));
            dt.Columns.Add("MINTEMPERATUREBATTERYNUM", Type.GetType("System.String"));
            dt.Columns.Add("ABTEMPERATURE", Type.GetType("System.String"));
            return dt;

            //使用
            DataRow dr = dt.NewRow();
            dr["CELl1"] = "";
            dt.Rows.Add(dr);

            CsvHelper export = new CsvHelper(titles);
            export.DataToCSV("c:\\234.csv", dt);
        }


    }
}
