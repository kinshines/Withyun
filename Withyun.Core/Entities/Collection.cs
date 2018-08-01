using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    /// <summary>
    /// 收藏夹
    /// </summary>
    public class Collection
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name="名称")]
        public string Title { get; set; }
        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
        public int UserId { get; set; }

        [Display(Name = "收藏时间")]
        public DateTime TimeStamp { get; set; }
    }
}