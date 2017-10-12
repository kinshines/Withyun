using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Domain.Models
{
    /// <summary>
    /// 链接
    /// </summary>
    public class Link
    {
        public int Id { get; set; }
        [StringLength(500)]
        [Display(Name = "描述")]
        public string Description { get; set; }

        [StringLength(500)]
        [Display(Name = "分享链接")]
        public string Url { get; set; }
        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool Invalide { get; set; }
        public virtual ICollection<LinkInvalid> LinkInvalids { get; set; } 

        [NotMapped]
        public string FixedUrl {
            get
            {
                if (Url.StartsWith("ed2k:", StringComparison.OrdinalIgnoreCase) ||
                    Url.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase) ||
                    Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return Url;
                }
                else
                {
                    return "http://" + Url;
                }
            }
        }
    }
}