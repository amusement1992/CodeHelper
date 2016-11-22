using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCreate
{
    /// <summary>
    /// 设置
    /// </summary>
    public class ConfigModel
    {

        #region 字段
        /// <summary>
        /// SQL的连接类型 
        /// </summary>
        public string MARK { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string str_nameSpace { get; set; }

        /// <summary>
        /// 数据访问类名
        /// </summary>
        public string str_SqlHelper { get; set; }

        /// <summary>
        /// 执行一条SQL语句方法名：
        /// </summary>
        public string str_ExecuteNonQuery { get; set; }

        /// <summary>
        /// 执行多条SQL语句方法名：
        /// </summary>
        public string str_UpdateDatabase { get; set; }

        /// <summary>
        /// 返回首行首列方法名：
        /// </summary>
        public string str_ExecuteScalar { get; set; }

        /// <summary>
        /// 返回DataTable方法名：
        /// </summary>
        public string str_GetDataTable { get; set; }

        /// <summary>
        /// 是否存在数据方法名：
        /// </summary>
        public string str_Exists { get; set; }

        /// <summary>
        /// 返回一个Model方法名：
        /// </summary>
        public string str_GetModel { get; set; }

        /// <summary>
        /// 返回多个Model方法名：
        /// </summary>
        public string str_GetModelList { get; set; }
        #endregion
    }
}