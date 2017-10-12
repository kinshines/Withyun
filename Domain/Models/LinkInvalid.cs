using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    /// <summary>
    /// 链接无效
    /// </summary>
    public class LinkInvalid
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LinkId { get; set; }
        public virtual Link Link { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}