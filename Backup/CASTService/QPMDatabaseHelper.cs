using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.DataAccess.Client;

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

        public static GBSApplication ReadApp(string connStr, string app, string amb, string lsv)
        {
            GBSApplication ret;
            // query = "SELECT * FROM ANAG_APPLICAZIONI_TECNICA,ANAG_APPLICAZIONI WHERE ANAG_APPLICAZIONI.COD_APPLICAZIONE=ANAG_APPLICAZIONI_TECNICA.COD_APPLICAZIONE AND ANAG_APPLICAZIONI.DEN_ACRONYM='" + app + "'";

            string query = "SELECT A.COD_APPLICAZIONE,A.DEN_ACRONYM,AT.TXT_CAST_URL_ADG,AT.TXT_CAST_GOLDEN_RULES,AT.TXT_CAST_EXCLUDED_RULES,AT.DEN_DB_LINK_CAST_ADG,AT.DEN_CAST_KB,AT.TXT_DEST_PATH,AT.DEN_CAST_CON_PROF_NAME,AT.DEN_CAST_DELIVERY_UNIT,AT.DEN_CAST_SYSTEM" 
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
                        if (reader["TXT_CAST_GOLDEN_RULES"] != DBNull.Value)
                        {
                            string goldenString = reader["TXT_CAST_GOLDEN_RULES"].ToString();
                            ret.Golden = goldenString.Split(',');
                        }
                        if (reader["TXT_CAST_EXCLUDED_RULES"] != DBNull.Value)
                        {
                            string excludedString = reader["TXT_CAST_EXCLUDED_RULES"].ToString();
                            ret.Exclude = excludedString.Split(',');
                        }

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

        
    }
}