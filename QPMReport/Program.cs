using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QPMReport
{
    class Program
    {
        static void Main(string[] args)
        {
            CAST.CASTRequestType rt = new CAST.CASTRequestType();
            rt.AMB = Properties.Settings.Default.AMB;
            rt.APP = Properties.Settings.Default.APP;
            rt.CDT = Properties.Settings.Default.CDT;
            rt.CHS = Properties.Settings.Default.CHS;
            rt.IDA = Properties.Settings.Default.IDA;
            rt.IDR = Properties.Settings.Default.IDR;
            rt.LSV = Properties.Settings.Default.LSV;
            rt.MAC = Properties.Settings.Default.MAC;
            rt.MAT = Properties.Settings.Default.MAT;
            rt.SNN = Properties.Settings.Default.SNN;
            rt.TAT = Properties.Settings.Default.TAT;
            rt.VBS = Properties.Settings.Default.VBS;
            rt.VER = Properties.Settings.Default.VER;
            rt.VPR = Properties.Settings.Default.VPR;

            CAST.RequestHandler rh = new CAST.RequestHandler(rt);

            CAST.CASTResponseType response = new CAST.CASTResponseType();
            if(rh.ValidateRequest(response))
                rh.QPMReport(rt);

        }
    }
}
