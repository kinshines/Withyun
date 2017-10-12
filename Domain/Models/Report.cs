using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
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

    public enum ReportType:byte{
        广告等垃圾信息,
        不友善内容,
        违反法律法规的内容,
        不宜公开讨论的政治内容,
        其他内容
    }
}