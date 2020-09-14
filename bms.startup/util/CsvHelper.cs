using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace bms.startup
{
    public class CsvHelper
    {
        #region Fields
        string _fileName;
        DataTable _dataSource;//数据源
        string[] _titles = null;//列标题
        string[] _fields = null;//字段名
        private bool _inittitle = false;
        #endregion
 
        #region .ctor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataSource">数据源</param>
        public CsvHelper()
        {
            
        }
 
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="titles">要输出到 Excel 的列标题的数组</param>
        /// <param name="fields">要输出到 Excel 的字段名称数组</param>
        /// <param name="dataSource">数据源</param>
        public CsvHelper(string[] titles, string[] fields, DataTable dataSource)
            : this(titles, dataSource)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentNullException("fields");
            if (titles.Length != fields.Length)
                throw new ArgumentException("titles.Length != fields.Length", "fields");
 
            _fields = fields;
        }

        public CsvHelper(string[] titles, DataTable dataSource)
            : this(dataSource)
        {
            if (titles == null || titles.Length == 0)
                throw new ArgumentNullException("titles");

            _titles = titles;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="titles">要输出到 Excel 的列标题的数组</param>
        /// <param name="dataSource">数据源</param>
        public CsvHelper(string[] titles)
        {
            if (titles == null || titles.Length == 0)
                _titles = null;
            _titles = titles;
        }
 
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataSource">数据源</param>
        public CsvHelper(DataTable dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            _dataSource = dataSource;
        }
 
        #endregion
 
        #region public Methods
 
        #region  导出到CSV文件并且提示下载
        /// <summary>
        /// 导出到CSV文件
        /// </summary>
        /// <param name="fileName"></param>
        public bool DataToCSV(string fileName, DataTable dataSource)
        {
            // 确保有一个合法的输出文件名
            FileStream fs = null;
            StreamWriter sw = null;
            try {
                if (fileName == null || fileName == string.Empty || !(fileName.ToLower().EndsWith(".csv")))
                    fileName = GetRandomFileName();
                string data = ExportCSV(dataSource);
                fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.Default);
                sw.Write(data);
                sw.Flush();
                return true;
            }
            catch (Exception ex) {
                return false;
            }
            finally {
                if(fs!=null)
                    sw.Close();
                if(sw!=null)
                    fs.Close();
            }
          
        }


        public bool ExportToCSV(string fileName,DataTable dt)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.GetEncoding("gb2312")))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sb.Append(dt.Columns[i].ColumnName.ToString() + "\t");
                    }
                    sb.Append(Environment.NewLine);

                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            sb.Append(dt.Rows[r][c].ToString() + "\t");
                        }
                        sb.Append(Environment.NewLine);
                    }
                    sw.Write(sb.ToString());
                    sw.Flush();
                    sw.Close();
                }
                return true;
            }
            catch (Exception ex) 
            {
                return false;
            }
          
        }

        #endregion
 
        /// <summary>
        /// 获取CSV导入的数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称(.csv不用加)</param>
        /// <returns></returns>
        public DataTable GetCsvData(string filePath,string fileName)
        {
            string path = Path.Combine(filePath, fileName + ".csv");
            string connString = @"Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" + filePath + ";Extensions=asc,csv,tab,txt;";
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (OdbcConnection odbcConn = new OdbcConnection(connString))
                {
                    odbcConn.Open();
                    OdbcCommand oleComm = new OdbcCommand();
                    oleComm.Connection = odbcConn;
                    oleComm.CommandText = "select * from [" + fileName + "#csv]";
                    OdbcDataAdapter adapter = new OdbcDataAdapter(oleComm);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, fileName);
                    odbcConn.Close();
                    return ds.Tables[0];
                }

             
            }
            catch (Exception ex)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                throw ex;
            }
        }
        #endregion


        public string ExportCSV2(DataTable dataSource) {
            if (dataSource == null)
                return null;

            StringBuilder strbData = new StringBuilder();
            if (_titles == null)
            { 
            
            }
            return null;
        }

        #region 返回写入CSV的字符串
        /// <summary>
        /// 返回写入CSV的字符串
        /// </summary>
        /// <returns></returns>
        public string ExportCSV(DataTable dataSource)
        {
            if (dataSource == null)
                return null;
 
            StringBuilder strbData = new StringBuilder();
            if (_titles == null)
            {
                //添加列名
                if (!_inittitle)
                {
                    foreach (DataColumn column in dataSource.Columns)
                    {
                        strbData.Append(column.ColumnName + ",");
                    }
                    strbData.Append("\n");
                    _inittitle = true;
                }
              
              
                foreach (DataRow dr in dataSource.Rows)
                {
                    for (int i = 0; i < dataSource.Columns.Count; i++)
                    {
                        strbData.Append(dr[i].ToString() + ",");
                    }
                    strbData.Append("\n");
                }
                return strbData.ToString();
            }
            else
            {
                if (!_inittitle) {
                    foreach (string columnName in _titles)
                    {
                        strbData.Append(columnName + ",");
                    }
                    strbData.Append("\n");
                    _inittitle = true;
                }
            
                if (_fields == null)
                {
                    foreach (DataRow dr in dataSource.Rows)
                    {
                        for (int i = 0; i < dataSource.Columns.Count; i++)
                        {
                            strbData.Append(dr[i].ToString() + ",");
                        }
                        strbData.Append("\n");
                    }
                    return strbData.ToString();
                }
                else
                {
                    foreach (DataRow dr in dataSource.Rows)
                    {
                        for (int i = 0; i < _fields.Length; i++)
                        {
                            strbData.Append(_fields[i].ToString() + ",");
                        }
                        strbData.Append("\n");
                    }
                    return strbData.ToString();
                }
            }
        }
        #endregion
 
        #region 得到一个随意的文件名
        /// <summary>
        /// 得到一个随意的文件名
        /// </summary>
        /// <returns></returns>
        private string GetRandomFileName()
        {
            Random rnd = new Random((int)(DateTime.Now.Ticks));
            string s = rnd.Next(Int32.MaxValue).ToString();
            return DateTime.Now.ToShortDateString() + "_" + s + ".csv";
        }
        #endregion

    }
}
