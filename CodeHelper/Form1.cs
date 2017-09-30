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

        //选择Excel文件
        private void txtFilePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel|*.xls";//*.xls;
            file.ShowDialog();
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                this.txtFilePath.Text = file.FileName;
            }
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

                for (int j = 0; j < 7; j++)
                {
                    var cell = row.GetCell(j);
                    if (cell == null) continue;

                    string str = cell.ToString().Trim();
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    list_str.Add(str);
                }
            }

            //按照格式生成文档
            Generate(list_str);

            this.btnOpen.Visible = true;
            this.lblResult.Text = "成功！请点击打开按钮，生成的.xls";
            this.lblResult.ForeColor = Color.Green;
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

            if (workbook.NumberOfSheets == 1)
            {
                workbook.CreateSheet("生成的Sheet");
            }
            ISheet sheet = (HSSFSheet)workbook.GetSheetAt(1);

            //ISheet newSheet = sheet.CopySheet("生成的Sheet", false);
            //newSheet.SetActive(true);

            //for (int i = 0; i < newSheet.LastRowNum + 1; i++)
            //{
            //    newSheet.RemoveRow(newSheet.GetRow(i));
            //}

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

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox17_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    textBox17.Text = dialog.SelectedPath;
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            timer1.Start();

            //label37.Text = "暂时没内容";
            var dir = textBox17.Text;
            if (dir == "")
            {
                MessageBox.Show("请先选择文件夹路径");
                return;
            }
            List<GlitzhomeOrder> listModel = new List<CodeHelper.GlitzhomeOrder>();

            foreach (string filePath in System.IO.Directory.GetFiles(dir, "*.xlsx"))
            {
                string fileName = "";
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    XSSFWorkbook workbook;
                    workbook = new XSSFWorkbook(file);
                    fileName = file.Name;
                    file.Close();

                    var sheet = workbook.GetSheetAt(0);

                    for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null)
                        {
                            continue;
                        }

                        GlitzhomeOrder model = new GlitzhomeOrder();
                        model.orderid = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('A'));

                        if (string.IsNullOrEmpty(model.orderid))
                        {
                            continue;
                        }
                        else if (model.orderid == "orderid")
                        {
                            continue;
                        }

                        model.clientpo = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('B'));
                        model.shipping_instructions = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('C'));
                        model.comments = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('D'));
                        model.start_ship_date_value = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('E'));
                        model.orddate = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('F'));
                        model.ship_complete = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('G'));
                        model.shipMethod = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('H'));
                        model.alternateid = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('I'));
                        model.ship2name = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('J'));
                        model.ship2attention = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('K'));
                        model.ship2address1 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('L'));
                        model.ship2address2 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('M'));
                        model.ship2address3 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('N'));
                        model.ship2city = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('O'));
                        model.ship2state = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('P'));
                        model.ship2zip = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Q'));
                        model.ship2country = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('R'));
                        model.ship2isresidential = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('S'));
                        model.ship2phone = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('T'));
                        model.ship2email = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('U'));
                        model.carrier = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('V'));
                        model.weight = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('W'));
                        model.num_boxes = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('X'));
                        model.length = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Y'));
                        model.width = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Z'));

                        model.height = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('A', 'A'));
                        model.pakage_type = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('B', 'A'));
                        model.billingoption = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('C', 'A'));
                        model.ref1 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('D', 'A'));
                        model.ref2 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('E', 'A'));
                        model.ref3 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('F', 'A'));
                        model.ref4 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('G', 'A'));
                        model.ref5 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('H', 'A'));
                        model.ref6 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('I', 'A'));
                        model.tp_accnt = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('J', 'A'));
                        model.billAddr1 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('K', 'A'));
                        model.billAddr2 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('L', 'A'));
                        model.billname = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('M', 'A'));
                        model.billAttention = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('N', 'A'));
                        model.billCity = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('O', 'A'));
                        model.billCountry = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('P', 'A'));
                        model.billPhone = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Q', 'A'));
                        model.billState = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('R', 'A'));
                        model.billZip = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('S', 'A'));
                        model.fromadd1 = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('T', 'A'));
                        model.fromcity = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('U', 'A'));
                        model.fromstate = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('V', 'A'));
                        model.fromzip = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('W', 'A'));
                        model.skued = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('X', 'A'));
                        model.clientskued = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Y', 'A'));
                        model.qtyed = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('Z', 'A'));

                        model.alt_nameed = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('A', 'B'));
                        model.descriptioned = ExcelHelper.GetCellValue(workbook, sheet, i, ExcelHelper.GetNumByChar('B', 'B'));

                        model.FileName = fileName;
                        model.CreateDate = DateTime.Now;

                        listModel.Add(model);
                    }
                }
            }

            if (listModel.Count > 0)
            {
                try
                {
                    using (MyDBEntities context = new MyDBEntities())
                    {
                        context.GlitzhomeOrders.AddRange(listModel);
                        int i = context.SaveChanges();
                        if (i > 0)
                        {
                            timer1.Stop();

                            MessageBox.Show("成功导入了" + i + "条数据！去数据库看看。");

                            label37.Text = "成功导入了" + i + "条数据！去数据库看看。";
                            label37.ForeColor = Color.Green;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int time = int.Parse(label39.Text.ToString()) + 1;
            label39.Text = time.ToString();
        }

        private void label41_Click(object sender, EventArgs e)
        {
        }

        private void textBox19_TextChanged(object sender, EventArgs e)
        {
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //<th data-options="field:'OuterBoxRate',width:100,align:'center'">外箱率</th>
            //{ field: 'OuterBoxRate',width: 80,align: 'center',title: 'Case Pack'},

            string str = richTextBox18.Text;

            str = str.Replace("\n", "^");
            string[] arr = str.Split('^');

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                string item = arr[i];
                if (item.Contains("<th data-options=") || item.Contains("</th>"))
                {
                    string temp = item.Replace("<th data-options=\"", "{ ")
                        .Replace("'\">", ",title: '")
                        .Replace(",formatter:productNoFormatter\">", ",title: '")
                        .Replace("</th>", "'},")
                        .Replace("\">", "")
                        .Replace("align:'left,title", "align:'left',title")
                        .Replace("align:'center,title: '", "align:'center',title: '")
                        .Replace("align:'center,title: '", "align:'center',title: '")
                        .Replace(",hidden:true'}", ",hidden:true}")
                        .Replace(",editor:{type:'validatebox'}", ",editor:{type:'validatebox'},title: '")
                        .Replace(",options:{precision:0}}", ",options:{precision:0}},title: '")
                        .Replace(",options:{required:true,validType:['integer']}}", ",options:{required:true,validType:['integer']}},title: '")
                        .Replace(",editor:{type:'validatebox',options:{required:true}}", ",editor:{type:'validatebox',options:{required:true}},title: '")
                        .Trim();
                    sb.Append(temp + "\r\n");
                }
                else if (i > 0 && arr[i - 1].Contains("formatter"))
                {
                    sb.Append(item + "\r\n");
                }
            }
            richTextBox17.Text = sb.ToString();
        }

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

        private void button12_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.Combine(Application.StartupPath, "Data"));
        }

        private void txtFilePath2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel|*.xls";//*.xls;
            file.ShowDialog();
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                this.txtFilePath2.Text = file.FileName;
            }
        }

        private void btnGen2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.txtFilePath2.Text == "")
                {
                    MessageBox.Show("请先选择Excel路径");
                    return;
                }

                HSSFWorkbook workbook;
                using (FileStream file = new FileStream(this.txtFilePath2.Text, FileMode.Open, FileAccess.Read))
                {
                    workbook = new HSSFWorkbook(file);
                    file.Close();
                }
                var sheet = workbook.GetSheetAt(0);

                List<DTModel> listModel = new List<DTModel>();
                for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var cell = row.GetCell(3);
                    if (cell == null) continue;

                    string str = cell.ToString().Trim();
                    string desc = "";
                    if (!string.IsNullOrEmpty(str))
                    {
                        var cellDesc = row.GetCell(3);
                        if (cellDesc != null)
                        {
                            desc = row.GetCell(4).ToString().Trim();
                        }

                        listModel.Add(new DTModel()
                        {
                            Name = str,
                            Desc = desc,
                        });
                    }
                }
                List<DTModel> listContains = new List<DTModel>();//包含的
                for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var cell = row.GetCell(0);
                    if (cell == null) continue;

                    string str = cell.ToString().Trim();

                    var query = listModel.Where(d => d.Name == str);
                    if (query.Count() > 0)
                    {
                        ExcelHelper.SetCellValue(sheet, i, 6, query.FirstOrDefault().Name);
                        ExcelHelper.SetCellValue(sheet, i, 7, query.FirstOrDefault().Desc);

                        listContains.Add(query.FirstOrDefault());
                    }
                }

                var listNotContains = listModel.Except(listContains).ToList();
                for (int i = 0; i < listNotContains.Count; i++)
                {
                    ExcelHelper.SetCellValue(sheet, i, 9, listNotContains[i].Name);
                    ExcelHelper.SetCellValue(sheet, i, 10, listNotContains[i].Desc);
                }

                FileStream swQuoteSheet = File.OpenWrite("Data\\Temp模板" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".xls");
                workbook.Write(swQuoteSheet);
                swQuoteSheet.Close();
                MessageBox.Show("Excel生成成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

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
                            model.IsNull = cellValue == "Checked" ? "NULL" : "NOT NULL";
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
                        sb1.AppendLine(string.Format(" [{0}] {1} {2},", item2.Name, item2.DataType, item2.IsNull));


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

                    richTextBox11.Text = thisFolder + GetAllFiles(selectedPath, ParentPath, 0);
                }
            }
        }

        public static string GetAllFiles(string TheFolder, string ParentPath, int Index)
        {
            StringBuilder sb = new StringBuilder();
            if (Directory.Exists(TheFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TheFolder);
                foreach (FileInfo item2 in di.GetFiles().OrderBy(d => d.Name))//遍历文件
                {
                    sb.AppendLine(item2.Name);
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

    }
}