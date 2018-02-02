using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CAST
{
    public class GBSApplication
    {
        private decimal appID;
 
        public string Name { get; set; }
        public string SrcPath { get; set; }
        public string CMSProfile { get; set; }
        public string CMSDeliveryUnit { get; set; }
        public string CMSSystem { get; set; }
        public string ADG { get; set; }
        public string KB { get; set; }
        public string URL { get; set; }
        public string AIP_VER { get; set; }

        public string[] Golden { get; set; }
        public string[] Exclude { get; set; }


        public GBSApplication(decimal id)
        {
            appID = id;
        }

    }
}