using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Oracle.DataAccess.Client;
using System.Data;

namespace CAST
{
    public static class CASTDatabaseHelper
    {
        public static void DumpViolations(string output_file, DataRowCollection rows)
        {


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(output_file))
            {
                if (rows.Count == 0)
                {
                    file.WriteLine("No data");
                    return;
                }

                foreach (DataColumn col in rows[0].Table.Columns)
                {
                    file.Write(col.ColumnName + ";");
                }
                file.WriteLine();

                foreach (DataRow dr in rows)
                {
                    foreach (DataColumn col in dr.Table.Columns)
                    {
                        file.Write(dr[col.ColumnName] + ";");
                    }
                    file.WriteLine();
                }
            }
        }

        public static void DumpViolations(string output_file, DataTable table)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(output_file))
            {
                table.TableName = "AddedViolations";
                table.WriteXml(file);
            }

 
        }

        public static void DumpViolations(System.IO.StreamWriter stream, DataTable table, string tablename)
        {
                table.TableName = tablename;
                table.WriteXml(stream);
        }

        //public static void DumpViolations(HtmlTextWriter writer, DataRowCollection rows)
        //{
        //    writer.WriteFullBeginTag("table");
        //    writer.WriteLine();

        //    writer.Indent++;

        //    writer.WriteFullBeginTag("tr");
        //    writer.WriteLine();
 

        //    foreach (DataColumn col in rows[0].Table.Columns)
        //    {
        //        writer.WriteFullBeginTag("td");
        //        writer.Write(col.ColumnName);
        //        writer.WriteEndTag("td");
        //        writer.WriteLine();
 
        //    }

        //    writer.WriteEndTag("tr");
        //    writer.WriteLine();

        //    foreach (DataRow dr in rows)
        //    {
        //        writer.WriteFullBeginTag("tr");
        //        writer.WriteLine();
 

        //        foreach (DataColumn col in dr.Table.Columns)
        //        {
        //            writer.WriteFullBeginTag("td");
        //            writer.Write(dr[col.ColumnName]);
        //            writer.WriteEndTag("td");
        //            writer.WriteLine();
        //        }
        //        writer.WriteEndTag("tr");
        //        writer.WriteLine();
 
        //    }
        //    writer.Indent--;
 
        //    writer.WriteEndTag("table");
        //    writer.WriteLine();
        //}

        public static int GetSnapshotID(string application_name, string snapshot_name, string connStr)
        {
            int snapshot_id;
            string query;

            query = "SELECT max(snapshot_id) FROM CSV_PORTF_TREE WHERE app_name=:app AND snapshot_name=:snapshot";


            using (OracleConnection conn = new OracleConnection(connStr))
            {


                OracleCommand cmd = new OracleCommand(query, conn);


                cmd.Parameters.Add("app", OracleDbType.Varchar2).Value = application_name;
                cmd.Parameters.Add("snapshot", OracleDbType.Varchar2).Value = snapshot_name;


                conn.Open();
                snapshot_id = Convert.ToInt32(cmd.ExecuteScalar());

            }


            return snapshot_id;
        }

        public static string GetSnapshotVersion(int snapshot_id, string connStr)
        {
            string version;
            string query;
            query = "select distinct object_version from DSS_SNAPSHOT_INFO where snapshot_id=:snapshot";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add("snapshot", OracleDbType.Int32).Value = snapshot_id;

                conn.Open();
                version = Convert.ToString(cmd.ExecuteScalar());
            }
            return version;
        }

        public static int GetApplicationID(string application_name, string snapshot_name, string connStr)
        {
            int app_id;
            string query;

            query = "SELECT app_id FROM CSV_PORTF_TREE WHERE app_name=:app AND snapshot_name=:snapshot";


            using (OracleConnection conn = new OracleConnection(connStr))
            {


                OracleCommand cmd = new OracleCommand(query, conn);


                cmd.Parameters.Add("app", OracleDbType.Varchar2).Value = application_name;
                cmd.Parameters.Add("snapshot", OracleDbType.Varchar2).Value = snapshot_name;


                conn.Open();
                app_id = Convert.ToInt32(cmd.ExecuteScalar());

            }


            return app_id;
        }

        
        //old version
        public static DataTable GetViolations(int application_id, int snapshot_id, string connStr, string url,string kb, List<string> dev_objects,string[] golden, out decimal v_count,out decimal v_golden_count)
        {
            DataTable dt = new DataTable();
            
             
            string query = Properties.Resources.Query1;
            string query2 = Properties.Resources.Query2.Replace(":KB", kb);

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                cmd.Parameters.Add("app", OracleDbType.Int32).Value = application_id;
                cmd.Parameters.Add("snapshot", OracleDbType.Int32).Value = snapshot_id;

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);

                v_count = (decimal)dt.Compute("Sum(VIOLATIONS_COUNT)", "");
                v_golden_count = GetGoldenCount(dt, golden);

                dt.Columns.Add("dashboard_link", typeof(string));
                dt.Columns.Add("file_path", typeof(string));
                dt.Columns["file_path"].SetOrdinal(3);

                OracleCommand cmd2 = new OracleCommand(query2, conn);
               
                cmd2.Parameters.Add("ID", OracleDbType.Int32);

                List<DataRow> rowsToRemove = new List<DataRow>();
                foreach (DataRow dr in dt.Rows)
                    if (!dev_objects.Contains(dr["object_name"]))
                       rowsToRemove.Add(dr);
                foreach (DataRow dr in rowsToRemove)
                    dt.Rows.Remove(dr);

                foreach (DataRow dr in dt.Rows)
                {
                    dr["dashboard_link"] = url + "?frame=FRAME_PORTAL_INVESTIGATION_VIEW&snapshot=" + snapshot_id + "&metric=" + dr["diag_id"].ToString() + "&treeobject=" + dr["object_id"].ToString();

                    cmd2.Parameters["ID"].Value = dr["object_id"].ToString();

                    dr["file_path"] = cmd2.ExecuteScalar();
                }


            }

 
            
            return dt;
        }

        public static DataTable GetViolations(int application_id, int snapshot_id, string connStr, string url, string aed_url, string cb, string kb, string qcl)
        {
            DataTable dt = new DataTable();

            string query = Properties.Resources.Query4;
            query = query.Replace(":KB", kb);
            query = query.Replace(":CB", cb);
            query = query.Replace(":QCL", qcl);
            query = query.Replace(":url", url);
            query = query.Replace(":aed_url", aed_url);

            query = query.Replace(":snapshot", snapshot_id.ToString());
            query = query.Replace(":app", application_id.ToString());

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);
            }
            return dt;
        }



        public static List<string> GetObjectNames(string connStr,string kb)
        {
            List<string> objects = new  List<string>();

            string query = Properties.Resources.Query3.Replace(":KB", kb);

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["object_fullname"] != DBNull.Value)
                            objects.Add(reader["object_fullname"].ToString());
                    }
                }
            }

            return objects;
        }

        public static decimal GetGoldenCount(DataTable prod_all, string[] golden)
        {
            decimal count = 0;
            foreach (DataRow dr in prod_all.Rows)
            {
                if (golden.Count(pe => pe == dr["diag_id"].ToString()) > 0) count += Convert.ToDecimal(dr["VIOLATIONS_COUNT"]);

            }

            return count;
        }


        public static decimal GetViolationCount(int application_id, int snapshot_id, string connStr,string[] golden, out decimal v_golden_count)
        {
            decimal v_count = 0;
            DataTable dt = new DataTable();

            string query = Properties.Resources.Query5;

            List<int> g_vector = new List<int>();
            if(golden!=null)
                foreach (string rule in golden)
                    g_vector.Add(Convert.ToInt32(rule));

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                cmd.Parameters.Add("app", OracleDbType.Int32).Value = application_id;
                cmd.Parameters.Add("snapshot", OracleDbType.Int32).Value = snapshot_id;

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);

                v_golden_count = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (g_vector.Count(pe => pe == Convert.ToInt32(dr["diag_id"])) > 0) v_golden_count++;
                    v_count++;
                }
            }
            return v_count;
        }


        public static DataTable GetRemovedViolations(DataTable prod_all, DataTable dev_all, string[] golden, string[] exclude, string url)
        {
            string expr;
            DataTable removed_violations = prod_all.Clone();
            removed_violations.Columns.Add("prod_count", typeof(decimal));
            removed_violations.Columns.Add("dev_count", typeof(decimal));
            removed_violations.Columns.Add("is_golden", typeof(string));
            //removed_violations.Columns.Add("is_excluded", typeof(string));
            removed_violations.Columns["dashboard_link"].SetOrdinal(removed_violations.Columns.Count - 1);
            removed_violations.Columns["aed_link"].SetOrdinal(removed_violations.Columns.Count - 1);
            removed_violations.Columns["t_criterion_name"].SetOrdinal(removed_violations.Columns.Count - 1);
            removed_violations.Columns["b_criterion_name"].SetOrdinal(removed_violations.Columns.Count - 1);
            removed_violations.Columns["language"].SetOrdinal(removed_violations.Columns.Count - 1);

            DataRow newdr;

            foreach (DataRow dr in prod_all.Rows)
            {
                expr = string.Format("diag_id={0} AND object_name='{1}'", dr["diag_id"], dr["object_name"]);

                DataRow[] devSel = dev_all.Select(expr);

                decimal prodCont = (decimal)dr["violations_count"];

                if (devSel.Length == 0)
                {

                    newdr = removed_violations.NewRow();
                    newdr["diag_id"] = dr["diag_id"].ToString();
                    newdr["object_id"] = dr["object_id"].ToString();
                    newdr["dashboard_link"] = dr["dashboard_link"].ToString();
                    newdr["aed_link"] = dr["aed_link"].ToString();
                    newdr["t_criterion_name"] = dr["t_criterion_name"].ToString();
                    newdr["b_criterion_name"] = dr["b_criterion_name"].ToString();
                    newdr["language"] = dr["language"].ToString();
                    newdr["diag_name"] = dr["diag_name"].ToString();
                    newdr["object_name"] = dr["object_name"].ToString();
                    newdr["file_path"] = dr["file_path"].ToString();
                    newdr["violations_count"] = prodCont;
                    newdr["prod_count"] = prodCont;
                    newdr["dev_count"] = 0;

                    if (golden.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                        newdr["is_golden"] = "yes";
                    else
                        newdr["is_golden"] = "no";

                    //if (exclude.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                    //    newdr["is_excluded"] = "yes";
                    //else
                    //    newdr["is_excluded"] = "no";

                    removed_violations.Rows.Add(newdr);
                }
                else
                {
                    decimal devCont = (decimal)devSel[0]["violations_count"];
                    if (prodCont > devCont)
                    {
                        newdr = removed_violations.NewRow();
                        newdr["diag_id"] = dr["diag_id"].ToString();
                        newdr["object_id"] = dr["object_id"].ToString();
                        newdr["dashboard_link"] = dr["dashboard_link"].ToString();
                        newdr["aed_link"] = dr["aed_link"].ToString();
                        newdr["t_criterion_name"] = dr["t_criterion_name"].ToString();
                        newdr["b_criterion_name"] = dr["b_criterion_name"].ToString();
                        newdr["language"] = dr["language"].ToString();
                        newdr["diag_name"] = dr["diag_name"].ToString();
                        newdr["object_name"] = dr["object_name"].ToString();
                        newdr["file_path"] = dr["file_path"].ToString();
                        newdr["violations_count"] = prodCont - devCont;
                        newdr["prod_count"] = prodCont;
                        newdr["dev_count"] = devCont;

                        if (golden.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                            newdr["is_golden"] = "yes";
                        else
                            newdr["is_golden"] = "no";

                        //if (exclude.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                        //    newdr["is_excluded"] = "yes";
                        //else
                        //    newdr["is_excluded"] = "no";

                        removed_violations.Rows.Add(newdr);
                    }

                }

            }

            removed_violations.Columns["violations_count"].ColumnName = "removed_count";



            return removed_violations;

        }

        public static DataTable GetNewViolations(DataTable prod_all, DataTable dev_all, string[] golden, string[] exclude, string url)
        {
            string expr;
            DataTable new_violations = dev_all.Clone();
            new_violations.Columns.Add("prod_count", typeof(decimal));
            new_violations.Columns.Add("dev_count", typeof(decimal));
            new_violations.Columns.Add("is_golden", typeof(string));
            //new_violations.Columns.Add("is_excluded", typeof(string));
            new_violations.Columns["dashboard_link"].SetOrdinal(new_violations.Columns.Count - 1);
            new_violations.Columns["aed_link"].SetOrdinal(new_violations.Columns.Count - 1);
            new_violations.Columns["t_criterion_name"].SetOrdinal(new_violations.Columns.Count - 1);
            new_violations.Columns["b_criterion_name"].SetOrdinal(new_violations.Columns.Count - 1);
            new_violations.Columns["language"].SetOrdinal(new_violations.Columns.Count - 1);          

            DataRow newdr;

            foreach (DataRow dr in dev_all.Rows)
            {
                expr = string.Format("diag_id={0} AND object_name='{1}'", dr["diag_id"], dr["object_name"]);

                DataRow[] prodSel = prod_all.Select(expr);

                decimal devCont = (decimal)dr["violations_count"];

                if (prodSel.Length == 0)
                {
                    newdr = new_violations.NewRow();
                    newdr["diag_id"] = dr["diag_id"].ToString();
                    newdr["object_id"] = dr["object_id"].ToString();
                    newdr["dashboard_link"] = dr["dashboard_link"].ToString();
                    newdr["aed_link"] = dr["aed_link"].ToString();
                    newdr["t_criterion_name"] = dr["t_criterion_name"].ToString();
                    newdr["b_criterion_name"] = dr["b_criterion_name"].ToString();
                    newdr["language"] = dr["language"].ToString();
                    newdr["diag_name"] = dr["diag_name"].ToString();
                    newdr["object_name"] = dr["object_name"].ToString();
                    newdr["file_path"] = dr["file_path"].ToString();
                    newdr["violations_count"] = devCont;
                    newdr["prod_count"] = 0;
                    newdr["dev_count"] = devCont;

                    if (golden.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                        newdr["is_golden"] = "yes";
                    else
                        newdr["is_golden"] = "no";


                    //if (exclude.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                    //    newdr["is_excluded"] = "yes";
                    //else
                    //    newdr["is_excluded"] = "no";

                    new_violations.Rows.Add(newdr);
                }
                else
                {
                    decimal prodCont = (decimal)prodSel[0]["violations_count"];
                    if (devCont > prodCont)
                    {
                        newdr = new_violations.NewRow();
                        newdr["diag_id"] = dr["diag_id"].ToString();
                        newdr["object_id"] = dr["object_id"].ToString();
                        newdr["dashboard_link"] = dr["dashboard_link"].ToString();
                        newdr["aed_link"] = dr["aed_link"].ToString();
                        newdr["t_criterion_name"] = dr["t_criterion_name"].ToString();
                        newdr["b_criterion_name"] = dr["b_criterion_name"].ToString();
                        newdr["language"] = dr["language"].ToString();
                        newdr["diag_name"] = dr["diag_name"].ToString();
                        newdr["object_name"] = dr["object_name"].ToString();
                        newdr["file_path"] = dr["file_path"].ToString();
                        newdr["violations_count"] = devCont - prodCont;
                        newdr["prod_count"] = prodCont;
                        newdr["dev_count"] = devCont;

                        if (golden.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                            newdr["is_golden"] = "yes";
                        else
                            newdr["is_golden"] = "no";

                        //if (exclude.Count(pe => pe == newdr["diag_id"].ToString()) > 0)
                        //    newdr["is_excluded"] = "yes";
                        //else
                        //    newdr["is_excluded"] = "no";

                        new_violations.Rows.Add(newdr);
                    }

                }

            }

            new_violations.Columns["violations_count"].ColumnName = "new_count";

            return new_violations;

        }

        public static DataTable GetSnapshotDates(string app, string connStr)
        {
            DataTable dt = new DataTable();

            string query = "select distinct SNAPSHOT_DATE from CSV_PORTF_TREE where APP_NAME=:app";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(query, conn);

                cmd.Parameters.Add("app", OracleDbType.Varchar2).Value = app;

                conn.Open();

                OracleDataAdapter od = new OracleDataAdapter(cmd);

                od.Fill(dt);
            }

            return dt;
        }

        public static string GetApplicationVersion(int snapshotID,int applicationID, string connStr)
        {
            string appVersion;
            string query;

            query = "SELECT DISTINCT OBJECT_VERSION FROM DSS_SNAPSHOT_INFO WHERE SNAPSHOT_ID=:snapshot AND OBJECT_ID=:application";


            using (OracleConnection conn = new OracleConnection(connStr))
            {


                OracleCommand cmd = new OracleCommand(query, conn);


                cmd.Parameters.Add("snapshot", OracleDbType.Int32).Value = snapshotID;
                cmd.Parameters.Add("application", OracleDbType.Int32).Value = applicationID;


                conn.Open();
                appVersion = Convert.ToString(cmd.ExecuteScalar());

            }


            return appVersion;
        }

        
        
        public static int GetMeasure(int application_id, int snapshot_id, int measure, string connStr)
        {
            int val;
            string query;

            query = "SELECT MEAS_VALUE FROM CSV_QUANTITY_VAL WHERE SNAPSHOT_ID=:snapshot AND CONTEXT_ID=:app AND MEASURE_ID=:measure";


            using (OracleConnection conn = new OracleConnection(connStr))
            {


                OracleCommand cmd = new OracleCommand(query, conn);

                cmd.Parameters.Add("snapshot", OracleDbType.Int32).Value = snapshot_id;
                cmd.Parameters.Add("app", OracleDbType.Int32).Value = application_id;
                cmd.Parameters.Add("measure", OracleDbType.Int32).Value = measure;


                conn.Open();
                val = Convert.ToInt32(cmd.ExecuteScalar());

            }


            return val;
        }
 
    }
}