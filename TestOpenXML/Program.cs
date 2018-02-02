using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using CAST;

namespace TestOpenXML
{
    class Program
    {
        static void Main(string[] args)
        {
            string templatePath = @".\ExcelReportTemplate.xlsx";
            templatePath = Path.GetFullPath(templatePath);

            string outputPath = @".\ExcelReport.xlsx";
            outputPath = Path.GetFullPath(outputPath);

            File.Copy(templatePath, outputPath, true);

            string DevConnStr = "Data Source=orcl;Persist Security Info=True;User ID=GBS_AD;Password=GBS_AD;";
 
 

            DataTable dev_all = CASTDatabaseHelper.GetViolations(3, 2, DevConnStr, "URL", "GBS_KB",null);

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(outputPath, true))
            {
                WorkbookPart wbPart = document.WorkbookPart;

                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "Quality Check Result").FirstOrDefault();

                if (theSheet != null)
                {
                    Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(theSheet.Id))).Worksheet;

                    Cell theCell = InsertCellInWorksheet(ws, "B6");

                    theCell.CellValue = new CellValue("Passed");
                    theCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    //ws.Save();
                }

                theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "Added Violations").FirstOrDefault();

                if (theSheet != null)
                {
                    Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(theSheet.Id))).Worksheet;

                    DumpViolations(ws, dev_all.Rows);

                    //ws.Save();
                }

                wbPart.Workbook.Save();
            }

            

        }

        public static void DumpViolations(Worksheet ws, DataRowCollection rows)
        {
            string address = null;
            Cell cell = null;

            int row = 1;
            double result;
            string val;

            foreach (DataColumn col in rows[0].Table.Columns)
            {
                address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());
                cell = InsertCellInWorksheet(ws, address);
                
                cell.CellValue = new CellValue(col.ColumnName);

                cell.DataType = new EnumValue<CellValues>(CellValues.String);
                
            }
            
            foreach (DataRow dr in rows)
            {
                row++;
                foreach (DataColumn col in dr.Table.Columns)
                {
                    address = String.Concat(Char.ConvertFromUtf32(65 + col.Ordinal), row.ToString());
                    cell = InsertCellInWorksheet(ws, address);
                    val = dr[col.ColumnName].ToString();

                    cell.CellValue = new CellValue(val);

                    if(Double.TryParse(val,out result))
                        cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    else
                        cell.DataType = new EnumValue<CellValues>(CellValues.String);
                }
            }
        }



        // Given a Worksheet and an address (like "AZ254"), either return a 
        // cell reference, or create the cell reference and return it.
        private static Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private static Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        // Return the row at the specified rowIndex located within
        // the sheet data passed in via wsData. If the row does not
        // exist, create it.
        private static Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
            Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }


    }
}
