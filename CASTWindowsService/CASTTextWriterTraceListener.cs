using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace CAST
{
    public class CASTTextWriterTraceListener : TextWriterTraceListener
    {
        public CASTTextWriterTraceListener(string file) : base(file)
        {
           
        }
        
        public override void WriteLine(string message)
        {
            base.WriteLine(message);
            this.Flush();
        }
    }
}