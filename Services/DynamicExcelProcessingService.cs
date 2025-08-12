using DocumentFormat.OpenXml.Spreadsheet;
using FaizHesaplamaAPI.Models;
using Microsoft.EntityFrameworkCore.Internal;
using OfficeOpenXml;
using System.Data;
using System.Linq.Expressions;

namespace FaizHesaplamaAPI.Services
{
    public class DynamicExcelProcessor
    {
        public MemoryStream ProcessExcelWithUserRules(IFormFile file , List<UserDefinedRule> rules)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var package = new ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            //Kurallar:

            foreach (var rule in rules)
            {
                int colIndex = FindColumnIndex(worksheet, rule.ColumnName);
                if (colIndex == -1)
                {
                    continue;
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        if (rule.Operation == "dateDiff" && rule.Values.Count >= 2)
                        {
                            int col1 = FindColumnIndex(worksheet, rule.Values[0]);
                            int col2 = FindColumnIndex(worksheet, rule.Values[1]);

                            if (col1 !=-1 && col2 != -1)
                            {
                                DateTime date1 = worksheet.Cells[row, col1].GetValue<DateTime>();
                                DateTime date2 = worksheet.Cells[row, col2].GetValue<DateTime>();

                                worksheet.Cells[row, colIndex].Value = (date2 - date1).Days;
                            }
                        }

                        else if (rule.Operation == "sum" && rule.Values.Count > 0)
                        {
                            double sum = 0;
                            foreach (var valCol in rule.Values)
                            {
                                int col = FindColumnIndex(worksheet, valCol);
                                if (col != -1)
                                    sum += worksheet.Cells[row, col].GetValue<double>();

                            }
                            worksheet.Cells[row, colIndex].Value = sum;
                        }

                    }
                    catch (Exception ex)
                    {
                        worksheet.Cells[row , colIndex].Value = $"Err : {ex.Message}";
                    }
                }

            }

            var outputStream = new MemoryStream(package.GetAsByteArray());
            return outputStream;
        }

        private int FindColumnIndex(ExcelWorksheet sheet , string ColumnName)
        {
            int colCount = sheet.Dimension.Columns;
            for (int col = 1; col <= colCount; col++)
            {
                if (sheet.Cells[1, col].Text.Trim().Equals(ColumnName, StringComparison.OrdinalIgnoreCase))
                    return col;
            }
            return -1;
        }

    }
       

}