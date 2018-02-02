using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using System.Data;


namespace CAST
{
    public class ExcelReportHelper
    {

        public static void BuildReport(CASTRequestType req, DataTable new_violations, DataTable removed_violations, string message, string file, int files_num, int obj_num, decimal prod_num, decimal prod_golden_num, string baseline_version)
        {
            var workbook = new XLWorkbook(AppDomain.CurrentDomain.BaseDirectory + "\\QualityCheckReport.xlsx");

            var worksheet1 = workbook.Worksheets.Worksheet("Quality Check");
            var worksheet2 = workbook.Worksheets.Add("Added Violations");
            var worksheet3 = workbook.Worksheets.Add("Removed Violations");

            DumpViolations(worksheet2, new_violations,"TableAdded");
            DumpViolations(worksheet3, removed_violations,"TableRemoved");

            worksheet1.Cell("B7").Value = req.APP;
            worksheet1.Cell("B8").Value = req.IDR;
            worksheet1.Cell("B9").Value = req.CDT;
            worksheet1.Cell("B10").Value = req.VER;
            worksheet1.Cell("B11").Value = baseline_version;

            worksheet1.Cell("B12").Value = files_num;
            worksheet1.Cell("B13").Value = obj_num;

            Decimal tot_new, tot_new_gold,tot_rem,tot_rem_gold;

            if (new_violations.Rows.Count > 0)
            {
                if(!Decimal.TryParse(new_violations.Compute("Sum(new_count)", "is_excluded='no'").ToString(), out tot_new))
                    tot_new = 0;
                if(!Decimal.TryParse(new_violations.Compute("Sum(new_count)", "is_excluded='no' and is_golden='yes'").ToString(), out tot_new_gold))
                    tot_new_gold = 0;
            }
            else
            {
                tot_new = 0;
                tot_new_gold = 0;
            }
  
            worksheet1.Cell("B14").Value = tot_new;
            worksheet1.Cell("B15").Value = tot_new_gold;

            if (removed_violations.Rows.Count > 0)
            {
                if (!Decimal.TryParse(removed_violations.Compute("Sum(removed_count)", "is_excluded='no'").ToString(), out tot_rem))
                    tot_rem = 0;
                if (!Decimal.TryParse(removed_violations.Compute("Sum(removed_count)", "is_excluded='no' and is_golden='yes'").ToString(), out tot_rem_gold))
                    tot_rem_gold = 0;
            }
            else
            {
                tot_rem = 0;
                tot_rem_gold = 0;
            }

            worksheet1.Cell("B16").Value = tot_rem;
            worksheet1.Cell("B17").Value = tot_rem_gold;

            worksheet1.Cell("B18").Value = message;

            if(message=="Passed")
                worksheet1.Cell("B18").Style.Fill.BackgroundColor = XLColor.LightGreen;
            else
                worksheet1.Cell("B18").Style.Fill.BackgroundColor = XLColor.Red;

            worksheet1.Cell("B22").Value = prod_num + tot_new - tot_rem;
            worksheet1.Cell("B23").Value = prod_golden_num + tot_new_gold - tot_rem_gold;

 
            workbook.SaveAs(file);

        }


        public static void DumpViolations(IXLWorksheet ws, DataTable table, string name)
        {
            string address = null;
            int row = 1;

            foreach (DataColumn col in table.Columns)
            {
                address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());
                IXLCell cell = ws.Cell(address);
                cell.Value = col.ColumnName;
            }

            foreach (DataRow dr in table.Rows)
            {
                row++;
                foreach (DataColumn col in dr.Table.Columns)
                {
                    address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());

                    ws.Cell(address).Value = dr[col.ColumnName];
                }
            }

            ws.Columns(1, table.Columns.Count).AdjustToContents();

            var rngData = ws.Range("A1:" + address);
            var excelTable = rngData.CreateTable(name);
        }
 
    }
}