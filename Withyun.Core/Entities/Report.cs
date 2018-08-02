using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Withyun.Core.Enums;

namespace Withyun.Core.Entities
{
    /// <summary>
    /// 举报
    /// </summary>
    public class Report
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public ReportType ReportType { get; set; }
        [StringLength(200)]
        public string Content { get; set; }
        public int UserId { get; set; }
        public virtual Blog Blog { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    
}