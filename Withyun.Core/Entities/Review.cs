using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    /// <summary>
    /// 评论
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "内容")]
        public string Content { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }

        [NotMapped]
        public int Distributor { get; set; }
        [NotMapped]
        public string BlogTitle { get; set; }
        public virtual Blog Blog { get; set; }
    }
}