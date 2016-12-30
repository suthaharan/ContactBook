using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contact2015.Models.ViewModels
{
    public class vmContact
    {

        public int id { get; set; }
        public string txtfirst { get; set; }
        public string txtlast { get; set; }
        public string txtphone { get; set; }
        public string txtstreet { get; set; }
        public string txtcity { get; set; }
        public string txtprovince { get; set; }
        public string txtpostal { get; set; }
        public string selcountry { get; set; }
        public string notes { get; set; }


    }
}