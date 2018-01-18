using CodeCreate;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Prime.DBUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// 通过分隔符换行——生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox18.Text.Trim().Length != 1)
            {
                return;
            }
            char split = Convert.ToChar(textBox18.Text.Trim().ToString());
            string splitStr = "";
            if (checkBox1.Checked)
            {
                splitStr = split.ToString();
            }
            string content = richTextBox1.Text;
            if (content.Contains(split))
            {
                string[] arr = content.Split(split);
                StringBuilder sb = new StringBuilder();
                foreach (string item in arr)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        sb.Append(item + splitStr + "\r\n");
                    }
                }
                richTextBox2.Text = sb.ToString();
            }
        }

        /// <summary>
        /// 拼接sql执行语句
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ConfigModel configModel = new ConfigModel();
            configModel.MARK = "@";//SQL的连接类型

            SqlHelper.connStr = textBox2.Text.Trim();

            DataTable dt = SqlHelper.GetDataTable(" select * from dbo.sysobjects where xtype = 'U' order by name");
            if (dt == null)
            {
                MessageBox.Show("数据库连接失敗了！");
                return;
            }

            string sql = @"
SELECT
    表名       = case when a.colorder=1 then d.name else '' end,
    表说明     = case when a.colorder=1 then isnull(f.value,'') else '' end,
    字段序号   = a.colorder,
    columnName     = a.name,
    标识       = case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then 'Y'else 'N' end,
    primaryKey       = case when exists(SELECT 1 FROM sysobjects where xtype='PK' and parent_obj=a.id and name in (
                     SELECT name FROM sysindexes WHERE indid in(
                        SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid))) then 'Y' else 'N' end,
    columnType       = b.name,
    占用字节数 = a.length,
    char_col_decl_length       = COLUMNPROPERTY(a.id,a.name,'PRECISION'),
    小数位数   = isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0),
    nullable     = case when a.isnullable=1 then 'N'else 'Y' end,
    data_default     = isnull(e.text,''),
    columnComment   = isnull(g.[value],'')
FROM  syscolumns a
left join systypes b on a.xusertype=b.xusertype
inner join sysobjects d on     a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties'
left join syscomments e on     a.cdefault=e.id
left join sys.extended_properties g on     a.id=g.major_id and a.colid=g.minor_id
left join    sys. extended_properties f on     d.id=f.major_id and f.major_id=0
where     d.name=" + configModel.MARK + "a order by     a.id,a.colorder";

            SqlParameter[] sps = { new SqlParameter(configModel.MARK + "a", comboBox1.Text.Trim()) };
            DataTable dt_tables = SqlHelper.GetDataTable(sql, sps);

            if (dt_tables == null)
            {
                return;
            }
            if (dt_tables.Rows.Count > 0)
            {
                #region 生成内容

                StringBuilder sb_column1 = new StringBuilder();//格式如 NO_ID,ST_NAME,ST_VALUES,NO_ORDER,ST_OTHER,ST_VALUES_ENG
                StringBuilder sb_column2 = new StringBuilder();//格式如 @NO_ID,@ST_NAME,@ST_VALUES,@NO_ORDER,@ST_OTHER,@ST_VALUES_ENG
                StringBuilder sb_column3 = new StringBuilder();//格式如 new SqlParameter("@NO_ID", SqlType.Number,4),
                StringBuilder sb_column4 = new StringBuilder();//格式如 parameters[0].Value = model.NO_ID;
                StringBuilder sb_column5 = new StringBuilder();//格式如 NO_ID=@NO_ID,ST_NAME=@ST_NAME
                for (int j = 0; j < dt_tables.Rows.Count; j++)
                {
                    string colName = dt_tables.Rows[j]["columnName"].ToString().Trim();//字段名
                    string colType = CommonCode.GetColType(dt_tables.Rows[j]["columnType"].ToString());//字段类型
                    if (j == dt_tables.Rows.Count - 1)
                    {
                        sb_column1.Append(colName);
                        sb_column2.Append(configModel.MARK + colName);
                        sb_column3.AppendLine("					new SqlParameter(\"" + configModel.MARK + colName + "\", model." + colName + ";)};");
                        //sb_column4.AppendLine("            parameters[" + j + "].Value = model." + colName + ";");

                        sb_column4.AppendLine("					 " + colName + "= model." + colName + ",");

                        sb_column5.Append(colName + "=" + configModel.MARK + colName);
                    }
                    else
                    {
                        sb_column1.Append(colName + ",");
                        sb_column2.Append(configModel.MARK + colName + ",");
                        sb_column3.AppendLine("					new SqlParameter(\"" + configModel.MARK + colName + "\", model." + colName + "),");
                        //sb_column4.AppendLine("            parameters[" + j + "].Value = model." + colName + ";");
                        sb_column4.AppendLine("					 " + colName + "= model." + colName + ",");

                        sb_column5.Append(colName + "=" + configModel.MARK + colName + ",");
                    }
                }
                richTextBox6.Text = sb_column1.ToString() + "\r\n\r\n";
                richTextBox6.Text += sb_column2.ToString() + "\r\n\r\n";
                richTextBox6.Text += sb_column5.ToString() + "\r\n\r\n";

                richTextBox3.Text = sb_column3.ToString() + "\r\n\r\n";
                richTextBox3.Text += sb_column4.ToString() + "\r\n\r\n";

                ////生成model
                //CreateModel(dt_tables);

                #endregion 生成内容
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            SqlHelper.connStr = textBox2.Text.ToString();
            DataTable dt = SqlHelper.GetDataTable(" select * from dbo.sysobjects where xtype = 'U' order by name");

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        comboBox1.Items.Add(dr["name"]);
                    }
                }
            }
            else
            {
                MessageBox.Show("数据库连接失敗了！");
            }
        }

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string oldStr = textBox6.Text.ToString().Trim();
            string newStr = textBox7.Text.ToString().Trim();

            richTextBox7.Text = richTextBox8.Text.Replace(oldStr, newStr);
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            string str = richTextBox16.Text;
            char split = textBox12.Text.Trim()[0];

            str = str.Replace("\n", "^");
            string[] arr = str.Split('^');

            List<string> temp = new List<string>();
            int i = 0;
            foreach (var item in arr)
            {
                temp.Add(item.Trim());
                if (item.Contains(split))
                {
                    if (i < item.Trim().IndexOf(split))
                    {
                        i = item.Trim().IndexOf(split);
                    }
                }
                else
                {
                    temp.Add(item);
                }
            }
            string temp2 = "";
            foreach (var item in temp)
            {
                if (item.Contains(split))
                {
                    string[] tempArr = item.Split(split);
                    temp2 += textBox13.Text + tempArr[0] + textBox16.Text;
                    temp2 += EmptyString(i - tempArr[0].Length) + split;
                    temp2 += textBox14.Text + tempArr[1] + textBox15.Text + "\r\n";
                }
                else
                {
                    temp2 += item;
                }
            }
            richTextBox15.Text = temp2;
        }

        private static string EmptyString(int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += " ";
            }
            return str;
        }

        /// <summary>
        /// 通过model生成Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                string str = richTextBox20.Text;

                str = str.Replace("\n", "^");
                string[] arr = str.Split('^');

                List<DTModel> listModel = new List<DTModel>();
                string tableName = "";
                string tableDesc = "";
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Contains("public class"))
                    {
                        tableName = arr[i].Split(' ').Where(d => d != "").ToList()[2];

                        if (arr[i - 1].Contains("[Serializable]"))
                        {
                            tableDesc = arr[i - 3].Replace("/// ", "").Trim();
                        }
                        else
                        {
                            tableDesc = arr[i - 2].Replace("/// ", "").Trim();
                        }
                    }

                    if (arr[i].Contains("get;") || arr[i].Contains("set;"))
                    {
                        string name = arr[i].Split(' ').Where(d => d != "").ToList()[2];
                        string desc = "";
                        if (arr[i - 1].Contains("/// </summary>"))
                        {
                            desc = arr[i - 2].Replace("/// ", "").Trim();
                        }

                        listModel.Add(new DTModel()
                        {
                            TableName = tableName,
                            TableDesc = tableDesc,
                            Index = i,
                            Name = name,
                            Desc = desc,
                        });
                    }
                }

                listModel = listModel.OrderBy(d => d.Index).ToList();
                if (listModel.Count == 0)
                {
                    MessageBox.Show("Model没内容！");
                    return;
                }

                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet();

                for (int i = 0; i < listModel.Count; i++)//行
                {
                    var model = listModel[i];

                    int thisCell = 0;

                    ExcelHelper.SetCellValue(sheet, i, thisCell, model.TableName.ToString());

                    thisCell++;

                    ExcelHelper.SetCellValue(sheet, i, thisCell, model.TableDesc.ToString());
                    thisCell++;

                    ExcelHelper.SetCellValue(sheet, i, thisCell, model.Index.ToString());
                    thisCell++;

                    ExcelHelper.SetCellValue(sheet, i, thisCell, model.Name);
                    thisCell++;

                    ExcelHelper.SetCellValue(sheet, i, thisCell, model.Desc);
                    thisCell++;
                }

                FileStream swQuoteSheet = File.OpenWrite("Data\\生成的" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".xls");
                workbook.Write(swQuoteSheet);
                swQuoteSheet.Close();
                MessageBox.Show("Excel生成成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.Combine(Application.StartupPath, "Data"));
        }

        /// <summary>
        /// Excel文件 打开对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFilePath3_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel|*.xls";//*.xls;
            file.ShowDialog();
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                this.txtFilePath3.Text = file.FileName;
            }
        }

        /// <summary>
        /// 根据excel生成sql脚本——生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGen3_Click(object sender, EventArgs e)
        {
            if (this.txtFilePath3.Text == "")
            {
                MessageBox.Show("请先选择Excel路径");
                return;
            }

            HSSFWorkbook workbook;
            using (FileStream file = new FileStream(this.txtFilePath3.Text, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(file);
                file.Close();
            }
            var sheet = workbook.GetSheetAt(0);

            string tableName = "";
            string tableDesc = "";

            List<DTModel> listModel = new List<DTModel>();
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                DTModel model = new DTModel();
                model.Index = i;
                for (int j = 1; j < 8; j++)
                {
                    var cell = row.GetCell(j);
                    if (cell == null) continue;

                    string cellValue = cell.ToString().Trim();
                    switch (j)
                    {
                        case 1://表名
                            tableName = cellValue;
                            break;

                        case 2://表名描述
                            tableDesc = cellValue;
                            break;

                        case 3://列名
                            model.Name = cellValue;
                            break;

                        case 4://数据类型
                            model.DataType = cellValue;
                            break;

                        case 5://是否允许Null
                            model.IsNull = cellValue == "Checked";
                            break;

                        case 6://列名说明
                            model.Desc = cellValue;
                            break;

                        case 7://是否是主键
                            model.IsPK = cellValue == "PK";
                            break;

                        case 8://默认值
                            model.DefaultValue = cellValue;
                            break;

                        default:
                            break;
                    }
                    model.TableName = tableName;
                    model.TableDesc = tableDesc;
                }
                if (!string.IsNullOrEmpty(model.TableName) && !string.IsNullOrEmpty(model.Name))
                {
                    listModel.Add(model);
                }
            }

            if (listModel != null && listModel.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                string temp = @"USE master
GO

create database HRS4
GO

USE HRS4
GO";
                sb.AppendLine(temp);

                var query = listModel.GroupBy(d => d.TableName);
                foreach (var item in query)
                {
                    string tableName2 = item.FirstOrDefault().TableName;
                    string primaryKey = "";
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    StringBuilder sb3 = new StringBuilder();
                    foreach (var item2 in item)
                    {
                        sb1.AppendLine(string.Format(" [{0}] {1} {2},", item2.Name, item2.DataType, item2.IsNull ? "NULL" : "NOT NULL"));

                        if (!string.IsNullOrEmpty(item2.DefaultValue))
                        {
                            sb2.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] ADD  DEFAULT (({1})) FOR [{2}]", item2.TableName, item2.DefaultValue, item2.Name));
                            sb2.AppendLine("GO");
                            sb2.AppendLine();
                        }

                        sb3.AppendLine(string.Format("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{0}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{1}', @level2type=N'COLUMN',@level2name=N'{2}'", item2.Desc, item2.TableName, item2.Name));
                        sb3.AppendLine("GO");
                        sb3.AppendLine();

                        if (item2.IsPK)
                        {
                            primaryKey = item2.Name;
                        }
                    }

                    string str = string.Format(@"
CREATE TABLE [dbo].[{0}](
	{1}
 CONSTRAINT [{2}] PRIMARY KEY CLUSTERED
(
	[{3}] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

{4}

{5}

", tableName2, sb1.ToString(), "PK_" + tableName2 + "_" + primaryKey, primaryKey, sb2.ToString(), sb3.ToString());
                    sb.AppendLine(str);
                }

                richTextBox19.Text = sb.ToString();
            }
        }

        /// <summary>
        /// 获取文件夹的文件列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox9_Click(object sender, EventArgs e)
        {
            //richTextBox11.Text = GetAllFiles("C:\\Users\\admi\\Desktop\\新建文件夹", 0);
            //return;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    textBox9.Text = selectedPath;

                    DirectoryInfo di = new DirectoryInfo(selectedPath);

                    string ParentPath = "";
                    string thisFolder = "----文件夹：" + di.Name + "\r\n";
                    if (di.Parent != null)
                    {
                        ParentPath = di.Parent.FullName;
                    }
                    if (cbox2.Checked)
                    {
                        ParentPath = "";
                        thisFolder = "----文件夹：" + di.FullName + "\r\n";
                    }

                    richTextBox11.Text = thisFolder + CommonCode.GetAllFiles(selectedPath, ParentPath, 0);
                }
            }
        }

        /// <summary>
        /// 生成U8任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            foreach (var item in Enum.GetValues(typeof(U8InterfaceEnum)))
            {
                string enumDesc = CommonCode.GetCustomEnumDesc(typeof(U8InterfaceEnum), (U8InterfaceEnum)item);
                sb.AppendLine(GetJob_Json(item.ToString(), enumDesc, "AddList"));
                sb.AppendLine(GetJob_Json(item.ToString(), enumDesc, "SendData"));

                sb2.AppendLine(GetJob_Method(item.ToString(), enumDesc, "AddList"));
                sb2.AppendLine(GetJob_Method(item.ToString(), enumDesc, "SendData"));

                sb3.AppendLine(GetJob_Rejister(item.ToString(), enumDesc, "AddList"));
                sb3.AppendLine(GetJob_Rejister(item.ToString(), enumDesc, "SendData")).AppendLine();
            }
            string str = @"
//cron-expression说明：
//格式：分钟 小时 天 月 星期
//示例：0 2 1 * *  每月的第一天02:00执行

";
            richTextBox12.Text = str + "[" + sb.ToString() + "]";
            richTextBox21.Text = sb2.ToString();
            richTextBox22.Text = sb3.ToString();
        }

        private string GetJob_Json(string enumName, string enumDesc, string methodType)
        {
            string cronExpression = textBox10.Text;
            if (methodType == "SendData")
            {
                cronExpression = textBox21.Text;
            }

            // 'ExecuteTime': '{4}',
            string str = @"
  __1
    'job-name': '{1} {0}_{10}',
    'job-type': '{2}.Jobs.{0}_{10}, {2}',
    'cron-expression': '{3}',
    'timezone': 'China Standard Time',
    'queue': 'jobs',
    'job-data': __1
      'ServiceDesc': '{1}',
      'TimeSpan': {5},
      'TimeSpanType': '{6}',
      'PageSize': {9},
      'ErrorRerunNumNumber': {8},
      'IsSendSummary': {7},
      'IsEnable': {11}
    __2
  __2,";
            return string.Format(str, enumName, enumDesc, textBox11.Text, cronExpression, textBox19.Text, textBox20.Text, comboBox2.Text, comboBox3.Text, textBox22.Text, textBox23.Text, methodType, comboBox4.Text)
                .Replace("__1", "{").Replace("__2", "}").Replace("'", "\"");
        }

        private string GetJob_Method(string enumName, string enumDesc, string methodType)
        {
            string str = @"
    /// <summary>
    /// {1}
    /// </summary>
    public class {0}_{2} : IRecurringJob
    __1
        /// <summary>
        /// {1} {0}_{2}
        /// </summary>
        /// <param name='context'></param>
        [DisplayName('{1} {0}_{2}')]
        public void Execute(PerformContext context)
        __1
            BaseJobs.{2}(context);
        __2
    __2";
            return string.Format(str, enumName, enumDesc, methodType)
                .Replace("__1", "{").Replace("__2", "}").Replace("'", "\"");
        }

        private string GetJob_Rejister(string enumName, string enumDesc, string methodType)
        {
            string str = @"builder.Register(x => new {0}_{2}());";
            return string.Format(str, enumName, enumDesc, methodType);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var list = CommonCode.DataBaseToListModel_T4(textBox2.Text, comboBox1.Text).ToList();
            richTextBox3.Text = CommonCode.CreateModel(list, "Model", "Model", "    [Serializable][DataContract]", "        [DataMember]");
        }
    }
}