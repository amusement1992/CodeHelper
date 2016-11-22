using CodeHelper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCreate
{
    public class ExcelHelper
    {
        /// <summary>
        /// 字符串必须大写
        /// </summary>
        /// <param name="c"></param>
        /// <param name="first"></param>
        /// <returns></returns>
        public static int GetNumByChar(char c, char first = '0')
        {
            if (first == '0')
                return c - 65;
            else
                return (first - 64) * 26 + GetNumByChar(c);
        }

        /// <summary>
        /// 设置单元格的内容
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="cellIndex"></param>
        /// <param name="value"></param>
        public static void SetCellValue(ISheet sheet, int rowIndex, int cellIndex, string value, ICellStyle cellStyle = null)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null)
            {
                row = sheet.CreateRow(rowIndex);
            }
            row.CreateCell(cellIndex).SetCellValue(value);

            if (cellStyle != null)
            {
                row.GetCell(cellIndex).CellStyle = cellStyle;
            }
        }


        /// <summary>
        /// 获取单元格的内容
        /// </summary>
        /// <param name="sheetQuoteSheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="cellIndex"></param>
        /// <param name="value"></param>
        public static string GetCellValue(IWorkbook workbook, ISheet sheetQuoteSheet, int rowIndex, int cellIndex)
        {
            IRow row = sheetQuoteSheet.GetRow(rowIndex);
            if (row != null)
            {
                ICell cell = row.GetCell(cellIndex);
                if (cell != null)
                {
                    return GetCellForamtValue(cell, workbook);
                }
            }
            return null;
        }

        public static string GetCellForamtValue(ICell cell, IWorkbook workbook)
        {
            try
            {

                string value = "";
                switch (cell.CellType)
                {
                    case CellType.Blank: //空数据类型处理
                        value = "";
                        break;

                    case CellType.String: //字符串类型
                        value = cell.StringCellValue;
                        break;

                    case CellType.Numeric: //数字类型
                        if (Utils.IsDouble(cell.NumericCellValue.ToString()))
                        {
                            value = cell.NumericCellValue.ToString();
                        }
                        else
                        {
                            value = cell.DateCellValue.ToString();
                        }
                        break;

                    case CellType.Formula:
                        if (workbook.GetType() == typeof(XSSFWorkbook))
                        {

                            XSSFFormulaEvaluator e = new XSSFFormulaEvaluator(workbook);
                            value = e.Evaluate(cell).NumberValue.ToString();
                        }
                        else
                        {
                            HSSFFormulaEvaluator e = new HSSFFormulaEvaluator(workbook);
                            value = e.Evaluate(cell).NumberValue.ToString();
                        }
                        //value = cell.CellFormula;
                        break;

                    default:
                        value = "";
                        break;
                }
                return value;
            }
            catch (Exception)
            {
                return cell.ToString();
            }
        }
    }
}