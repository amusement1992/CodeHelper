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

        public static string GetAllFiles(string TheFolder, string ParentPath, int Index)
        {
            StringBuilder sb = new StringBuilder();
            if (Directory.Exists(TheFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TheFolder);
                if (di.GetFiles() != null && di.GetFiles().Length > 0)
                {
                    int i = 1;
                    int fileCount = di.GetFiles().Count();
                    foreach (FileInfo item2 in di.GetFiles().OrderBy(d => d.Name))//遍历文件
                    {
                        sb.AppendLine(string.Format("{0} {1} ({2})", i.ToString().PadLeft(fileCount.ToString().Length, '0'), item2.Name, GetFileSize(item2.Length)));
                        i++;
                    }
                }

                foreach (DirectoryInfo item in di.GetDirectories().OrderBy(d => d.Name))//遍历文件夹
                {
                    sb.AppendLine();
                    sb.AppendLine("----文件夹：" + (ParentPath == "" ? item.FullName : item.FullName.Replace(ParentPath, "")));
                    sb.Append(GetAllFiles(item.FullName, ParentPath, Index++));
                }
            }
            return sb.ToString();
        }

        public static string GetEmptyString(int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public static string GetFileSize(long length)
        {
            StringBuilder sb = new StringBuilder();
            if (length < 1024 * 1024)
            {
                return Math.Round(length / 1024d, 2) + " KB";
            }
            else
            {
                return Math.Round(length / 1024d / 1024d, 2) + " MB";
            }
        }
    }
}