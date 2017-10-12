using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    /// <summary>
    /// 关注
    /// </summary>
    public class Follow
    {
        public int Id { get; set; }

        public int DistributorId { get; set; }
        [Required]
        [StringLength(10)]
        public string DistributorName { get; set; }

        public int UserId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}