using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Withyun.Web.ViewModels
{
    public class MessageViewModel
    {
        [Required]
        [Display(Name = "留言")]
        public string Message { get; set; }
        public string Contacts { get; set; }
    }
}