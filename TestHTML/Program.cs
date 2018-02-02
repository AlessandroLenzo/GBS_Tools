using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CAST;
using System.Web;
using System.Web.UI;
using System.IO;

namespace TestHTML
{
    class Program
    {
        static void Main(string[] args)
        {
            string DevConnStr = "Data Source=orcl;Persist Security Info=True;User ID=GBS_AD;Password=GBS_AD;";
            string dev_all_path = @"D:\Customers\Generali\CASTService\TestHTML\violations.csv";
            string dev_all_path2 = @"D:\Customers\Generali\CASTService\TestHTML\violations.xml";
            string dev_all_path3 = @"D:\Customers\Generali\CASTService\TestHTML\violations.html";
 

             DataTable dev_all = CASTDatabaseHelper.GetViolations(3, 1, DevConnStr, "URL", "GBS_KB",null);

            //CASTDatabaseHelper.DumpViolations(dev_all_path, dev_all.Rows);

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(dev_all_path2))
            //{
 
            //    CASTDatabaseHelper.DumpViolations(file, dev_all, "Added");

            //    CASTDatabaseHelper.DumpViolations(file, dev_all, "Deleted");

            //}

            using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(dev_all_path3))
            {
                using (HtmlTextWriter writer = new HtmlTextWriter(file2))
                {
                    writer.WriteFullBeginTag("html");
                    writer.WriteLine();
                    writer.Indent++;
                    writer.WriteFullBeginTag("body");
                    writer.WriteLine();
                    writer.Indent++;
  
                    CASTDatabaseHelper.DumpViolations(writer, dev_all.Rows);

                    writer.WriteLine();
                    writer.Indent--;
                    writer.WriteEndTag("body");
                    writer.WriteLine();
                    writer.Indent--;
                    writer.WriteEndTag("html");
                    writer.WriteLine();
 

                }
            }


        }
    }
}
