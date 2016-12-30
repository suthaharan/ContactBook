using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Contact2015.Models.ViewModels
{
    public class vmUserEdit
    {

        public int id { get; set; }

        [Display(Name = "Enter first name")]
        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[^\\/:*<>?!~',;\.\)\(]+$", ErrorMessage = "First name should not allow the special characters like ':', '.' ';', '*', '/' and '\' ")]
        public string userfirst { get; set; }

        [Display(Name = "Enter last name")]
        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[^\\/:*<>?!~',;\.\)\(]+$", ErrorMessage = "Last name should not allow the special characters like ':', '.' ';', '*', '/' and '\' ")]
        public string userlast { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string useremail { get; set; }
        public string userpass { get; set; }

    }
}