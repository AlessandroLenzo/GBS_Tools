using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceProcess;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;


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

            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fieVersionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            var version = fieVersionInfo.FileVersion;

            listener.WriteLine(DateTime.Now.ToString() + " Starting CASTService ver."+ version + " ...");

            if (ConfigurationManager.AppSettings["SrcDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["SrcDiskMap"], ConfigurationManager.AppSettings["SrcRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DestDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DestDiskMap"], ConfigurationManager.AppSettings["DestRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["StorageDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["StorageDiskMap"], ConfigurationManager.AppSettings["StorageRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["LinkDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["LinkDiskMap"], ConfigurationManager.AppSettings["LinkRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DeliveryDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DeliveryDiskMap"], ConfigurationManager.AppSettings["DeliveryRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DeployDiskMap"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DeployDiskMap"], ConfigurationManager.AppSettings["DeployRoot"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DeliveryDiskMapQC"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DeliveryDiskMapQC"], ConfigurationManager.AppSettings["DeliveryRootQC"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            if (ConfigurationManager.AppSettings["DeployDiskMapQC"].Length > 0)
                RequestHandler.MapDisk(ConfigurationManager.AppSettings["DeployDiskMapQC"], ConfigurationManager.AppSettings["DeployRootQC"], ConfigurationManager.AppSettings["QPM_User"], ConfigurationManager.AppSettings["QPM_Pw"], listener);
            try
            {
                if (QPMDatabaseHelper.TestConn(ConfigurationManager.AppSettings["QPM_ConnStr"]))
                    listener.WriteLine(DateTime.Now.ToString() + " QPM DB Connection OK");

                listener.WriteLine(DateTime.Now.ToString() + " CASTService Ready");
            }
            catch(Exception ex)
            {
                listener.WriteLine(DateTime.Now.ToString() + " " + ex.Message);
            }

           
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

    public class CASTWindowsService : ServiceBase
    {
        public ServiceHost serviceHost = null;
        public CASTWindowsService()
        {
            // Name the Windows Service
            ServiceName = "CASTWindowsService";
        }

        public static void Main()
        {
            ServiceBase.Run(new CASTWindowsService());
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(CASTService));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }

    }

    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "CASTWindowsService";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
