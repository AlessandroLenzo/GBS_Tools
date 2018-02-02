using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using System.Data;
using CAST;

namespace TestClosedXML
{
    class Program
    {
        static void Main(string[] args)
        {
              var workbook = new XLWorkbook("template.xlsx");
              var worksheet1 = workbook.Worksheets.Worksheet("Quality Check");
              //worksheet1.Cell("A6").Value = "QualityCheck Report";
              //worksheet1.Cell("A6").Style.Font.Bold = true;
              //worksheet1.Cell("A6").Style.Font.SetFontSize(20);
              //workbook.SaveAs("Report.xlsx");

              //worksheet1.Style.Fill.BackgroundColor = XLColor.White;


              worksheet1.Cell("B7").Value = "app1";
              worksheet1.Cell("B8").Value = "1558";
              worksheet1.Cell("B9").Value = "25/02/2015 13:05";
             
              worksheet1.Cell("B10").Value = "v1.0";
              worksheet1.Cell("B11").Value = "bla";

              worksheet1.Cell("B12").Value = 120;
              worksheet1.Cell("B13").Value = 222;

              worksheet1.Cell("B14").Value = 1000;
              worksheet1.Cell("B15").Value = 2555;


              worksheet1.Cell("B16").Value = 7500;
              worksheet1.Cell("B17").Value = 128;


              string message = "Passed";
              worksheet1.Cell("B18").Value = message;

            
              //worksheet1.Range("A7:B18").Style.Font.Bold = true;
              //worksheet1.Range("A7:A18").Style.Font.FontColor = XLColor.DarkBlue;

              //worksheet1.Range("A7:A18").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
              //worksheet1.Range("A7:A18").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
              //worksheet1.Range("A7:A18").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
              //worksheet1.Range("A7:A18").Style.Fill.BackgroundColor = XLColor.LightYellow;
              //worksheet1.Columns(1, 2).AdjustToContents();

              if (message == "Passed")
                  worksheet1.Cell("B18").Style.Fill.BackgroundColor = XLColor.LightGreen;
              else
                  worksheet1.Cell("B18").Style.Fill.BackgroundColor = XLColor.Red;


              //worksheet1.Cell("A21").Value = "Backlog Report";
              //worksheet1.Cell("A21").Style.Font.Bold = true;
              //worksheet1.Cell("A21").Style.Font.SetFontSize(20);
              //worksheet1.Cell("A22").Value = "Remaining Violations";
              //worksheet1.Cell("A23").Value = "   Golden Violations";

              worksheet1.Cell("B22").Value = 55125;
              worksheet1.Cell("B23").Value = 1880;
              //worksheet1.Range("A22:B23").Style.Fill.BackgroundColor = XLColor.LightYellow;
 

              workbook.SaveAs("Report.xlsx");


        //    string DevConnStr = "Data Source=orcl;Persist Security Info=True;User ID=GBS_AD;Password=GBS_AD;";

        //    DataTable dev_all = CASTDatabaseHelper.GetViolations(3, 2, DevConnStr, "URL", "GBS_KB",null);

        //    var workbook = new XLWorkbook();
        //    var worksheet1 = workbook.Worksheets.Add("Quality Check");


            
        //    worksheet1.Cell("A1").Value = "QualityCheck Report";
        //    worksheet1.Cell("A1").Style.Font.Bold = true;
        //    worksheet1.Cell("A1").Style.Font.SetFontSize(20);


        //    worksheet1.Cell("A2").Value = "Application";
        //    worksheet1.Cell("A3").Value = "RequestID";
        //    worksheet1.Cell("A4").Value = "Date";
        //    worksheet1.Cell("A5").Value = "Baseline";
        //    worksheet1.Cell("A6").Value = "Status";

        //    worksheet1.Style.Fill.BackgroundColor = XLColor.White;
        //    worksheet1.Range("A2:A6").Style.Font.Bold = true;
        //    worksheet1.Range("A2:A6").Style.Font.FontColor = XLColor.DarkBlue;

        //    worksheet1.Range("A2:B6").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //    worksheet1.Range("A2:B6").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //    worksheet1.Range("A1:B6").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //    worksheet1.Range("A1:B6").Style.Fill.BackgroundColor = XLColor.LightYellow;
        //    worksheet1.Columns(1,2).AdjustToContents();


 
        //    var worksheet2 = workbook.Worksheets.Add("Added Violations");
        //    //DumpViolations(worksheet2, dev_all.Rows);
        //    IXLTable added = worksheet2.Cell(1, 1).InsertTable(dev_all,"added");
        //    worksheet2.Columns(1, added.ColumnCount()).AdjustToContents();

        //    worksheet1.Cell("A7").Value = dev_all.Compute("Sum(VIOLATIONS_COUNT)", "diag_id='4672'");

        //    workbook.SaveAs("QualityCheckReport.xlsx");
        }


        public static void DumpViolations(IXLWorksheet ws, DataRowCollection rows)
        {
            string address = null;
            int row = 1;
 
            foreach (DataColumn col in rows[0].Table.Columns)
            {
                address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());
                IXLCell cell = ws.Cell(address);
                cell.Value = col.ColumnName;
                //cell.Style.Fill.BackgroundColor = XLColor.BleuDeFrance;
                //cell.Style.Font.Bold = true;
                //cell.Style.Font.FontColor = XLColor.White;


            }

            foreach (DataRow dr in rows)
            {
                row++;
                foreach (DataColumn col in dr.Table.Columns)
                {
                    address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());
   
                    ws.Cell(address).Value = dr[col.ColumnName];
                }
            }

            ws.Columns(1, rows[0].Table.Columns.Count).AdjustToContents();

            var rngData = ws.Range("A1:" + address);
            var excelTable = rngData.CreateTable();
        }
    }
}
