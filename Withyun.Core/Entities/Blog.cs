using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using SolrNet.Attributes;
using Withyun.Core.Enums;

namespace Domain.Models
{
    /// <summary>
    /// 博客
    /// </summary>
    public class Blog
    {
        [SolrUniqueKey("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name="标题")]
        [SolrField("title")]
        public string Title { get; set; }

        [Display(Name = "内容")]
        [SolrField("content")]
        public string Content { get; set; }
        [DataType(DataType.Html)]
        [Display(Name = "内容")]
        public string HtmlContent { get; set; }

        [SolrField("userid")]
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        [SolrField("username")]
        public string UserName { get; set; }

        [Display(Name = "发布时间")]
        [SolrField("timestamp")]
        public DateTime TimeStamp { get; set; }
        [SolrField("score")]
        public int Score { get; set; }
        public BlogStatus Status { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<VoteUp> VoteUps { get; set; }
        public virtual ICollection<VoteDown> VoteDowns { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Collection> Collections { get; set; }
        public virtual ICollection<Link> Links { get; set; }
        public virtual ICollection<ImageUrl> ImageUrls { get; set; } 
    }
}