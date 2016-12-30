using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Contact2015.Models.ViewModels
{
    public class vmLogin
    {

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string txtemail { get; set; }
        public string txtpassword { get; set; } 

    }
}