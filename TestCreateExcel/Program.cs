using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace TestCreateExcel
{
    class Program
    {
        static void Main(string[] args)
        {
            var workbook = new XLWorkbook();
            var worksheet1 = workbook.Worksheets.Add("Quality Check");
            workbook.SaveAs("TestExcel.xlsx");

        }
    }
}
