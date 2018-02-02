using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.DataAccess.Client;
using System.Data;

namespace CAST
{
    public static class QPMDatabaseHelper
    {
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

        public static bool TestConn(string connStr)
        {
            bool ret = false;
 

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                ret = true;
            }

            return ret;
        }


        public static decimal GetAppID(string connStr, string app)
        {
            decimal ret;
            string query = "SELECT COD_APPLICAZIONE FROM ANAG_APPLICAZIONI WHERE DEN_ACRONYM='" + app + "'";


            using (OracleConnection conn = new OracleConnection(connStr))
            {

                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                ret = (decimal)cmd.ExecuteScalar();
            }

            return ret;
        }

        public static void ReadGoldenRules(string connStr,GBSApplication app)
        {
            DataTable dt = new DataTable();
            List<string> golden = new List<string>();

            string query = "SELECT IDGOLDENMETRIC FROM ANAG_GOLDEN_QM WHERE IDAMBITO=1";


            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);
            }

            foreach (DataRow dr in dt.Rows)
                golden.Add(dr["IDGOLDENMETRIC"].ToString());

            app.Golden = golden.ToArray();

        }



        public static GBSApplication ReadApp(string connStr, string app, string amb, string lsv)
        {
            GBSApplication ret;
            //string query = "SELECT A.COD_APPLICAZIONE,A.DEN_ACRONYM,AT.TXT_CAST_URL_ADG,AT.TXT_CAST_GOLDEN_RULES,AT.TXT_CAST_EXCLUDED_RULES,AT.DEN_DB_LINK_CAST_ADG,AT.DEN_CAST_KB,AT.TXT_DEST_PATH,AT.DEN_CAST_CON_PROF_NAME,AT.DEN_CAST_DELIVERY_UNIT,AT.DEN_CAST_SYSTEM, AT.VERSIONE_CAST"
            //               + " FROM ANAG_APPLICAZIONI A,ANAG_APPLICAZIONI_TECNICA AT"
            //               + " WHERE A.COD_APPLICAZIONE=AT.COD_APPLICAZIONE"
            //               + " and  AT.DAT_FINE_VALIDITA>SYSDATE"
            //               + " and AT.COD_AMBIENTE=" + amb
            //               + " and AT.COD_LINEA_SVILUPPO=" + lsv
            //               + " and A.DEN_ACRONYM='" + app + "'";

            string query = "SELECT A.COD_APPLICAZIONE,A.DEN_ACRONYM,AT.TXT_CAST_URL_ADG,AT.DEN_DB_LINK_CAST_ADG,AT.DEN_CAST_KB,AT.TXT_DEST_PATH,AT.DEN_CAST_CON_PROF_NAME,AT.DEN_CAST_DELIVERY_UNIT,AT.DEN_CAST_SYSTEM, AT.VERSIONE_CAST"
                           + " FROM ANAG_APPLICAZIONI A,ANAG_APPLICAZIONI_TECNICA AT"
                           + " WHERE A.COD_APPLICAZIONE=AT.COD_APPLICAZIONE"
                           + " and  AT.DAT_FINE_VALIDITA>SYSDATE"
                           + " and AT.COD_AMBIENTE=" + amb
                           + " and AT.COD_LINEA_SVILUPPO=" + lsv
                           + " and A.DEN_ACRONYM='" + app + "'";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ret = new GBSApplication((decimal)reader["COD_APPLICAZIONE"]);
                        if(reader["DEN_CAST_CON_PROF_NAME"]!=DBNull.Value)
                            ret.CMSProfile = reader["DEN_CAST_CON_PROF_NAME"].ToString();
                        if (reader["DEN_CAST_DELIVERY_UNIT"] != DBNull.Value)
                            ret.CMSDeliveryUnit = reader["DEN_CAST_DELIVERY_UNIT"].ToString();
                        if (reader["DEN_CAST_SYSTEM"] != DBNull.Value)
                            ret.CMSSystem = reader["DEN_CAST_SYSTEM"].ToString();
                        if (reader["TXT_DEST_PATH"] != DBNull.Value)
                            ret.SrcPath = reader["TXT_DEST_PATH"].ToString();
                        if (reader["DEN_ACRONYM"] != DBNull.Value)
                            ret.Name = reader["DEN_ACRONYM"].ToString();
                        if (reader["DEN_DB_LINK_CAST_ADG"] != DBNull.Value)
                            ret.ADG = reader["DEN_DB_LINK_CAST_ADG"].ToString();
                        if (reader["DEN_CAST_KB"] != DBNull.Value)
                            ret.KB = reader["DEN_CAST_KB"].ToString();
                        if (reader["TXT_CAST_URL_ADG"] != DBNull.Value)
                            ret.URL = reader["TXT_CAST_URL_ADG"].ToString();
                        //if (reader["TXT_CAST_GOLDEN_RULES"] != DBNull.Value)
                        //{
                        //    string goldenString = reader["TXT_CAST_GOLDEN_RULES"].ToString();
                        //    ret.Golden = goldenString.Split(',');
                        //}
                        //if (reader["TXT_CAST_EXCLUDED_RULES"] != DBNull.Value)
                        //{
                        //    string excludedString = reader["TXT_CAST_EXCLUDED_RULES"].ToString();
                        //    ret.Exclude = excludedString.Split(',');
                        //}
                        if (reader["VERSIONE_CAST"] != DBNull.Value)
                            ret.AIP_VER = reader["VERSIONE_CAST"].ToString();

                    }

                    else
                        ret = null;
                }           
            }

            return ret;
        }

        public static string GetBaselineSrcPath(string snapshot_name, string connStr)
        {
            string path;
            string query;

            query = "SELECT TXT_SRC_PATH FROM ANAG_BASELINE WHERE DEN_SNAPSHOT=:snapshot";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                cmd.Parameters.Add("snapshot", OracleDbType.Varchar2).Value = snapshot_name;

                conn.Open();
                path = cmd.ExecuteScalar().ToString();

            }

            return path;
        }

        public static string GetContractBaseline(int idr, string connStr)
        {
            string ret;
            string query;

            query = "select den_versione_applicazione from anag_baseline where cod_baseline = get_contract_baseline(" + idr+")";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();
                ret = cmd.ExecuteScalar().ToString();

            }

            return ret;
        }

        public static void GetRelease(int idr, string connStr, out string den_release, out string COD_RELEASE_RLM )
        {
            DataTable dt = new DataTable();
            string query = "select REL.DEN_RELEASE,REL.COD_RELEASE_RLM from anag_release rel, richieste r where R.COD_RELEASE_QPM = REL.COD_RELEASE_QPM and R.COD_RICHIESTA =" + idr;

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);
            }

            den_release = dt.Rows[0]["DEN_RELEASE"].ToString();
            COD_RELEASE_RLM = dt.Rows[0]["COD_RELEASE_RLM"].ToString();

        }


    }
}