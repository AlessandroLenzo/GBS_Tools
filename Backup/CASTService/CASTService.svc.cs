using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace CAST
{
    [ServiceBehavior(Namespace = "http://CAST/Service", InstanceContextMode = InstanceContextMode.Single)]
    public class CASTService : ICASTService
    {
        CASTTextWriterTraceListener listener;
       
        public CASTService()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

            string log_file_name = AppDomain.CurrentDomain.BaseDirectory + "\\log\\main_tracing.log";

            listener = new CASTTextWriterTraceListener(log_file_name);
            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;

            listener.WriteLine(DateTime.Now.ToString() + " Starting CASTService...");
            
            if(ConfigurationManager.AppSettings["SrcDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["SrcDiskMap"], ConfigurationManager.AppSettings["SrcRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DestDiskMap"].Length > 0)           
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DestDiskMap"], ConfigurationManager.AppSettings["DestRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["StorageDiskMap"].Length > 0)                       
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["StorageDiskMap"], ConfigurationManager.AppSettings["StorageRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["LinkDiskMap"].Length > 0)                                   
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["LinkDiskMap"], ConfigurationManager.AppSettings["LinkRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);

            listener.WriteLine(DateTime.Now.ToString() + " CASTService Ready");
        }

        public CASTResponseType Call(CASTRequestType request)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
 
            listener.WriteLine(DateTime.Now.ToString() + " Processing Request Id: " + request.IDR);
            CASTResponseType response = new CASTResponseType();

            try
            {
                RequestHandler handler = new RequestHandler(request);

                if (!handler.ValidateRequest(response))
                {
                    listener.WriteLine(DateTime.Now.ToString() + " Validation failed for Request Id: " + request.IDR);
                    return response;
                }
                else
                {
                    Thread t = new Thread(new ThreadStart(handler.ThreadProc));
                    t.Start();

                    listener.WriteLine(DateTime.Now.ToString() + " Thread started for Request Id: " + request.IDR);
 
                    response.CODICE = "OK";
                    response.MESSAGGIO = "Richiesta accettata e presa in carico";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.CODICE = "KO";
                response.MESSAGGIO = "Errore: " + ex.Message;
                return response;
            }
        }
    }
}
