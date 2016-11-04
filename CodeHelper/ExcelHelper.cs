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
        public static void SetCellValue(XSSFSheet sheet, int rowIndex, int cellIndex, string value, ICellStyle cellStyle)
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

    }
}
