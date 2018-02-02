using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Data;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace CAST
{
    public class RequestHandler
    {
        private System.Timers.Timer timer;

        private qpmAttivita qpm;
        private string QPMDbConnStr;
        private GBSApplication app, prodApp;
        private CASTRequestType req;
        private string output_path, aed_url;
        private string src_path;
        private string log_file_name, snapshot_log_file_name,delivery_log_file_name;
        private string castExePath7, castExePath8, castLogPath, castSrcPath, castBaselineSrcPath, castDBServer;
        TextWriterTraceListener listener;
        private long previousEndPosition;
        private Process process;
        private string attCode;
        private bool saveDebugFiles;
        private bool skipSnapshot;

        List<qpm_fileResult> filesList;

        public RequestHandler(CASTRequestType request)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
 
            req = request;

            qpm = new qpmAttivita();
            qpm.Timeout = 60 * 5 * 1000;
            qpm.Url = ConfigurationManager.AppSettings["QPM_URL"];
            QPMDbConnStr = ConfigurationManager.AppSettings["QPM_ConnStr"];
            output_path = ConfigurationManager.AppSettings["CAST_OutputPath"] + "\\" + req.APP + "\\" + req.IDR;
            castExePath7 = ConfigurationManager.AppSettings["CASTAIP_7.X_Path"];
            castExePath8 = ConfigurationManager.AppSettings["CASTAIP_8.X_Path"];
            castLogPath = ConfigurationManager.AppSettings["CASTAIP_Log"];
            castSrcPath = ConfigurationManager.AppSettings["CASTAIP_Src"] + "\\" + req.APP;
            castBaselineSrcPath = ConfigurationManager.AppSettings["CASTAIP_BaselineSrc"] + "\\" + req.APP;
            castDBServer = ConfigurationManager.AppSettings["CASTAIP_DataServer"];
            saveDebugFiles = ConfigurationManager.AppSettings["CAST_QC_SaveDebugFiles"] == "yes" ? true : false;
            skipSnapshot = ConfigurationManager.AppSettings["CAST_QC_SkipSnapshot"] == "yes" ? true : false;
            aed_url = ConfigurationManager.AppSettings["CAST_AED_URL"];


            filesList = new List<qpm_fileResult>();

            snapshot_log_file_name = castLogPath + "\\snapshot_" + req.APP + "_" + req.IDR + ".txt";
            delivery_log_file_name = castLogPath + "\\delivery_" + req.APP + "_" + req.IDR + ".txt";
            log_file_name = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\app_" + req.APP + "_req_" + req.IDR + "_" + DateTime.Now.ToString("dd-MMM-yyyy_HH-mm-ss-ffff") + "_tracing.log";
            
            listener = new CASTTextWriterTraceListener(log_file_name);
            Trace.Listeners.Add(listener);
            

            
            listener.IndentLevel = 0;
            listener.WriteLine(DateTime.Now.ToString() + " RequestHandler started...");
            listener.WriteLine("Request Parameters:");
            listener.IndentLevel = 1;
            listener.WriteLine("AMB=" + req.AMB);
            listener.WriteLine("APP=" + req.APP);
            listener.WriteLine("CDT=" + req.CDT);
            listener.WriteLine("CHS=" + req.CHS);
            listener.WriteLine("IDA=" + req.IDA);
            listener.WriteLine("IDR=" + req.IDR);
            listener.WriteLine("LSV=" + req.LSV);
            listener.WriteLine("MAC=" + req.MAC);
            listener.WriteLine("MAT=" + req.MAT);
            listener.WriteLine("SNN=" + req.SNN);
            listener.WriteLine("TAT=" + req.TAT);
            listener.WriteLine("VBS=" + req.VBS);
            listener.WriteLine("VER=" + req.VER);
            listener.WriteLine("VPR=" + req.VPR);
            listener.IndentLevel = 0;
            listener.WriteLine("Service Settings:");
            listener.IndentLevel = 1;
            listener.WriteLine("QPM_URL=" + qpm.Url);
            listener.WriteLine("QPM_ConnStr=" + QPMDbConnStr);
            listener.WriteLine("CAST_OutputPath=" + output_path);
            listener.WriteLine("CAST_QC_SaveDebugFiles=" + ConfigurationManager.AppSettings["CAST_QC_SaveDebugFiles"]);
            listener.WriteLine("CAST_QC_SkipSnapshot=" + ConfigurationManager.AppSettings["CAST_QC_SkipSnapshot"]);
            listener.WriteLine("CASTAIP_7.X_Path=" + castExePath7);
            listener.WriteLine("CASTAIP_8.X_Path=" + castExePath8);
            listener.WriteLine("CASTAIP_Log=" + castLogPath);
            listener.WriteLine("CASTAIP_Src=" + castSrcPath);
            listener.WriteLine("CASTAIP_BaselineSrc=" + castBaselineSrcPath);
            listener.WriteLine("CASTAIP_DataServer=" + castDBServer);
            
            listener.IndentLevel = 0;
  
 
        }

        public static void MapDisk(string diskMap, string root, string user, string pw, TextWriterTraceListener extlistener)
        {
            extlistener.WriteLine(DateTime.Now.ToString() + " Mapping disk " + diskMap + "...");
            
            
            string executableName = "net.exe";
            string executableParameter = "use /y " + diskMap + ": " + root + " /user:" + user + " " + pw;

            extlistener.WriteLine("Calling: " + executableName + " Params: " + executableParameter);
            
  

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;


            Process process = Process.Start(processStartInfo);
           
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                extlistener.WriteLine(result);
                
            }
            using (StreamReader reader = process.StandardError)
            {
                string result = reader.ReadToEnd();
                extlistener.WriteLine(result);
                
            }

            process.WaitForExit(4000);

            extlistener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);
            

        }

        public bool ValidateRequest(CASTResponseType res)
        {
            listener.WriteLine(DateTime.Now.ToString() + " ValidateRequest started...");
            

            bool ret = true;
            int num;

            try
            {
                if (req.IDA == null || req.IDA == string.Empty)
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "request field IDA is null or empty";
                    ret = false;
                }
                else if (req.IDR == null || req.IDR == string.Empty)
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "request field IDR is null or empty";
                    ret = false;
                }
                else if (req.TAT == null || req.TAT == string.Empty || !(req.TAT == "QCH" || req.TAT == "QCH_SNA_REP" || req.TAT == "COF_SNA_REP" || req.TAT == "COF" || req.TAT == "COF_DMT" || req.TAT == "COF_SNA"))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field TAT (valid values: QCH, QCH_SNA_REP, COF, COF_DMT, COF_SNA, COF_SNA_REP)";
                    ret = false;
                }
                else if (req.AMB == null || req.AMB == string.Empty || !(int.TryParse(req.AMB, out num)))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field AMB (valid values: integer)";
                    ret = false;

                }
                else if (req.LSV == null || req.LSV == string.Empty || !(int.TryParse(req.LSV, out num)))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field LSV (valid values: integer)";
                    ret = false;

                }
                else if (req.CDT == null || req.CDT == string.Empty || !ValidateDate(req.CDT))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field CDT";
                    ret = false;

                }

                else if (req.MAC == null || req.MAC == string.Empty || !(req.MAC == "I" || req.MAC == "F"))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field MAC";
                    ret = false;

                }

                else if (req.MAT == null || req.MAT == string.Empty || !(req.MAT == "B" || req.MAT == "N"))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field MAT";
                    ret = false;

                }

                else if ( (req.TAT == "QCH" || req.TAT == "QCH_SNA_REP" || req.TAT == "COF_SNA_REP") && (req.VPR == null || req.VPR == string.Empty || !(req.VPR == "V" || req.VPR == "N")))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field VPR";
                    ret = false;

                }
                else if ( (req.TAT == "QCH_SNA_REP" || req.TAT == "COF_SNA_REP") && req.MAT != "B")
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field MAT";
                    ret = false;

                }
                else if (req.SNN == null || req.SNN == string.Empty)
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field SNN";
                    ret = false;
                }
                else if (req.VBS == null || req.VBS == string.Empty)   //controllare anche esistenza snapshot baseline
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field VBS";
                    ret = false;

                }
                else if (req.VER == null || req.VER == string.Empty)
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field VER";
                    ret = false;

                }
                else if (req.APP == null || req.APP == string.Empty || !QPMDatabaseHelper.CheckApp(QPMDbConnStr, req.APP))
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "invalid request field APP";
                    ret = false;

                }
                else if ((app = QPMDatabaseHelper.ReadApp(QPMDbConnStr, req.APP, req.AMB, req.LSV)) == null)
                {
                    res.CODICE = "KO";
                    res.MESSAGGIO = "record missing in QPM database for application " + req.APP;
                    ret = false;
                }
                else
                {

                    listener.WriteLine(DateTime.Now.ToString() + " Application found");
                    listener.WriteLine("Application data:");
                    listener.IndentLevel = 1;
                    listener.WriteLine("CMSProfile=" + app.CMSProfile);
                    listener.WriteLine("SrcPath=" + app.SrcPath);
                    listener.WriteLine("AIP_VER=" + app.AIP_VER);

                    listener.IndentLevel = 0;

                    if (!(app.AIP_VER=="7.X" || app.AIP_VER == "8.X"))
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Bad AIP_VER field in QPM DB for application " + req.APP + " (expected \"7.X\" or \"8.X\")";
                        ret = false;
                    }
                    else if (app.CMSProfile == null || app.CMSProfile == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing DEN_CAST_CON_PROF_NAME field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (app.CMSDeliveryUnit == null || app.CMSDeliveryUnit == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing DEN_CAST_DELIVERY_UNIT field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (app.CMSSystem == null || app.CMSSystem == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing DEN_CAST_SYSTEM field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (app.SrcPath == null || app.SrcPath == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing TXT_DEST_PATH field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (app.ADG == null || app.ADG == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing DEN_DB_LINK_CAST_ADG field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (app.KB == null || app.KB == string.Empty)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "Missing DEN_CAST_KB field in QPM DB for application " + req.APP;
                        ret = false;
                    }
                    else if (req.MAT == "B" && (req.TAT == "QCH" || req.TAT == "QCH_SNA_REP") && (prodApp = QPMDatabaseHelper.ReadApp(QPMDbConnStr, req.APP, "1", "0")) == null)
                    {
                        res.CODICE = "KO";
                        res.MESSAGGIO = "record missing in QPM database for baseline for application " + req.APP ;
                        ret = false;
 
                    }
                    else if (req.MAT == "B" && (req.TAT == "COF_SNA_REP" || req.TAT == "COF" || req.TAT == "COF_DMT" || req.TAT == "COF_SNA"))
                    {
                        prodApp = app;
                    }
                    else
                    {
                        src_path = app.SrcPath + "\\" + req.APP + "\\" + req.IDR + "\\" + req.APP;

                        if (!Directory.Exists(src_path))
                        {
                            res.CODICE = "KO";
                            res.MESSAGGIO = "Cannot find source folder " + src_path;
                            ret = false;
                        }
                        else
                        {
                            listener.WriteLine(DateTime.Now.ToString() + " Source folder detected:" + src_path);

                        }

                        string dest_path = ConfigurationManager.AppSettings["CAST_OutputPath"] + "\\" + req.APP;

                        if (!Directory.Exists(dest_path))
                        {
                            res.CODICE = "KO";
                            res.MESSAGGIO = "Cannot find output folder " + dest_path;
                            ret = false;
                        }
                        else
                        {
                            listener.WriteLine(DateTime.Now.ToString() + " Output folder detected:" + dest_path);

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                listener.WriteLine(DateTime.Now.ToString() + " Errore: " + ex.Message);
                listener.WriteLine(DateTime.Now.ToString() + " StackTrace: " + ex.StackTrace);

                if (ex.InnerException != null)
                {
                    listener.WriteLine(DateTime.Now.ToString() + " InnerException: " + ex.InnerException.Message);
                
                }

                res.CODICE = "KO";
                res.MESSAGGIO = "Errore: " + ex.Message;
                ret = false;
            }

            if(ret)
                listener.WriteLine(DateTime.Now.ToString() + " ValidateRequest passed.");
            else
                listener.WriteLine(DateTime.Now.ToString() + " ValidateRequest failed: " + res.MESSAGGIO);

              return ret;
        }

        public bool ValidateDate(string s)
        {
            CultureInfo provider = new CultureInfo("it-IT");
            try
            {
                //DateTime dt = DateTime.ParseExact(s, "d", provider);
                DateTime dt = DateTime.Parse(s, provider);

            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        public void ThreadProc()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
 
            listener.WriteLine(DateTime.Now.ToString() + " ThreadProc started...");          

            try
            {
                listener.WriteLine(DateTime.Now.ToString() + " Creating output folder...");    
                Directory.CreateDirectory(output_path);

                switch (req.TAT)
                {
                    case "COF":
                        ConsolidamentoMetriche(req,app,true,true,true);
                        break;
                    case "COF_DMT":
                        ConsolidamentoMetriche(req, app,true,false,true);
                        break;
                    case "COF_SNA":
                        ConsolidamentoMetriche(req, app,false,true,true);
                        break;
                    case "QPM_SNA_REP":
                    case "COF_SNA_REP":
                        QPMReport(req);
                        break;
                    case "QCH":
                        //QualityCheck(req,app);
                        break;
                    default:
                        break;
                }

                listener.WriteLine(DateTime.Now.ToString() + " Notifica Attività eseguita con successo a QPM...");

                AllegaFile(log_file_name);

#if  QPM
                qpmResult res = qpm.chiudiAttivita(
                    req.IDR,
                    req.IDA,
                    req.APP,
                    DateTime.Now.ToString(),
                    "OK",
                    "Attività eseguita con successo",
                    "0",
                    filesList.ToArray()
                    );

                listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

            }
            catch (Exception ex)
            {
                listener.WriteLine(DateTime.Now.ToString() + " Errore: " + ex.Message  );
                listener.WriteLine("StackTrace: " + ex.StackTrace);
                listener.WriteLine(DateTime.Now.ToString() + " Notifica Attività interrotta per errore a QPM...");

                try
                {

                    AllegaFile(log_file_name);

#if QPM
                    qpmResult res = qpm.chiudiAttivita(
                        req.IDR,
                        req.IDA,
                        req.APP,
                        DateTime.Now.ToString(),
                        "KO",
                        "Attività non completata con errore: " + ex.Message,
                        "2",
                        filesList.ToArray()
                        );

                    listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
                }
                catch (Exception ex2)
                {
                    listener.WriteLine(DateTime.Now.ToString() + " Errore: " + ex2.Message);
                }
            }

            listener.WriteLine(DateTime.Now.ToString() + " ThreadProc ended.");
            
        }

        private void AllegaFile(string fpath)
        {
            qpm_fileResult result = new qpm_fileResult();
            result.pathFile = output_path + "\\" + Path.GetFileName(fpath);
            result.tipo = Path.GetExtension(fpath);
            result.allegare = false;
            filesList.Add(result);

            try
            {
                File.Copy(fpath, result.pathFile, true);
            }
            catch
            {
                listener.WriteLine(DateTime.Now.ToString() + " Cannot copy file  " + fpath + " to  " + result.pathFile);
            }
        }

        public void ConsolidamentoMetriche(CASTRequestType req, GBSApplication app, bool doDelivery, bool doSnapshot, bool mapFolders)
        {
            listener.WriteLine(DateTime.Now.ToString() + " ConsolidamentoMetriche started...");

            if (mapFolders)
            {
                MapFolder(castSrcPath, src_path);
                MapFolder(castBaselineSrcPath, src_path);
            }

            if (doDelivery && app.AIP_VER == "8.X")
                if (!AutomateDelivery(app, req))
                {
                    throw new Exception("Attività non completata con errore: delivery dei sorgenti non completata per errore, vedere il log per ulteriori informazioni");
                }

            if(doSnapshot)
                if (!GenerateSnapshot(app, req))
                {
                    throw new Exception("Attività non completata con errore: snapshot non completato per errore, vedere il log per ulteriori informazioni");
                }

            listener.WriteLine(DateTime.Now.ToString() + " ConsolidamentoMetriche completed");

        }

        public string MapFolder(string link, string target)
        {
            listener.WriteLine(DateTime.Now.ToString() + " MapFolder started...");
            string ret = "";

            qpmResult res;

#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");

            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "Mapping symbolic link " + link + " to target folder " + target, "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

            if (Directory.Exists(link))
            {
                
                  using (SafeFileHandle hFile = WinAPI.CreateFile(
                    link,
                    WinAPI.GENERIC_READ, FileShare.Read,
                    IntPtr.Zero,
                    (FileMode)WinAPI.OPEN_EXISTING,
                    WinAPI.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero
                    ))
                {
                    StringBuilder sBuffer = new StringBuilder(255);

                    int i = WinAPI.GetFinalPathNameByHandle(hFile, sBuffer, 255, 0x0);

                    if (sBuffer.Length > 4)
                        ret = sBuffer.Remove(0, 4).ToString();
                    else
                        ret = sBuffer.ToString();

                    listener.WriteLine(DateTime.Now.ToString() + " Old target folder was: " + ret);

                 }              
                      
                
                listener.WriteLine(DateTime.Now.ToString() + " Deleting old link: " + link);
                Directory.Delete(link);
            }

            listener.WriteLine(DateTime.Now.ToString() + " Mapping symbolic link: " + link + " to target folder: " + target);

            if (!WinAPI.CreateSymbolicLink(link, target, 0x1))
            {
                
                throw new Exception("Mapping symbolic link: " + link + " to target folder: " + target + " failed");

            }


#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

            qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "Symbolic link mapped", "F", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

            listener.WriteLine(DateTime.Now.ToString() + " MapFolder completed");

            return ret;
        }

        private void Execute(string executableName, string executableParameter)
        {
            listener.WriteLine("Calling: " + executableName + " Params: " + executableParameter);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;


            Process process = Process.Start(processStartInfo);

            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                listener.WriteLine(result);

            }
            using (StreamReader reader = process.StandardError)
            {
                string result = reader.ReadToEnd();
                listener.WriteLine(result);

            }

            process.WaitForExit(4000);


            listener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);

            if (process.ExitCode != 0)
                throw new Exception("Command Failed: " + executableName);

            listener.WriteLine(DateTime.Now.ToString() + " Command completed");
         }

        public void QPMReport(CASTRequestType req)
        {
            listener.WriteLine(DateTime.Now.ToString() + " QPMReport started...");
#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");
            qpmResult res;
            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "QPM CAST Report started", "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
            string DevConnStr = string.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2};", castDBServer, app.ADG, app.ADG);
            listener.WriteLine(DateTime.Now.ToString() + " DevConnStr=" + DevConnStr);

            string ProdConnStr = string.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2};", castDBServer, prodApp.ADG, prodApp.ADG);
            listener.WriteLine(DateTime.Now.ToString() + " ProdConnStr=" + ProdConnStr);

            int dev_app_id = CASTDatabaseHelper.GetApplicationID(req.APP, req.SNN, DevConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " dev_app_id=" + dev_app_id);

            int dev_snapshot_id = CASTDatabaseHelper.GetSnapshotID(req.APP, req.SNN, DevConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " dev_snapshot_id=" + dev_snapshot_id);

            listener.WriteLine(DateTime.Now.ToString() + " Getting dev violations...");

            DataTable dev_all = CASTDatabaseHelper.GetViolations(dev_app_id, dev_snapshot_id, DevConnStr, app.URL, aed_url, app.ADG, app.KB, app.KB);

            if (saveDebugFiles)
            {
                string dev_all_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\dev_all_" + req.APP + "_" + req.IDR + ".csv";
                listener.WriteLine(DateTime.Now.ToString() + " Dump dev violations on file: " + dev_all_path);
                CASTDatabaseHelper.DumpViolations(dev_all_path, dev_all.Rows);
            }

            listener.WriteLine(DateTime.Now.ToString() + " APP=\"" + req.APP + "\"");
            listener.WriteLine(DateTime.Now.ToString() + " VBS=\"" + req.VBS + "\"");
            listener.WriteLine(DateTime.Now.ToString() + " Calling GetSnapshotID...");

            int prod_snapshot_id = CASTDatabaseHelper.GetSnapshotID(req.APP, req.VBS, ProdConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " prod_snapshot_id=" + prod_snapshot_id);

            int prod_app_id = CASTDatabaseHelper.GetApplicationID(req.APP, req.VBS, ProdConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " prod_app_id=" + prod_app_id);

            listener.WriteLine(DateTime.Now.ToString() + " Getting Baseline Violations...");
            DataTable prod_all = CASTDatabaseHelper.GetViolations(prod_app_id, prod_snapshot_id, ProdConnStr, prodApp.URL, aed_url, prodApp.ADG, prodApp.KB, app.KB);


            decimal prod_count, prod_golden_count;

            listener.WriteLine(DateTime.Now.ToString() + " Getting Golden Rules...");
            QPMDatabaseHelper.ReadGoldenRules(QPMDbConnStr, app);
            listener.WriteLine(DateTime.Now.ToString() + " Getting Backlog Violations count...");
            prod_count = CASTDatabaseHelper.GetViolationCount(prod_app_id, prod_snapshot_id, ProdConnStr, app.Golden, out prod_golden_count);
            listener.WriteLine(DateTime.Now.ToString() + " prod_count=" + prod_count);
            listener.WriteLine(DateTime.Now.ToString() + " prod_golden_count=" + prod_golden_count);

            listener.WriteLine(DateTime.Now.ToString() + " Getting New Violations...");

            DataTable new_violations = CASTDatabaseHelper.GetNewViolations(prod_all, dev_all, app.Golden, app.Exclude, app.URL);

            listener.WriteLine(DateTime.Now.ToString() + " Getting Removed Violations...");

            DataTable removed_violations = CASTDatabaseHelper.GetRemovedViolations(prod_all, dev_all, app.Golden, app.Exclude, app.URL);

            if (saveDebugFiles)
            {
                string prod_all_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\prod_all_" + req.APP + "_" + req.IDR + ".csv";
                listener.WriteLine(DateTime.Now.ToString() + " Dump prod violations on file: " + prod_all_path);
                CASTDatabaseHelper.DumpViolations(prod_all_path, prod_all.Rows);

                string added_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\new_violations_" + req.APP + "_" + req.IDR + ".csv";
                listener.WriteLine(DateTime.Now.ToString() + " Dump New violations on file: " + added_path);
                CASTDatabaseHelper.DumpViolations(added_path, new_violations.Rows);

                string removed_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\removed_violations_" + req.APP + "_" + req.IDR + ".csv";
                listener.WriteLine(DateTime.Now.ToString() + " Dump Removed violations on file: " + removed_path);
                CASTDatabaseHelper.DumpViolations(removed_path, removed_violations.Rows);
            }

            int files_num = CASTDatabaseHelper.GetMeasure(dev_app_id, dev_snapshot_id, 10154, DevConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " files_num=" + files_num);

            int obj_num = CASTDatabaseHelper.GetMeasure(dev_app_id, dev_snapshot_id, 10152, DevConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " obj_num=" + obj_num);

            string baseline_version = CASTDatabaseHelper.GetApplicationVersion(prod_snapshot_id, prod_app_id, ProdConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " baseline_version=" + baseline_version);

            string contract_version = QPMDatabaseHelper.GetContractBaseline(int.Parse(req.IDR), QPMDbConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " contract_version=" + contract_version);

            string den_release, COD_RELEASE_RLM;
            QPMDatabaseHelper.GetRelease(int.Parse(req.IDR), QPMDbConnStr, out den_release, out COD_RELEASE_RLM);
            listener.WriteLine(DateTime.Now.ToString() + " DEN_RELEASE=" + den_release);
            listener.WriteLine(DateTime.Now.ToString() + " COD_RELEASE_RLM=" + COD_RELEASE_RLM);


            string message;
            if (new_violations.Select("is_golden='yes'").Length > 0)
                message = "Failed";
            else
                message = "Passed";
            Trace.WriteLine(DateTime.Now.ToString() + " Result: Quality Check " + message);

            qpm_fileResult result = new qpm_fileResult();
            
            //result.pathFile = output_path + "\\new_violations.csv";
            //result.tipo = Path.GetExtension("csv");
            //result.allegare = true;
            //filesList.Add(result);

            //CASTDatabaseHelper.DumpViolations(result.pathFile, new_violations.Rows);
            //Trace.WriteLine(DateTime.Now.ToString() + " Violations added saved to output file " + result.pathFile);
            //result = new qpm_fileResult();

            result.pathFile = output_path + "\\" + req.IDR + "_" + req.APP + "_GBS_CAST_Report.xlsx";   //2166_SVG_GBS_CAST_Report;
            result.tipo = Path.GetExtension("xlsx");
            result.allegare = true;
            filesList.Add(result);

            listener.WriteLine(DateTime.Now.ToString() + " Building Excel Report...");
            ExcelReportHelper.BuildReport(req, new_violations, removed_violations, message, result.pathFile, files_num, obj_num, prod_count, prod_golden_count, baseline_version,contract_version, den_release, COD_RELEASE_RLM);
            Trace.WriteLine(DateTime.Now.ToString() + " QualityCheckReport saved to output file " + result.pathFile);
#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);
            qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "QPM CAST Report completed", "F", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
            listener.WriteLine(DateTime.Now.ToString() + " QPMReport completed");
        }

//        public void QualityCheck(CASTRequestType req, GBSApplication app)
//        {
//            listener.WriteLine(DateTime.Now.ToString() + " QualityCheck started...");
//            qpmResult res;
        
//            string DevConnStr = string.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2};", castDBServer, app.ADG, app.ADG);
//            listener.WriteLine(DateTime.Now.ToString() + " DevConnStr=" + DevConnStr);


//            if (!skipSnapshot)
//            {
//                DataTable dates = CASTDatabaseHelper.GetSnapshotDates(req.APP, DevConnStr);

//                foreach (DataRow dr in dates.Rows)
//                    DeleteSnapshot(app, (DateTime)dr[0]);

 
//                string baseline_path;

//                if (req.MAT == "B")
//                    baseline_path = QPMDatabaseHelper.GetBaselineSrcPath(req.VBS, QPMDbConnStr) + "\\" + req.APP;
//                else
//                    baseline_path = src_path;

//                MapFolder(castSrcPath, src_path);

//                MapFolder(castBaselineSrcPath, baseline_path);
 
//                if (!GenerateSnapshot(app, req))
//                {
//                    listener.WriteLine(DateTime.Now.ToString() + " QualityCheck cannot start: snapshot error");
//                    throw new Exception("Attività non completata con errore: snapshot non completato per errore, vedere il log per ulteriori informazioni");
//                }
                 
//                MapFolder(castSrcPath, baseline_path);
//            }


//#if  QPM
//            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");

//            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "Quality check started", "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
//            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
//            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
//#endif
            

//            int dev_app_id = CASTDatabaseHelper.GetApplicationID(req.APP, req.SNN, DevConnStr);
//            listener.WriteLine(DateTime.Now.ToString() + " dev_app_id=" + dev_app_id);

//            int dev_snapshot_id = CASTDatabaseHelper.GetSnapshotID(req.APP, req.SNN, DevConnStr);
//            listener.WriteLine(DateTime.Now.ToString() + " dev_snapshot_id=" + dev_snapshot_id);

//            //listener.WriteLine(DateTime.Now.ToString() + " Getting dev object names...");

//            //List<string> dev_objects = CASTDatabaseHelper.GetObjectNames(DevConnStr, app.KB);

//            listener.WriteLine(DateTime.Now.ToString() + " Getting dev violations...");

//            //decimal dev_count, dev_golden_count;
//            decimal prod_count, prod_golden_count;
//            //DataTable dev_all = CASTDatabaseHelper.GetViolations(dev_app_id, dev_snapshot_id, DevConnStr, app.URL, app.KB, dev_objects, app.Golden, out dev_count, out dev_golden_count);

//            DataTable dev_all = CASTDatabaseHelper.GetViolations(dev_app_id, dev_snapshot_id, DevConnStr, app.URL, app.KB, app.KB);



//            if (saveDebugFiles)
//            {
//                string dev_all_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\dev_all_" + req.APP + "_" + req.IDR + ".csv";
//                listener.WriteLine(DateTime.Now.ToString() + " Dump dev violations on file: " + dev_all_path);

//                CASTDatabaseHelper.DumpViolations(dev_all_path, dev_all.Rows);
//            }

//            if (req.MAT == "B")
//            {
//                string ProdConnStr = string.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2};", castDBServer, prodApp.ADG, prodApp.ADG);
//                listener.WriteLine(DateTime.Now.ToString() + " ProdConnStr=" + ProdConnStr);

//                listener.WriteLine(DateTime.Now.ToString() + " APP=\"" + req.APP +"\"");
//                listener.WriteLine(DateTime.Now.ToString() + " VBS=\"" + req.VBS +"\"");
//                listener.WriteLine(DateTime.Now.ToString() + " Calling GetSnapshotID...");

//                int prod_snapshot_id = CASTDatabaseHelper.GetSnapshotID(req.APP, req.VBS, ProdConnStr);
//                listener.WriteLine(DateTime.Now.ToString() + " prod_snapshot_id=" + prod_snapshot_id);

//                int prod_app_id = CASTDatabaseHelper.GetApplicationID(req.APP, req.VBS, ProdConnStr);
//                listener.WriteLine(DateTime.Now.ToString() + " prod_app_id=" + prod_app_id);

//                listener.WriteLine(DateTime.Now.ToString() + " Getting Baseline Violations...");

//                //DataTable prod_all = CASTDatabaseHelper.GetViolations(prod_app_id, prod_snapshot_id, ProdConnStr, prodApp.URL, prodApp.KB, dev_objects,app.Golden, out prod_count,out prod_golden_count);

//                DataTable prod_all = CASTDatabaseHelper.GetViolations(prod_app_id, prod_snapshot_id, ProdConnStr, prodApp.URL, prodApp.KB, app.KB);

//                listener.WriteLine(DateTime.Now.ToString() + " Getting Backlog Violations count...");

//                prod_count = CASTDatabaseHelper.GetViolationCount(prod_app_id, prod_snapshot_id, ProdConnStr, app.Golden, out prod_golden_count);
//                listener.WriteLine(DateTime.Now.ToString() + " prod_count=" + prod_count);
//                listener.WriteLine(DateTime.Now.ToString() + " prod_golden_count=" + prod_golden_count);   


//                listener.WriteLine(DateTime.Now.ToString() + " Getting New Violations...");

//                DataTable new_violations = CASTDatabaseHelper.GetNewViolations(prod_all, dev_all, app.Golden, app.Exclude,app.URL);

//                listener.WriteLine(DateTime.Now.ToString() + " Getting Removed Violations...");
                
//                DataTable removed_violations = CASTDatabaseHelper.GetRemovedViolations(prod_all, dev_all, app.Golden, app.Exclude, app.URL);

//                if (saveDebugFiles)
//                {
//                    string prod_all_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\prod_all_" + req.APP + "_" + req.IDR + ".csv";
//                    listener.WriteLine(DateTime.Now.ToString() + " Dump prod violations on file: " + prod_all_path);                   
//                    CASTDatabaseHelper.DumpViolations(prod_all_path, prod_all.Rows);

//                    string added_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\new_violations_" + req.APP + "_" + req.IDR + ".csv";
//                    listener.WriteLine(DateTime.Now.ToString() + " Dump New violations on file: " + added_path);                   
//                    CASTDatabaseHelper.DumpViolations(added_path, new_violations.Rows);

//                    string removed_path = ConfigurationManager.AppSettings["CAST_LogPath"] + "\\removed_violations_" + req.APP + "_" + req.IDR + ".csv";
//                    listener.WriteLine(DateTime.Now.ToString() + " Dump Removed violations on file: " + removed_path);                   
//                    CASTDatabaseHelper.DumpViolations(removed_path, removed_violations.Rows);

//                }

//                int files_num = CASTDatabaseHelper.GetMeasure(dev_app_id, dev_snapshot_id, 10154, DevConnStr);
//                listener.WriteLine(DateTime.Now.ToString() + " files_num=" + files_num);                   

//                int obj_num = CASTDatabaseHelper.GetMeasure(dev_app_id, dev_snapshot_id, 10152, DevConnStr);
//                listener.WriteLine(DateTime.Now.ToString() + " obj_num=" + obj_num);

//                string baseline_version = CASTDatabaseHelper.GetApplicationVersion(prod_snapshot_id, prod_app_id, ProdConnStr);
//                listener.WriteLine(DateTime.Now.ToString() + " baseline_version=" + baseline_version);


//                string message;

//                if (new_violations.Select("is_golden='yes' AND is_excluded='no'").Length > 0)
//                {
//                    message = "Failed";
//                }
//                else
//                {
//                    message = "Passed";
//                }

//                Trace.WriteLine(DateTime.Now.ToString() + " Result: Quality Check " + message);


//                qpm_fileResult result = new qpm_fileResult();
//                result.pathFile = output_path + "\\new_violations.csv";
//                result.tipo = Path.GetExtension("csv");
//                result.allegare = true;

//                filesList.Add(result);

//                CASTDatabaseHelper.DumpViolations(result.pathFile, new_violations.Rows);

//                Trace.WriteLine(DateTime.Now.ToString() + " Violations added saved to output file " + result.pathFile);


//                result = new qpm_fileResult();
//                result.pathFile = output_path + "\\QualityCheckReport.xlsx";
//                result.tipo = Path.GetExtension("xlsx");
//                result.allegare = true;
                
//                filesList.Add(result);

//                listener.WriteLine(DateTime.Now.ToString() + " Building Excel Report...");

//                ExcelReportHelper.BuildReport(req, new_violations, removed_violations, message, result.pathFile, files_num, obj_num, prod_count, prod_golden_count, baseline_version, contract_version);

//                Trace.WriteLine(DateTime.Now.ToString() + " QualityCheckReport saved to output file " + result.pathFile);

//            }
//            else
//            {
//                qpm_fileResult result = new qpm_fileResult();
//                result.pathFile = output_path + "\\dev_violations.csv";
//                result.tipo = Path.GetExtension("csv");
//                result.allegare = true;

//                filesList.Add(result);

//                CASTDatabaseHelper.DumpViolations(result.pathFile, dev_all.Rows);

//                Trace.WriteLine(DateTime.Now.ToString() + " dev_violations saved to file " + result.pathFile);

//            }

//#if  QPM
//            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

//            qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "Quality Check completed", "F", DateTime.Now.ToString(), out res);
//            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
//#endif

 

//            listener.WriteLine(DateTime.Now.ToString() + " QualityCheck completed");
//        }

        public void RunAnalysis(GBSApplication app, string logFilePath)
        {
            listener.WriteLine(DateTime.Now.ToString() + " Inizio esecuzione analisi...");


            string executableName = castExePath7 + "\\CAST-MS-cli.exe";
            string executableParameter = "RunAnalysis -connectionProfile \"" + app.CMSProfile + "\" -deliveryUnit \"" + app.CMSDeliveryUnit + "\" -system \"" + app.CMSSystem + "\" -appli \"" + app.Name + "\" -logFilePath \"" + logFilePath + "\"";


            listener.WriteLine("Calling: " + executableName);
            listener.WriteLine("Params: " + executableParameter);



            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += new DataReceivedEventHandler(CMSOutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(CMSOutputHandler);
  
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

 
            process.WaitForExit();
            

            listener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);

            if (process.ExitCode != 0)
                throw new Exception("L'analisi è stata interrotta a causa di un errore, esaminare i log prodotti per ulteriori informazioni");

            listener.WriteLine(DateTime.Now.ToString() + " Analisi completata con successo");
      
        }

        public void DeleteSnapshot(GBSApplication app, DateTime dt)
        {
            listener.WriteLine(DateTime.Now.ToString() + " Inizio DeleteSnapshot...");

#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");
            qpmResult res;
            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "Deleting Snapshot for " + app.Name + " for date " + dt.ToShortDateString(), "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
            string captureDate;
   
            if(dt.Hour!=0 || dt.Minute!=0)
                captureDate = dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00") + dt.Hour.ToString("00") + dt.Minute.ToString("00");
            else
                captureDate = dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00");

            string executableName = castExePath7 + "\\CAST-MS-cli.exe";
            string executableParameter = "DeleteSnapshot -connectionProfile \"" + app.CMSProfile + "\" -deliveryUnit \"" + app.CMSDeliveryUnit + "\" -appli \"" + app.Name + "\" -captureDate " + captureDate ;


            listener.WriteLine("Calling: " + executableName);
            listener.WriteLine("Params: " + executableParameter);



            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += new DataReceivedEventHandler(CMSOutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(CMSOutputHandler);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            process.WaitForExit();


            listener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);

            if (process.ExitCode != 0)
                throw new Exception("La cancellazione è stata interrotta a causa di un errore, esaminare i log prodotti per ulteriori informazioni");

#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

            qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "Snapshot deleted", "F", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

            listener.WriteLine(DateTime.Now.ToString() + " DeleteSnapshot completata con successo");

        }

        public bool AutomateDelivery(GBSApplication app, CASTRequestType req)
        {
            listener.WriteLine(DateTime.Now.ToString() + " AntomateDelivery started...");

            qpmResult res;
#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");

            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "AntomateDelivery started", "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

            DateTime dt = DateTime.Parse(req.CDT, new CultureInfo("it-IT"));
            string captureDate = dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00") + dt.Hour.ToString("00") + dt.Minute.ToString("00");

            string ConnStr = string.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2};", castDBServer, app.ADG, app.ADG);
            listener.WriteLine(DateTime.Now.ToString() + " ConnStr=" + ConnStr);

            int snapshot_id = CASTDatabaseHelper.GetSnapshotID(app.Name, req.VBS, ConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " snapshot_id=" + snapshot_id);

            string previousVersion = CASTDatabaseHelper.GetSnapshotVersion(snapshot_id, ConnStr);
            listener.WriteLine(DateTime.Now.ToString() + " previousVersion=" + previousVersion);


            string executableName, executableParameter;

            executableName = castExePath8 + "\\CAST-MS-cli.exe";

            executableParameter = "AutomateDelivery -connectionProfile \"" + app.CMSProfile +
                                            "\" -appli \"" + app.Name +
                                            "\" -logFilePath \"" + delivery_log_file_name +
                                            "\" -date " + captureDate +
                                            " -version \"" + req.VER + "\"" +
                                           " -fromVersion \"" + previousVersion + "\"";


            listener.WriteLine("Calling: " + executableName);
            listener.WriteLine("Params: " + executableParameter);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;

            process = new Process();
            process.StartInfo = processStartInfo;

            process.Start();

            process.WaitForExit();

            listener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);
            AllegaFile(delivery_log_file_name);

            listener.WriteLine(DateTime.Now.ToString() + " AntomateDelivery ended");

            if (process.ExitCode == 0)
            {
#if QPM
                listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

                qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "AntomateDelivery ended successfully", "F", DateTime.Now.ToString(), out res);
                listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif

                return true;
            }
               
            else
                return false;
        }

        public bool GenerateSnapshot(GBSApplication app, CASTRequestType req)
        {
            listener.WriteLine(DateTime.Now.ToString() + " GenerateSnapshot started...");
            qpmResult res;

            string attCode1 = String.Empty;

#if  QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");

            attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", "Preparing GenerateSnapshot", "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode1);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
            //DateTime dt = DateTime.ParseExact(req.CDT, "d", new CultureInfo("it-IT"));
            DateTime dt = DateTime.Parse(req.CDT, new CultureInfo("it-IT"));
            string captureDate = dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00") + dt.Hour.ToString("00") + dt.Minute.ToString("00");
            string executableName, executableParameter;
            if (app.AIP_VER == "7.X")
            {
                executableName = castExePath7 + "\\CAST-MS-cli.exe";

                executableParameter = "GenerateSnapshot -connectionProfile \"" + app.CMSProfile +
                              "\" -deliveryUnit \"" + app.CMSDeliveryUnit +
                              "\" -appli \"" + app.Name +
                              "\" -logFilePath \"" + snapshot_log_file_name +
                              "\" -snapshot \"" + req.SNN +
                              "\" -captureDate " + captureDate +
                              " -version \"" + req.VER +
                              "\" -ignoreEmptyModule true ";  // -skipAnalysisJob true";
            }
            else
            {
                executableName = castExePath8 + "\\CAST-MS-cli.exe";

                executableParameter = "GenerateSnapshot -connectionProfile \"" + app.CMSProfile +
                                             "\" -appli \"" + app.Name +
                                             "\" -logFilePath \"" + snapshot_log_file_name +
                                             "\" -snapshot \"" + req.SNN +
                                             "\" -captureDate " + captureDate +
                                             " -version \"" + req.VER +
                                             "\" -ignoreEmptyModule true "+
                                             " -consolidateMeasures true ";
            }


            listener.WriteLine("Calling: " + executableName);
            listener.WriteLine("Params: " + executableParameter);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, executableParameter);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardOutput = true;
                      
            processStartInfo.CreateNoWindow = true;
 
            process = new Process();
            process.StartInfo = processStartInfo;
            process.EnableRaisingEvents = true;
           
            process.OutputDataReceived += new DataReceivedEventHandler(CMSOutputHandler);
            //process.Exited += new EventHandler(process_Exited);
            
            process.Start();

#if QPM
            listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

            qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", "GenerateSnapshot launched successfully", "F", DateTime.Now.ToString(), out res);
            listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
            attCode = String.Empty;

            process.BeginOutputReadLine();

            timer = new System.Timers.Timer();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
           
            timer.Interval = (1000) * (30);              
            timer.Enabled = true;                       
            timer.Start();

            while (!process.HasExited)
            {
                Thread.Sleep(100);
            }
 
            timer.Stop();

            Thread.Sleep(500);

            ReadLog();

            listener.WriteLine(DateTime.Now.ToString() + " ExitCode: " + process.ExitCode);

            AllegaFile(snapshot_log_file_name);


            listener.WriteLine(DateTime.Now.ToString() + " GenerateSnapshot ended");

            if (process.ExitCode == 0)
            {
                return true;
            }       
            else
                return false;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReadLog();
        }

        private void ReadLog()
        { 
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

            listener.WriteLine(DateTime.Now.ToString() + " ReadLog enter");
            listener.WriteLine(DateTime.Now.ToString() + " previousEndPosition=" + previousEndPosition);

            qpmResult res;
            string line = string.Empty;
            string descr;

            using (FileStream file = new FileStream(snapshot_log_file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Seek(previousEndPosition, SeekOrigin.Begin);
                previousEndPosition = file.Length;
                using (TextReader reader = new StreamReader(file))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (attCode == String.Empty)
                        {
                            if (line.Contains("starting Task Snapshot generation"))
                                continue;
                            if (line.Contains("starting Task Run UserProject"))
                                continue;
                            if (line.Contains("starting Task Run Snapshot Preparation Assistant \""))
                                continue;

                            int i = line.LastIndexOf("starting Task");
                            if (i > -1)
                            {
                                descr = line.Substring(i);
                                listener.WriteLine(DateTime.Now.ToString() + " [CAST-MS-cli LOG] " + descr);
                                attCode = descr;

#if  QPM
                                listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita...");

                                attCode = qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, "", descr, "OK", "Attività In corso...", "I", DateTime.Now.ToString(), out res);
                                listener.WriteLine(DateTime.Now.ToString() + " attCode: " + attCode);
                                listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
                            }
                        }
                        else
                        {

                            if (line.Contains("Task message: The analysis has not ended correctly"))
                            {
                                listener.WriteLine(DateTime.Now.ToString() + " [CAST-MS-cli LOG] Task message: The analysis has not ended correctly");


#if  QPM
                                listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

                                qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "The analysis has not ended correctly", "KO", "Please see the analysis log for further details", "E", DateTime.Now.ToString(), out res);
                                listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
                                attCode = String.Empty;
                            }
                            else if (line.Contains("Task message:"))
                            {
                                int i = line.LastIndexOf("Task message:");
                                descr = line.Substring(i);
                                listener.WriteLine(DateTime.Now.ToString() + " [CAST-MS-cli LOG] " + descr);

#if  QPM
                                listener.WriteLine(DateTime.Now.ToString() + " calling registraSottoAttivita: " + attCode);

                                qpm.registraSottoAttivita(req.IDR, req.IDA, req.APP, attCode, "Attività completata", "OK", descr, "F", DateTime.Now.ToString(), out res);
                                listener.WriteLine(DateTime.Now.ToString() + " qpmResult: " + res.codResult + " " + res.descrResult);
#endif
                                attCode = String.Empty;
                            }

                        }

                    }

                }

            }

            listener.WriteLine(DateTime.Now.ToString() + " previousEndPosition=" + previousEndPosition);
            listener.WriteLine(DateTime.Now.ToString() + " ReadLog exit");
 
        }

        private void CMSOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {

                // Add the text to the collected output.
                listener.WriteLine(DateTime.Now.ToString() + " [CAST-MS-cli] " + outLine.Data);
            }
        }

    }

    class WinAPI
    {
        internal const int
            GENERIC_READ = unchecked((int)0x80000000),
            FILE_FLAG_BACKUP_SEMANTICS = unchecked((int)0x02000000),
            OPEN_EXISTING = unchecked((int)3);

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_OBJECTID_BUFFER
        {
            public struct Union
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] BirthVolumeId;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] BirthObjectId;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] DomainId;
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] ObjectId;

            public Union BirthInfo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] ExtendedInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            int nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            String fileName,
            int dwDesiredAccess,
            System.IO.FileShare dwShareMode,
            IntPtr securityAttrs_MustBeZero,
            System.IO.FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile_MustBeZero
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandle(
            IntPtr hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);


        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetFinalPathNameByHandle(
        SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszFilePath, int cchFilePath, int dwFlags);

    }

}
