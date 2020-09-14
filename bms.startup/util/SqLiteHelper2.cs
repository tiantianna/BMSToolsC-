﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace bms.startup.util
{
    /// <summary>
    /// SQLite 操作类
    /// </summary>
    public class SqLiteHelper
    {

        private static SqLiteHelper _instance = null;
        private static readonly object locker = new object();
        private string _connectionString;
        private SqLiteHelper() { }

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


        public void OpenConnection(string connectionString)
        {
            try
            {
                if (dbConnection == null)
                {
                    dbConnection = new SQLiteConnection(connectionString);
                    dbConnection.Open();
                }

            }
            catch (Exception e)
            {
                Log(e.ToString());
            }

        }



        /// <summary>
        /// 数据库连接定义
        /// </summary>
        private SQLiteConnection dbConnection;

        /// <summary>
        /// SQL命令定义
        /// </summary>
        private SQLiteCommand dbCommand;

        /// <summary>
        /// 数据读取定义
        /// </summary>
        private SQLiteDataReader dataReader;

        private string connectionString;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接SQLite库字符串</param>
        public SqLiteHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        //public SQLiteConnection OpenConnection()
        //{
        //    dbConnection = new SQLiteConnection(connectionString);
        //    return dbConnection;
        //}

        //public  void dbConnect(string connectionString){
        //Console.WriteLine(dbConnection==null);
        //    try
        //    {
        //        dbConnection = new SQLiteConnection(connectionString);
        //        dbConnection.Open();
        //    }
        //    catch (Exception e)
        //    {
        //        Log(e.ToString());
        //    }
        //}

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <returns>The query.</returns>
        /// <param name="queryString">SQL命令字符串</param>
        //public SQLiteDataReader ExecuteQuery(string queryString)
        //{
        //    try
        //    {
        //        dbCommand = dbConnection.CreateCommand();
        //        dbCommand.CommandText = queryString;
        //        dataReader = dbCommand.ExecuteReader();
        //    }
        //    catch (Exception e)
        //    {
        //        Log(e.Message);
        //    }

        //    return dataReader;
        //}
        public SQLiteDataReader ExecuteQuery(string queryString)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = queryString;
                    dataReader = cmd.ExecuteReader();
                    dataReader.Close();
                    conn.Close();
                    return dataReader;
                }
            }
        }
        /// <summary>
        /// 使用占位符的执行语句
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteQuery2(string queryString, Dictionary<string, string> dic)
        {
            try
            {
                dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = queryString;
                foreach (string key in dic.Keys)
                {
                    dbCommand.Parameters.Add(new SQLiteParameter(key, dic[key]));
                }
                dataReader = dbCommand.ExecuteReader();

            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            return dataReader;
        }


        /// <summary>
        /// 带占位符的插入
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public int ExecuteInsert(string queryString, Dictionary<string, string> dic)
        {
            int rowsUpdated = -1;
            try
            {
                dbCommand = new SQLiteCommand(dbConnection) { CommandText = queryString };
               // dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = queryString;
                foreach (string key in dic.Keys)
                {
                    dbCommand.Parameters.Add(new SQLiteParameter(key, dic[key]));
                }
                //dataReader = dbCommand.ExecuteReader();
                rowsUpdated = dbCommand.ExecuteNonQuery();

               // var cmd = new SQLiteCommand(dbConnection) { CommandText = queryString };
               // var rowsUpdated = cmd.ExecuteNonQuery();
               // return rowsUpdated;

            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            return rowsUpdated;
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
            //销毁Commend
            if (dbCommand != null)
            {
                dbCommand.Cancel();
            }
            dbCommand = null;
            //销毁Reader
            if (dataReader != null)
            {
                dataReader.Close();
            }
            dataReader = null;
            //销毁Connection
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
            dbConnection = null;

        }

        /// <summary>
        /// 读取整张数据表
        /// </summary>
        /// <returns>The full table.</returns>
        /// <param name="tableName">数据表名称</param>
        public SQLiteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return ExecuteQuery(queryString);
        }


        /// <summary>
        /// 向指定数据表中插入数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="values">插入的数值</param>
        public int InsertValues(string tableName, string[] values)
        {
            //获取数据表中字段数目
            //int fieldCount = ReadFullTable(tableName).FieldCount;
            ////当插入的数据长度不等于字段数目时引发异常
            //if (values.Length != fieldCount)
            //{
            //    throw new SQLiteException("values.Length!=fieldCount");
            //}

            //string queryString = "INSERT INTO " + tableName + " VALUES (" + "'" + values[0] + "'";
            //string queryString = "INSERT INTO " + tableName + " VALUES (";
            //for (int i = 0; i < values.Length; i++)
            //{
            //    if (i == 0) { queryString += (values[i] == null ? "null" : "'" + values[i] + "'"); }
            //    else { queryString += (values[i] == null ? "null" : ", " + "'" + values[i] + "'"); }
            //}
            //queryString += " )";
            //Console.WriteLine(queryString);

            //return ExecuteNonQuery(queryString);

            string queryString = "INSERT INTO " + tableName + " VALUES (" + "'" + values[0] + "'";
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + "'" + values[i] + "'";
            }
            queryString += " )";
            return ExecuteNonQuery(queryString);
        }
        public int InsertValues2(string tableName, string[] values)
        {
            //获取数据表中字段数目
            //int fieldCount = ReadFullTable(tableName).FieldCount;
            ////当插入的数据长度不等于字段数目时引发异常
            //if (values.Length != fieldCount)
            //{
            //    throw new SQLiteException("values.Length!=fieldCount");
            //}

            string queryString = "INSERT INTO " + tableName + " VALUES (" + "'" + values[0] + "'";
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + "'" + values[i] + "'";
            }
            queryString += " )";
            return ExecuteNonQuery(queryString);
        }

        public int ExecuteNonQuery(string queryString)
        {
            var cmd = new SQLiteCommand(dbConnection) { CommandText = queryString };
            var rowsUpdated = cmd.ExecuteNonQuery();
            return rowsUpdated;
        }
        /// <summary>
        /// 更新指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        /// <param name="key">关键字</param>
        /// <param name="value">关键字对应的值</param>
        /// <param name="operation">运算符：=,<,>,...，默认“=”</param>
        public SQLiteDataReader UpdateValues(string tableName, string[] colNames, string[] colValues, string key, string value, string operation = "=")
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length");
            }

            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + "'" + colValues[i] + "'";
            }
            queryString += " WHERE " + key + operation + "'" + value + "'";
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public SQLiteDataReader DeleteValuesOR(string tableName, string[] colNames, string[] colValues, string[] operations)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[0] + "'" + colValues[i] + "'";
            }
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public SQLiteDataReader DeleteValuesAND(string tableName, string[] colNames, string[] colValues, string[] operations)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += " AND " + colNames[i] + operations[i] + "'" + colValues[i] + "'";
            }
            return ExecuteQuery(queryString);
        }


        /// <summary>
        /// 创建数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public SQLiteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
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
        /// Reads the table.
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="tableName">Table name.</param>
        /// <param name="items">Items.</param>
        /// <param name="colNames">Col names.</param>
        /// <param name="operations">Operations.</param>
        /// <param name="colValues">Col values.</param>
        public SQLiteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 本类log
        /// </summary>
        /// <param name="s"></param>
        void Log(string s)
        {
            Console.WriteLine("class SqLiteHelper:::" + s);
        }
    }
}
