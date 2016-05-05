using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models
{
    public class BetaViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string NameText { get; set; }

        public string CompanyText { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string EmailText { get; set; }

        public string PhoneText { get; set; }
        public string ContactPreference { get; set; }
    }
}