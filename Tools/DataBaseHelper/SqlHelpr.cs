using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.DataBaseHelper
{
    /// <summary>
    /// sql数据库操作
    /// </summary>
    public class SqlHelp
    {
        private static bool mLogrecording = false;

        /// <summary>
        /// 数据为参数结构体,包含数据库连接的所有参数
        /// </summary>
        public struct DatabaseParaStrct
        {
            public string SVS;
            public string DB;
            public string USER;
            public string PASSWORD;
        }

        /// <summary>
        /// 连接测试
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <returns>true or false</returns>
        public static bool ConnectTest(string SVS, string DB, string USER, string PWD)
        {
            using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2"))
            {
                try
                {
                    mCon.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 连接测试
        /// </summary>
        /// <param name="DBStruct">数据库连接参数结构体</param>
        /// <returns>true or false</returns>
        public static bool ConnectTest(DatabaseParaStrct DBStruct)
        {
            using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
            {
                try
                {
                    mCon.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 根据参数获取连接并打开
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <returns>null or sqlconnction</returns>
        public static SqlConnection getConnectAndOpen(string SVS, string DB, string USER, string PWD)
        {
            SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2");
            try
            {
                mCon.Open();
                return mCon;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接执行单条语句
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(SqlConnection Con, CommandType commandType, string commandText)
        {
            if (Con.State != ConnectionState.Open)
            {
                Con.Open();
            }
            try
            {
                using (SqlCommand mComm = Con.CreateCommand())
                {
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    mComm.ExecuteNonQuery();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 使用参数建立新连接执行单条语句
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(string SVS, string DB, string USER, string PWD, CommandType commandType, string commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    mComm.ExecuteNonQuery();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 执行单条语句
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(DatabaseParaStrct DBStruct, CommandType commandType, string commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                    ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    mComm.ExecuteNonQuery();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 使用已有连接同时执行多条语句(事务集处理方式)
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(SqlConnection Con, CommandType commandType, string[] commandText)
        {
            if (Con.State != ConnectionState.Open)
            {
                Con.Open();
            }
            try
            {
                using (SqlCommand mComm = Con.CreateCommand())
                {
                    SqlTransaction mTran = Con.BeginTransaction();
                    mComm.Transaction = mTran;
                    mComm.CommandType = commandType;
                    for (int i = 0; i < commandText.Length; i++)
                    {
                        mComm.CommandText = commandText[i];
                        mComm.ExecuteNonQuery();
                    }
                    mTran.Commit();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// 使用参数建立新连接同时执行多条语句(事务集处理方式)
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(string SVS, string DB, string USER, string PWD, CommandType commandType, string[] commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    SqlTransaction mTran = mCon.BeginTransaction();
                    mComm.Transaction = mTran;
                    mComm.CommandType = commandType;
                    for (int i = 0; i < commandText.Length; i++)
                    {
                        mComm.CommandText = commandText[i];
                        mComm.ExecuteNonQuery();
                    }
                    mTran.Commit();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 执行多条语句(事务集处理方式)
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>成功返回ok,失败返回错误内容</returns>
        public static string ExecuteNoQuery(DatabaseParaStrct DBStruct, CommandType commandType, string[] commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                    ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    SqlTransaction mTran = mCon.BeginTransaction();
                    mComm.Transaction = mTran;
                    mComm.CommandType = commandType;
                    for (int i = 0; i < commandText.Length; i++)
                    {
                        mComm.CommandText = commandText[i];
                        mComm.ExecuteNonQuery();
                    }
                    mTran.Commit();
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 使用已有连接获取dataset
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or dataset</returns>
        public static DataSet ExecuteDataset(SqlConnection Con, string SqlStr)
        {
            try
            {
                if (Con.State != ConnectionState.Open)
                {
                    Con.Open();
                }
                using (SqlDataAdapter sda = new SqlDataAdapter(SqlStr, Con))
                {
                    DataSet mDset = new DataSet();
                    sda.Fill(mDset);
                    return mDset;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接获取数据表
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatable</returns>
        public static DataTable ExecuteDataTable(SqlConnection Con, string SqlStr)
        {
            try
            {
                if (Con.State != ConnectionState.Open)
                {
                    Con.Open();
                }
                using (SqlDataAdapter sda = new SqlDataAdapter(SqlStr, Con))
                {
                    DataSet mDset = new DataSet();
                    sda.Fill(mDset);
                    if (mDset.Tables.Count > 0)
                    {
                        return mDset.Tables[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用连接参数获取数据表
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="User">用户名</param>
        /// <param name="Pwd">密码</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatable</returns>
        public static DataTable ExecuteDataTable(string SVS, string DB, string User, string Pwd, string SqlStr)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + User + ";PWD=" + Pwd + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(SqlStr, mCon);
                    DataSet mDset = new DataSet();
                    sda.Fill(mDset);
                    if (mDset.Tables.Count > 0)
                    {
                        return mDset.Tables[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatable</returns>
        public static DataTable ExecuteDataTable(DatabaseParaStrct DBStruct, string SqlStr)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                    ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(SqlStr, mCon);
                    DataSet mDset = new DataSet();
                    sda.Fill(mDset);
                    if (mDset.Tables.Count > 0)
                    {
                        return mDset.Tables[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接获取多个数据表
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatables</returns>
        public static DataTable[] ExecuteDataTable(SqlConnection Con, string[] SqlStr)
        {
            try
            {
                if (Con.State != ConnectionState.Open)
                {
                    Con.Open();
                }

                DataTable[] dt = new DataTable[SqlStr.Length];
                SqlCommand mComm = Con.CreateCommand();
                mComm.CommandType = CommandType.Text;
                using (SqlTransaction mTran = Con.BeginTransaction())
                {
                    mComm.Transaction = mTran;
                    for (int i = 0; i < SqlStr.Length; i++)
                    {
                        SqlDataAdapter sda = new SqlDataAdapter(mComm);
                        mComm.CommandText = SqlStr[i];
                        dt[i] = new DataTable();
                        sda.Fill(dt[i]);
                    }
                    mTran.Commit();
                }
                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接获取多个数据表
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="User">用户名</param>
        /// <param name="Pwd">密码</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatables</returns>
        public static DataTable[] ExecuteDataTable(string SVS, string DB, string User, string Pwd, string[] SqlStr)
        {
            try
            {
                using (SqlConnection Con = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + User + ";PWD=" + Pwd + ";Connection Timeout=2"))
                {
                    Con.Open();
                    DataTable[] dt = new DataTable[SqlStr.Length];
                    SqlCommand mComm = Con.CreateCommand();
                    mComm.CommandType = CommandType.Text;
                    SqlTransaction mTran = Con.BeginTransaction();
                    mComm.Transaction = mTran;
                    for (int i = 0; i < SqlStr.Length; i++)
                    {
                        SqlDataAdapter sda = new SqlDataAdapter(mComm);
                        mComm.CommandText = SqlStr[i];
                        dt[i] = new DataTable();
                        sda.Fill(dt[i]);
                    }
                    mTran.Commit();
                    return dt;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接获取多个数据表
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>null or datatables</returns>
        public static DataTable[] ExecuteDataTable(DatabaseParaStrct DBStruct, string[] SqlStr)
        {
            try
            {
                using (SqlConnection Con = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                    ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
                {
                    Con.Open();
                    DataTable[] dt = new DataTable[SqlStr.Length];
                    SqlCommand mComm = Con.CreateCommand();
                    mComm.CommandType = CommandType.Text;
                    SqlTransaction mTran = Con.BeginTransaction();
                    mComm.Transaction = mTran;
                    for (int i = 0; i < SqlStr.Length; i++)
                    {
                        SqlDataAdapter sda = new SqlDataAdapter(mComm);
                        mComm.CommandText = SqlStr[i];
                        dt[i] = new DataTable();
                        sda.Fill(dt[i]);
                    }
                    mTran.Commit();
                    return dt;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用现有连接执行sql语句并返回结果
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>sql语句执行结果</returns>
        public static object ExecuteScalar(SqlConnection Con, CommandType commandType, string commandText)
        {
            try
            {
                if (Con.State != ConnectionState.Open)
                {
                    Con.Open();
                }
                using (SqlCommand mComm = Con.CreateCommand())
                {
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    return mComm.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用连接参数执行sql语句并返回结果
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本内容</param>
        /// <returns>sql语句执行结果</returns>
        public static object ExecuteScalar(string SVS, string DB, string USER, string PWD, CommandType commandType, string commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    return mComm.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 执行sql语句并返回结果
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文件内容</param>
        /// <returns>sql语句执行结果</returns>
        public static object ExecuteScalar(DatabaseParaStrct DBStruct, CommandType commandType, string commandText)
        {
            try
            {
                using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                    ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
                {
                    mCon.Open();
                    SqlCommand mComm = mCon.CreateCommand();
                    mComm.CommandType = commandType;
                    mComm.CommandText = commandText;
                    return mComm.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用已有连接判断数据表是否存在
        /// </summary>
        /// <param name="Con">数据库连接</param>
        /// <param name="tableName">表名</param>
        /// <returns>true or false</returns>
        public static bool TableExtsis(SqlConnection Con, string tableName)
        {
            if (Con.State != ConnectionState.Open)
            {
                Con.Open();
            }
            using (SqlCommand mComm = Con.CreateCommand())
            {
                mComm.CommandType = CommandType.Text;
                mComm.CommandText = "if object_id(N'',N'U')is not null select 1 else select 0";
                if (mComm.ExecuteScalar().ToString() == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 不执行sql语句检查sql语句法正确性
        /// </summary>
        /// <param name="SVS">服务器名</param>
        /// <param name="DB">数据库名</param>
        /// <param name="USER">用户名</param>
        /// <param name="PWD">密码</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>sql语句语法正确返回ok,错误时返回错误内容</returns>
        public static string ValidateSQL(string SVS, string DB, string USER, string PWD, string SqlStr)
        {
            string sResult;
            using (SqlConnection mCon = new SqlConnection("Server=" + SVS + ";Database=" + DB + ";UID=" + USER + ";PWD=" + PWD + ";Connection Timeout=2"))
            {
                mCon.Open();
                SqlCommand mComm = mCon.CreateCommand();
                mComm.CommandText = "SET NOEXEC ON";
                mComm.CommandType = CommandType.Text;
                mComm.ExecuteNonQuery();
                try
                {
                    mComm.CommandText = SqlStr;
                    mComm.ExecuteNonQuery();
                    sResult = "ok";
                }
                catch (Exception ex)
                {
                    sResult = ex.Message;
                }
                finally
                {
                    mComm.CommandText = "SET NOEXEC OFF";
                    mComm.ExecuteNonQuery();
                }
            }
            return sResult;
        }

        /// <summary>
        /// 不执行sql语句检查sql语句法正确性
        /// </summary>
        /// <param name="DBStruct">数据库参数结构体</param>
        /// <param name="SqlStr">sql语句</param>
        /// <returns>sql语句语法正确返回ok,错误时返回错误内容</returns>
        public static string ValidateSQL(DatabaseParaStrct DBStruct, string SqlStr)
        {
            string sResult;
            using (SqlConnection mCon = new SqlConnection("Server=" + DBStruct.SVS + ";Database=" + DBStruct.DB + ";UID=" + DBStruct.USER +
                ";PWD=" + DBStruct.PASSWORD + ";Connection Timeout=2"))
            {
                mCon.Open();
                SqlCommand mComm = mCon.CreateCommand();
                mComm.CommandText = "SET NOEXEC ON";
                mComm.CommandType = CommandType.Text;
                mComm.ExecuteNonQuery();
                try
                {
                    mComm.CommandText = SqlStr;
                    mComm.ExecuteNonQuery();
                    sResult = "ok";
                }
                catch (Exception ex)
                {
                    sResult = ex.Message;
                }
                finally
                {
                    mComm.CommandText = "SET NOEXEC OFF";
                    mComm.ExecuteNonQuery();
                }
            }
            return sResult;
        }
    }
}
