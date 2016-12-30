using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Contact2015.Models.ViewModels
{
    public class vmReset
    {
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string txtremail { get; set; }
        public string txtroldpwd { get; set; }
        public string txtrpwd1 { get; set; }
        public string txtrpwd2 { get; set; } 
    }
}