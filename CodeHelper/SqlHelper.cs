using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Prime.DBUtility
{
    /// <summary>
    /// Sql數據庫的連接
    /// </summary>
    public class SqlHelper
    {
        public static string connStr = "";

        #region ExecuteNonQuery() 返回受影響的行數。（常用：update、delete、insert）

        /// <summary>
        /// 返回受影響的行數。（常用：update、delete、insert）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <param name="sps">sql中需要的參數</param>
        /// <returns>影响行数(返回-1：沒有影響，返回0：出錯，返回大於0：成功影響的行數)</returns>
        public static int ExecuteNonQuery(CommandType type, string sql, SqlParameter[] sps)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.CommandType = type;
                        conn.Open();//打开链接
                        if (sps != null)//添加参数
                        {
                            //这个sql语句需要参数，做添加操作
                            foreach (SqlParameter sp in sps)
                            {
                                if (sp != null)//sps中的某个元素可能为空
                                {
                                    cmd.Parameters.Add(sp);
                                }
                            }
                        }
                        int i = cmd.ExecuteNonQuery();//执行命令
                        cmd.Parameters.Clear();
                        ////关闭链接
                        //cmd.Dispose();
                        //conn.Close();
                        //conn.Dispose();
                        return i;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// 返回受影響的行數（常用：update、delete、insert）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <param name="sps">參數列表</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, SqlParameter[] sps)
        {
            return ExecuteNonQuery(CommandType.Text, sql, sps);
        }

        /// <summary>
        /// 返回受影響的行數（常用：update、delete、insert）
        /// </summary>
        /// <param name="type">command 的执行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(CommandType type, string sql)
        {
            return ExecuteNonQuery(type, sql, null);
        }

        /// <summary>
        /// 返回受影響的行數（常用：update、delete、insert）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(CommandType.Text, sql, null);
        }

        #endregion ExecuteNonQuery() 返回受影響的行數。（常用：update、delete、insert）

        #region UpdateDatabase() 批量修改（常用：update、delete、insert）

        /// <summary>
        /// 批量修改（常用：update、delete、insert）
        /// </summary>
        /// <param name="list_sql"></param>
        /// <returns></returns>
        public static int UpdateDatabase(List<string> list_sql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    int num = 1;
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    cmd.Connection = conn;
                    cmd.Transaction = tran;
                    try
                    {
                        for (int i = 0; i < list_sql.Count; i++)
                        {
                            cmd.CommandText = list_sql[i];
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        num = 0;
                    }
                    tran.Dispose();
                    //conn.Close();
                    return num;
                }
            }
        }

        /// <summary>
        /// 批量修改（常用：update、delete、insert）
        /// </summary>
        /// <param name="list_sql">SQL語句集</param>
        /// <param name="sql">返回錯誤的SQL語句</param>
        /// <returns></returns>
        public static int UpdateDatabase(List<string> list_sql, ref string sql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    int num = 1;
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    cmd.Connection = conn;
                    cmd.Transaction = tran;
                    try
                    {
                        for (int i = 0; i < list_sql.Count; i++)
                        {
                            sql = list_sql[i];
                            cmd.CommandText = list_sql[i];
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        sql = string.Empty;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        num = 0;
                    }
                    return num;
                }
            }
        }

        #endregion UpdateDatabase() 批量修改（常用：update、delete、insert）

        #region ExecuteScalar() 返回結果的首行首列（常用：count）

        /// <summary>
        /// 返回結果的首行首列（常用：count）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <param name="sps">sql中需要的參數</param>
        /// <returns>返回結果集中的首行首列，結果為object類型</returns>
        public static object ExecuteScalar(CommandType type, string sql, SqlParameter[] sps)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.CommandType = type;
                        conn.Open();//打开链接
                        object obj = null;//定义的一返回变量

                        if (sps != null)//添加参数
                        {
                            foreach (SqlParameter sp in sps)//这个sql语句需要参数，做添加操作
                            {
                                if (sp != null)//sps中的某个元素可能为空
                                {
                                    cmd.Parameters.Add(sp);
                                }
                            }
                        }
                        obj = cmd.ExecuteScalar();//执行命令
                        cmd.Parameters.Clear();
                        //cmd.Dispose();
                        //sqlConn.Close();
                        //sqlConn.Dispose();
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 返回結果的首行首列（常用：count）
        /// </summary>
        /// <param name="sql">SQL語句</param>
        /// <param name="sps">參數列表</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, SqlParameter[] sps)
        {
            return ExecuteScalar(CommandType.Text, sql, sps);
        }

        /// <summary>
        /// 返回結果的首行首列
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <returns></returns>
        public static object ExecuteScalar(CommandType type, string sql)
        {
            return ExecuteScalar(type, sql, null);
        }

        /// <summary>
        /// 返回結果的首行首列（常用：count）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql)
        {
            return ExecuteScalar(CommandType.Text, sql, null);
        }

        #endregion ExecuteScalar() 返回結果的首行首列（常用：count）

        #region GetDataTable() 返回DataTable數據表（常用：select）

        /// <summary>
        /// 返回DataTable數據表（常用：select）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <param name="sps">sql中需要的參數</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(CommandType type, string sql, SqlParameter[] sps)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.CommandType = type;
                        conn.Open();
                        SqlDataAdapter sqlAdap = new SqlDataAdapter();
                        //添加参数
                        if (sps != null)
                        {
                            //这个sql语句需要参数，做添加操作
                            foreach (SqlParameter sp in sps)
                            {
                                if (sp != null)
                                {
                                    cmd.Parameters.Add(sp);
                                }
                            }
                        }
                        sqlAdap.SelectCommand = cmd;

                        DataTable dt = null;//新建返回数据表

                        DataSet ds = new DataSet();//新建承载二维表容器

                        sqlAdap.Fill(ds);//填充容器
                        dt = ds.Tables[0];

                        cmd.Parameters.Clear();//关闭链接返回结果
                        //cmd.Dispose();
                        sqlAdap.Dispose();
                        ds.Dispose();
                        //sqlConn.Close();
                        //sqlConn.Dispose();
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 返回DataTable數據表（常用：select）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <param name="sps">參數列表</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string sql, SqlParameter[] sps)
        {
            return GetDataTable(CommandType.Text, sql, sps);
        }

        /// <summary>
        /// 返回DataTable數據表（常用：select）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(CommandType type, string sql)
        {
            return GetDataTable(type, sql, null);
        }

        /// <summary>
        /// 返回DataTable數據表（常用：select）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string sql)
        {
            return GetDataTable(CommandType.Text, sql, null);
        }

        #endregion GetDataTable() 返回DataTable數據表（常用：select）

        #region Exists() 判斷是否存在數據。true:存在，false:不存在。（常用：select）

        /// <summary>
        /// 判斷是否存在數據。true:存在，false:不存在。（常用：select）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <param name="sps">參數列表</param>
        /// <returns></returns>
        public static bool Exists(CommandType type, string sql, SqlParameter[] sps)
        {
            DataTable dt = SqlHelper.GetDataTable(type, sql, sps);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判斷是否存在數據。true:存在，false:不存在。（常用：select）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <param name="sps">參數列表</param>
        /// <returns></returns>
        public static bool Exists(string sql, SqlParameter[] sps)
        {
            return Exists(CommandType.Text, sql, sps);
        }

        /// <summary>
        /// 判斷是否存在數據。true:存在，false:不存在。（常用：select）
        /// </summary>
        /// <param name="type">command 的執行方式</param>
        /// <param name="sql">執行的SQL語句或存儲過程</param>
        /// <returns></returns>
        public static bool Exists(CommandType type, string sql)
        {
            return Exists(type, sql, null);
        }

        /// <summary>
        /// 判斷是否存在數據。true:存在，false:不存在。（常用：select）
        /// </summary>
        /// <param name="sql">執行的SQL語句</param>
        /// <returns></returns>
        public static bool Exists(string sql)
        {
            return Exists(CommandType.Text, sql, null);
        }

        #endregion Exists() 判斷是否存在數據。true:存在，false:不存在。（常用：select）
    }
}