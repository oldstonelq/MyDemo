using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.FileHelp
{
    /// <summary>
    /// sqlite路径库文件操作
    /// </summary>
    public class SQLiteHelp : IDisposable
    {
        //默认密码
        private const string passWord = "LA-8888";
        //private const string passWord = "";
        private static bool mLogrecording = false;

        #region 字段结构
        /// <summary>
        /// 表字段结构体
        /// </summary>
        public struct ColumnStruct
        {
            /// <summary>
            /// 字段名
            /// </summary>
            public string columnName;
            /// <summary>
            /// 字段类型
            /// </summary>
            public ColumnType columnType;
        }
        /// <summary>
        /// 所有字段类型
        /// </summary>
        public enum ColumnType
        {
            TEXT,
            NUMERIC,
            INTEGER,
            REAL,
            NONE,
            AUTOADD,        //自增
        }
        #endregion

        #region Disposse
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region 更换密码
        /// <summary>
        /// 更换数据密码
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="BeforePassword">之前密码</param>
        /// <param name="NewPassWord">更改后密码</param>
        public static void ChanngePassword(string dataSource, string BeforePassword, string NewPassWord)
        {
            if (!File.Exists(dataSource))
            {
                throw new Exception("数据文件不存在");
            }
            else
            {
                using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + BeforePassword))
                {
                    try
                    {
                        TempCon.Open();
                        TempCon.ChangePassword(NewPassWord);
                        TempCon.Close();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        #endregion

        #region 创建新表
        /// <summary>
        /// 创建新表,如果数据库不存在,则自动建立
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="tableName">表名</param>
        /// <param name="columns">所有列名</param>
        /// <param name="primarykeyIndex">主键的字段序号,为0时不创建主键</param>
        public static void CreateTable(string dataSource, string tableName, ColumnStruct[] columns, int primarykeyIndex)
        {
            if (tableName == "")
            {
                return;
            }
            if (columns.Length <= 0)
            {
                return;
            }
            if (primarykeyIndex > columns.Length || primarykeyIndex < 0)
            {
                return;
            }
            if (columns.Count(obj => obj.columnType == ColumnType.AUTOADD) > 1)
            {
                return;
            }
            if (columns.Count(obj => obj.columnType == ColumnType.AUTOADD) == 1 && primarykeyIndex != 0)
            {
                return;
            }
            using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord))
            {
                try
                {
                    TempCon.Open();
                    string TempSql = "";
                    for (int i = 0; i < columns.Length; i++)
                    {
                        if (i != columns.Length - 1)
                        {
                            if (columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + columns[i].columnName + " INTEGER PRIMARY KEY,";
                            }
                            else
                            {
                                TempSql = TempSql + columns[i].columnName + " " + columns[i].columnType + ",";
                            }
                        }
                        else
                        {
                            if (columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + columns[i].columnName + " INTEGER PRIMARY KEY";
                            }
                            else
                            {
                                TempSql = TempSql + columns[i].columnName + " " + columns[i].columnType;
                            }
                        }
                    }
                    if (primarykeyIndex != 0)
                    {
                        TempSql = TempSql + ",primary key(" + columns[primarykeyIndex - 1].columnName + ")";
                    }
                    TempSql = "create table " + tableName + "(" + TempSql + ")";
                    using (SQLiteCommand TempComm = TempCon.CreateCommand())
                    {
                        TempComm.CommandText = "PRAGMA auto_vacuum = 1";
                        TempComm.ExecuteNonQuery();
                        TempComm.CommandText = TempSql;
                        TempComm.CommandType = CommandType.Text;
                        TempComm.ExecuteNonQuery();
                    }
                    TempCon.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 使用字段结构体创建新表,如果数据库不存在,则自动建立
        /// </summary>
        /// <param name="tableStruct"></param>
        public static void CreateTable(SQLiteTableStruct tableStruct)
        {
            if (tableStruct.TableName == "")
            {
                return;
            }
            if (tableStruct.Columns.Length <= 0)
            {
                return;
            }
            if (tableStruct.PrimarykeyIndex > tableStruct.Columns.Length || tableStruct.PrimarykeyIndex < 0)
            {
                return;
            }
            if (tableStruct.Columns.Count(obj => obj.columnType == ColumnType.AUTOADD) > 1)
            {
                return;
            }
            if (tableStruct.Columns.Count(obj => obj.columnType == ColumnType.AUTOADD) == 1 && tableStruct.PrimarykeyIndex != 0)
            {
                return;
            }
            using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + tableStruct.SourceName + ";password=" + passWord))
            {
                try
                {
                    TempCon.Open();
                    string TempSql = "";
                    for (int i = 0; i < tableStruct.Columns.Length; i++)
                    {
                        if (i != tableStruct.Columns.Length - 1)
                        {
                            if (tableStruct.Columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + tableStruct.Columns[i].columnName + " INTEGER PRIMARY KEY,";
                            }
                            else
                            {
                                TempSql = TempSql + tableStruct.Columns[i].columnName + " " + tableStruct.Columns[i].columnType + ",";
                            }
                        }
                        else
                        {
                            if (tableStruct.Columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + tableStruct.Columns[i].columnName + " INTEGER PRIMARY KEY";
                            }
                            else
                            {
                                TempSql = TempSql + tableStruct.Columns[i].columnName + " " + tableStruct.Columns[i].columnType;
                            }
                        }
                    }
                    if (tableStruct.PrimarykeyIndex != 0)
                    {
                        TempSql = TempSql + ",primary key(" + tableStruct.Columns[tableStruct.PrimarykeyIndex - 1].columnName + ")";
                    }
                    TempSql = "create table " + tableStruct.TableName + "(" + TempSql + ")";
                    using (SQLiteCommand TempComm = TempCon.CreateCommand())
                    {
                        TempComm.CommandText = "PRAGMA auto_vacuum = 1";
                        TempComm.ExecuteNonQuery();
                        TempComm.CommandText = TempSql;
                        TempComm.CommandType = CommandType.Text;
                        TempComm.ExecuteNonQuery();
                    }
                    TempCon.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 创建多个相同结构的表
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="tableNames">所有表名</param>
        /// <param name="columns">所有列名</param>
        /// <param name="primarykeyIndex">主键的字段序号,为0时不创建主键</param>
        public static void CreateTable(string dataSource, string[] tableNames, ColumnStruct[] columns, int primarykeyIndex)
        {
            if (tableNames == null)
            {
                return;
            }
            if (columns.Length <= 0)
            {
                return;
            }
            if (primarykeyIndex > columns.Length || primarykeyIndex < 0)
            {
                return;
            }
            if (columns.Count(obj => obj.columnType == ColumnType.AUTOADD) > 1)
            {
                return;
            }
            if (columns.Count(obj => obj.columnType == ColumnType.AUTOADD) == 1 && primarykeyIndex != 0)
            {
                return;
            }
            using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord))
            {
                try
                {
                    TempCon.Open();
                    string TempSql = "";
                    string[] ExeSql = new string[tableNames.Length];
                    for (int i = 0; i < columns.Length; i++)
                    {
                        if (i != columns.Length - 1)
                        {
                            if (columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + columns[i].columnName + " INTEGER PRIMARY KEY AUTOINCREMENT,";
                            }
                            else
                            {
                                TempSql = TempSql + columns[i].columnName + " " + columns[i].columnType + ",";
                            }
                        }
                        else
                        {
                            if (columns[i].columnType == ColumnType.AUTOADD)
                            {
                                TempSql = TempSql + columns[i].columnName + " INTEGER PRIMARY KEY AUTOINCREMENT";
                            }
                            else
                            {
                                TempSql = TempSql + columns[i].columnName + " " + columns[i].columnType;
                            }
                        }
                    }
                    if (primarykeyIndex != 0)
                    {
                        TempSql = TempSql + ",primary key(" + columns[primarykeyIndex - 1].columnName + ")";
                    }
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        ExeSql[i] = "create table " + tableNames[i] + "(" + TempSql + ")";
                    }
                    using (SQLiteTransaction tran = TempCon.BeginTransaction())//实例化一个事务
                    {
                        SQLiteCommand cmd = new SQLiteCommand(TempCon);
                        cmd.Transaction = tran;
                        cmd.CommandText = "PRAGMA auto_vacuum = 1";
                        cmd.ExecuteNonQuery();
                        for (int i = 0; i < ExeSql.Length; i++)
                        {
                            cmd.CommandText = ExeSql[i];
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    TempCon.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }
        #endregion

        #region 判断表是否存在
        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool TableExist(string dataSource, string tableName)
        {
            if (!File.Exists(dataSource))
            {
                return false;
            }
            if (tableName == "")
            {
                return false;
            }
            try
            {
                using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord))
                {
                    TempCon.Open();
                    using (SQLiteCommand TempComm = TempCon.CreateCommand())
                    {
                        TempComm.CommandText = "SELECT COUNT(*) FROM sqlite_master where type='table' and name='" + tableName + "'";
                        TempComm.CommandType = CommandType.Text;
                        if (TempComm.ExecuteScalar().ToString() == "0")
                        {
                            TempCon.Close();
                            return false;
                        }
                        else
                        {
                            TempCon.Close();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region 执行语句
        /// <summary>
        /// 执行单条语句
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="queryStr">执行语句</param>
        public static void ExecuteNonQuery(string dataSource, string queryStr)
        {
            if (!File.Exists(dataSource))
            {
                // LogRecord("QueryNumber:" + QueryNumber.ToString() + "|数据文件不存在:" + dataSource);
                return;
            }
            try
            {
                using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord))
                {
                    TempCon.Open();
                    using (SQLiteCommand TempComm = TempCon.CreateCommand())
                    {
                        TempComm.CommandText = queryStr;
                        TempComm.CommandType = CommandType.Text;
                        TempComm.ExecuteNonQuery();
                    }
                    TempCon.Close();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// 执行多条语句
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="queryStr">执行语句</param>
        public static void ExecuteNonQuery(string dataSource, string[] queryStr)
        {
            if (!File.Exists(dataSource))
            {
                // LogRecord("QueryNumber:" + QueryNumber.ToString() + "|数据文件不存在:" + dataSource);
                return;
            }
            try
            {
                using (SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord))
                {
                    TempCon.Open();
                    SQLiteCommand cmd = new SQLiteCommand(TempCon);
                    cmd.CommandText = "PRAGMA synchronous = 0";
                    cmd.ExecuteNonQuery();
                    using (SQLiteTransaction tran = TempCon.BeginTransaction())//实例化一个事务
                    {
                        cmd.Transaction = tran;
                        for (int i = 0; i < queryStr.Length; i++)
                        {
                            cmd.CommandText = queryStr[i];
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    TempCon.Close();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// 返回数据表
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="queryStr">执行语句</param>
        /// <returns>null or datatable</returns>
        public static DataTable ExecuteDataTable(string dataSource, string queryStr)
        {
            if (!File.Exists(dataSource))
            {
                //     LogRecord("QueryNumber:" + QueryNumber.ToString() + "|数据文件不存在:" + dataSource);
                return null;
            }
            SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord);
            try
            {
                DataTable dt = new DataTable();
                TempCon.Open();
                using (SQLiteCommand TempComm = TempCon.CreateCommand())
                {
                    SQLiteDataAdapter TempAdapter = new SQLiteDataAdapter(TempComm);
                    TempComm.CommandText = queryStr;
                    TempComm.CommandType = CommandType.Text;
                    TempAdapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                TempCon.Close();
            }
        }

        /// <summary>
        /// 返回多个数据表--dataset方式
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="queryStr">执行语句</param>
        /// <returns>null or datatables</returns>
        public static DataTable[] ExecuteDataTable(string dataSource, string[] queryStr)
        {
            if (!File.Exists(dataSource))
            {
                //     LogRecord("QueryNumber:" + QueryNumber.ToString() + "|数据文件不存在:" + dataSource);
                return null;
            }

            SQLiteConnection TempCon = new SQLiteConnection("Pooling=true;data source=" + dataSource + ";password=" + passWord);
            try
            {
                DataTable[] dt = new DataTable[queryStr.Length];
                TempCon.Open();
                SQLiteCommand cmd = new SQLiteCommand(TempCon);
                cmd.CommandText = "PRAGMA synchronous = OFF";
                cmd.ExecuteNonQuery();

                using (SQLiteTransaction tran = TempCon.BeginTransaction())//实例化一个事务
                {
                    cmd.Transaction = tran;
                    for (int i = 0; i < queryStr.Length; i++)
                    {
                        dt[i] = new DataTable();
                        SQLiteDataAdapter TempAdapter = new SQLiteDataAdapter(cmd);
                        cmd.CommandText = queryStr[i];
                        cmd.CommandType = CommandType.Text;
                        TempAdapter.Fill(dt[i]);
                    }
                    tran.Commit();
                }

                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                TempCon.Close();
            }
        }

        /// <summary>
        /// 返回多个数据表--datareader方式,实际效果比dataset要慢,留做对比
        /// </summary>
        /// <param name="dataSource">数据文件</param>
        /// <param name="queryStr">执行语句</param>
        /// <returns></returns>
        public static DataTable[] ExecuteDataTable2(string dataSource, string[] queryStr)
        {
            if (!File.Exists(dataSource))
            {
                return null;
            }

            SQLiteDataReader dataReader = null;
            SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord);
            try
            {
                DataTable[] dt = new DataTable[queryStr.Length];
                TempCon.Open();
                SQLiteCommand cmd = new SQLiteCommand(TempCon);
                cmd.CommandText = "PRAGMA synchronous = OFF";
                cmd.ExecuteNonQuery();

                using (SQLiteTransaction tran = TempCon.BeginTransaction())//实例化一个事务
                {
                    cmd.Transaction = tran;
                    for (int i = 0; i < queryStr.Length; i++)
                    {
                        dt[i] = new DataTable();
                        SQLiteDataAdapter TempAdapter = new SQLiteDataAdapter(cmd);
                        cmd.CommandText = queryStr[i];
                        cmd.CommandType = CommandType.Text;

                        dataReader = cmd.ExecuteReader();
                        dt[i].Load(dataReader);
                    }
                    tran.Commit();
                }

                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Dispose();
                }
                TempCon.Close();
            }
        }


        /// <summary>
        /// 返回执行结果
        /// </summary>
        /// <param name="dataSource">数据库文件</param>
        /// <param name="queryStr">执行语句</param>
        /// <returns>null or value</returns>
        public static object ExecuteScalar(string dataSource, string queryStr)
        {
            if (!File.Exists(dataSource))
            {
                // LogRecord("QueryNumber:" + QueryNumber.ToString() + "|数据文件不存在:" + dataSource);
                return null;
            }

            SQLiteConnection TempCon = new SQLiteConnection("data source=" + dataSource + ";password=" + passWord);
            try
            {
                object obj;
                TempCon.Open();
                using (SQLiteCommand TempComm = TempCon.CreateCommand())
                {
                    TempComm.CommandText = queryStr;
                    TempComm.CommandType = CommandType.Text;
                    obj = TempComm.ExecuteScalar();
                    return obj;
                }
            }
            catch (Exception)
            {
                //LogRecord("QueryNumber:" + QueryNumber.ToString() + "|datasource:" + dataSource + "|commandtext:" + queryStr + "|" + ex.Message);
                return null;
            }
            finally
            {
                TempCon.Close();
            }
        }
        #endregion

        #region Sqlite表的结构　创建新表时可直接使用本结构
        /// <summary>
        /// sqlite表结构体
        /// </summary>
        public class SQLiteTableStruct
        {

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="cSourceName">数据库文件</param>
            /// <param name="cTableName">表名</param>
            /// <param name="cColumns">所有字段</param>
            /// <param name="cPrimarykeyIndex">主键序号</param>
            public SQLiteTableStruct(string cSourceName, string cTableName, ColumnStruct[] cColumns, int cPrimarykeyIndex)
            {
                mSourceName = cSourceName;
                mTableName = cTableName;
                mColumns = cColumns;
                mPrimarykeyIndex = cPrimarykeyIndex;
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="cTableName">表名</param>
            /// <param name="cColumns">所有字段</param>
            /// <param name="cPrimarykeyIndex">主键序号</param>
            public SQLiteTableStruct(string cTableName, ColumnStruct[] cColumns, int cPrimarykeyIndex)
            {
                mTableName = cTableName;
                mColumns = cColumns;
                mPrimarykeyIndex = cPrimarykeyIndex;
            }

            //数据库文件名，包含具体路径
            private string mSourceName;
            /// <summary>
            /// 数据库文件
            /// </summary>
            public string SourceName { set { mSourceName = value; } get { return mSourceName; } }
            //表名  
            private string mTableName;
            /// <summary>
            /// 表名
            /// </summary>
            public string TableName { set { mTableName = value; } get { return mTableName; } }
            //字段
            private ColumnStruct[] mColumns;
            /// <summary>
            /// 所有字段
            /// </summary>
            public ColumnStruct[] Columns { get { return mColumns; } }
            //主键列序号，为0时表示没有主建
            private int mPrimarykeyIndex;
            /// <summary>
            /// 主键序号
            /// </summary>
            public int PrimarykeyIndex { get { return mPrimarykeyIndex; } }
        }
        #endregion
    }
}
