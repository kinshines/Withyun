using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Domain.Models
{
    public class Recomment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        public int BlogId { get; set; }
        public RecommentCategory Category { get; set; }
        [StringLength(200)]
        public string CoverName { get; set; }

        [NotMapped]
        public string CoverUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(YunUrl))
                    return YunUrl;
                if (string.IsNullOrEmpty(CoverName))
                {
                    return "http://www.withyun.com/images/cover/noimage.png";
                }
                return "http://www.withyun.com" + ConstValues.CoverImageDirectory + CoverName.Replace('\\', '/');
            }
        }
        public DateTime TimeStamp { get; set; }
        public virtual Blog Blog { get; set; }
        [StringLength(200)]
        public string YunUrl { get; set; }
        public ImageStatus ImageStatus { get; set; }
        public bool Top { get; set; }

    }

    public enum RecommentCategory : byte
    {
        剧集=0,
        电影=1,
        美剧=2,
        韩剧=3,
        日剧=4,
        动漫=5,
        音乐=6,
        游戏=7,
        图书=8,
        综艺=9,
        软件=10,
        资料=11,
        教育=12
    }
}