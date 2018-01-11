using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeCreate
{
    public class CommonCode
    {

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="fileContent">文件内容</param>
        public static void Save(string fileName, string fileContent)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            byte[] buffer = Encoding.Default.GetBytes(fileContent);

            fs.Write(buffer, 0, buffer.Length);

            fs.Flush();
            fs.Close();
        }

        /// <summary>
        /// 获取字段的数据类型
        /// </summary>
        /// <param name="colType"></param>
        /// <returns></returns>
        public static string GetColType(string colType)
        {
            colType = colType.Trim().ToLower();
            switch (colType)
            {
                case "int":
                case "bigint":
                case "binary":
                case "bit":
                case "decimal":
                case "float":
                case "money":
                case "numeric":
                case "smallint":
                case "smallmoney":
                case "tinyint":
                case "varbinary":
                    colType = "SqlType.Number";
                    break;

                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "time":
                    colType = "SqlType.DateTime";
                    break;

                case "text":
                case "ntext":
                    colType = "SqlType.Clob";
                    break;

                default:
                    colType = "SqlType.NVarChar";
                    break;
            }
            return colType;
        }

        public static string GetCustomEnumDesc(Type t, Enum e)
        {
            DescriptionAttribute da = null;
            FieldInfo fi;
            foreach (Enum type in Enum.GetValues(t))
            {
                fi = t.GetField((type.ToString()));
                da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                if (da != null && e.ToString() == type.ToString())
                    return da.Description;
            }
            return string.Empty;
        }

        public static System.Enum GetCustomEnumByDesc(Type t, string description)
        {
            System.Enum e = null;
            DescriptionAttribute da = null;
            FieldInfo fi;
            foreach (System.Enum type in System.Enum.GetValues(t))
            {
                fi = t.GetField((type.ToString()));
                da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                if (da != null && description == da.Description)
                    e = type;
            }
            return e;
        }

    }
}
