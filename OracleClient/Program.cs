using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=qpfdbc01.generali.it)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SID=QPCC1)));User Id=qpm_qpm;Password=qpm;";

            bool ret = CheckApp(connStr, "FOS");

            Console.WriteLine("Esito:" + ret);

        }


        public static bool CheckApp(string connStr, string app)
        {
            bool ret;
            string query = "SELECT * FROM ANAG_APPLICAZIONI WHERE DEN_ACRONYM='" + app + "'";


            using (OracleConnection conn = new OracleConnection(connStr))
            {

                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        ret = true;
                    else
                        ret = false;
                }
            }

            return ret;
        }
    }
}
