using CodeHelper;
using Prime.DBUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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

        /// <summary>
        /// 通过数据库连接，生成Model列表
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <returns></returns>
        public static List<DTModel> DataBaseToListModel_T4(string ConnectionString, string tableName)
        {
            SqlHelper.connStr = ConnectionString;

            DataTable dt = SqlHelper.GetDataTable(" select * from dbo.sysobjects where xtype = 'U' and name='" + tableName + "' order by name");

            List<DTModel> listModel = new List<DTModel>();
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    #region 根据数据表获取Model列表

                    foreach (DataRow dr in dt.Rows)
                    {
                        string DataTableName = dr["name"].ToString();

                        string sql = @"
SELECT
    表名       = case when a.colorder=1 then d.name else '' end,
    表说明     = case when a.colorder=1 then isnull(f.value,'') else '' end,
    字段序号   = a.colorder,
    columnName     = a.name,
    是否自增长       = case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then 'Y'else 'N' end,
    primaryKey       = case when exists(SELECT 1 FROM sysobjects where xtype='PK' and parent_obj=a.id and name in (
                     SELECT name FROM sysindexes WHERE indid in(
                        SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid))) then 'Y' else 'N' end,
    columnType       = b.name,
    占用字节数 = a.length,
    char_col_decl_length       = COLUMNPROPERTY(a.id,a.name,'PRECISION'),
    小数位数   = isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0),
    isnullable     = case when a.isnullable=1 then 'Y'else 'N' end,
    data_default     = isnull(e.text,''),
    columnComment   = isnull(g.[value],'')
FROM  syscolumns a
left join systypes b on a.xusertype=b.xusertype
inner join sysobjects d on     a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties'
left join syscomments e on     a.cdefault=e.id
left join sys.extended_properties g on     a.id=g.major_id and a.colid=g.minor_id
left join    sys. extended_properties f on     d.id=f.major_id and f.major_id=0
where     d.name=@a order by     a.id,a.colorder";

                        SqlParameter[] sps = { new SqlParameter("@a", DataTableName) };
                        DataTable dt_tables = SqlHelper.GetDataTable(sql, sps);

                        if (dt_tables != null && dt_tables.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt_tables.Rows.Count; i++)
                            {
                                var thisRow = dt_tables.Rows[i];
                                string DataType = thisRow[6].ToString();
                                if (DataType.Contains("char"))
                                {
                                    string biteSize = thisRow[7].ToString();
                                    int bit = Utils.StrToInt(biteSize, 0);
                                    if (bit == -1 || bit > 4000)
                                    {
                                        biteSize = "max";
                                    }
                                    DataType += "(" + biteSize + ")";
                                }
                                else if (DataType.Contains("decimal") || DataType.Contains("numeric"))
                                {
                                    DataType += string.Format("({0},{1})", thisRow[8].ToString(), thisRow[9].ToString());
                                }
                                listModel.Add(new DTModel()
                                {
                                    TableName = DataTableName,
                                    Name = thisRow[3].ToString(),
                                    IsIdentity = thisRow[4].ToString().ToUpper() == "Y",
                                    IsPK = thisRow[5].ToString().ToUpper() == "Y",
                                    DataType = DataType,
                                    IsNull = thisRow[10].ToString().ToUpper() == "Y",
                                    DefaultValue = thisRow[11].ToString(),
                                    Desc = thisRow[12].ToString().Trim(),
                                    Index = i,
                                });
                            }
                        }
                    }

                    #endregion 根据数据表获取Model列表
                }
            }
            return listModel;
        }

        /// <summary>
        /// 生成Model文件.cs
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="NameSpace">命名空间</param>
        /// <param name="FileNameSuffix">Model后缀名</param>
        /// <param name="ClassAttribute">Class的特性</param>
        /// <param name="PropertyAttribute">Property的特性</param>
        /// <returns></returns>
        public static string CreateModel(List<DTModel> listModel, string NameSpace, string FileNameSuffix, string ClassAttribute, string PropertyAttribute)
        {
            var query = listModel.GroupBy(d => d.TableName);

            StringBuilder sb = new StringBuilder();
            foreach (var item in query)
            {
                var queryFirst = item.FirstOrDefault();

                #region Model生成

                sb.AppendLine("using System;");
                sb.AppendLine("using System.Runtime.Serialization;");
                sb.AppendLine("");
                sb.AppendLine("namespace " + NameSpace);
                sb.AppendLine("{");
                sb.AppendLine("    /// <summary>");
                sb.AppendLine("    /// Model实体类：" + queryFirst.TableName);
                sb.AppendLine("    /// </summary>");
                sb.AppendLine(ClassAttribute);
                sb.AppendLine("    public class " + queryFirst.TableName + FileNameSuffix);
                sb.AppendLine("    {");

                sb.AppendLine("        #region Model");

                //遍历每个字段
                foreach (var model in item)
                {
                    string nullable = "";
                    string columnType = GetDataType(model.DataType);//字段类型

                    if (model.IsNull && columnType != "string")
                    {
                        nullable = "?";
                    }
                    else
                    {
                        nullable = "";
                    }
                    if (model.IsPK)
                    {
                        model.Desc = "主键：" + model.Desc;
                    }
                    sb.AppendLine("");
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine("        /// " + model.Desc);
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine(PropertyAttribute);
                    sb.AppendLine(string.Format("        public {0}{1} {2} {3}", columnType, nullable, model.Name, "{ get; set; }"));
                }
                sb.AppendLine("");
                sb.AppendLine("        #endregion Model");
                sb.AppendLine("    }");
                sb.AppendLine("}");

                #endregion Model生成
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static string GetDataType(string columnType)
        {
            switch (columnType)
            {
                case "uniqueidentifier":
                    columnType = "Guid";
                    break;

                case "tinyint":
                    columnType = "byte";
                    break;

                case "smallint":
                    columnType = "short";
                    break;

                case "int":
                    columnType = "int";
                    break;

                case "bigint":
                    columnType = "long";
                    break;

                case "bit":
                    columnType = "bool";
                    break;

                case "date":
                case "datetime":
                    columnType = "DateTime";
                    break;

                default:

                    if (columnType.Contains("decimal") || columnType.Contains("numeric"))
                    {
                        columnType = "decimal";
                    }
                    else
                    {
                        columnType = "string";
                    }
                    break;
            }
            return columnType;
        }
    }
}