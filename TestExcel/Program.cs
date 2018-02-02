using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace TestExcel
{
    public class ExcelHelper
    {

        public static void FillExcelReport()
        {
            Excel.Application excelApp = null;
            Excel.Worksheet xlSheet;
            Excel.Workbook xlBook;
            try
            {
                excelApp = new Excel.Application();

                string templatePath = @".\ExcelReportTemplate.xlsx";
                templatePath = Path.GetFullPath(templatePath);

                string outputPath = @".\ExcelReport.xlsx";
                outputPath = Path.GetFullPath(outputPath);

                xlBook = (Excel.Workbook)excelApp.Workbooks.Open(templatePath);

                xlSheet = (Excel.Worksheet)xlBook.Worksheets[1];

                xlSheet.Cells[6, 2] = "Pippo";

                xlBook.SaveAs(outputPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                System.Threading.Thread.Sleep(1000);
                excelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }


        }



    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            ExcelHelper.FillExcelReport();

        }
    }
}
