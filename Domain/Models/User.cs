using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Domain.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string UserName { get; set; }

        [Required]
        [StringLength(128)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string SecurityStamp { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? LoginTime { get; set; }

        public bool HasAlipay { get; set; }
        public bool HasWechat { get; set; }

    }
}