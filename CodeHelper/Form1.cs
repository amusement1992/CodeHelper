﻿using CodeCreate;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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
        /// 格式化换行——生成
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
                for (int j = 0; j < dt_tables.Rows.Count; j++)
                {
                    string colName = dt_tables.Rows[j]["columnName"].ToString().Trim().ToUpper();//字段名
                    string colType = CommonCode.GetColType(dt_tables.Rows[j]["columnType"].ToString());//字段类型
                    if (j == dt_tables.Rows.Count - 1)
                    {
                        sb_column1.Append("a.");
                        sb_column1.Append(colName);
                        sb_column2.Append(configModel.MARK + colName);
                        sb_column3.AppendLine("					new SqlParameter(\"" + configModel.MARK + colName + "\", " + colType + ")};");
                        sb_column4.AppendLine("            parameters[" + j + "].Value = model." + colName + ";");
                    }
                    else
                    {
                        sb_column1.Append("a.");
                        sb_column1.Append(colName + ",");
                        sb_column2.Append(configModel.MARK + colName + ",");
                        sb_column3.AppendLine("					new SqlParameter(\"" + configModel.MARK + colName + "\", " + colType + "),");
                        sb_column4.AppendLine("            parameters[" + j + "].Value = model." + colName + ";");
                    }
                }
                richTextBox6.Text = sb_column1.ToString() + "\r\n\r\n";
                richTextBox6.Text += sb_column2.ToString();

                //生成model
                CreateModel(dt_tables);

                #endregion 生成内容
            }
        }

        public void CreateModel(DataTable dt_tables)
        {
            //[Desc];ValidDate||Date;PriceInputDate||Date;$ID||int;CtnsPallet||Int32;OuterBoxRate||Int32;InnerBoxRate||Int32;PDQPackRate||Int32;PackingMannerEnID||Int32;MOQZh||Int32;MOQEn||Int32;PackingMannerZhID||Int32;PortID||Int32;StyleID||Int32;CustomerID||Int32;FactoryID||Int32;ClassifyID||Int32;No;NoFactory;Name;[Image];Unit;[Length];Height;Width;[Weight];IngredientZh;IngredientEn;PDQLength;PDQWidth;PDQHeight;InnerLength;InnerWidth;InnerHeight;InnerWeight;OuterLength;OuterWidth;OuterHeight;OuterWeightGross;OuterWeightNet;PriceFactory;MiscImportLoad;Agent;SRP;HTS;DutyPercent;FreightRate;Remarks;Comment;

            bool isPrimeKey = false;
            string primaryKey = "";

            #region Model

            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            int i = 0;
            //遍历每个字段
            foreach (DataRow dr in dt_tables.Rows)
            {
                string columnName = dr["columnName"].ToString().Trim();//字段名
                string columnType = dr["columnType"].ToString().Trim();//字段类型
                string columnComment = dr["columnComment"].ToString().Trim();//字段注释
                string nullable = dr["nullable"].ToString().Trim();//是否可空（Y是不为空，N是为空）
                string data_default = dr["data_default"].ToString().Trim();//默認值
                string data_maxLength = dr["char_col_decl_length"].ToString().Trim();//最大長度
                string bool_primaryKey = dr["primaryKey"].ToString().Trim();//主键 值为Y或N
                if (bool_primaryKey.ToUpper() == "Y")//存在主键
                {
                    isPrimeKey = true;
                    primaryKey = columnName.ToUpper();
                }
                string type = "";

                switch (columnType.ToLower())
                {
                    case "int":
                        type = "Int";
                        break;

                    case "date":
                    case "datetime":
                        type = "Date";
                        break;

                    default:
                        break;
                }
                if (type.Length != 0)
                {
                    type = "||" + type;
                }
                //sb.Append(columnName + type + ";\r\n\r\n");
                sb.Append("<ds:Col SortExpression=\"a." + columnName + "\" ID=\"Col" + i + "\" runat=\"server\" ColExpression=\"" + columnName + "\"  HeaderText=\"" + columnComment + "\" />\r\n");

                sb2.Append("arrayList.Add(DB.CreateParameter(\"@" + columnName + "\", DbType.String," + columnName + "));\r\n");
                i++;
            }
            richTextBox3.Text = sb.ToString() + "\r\n\r\n";
            richTextBox3.Text += sb2.ToString();

            #endregion Model
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
        /// 新建表——生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string tableName = textBox1.Text.Trim();
            string content = richTextBox4.Text.Trim();
            //内容（格式如：字段名?类型?非空?主键?自增长|）：
            //OrderID?varchar(100)?P?Identity(1,1)?Null

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE " + tableName + "(\r\n");
            string[] arr = content.Split('|');
            for (int i = 0; i < arr.Length; i++)
            {
                string item = arr[i];
                if (!string.IsNullOrEmpty(item))
                {
                    if (item.Contains('?'))
                    {
                        string[] arr_children = item.Split('?');
                        for (int j = 0; j < arr_children.Length; j++)
                        {
                            switch (j)
                            {
                                case 0://字段名
                                    sb.Append(arr_children[j] + " ");
                                    break;

                                case 1://类型
                                    sb.Append(arr_children[j] + " ");
                                    break;

                                case 2://非空
                                    sb.Append(arr_children[j] + " ");
                                    break;

                                case 3://主键
                                    sb.Append(arr_children[j] + " ");
                                    break;

                                case 4://自增长
                                    sb.Append(arr_children[j] + " ");
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        sb.Append(item + " [nvarchar](100) null]");
                    }
                    if (i == arr.Length - 1)
                    {
                        sb.Append("\r\n");
                    }
                    else
                    {
                        sb.Append(",\r\n");
                    }
                }
            }
            sb.Append(")");

            richTextBox5.Text = sb.ToString();
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
        /// 生成拼接的代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            string str = textBox8.Text.Trim() + ".Append(@\"" + richTextBox10.Text.Replace('"', '\'').Replace("<%=", "\"+").Replace("%>", "+@\"") + "\");";
            richTextBox9.Text = str;
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox13.Text = richTextBox14.Text
                .Replace("J_Length_IN", "LengthIN.ToString()")
                .Replace("L_Width_IN", "WidthIN.ToString()")
                .Replace("N_Height_IN", "HeightIN.ToString()")
                .Replace("A_VendorItemNo", "No")
                .Replace("F_ProductDes", "Desc")
                .Replace("BZ_PackingType", "PackingMannerEnName")
                .Replace("AH_OutterRatio", "OuterBoxRate.ToString()")
                .Replace("X_InnerRatio", "InnerBoxRate.ToString()")
                .Replace("AO_Volumn", "OuterVolume.ToString()")
                .Replace("BB_FOBChinaPort", "Cost.ToString()")
                .Replace("AU_ShippingPort", "PortEnName.ToString()")
                .Replace("BY_UPC", "UPC")
                .Replace("CC_Remarks", "Remarks")
                .Replace("BL_HTSCode", "HTS")
                .Replace("CA_MaterialComposition", "IngredientEn")
                .Replace("A_VendorItemNo", "No")
                .Replace("CB_MOQ", "MOQEn.ToString()")
                .Replace("AJ_OutterLength_IN", "OuterLengthIN.ToString()")
                .Replace("AL_OutterWidth_IN", "OuterWidthIN.ToString()")
                .Replace("AN_OutterHeight_IN", "OuterHeightIN.ToString()")
                .Replace("BM_DutyRate", "DutyPercent.ToString()")
                .Replace("P_LBS", "WeightLBS.ToString()")
                .Replace("BE_FOBUS", "FOBUS.ToString()")
                .Replace("BO_FreightRate", "Freight.ToString()")
                .Replace("BV_Retail", "Retail.ToString()")

                .Replace("S_PDQLength_IN", "PDQLengthIN.ToString()")
                .Replace("U_PDQWidth_IN", "PDQWidthIN.ToString()")
                .Replace("W_PDQHeight_IN", "PDQHeightIN.ToString()")
                .Replace("AR_GrossWeightLBS", "OuterWeightGrossLBS")
                .Replace("Z_InnerLength_IN", "InnerLengthIN.ToString()")
                .Replace("AB_InnerWidth_IN", "InnerWidthIN.ToString()")
                .Replace("AD_InnerHeight_IN", "InnerHeightIN.ToString()")

                .Replace("H_Style", "StyleName")
                .Replace("AQ_GrossWeight", "OuterWeightGross.ToString()")
                .Replace("AS_NetWeight", "OuterWeightNet.ToString()")
                .Replace("AI_OutterLength_CM", "OuterLength.ToString()")
                .Replace("AK_OutterWidth_CM", "OuterWidth.ToString()")
                .Replace("AN_OutterHeight_IN", "OuterHeight.ToString()")
                .Replace("I_Length_CM", "Length.ToString()")
                .Replace("K_Width_CM", "Width.ToString()")
                .Replace("M_Height_CM", "Height.ToString()")
                .Replace("BG_DDP", "DDP.ToString()")
                .Replace("BW_SRP", "Retail.ToString()")
                .Replace("D_Manufactor", "FactoryName")

                .Replace("NPOIUtil.", "MakerExcel.")
                .Replace("Path.Combine(this.txtImagePath.Text, quote.No + \".jpg\")", " quote.Image");
            ;

            //.ToString()
        }

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


        //打开
        private void txtFilePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel|*.xls";//*.xls;
            file.ShowDialog();
            if (!string.IsNullOrWhiteSpace(file.FileName))
                this.txtFilePath.Text = file.FileName;


        }

        //生成excel
        private void btnGen_Click(object sender, EventArgs e)
        {
            if (this.txtFilePath.Text == "")
            {
                MessageBox.Show("请先选择Excel路径");
                return;
            }

            List<string> list_str = new List<string>();

            HSSFWorkbook workbook;
            using (FileStream file = new FileStream(this.txtFilePath.Text, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(file);
                file.Close();
            }
            var sheet = workbook.GetSheetAt(0);

            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                var cell = row.GetCell(0);
                if (cell == null) continue;
                string str = cell.ToString().Trim();
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                list_str.Add(str);
            }



            //按照格式生成文档
            Generate(list_str);

            this.btnOpen.Visible = true;
            this.lblResult.Text = "请点击打开按钮，生成的.xls";
        }


        private void Generate(List<string> list_str)
        {

            IWorkbook workbook = new HSSFWorkbook();

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            using (FileStream file = new FileStream(txtFilePath.Text, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(file);
                file.Close();
            }

            int rowCount = 0;
            if (list_str != null && list_str.Count > 0)
            {
                rowCount = list_str.Count / 7;
            }

            ISheet sheet = workbook.GetSheetAt(1);

            int index = 0;
            for (int i = 0; i < rowCount; i++)//行
            {
                for (int j = 0; j < 7; j++)//列
                {
                    ExcelHelper.SetCellValue(sheet, i, j, list_str[index]);
                    index++;
                }
            }

            FileStream swQuoteSheet = File.OpenWrite("Data\\生成的.xls");
            workbook.Write(swQuoteSheet);
            swQuoteSheet.Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.Combine(Application.StartupPath, "Data"));
        }
    }
}