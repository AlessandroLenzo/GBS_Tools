using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAST;

namespace TestGetSnapshotID
{
    class Program
    {
        static void Main(string[] args)
        {
            //string ProdConnStr = "Data Source=QPCC1.generali.it;Persist Security Info=True;User ID=GBS_AD;Password=GBS_AD;";
            string DevConnStr = "Data Source=orcl;Persist Security Info=True;User ID=GBS_AD;Password=GBS_AD;";


            string APP = "QualityCheckApp";
            string VBS = "AUTO_WEB 12/04/2013 12:04:25";
            Console.WriteLine("DevConnStr: " + DevConnStr);
            Console.WriteLine("APP: " + APP);
            Console.WriteLine("VBS: " + VBS);

            int prod_app_id = CASTDatabaseHelper.GetApplicationID(APP, VBS, DevConnStr);


            string baseline_version = CASTDatabaseHelper.GetApplicationVersion(9,3, DevConnStr);


            List<string> dev_objects = CASTDatabaseHelper.GetObjectNames(DevConnStr, "GBS_KB");

            int files_num = CASTDatabaseHelper.GetMeasure(3, 11, 10154, DevConnStr);

            int obj_num = CASTDatabaseHelper.GetMeasure(3, 11, 10152, DevConnStr);

            Console.ReadLine();

 
        }
    }
}
