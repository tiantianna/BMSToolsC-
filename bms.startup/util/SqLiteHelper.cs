using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    class SqLiteHelper
    {
        private static SqLiteHelper _instance = null;
        private static readonly object locker = new object();
        private string _connectionString;
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }
        private SqLiteHelper() { }

        // 数据库连接定义
        private SQLiteConnection dbConnection;
        // SQL命令定义
        private SQLiteCommand dbCommand;
        // 数据读取定义
        private SQLiteDataReader dataReader;


        /// <summary>
        /// 单例模式
        /// 必须通过同一个实例进行sqlLite数据库操作
        /// zhengzhonghua && GaoYa 2018-05-21 16:24
        /// </summary>
        public static SqLiteHelper getInstance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new SqLiteHelper();
                    }
                    return _instance;

                }
            }
        }

        /// <summary>
        /// 创建数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public DataTable CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            string queryString = "CREATE TABLE IF NOT EXISTS " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";

                  
            return ExecuteQuery(queryString);
        }


        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string queryString)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryString;
                    using (dataReader = cmd.ExecuteReader())
                    {
                        DataTable datatable = new DataTable();
                        ///动态添加表的数据列  
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            DataColumn myDataColumn = new DataColumn();
                            myDataColumn.DataType = dataReader.GetFieldType(i);
                            myDataColumn.ColumnName = dataReader.GetName(i);
                            datatable.Columns.Add(myDataColumn);
                        }

                        ///添加表的数据  
                        while (dataReader.Read())
                        {
                            DataRow myDataRow = datatable.NewRow();
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                myDataRow[i] = dataReader[i].ToString();
                            }
                            datatable.Rows.Add(myDataRow);
                            myDataRow = null;
                        }
                        return datatable;
                    }
                }
            }
        }

        /// <summary>
        /// 使用占位符的执行语句
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string queryString, Dictionary<string, string> dic)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryString;
                    foreach (string key in dic.Keys)
                    {
                        cmd.Parameters.Add(new SQLiteParameter(key, dic[key]));
                    }
                    using (dataReader = cmd.ExecuteReader())
                    {
                        DataTable datatable = new DataTable();
                        ///动态添加表的数据列  
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            DataColumn myDataColumn = new DataColumn();
                            myDataColumn.DataType = dataReader.GetFieldType(i);
                            myDataColumn.ColumnName = dataReader.GetName(i);
                            datatable.Columns.Add(myDataColumn);
                        }

                        ///添加表的数据  
                        while (dataReader.Read())
                        {
                            DataRow myDataRow = datatable.NewRow();
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                myDataRow[i] = dataReader[i].ToString();
                            }
                            datatable.Rows.Add(myDataRow);
                            myDataRow = null;
                        }
                        return datatable;
                    }
                }
            }
        }


        /// <summary>
        /// 插入完整表数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int InsertValues(string tableName, string[] values)
        {

            string queryString = "INSERT INTO " + tableName + " VALUES (";
            for (int i = 0; i < values.Length; i++)
            {
                if (i == 0) { queryString += (values[i] == null ? "null" : "'" + values[i] + "'"); }
                else { queryString += (values[i] == null ? "null" : ", " + "'" + values[i] + "'"); }
            }
            queryString += " )";     
            return ExecuteNonQuery(queryString);
        }
       

        /// <summary>
        /// 非查询命令
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string queryString)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryString;
                    var rowsUpdated = cmd.ExecuteNonQuery();
                    conn.Close();
                    return rowsUpdated;
                }
            }

        }

        /// <summary>
        /// 带占位符的非查询命令
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string queryString, Dictionary<string, string> dic)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryString;
                    foreach (string key in dic.Keys)
                    {
                        cmd.Parameters.Add(new SQLiteParameter(key, dic[key]));
                    }
                    var rowsUpdated = cmd.ExecuteNonQuery();
                    conn.Close();
                    return rowsUpdated;
                }
            }

        }



    }
}
