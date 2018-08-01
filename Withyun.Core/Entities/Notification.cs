using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Withyun.Core.Enums;

namespace Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public NotificationType NotificationType { get; set; }

        [NotMapped]
        [DataType(DataType.Html)]
        public string Title {
            get
            {
                string profileUrl = "";
                string blogUrl = "";
                string blogEditUrl = "";
                if (SourceId.HasValue)
                {
                    profileUrl = "/Profile/Index/" + SourceId;
                }
                if (BlogId.HasValue)
                {
                    blogUrl = "/Blog/Details/" + BlogId;
                    blogEditUrl = "/Blog/Edit/" + BlogId;
                }
                switch (NotificationType)
                {
                    case NotificationType.Collect:
                        return "<a href=\"" + profileUrl + "\" target=\"_blank\">" + SourceName +
                               "</a> 收藏了你的分享 <a href=\"" + blogUrl + "\" target=\"_blank\">" + BlogTitle + "</a>";
                    case NotificationType.Follow:
                        return "<a href=\"" + profileUrl + "\" target=\"_blank\">" + SourceName + "</a> 关注了你";
                    case NotificationType.LinkInvalid:
                        return "你的分享 <a href=\"" + blogEditUrl + "\" target=\"_blank\">" + BlogTitle + "</a> 链接已失效，请及时修复链接";
                    case NotificationType.Review:
                        return "<a href=\"" + profileUrl + "\" target=\"_blank\">" + SourceName +
                               "</a> 评论了你的分享 <a href=\"" + blogUrl + "\" target=\"_blank\">" + BlogTitle + "</a>";
                    case NotificationType.VoteUp:
                        return "<a href=\"" + profileUrl + "\" target=\"_blank\">" + SourceName +
                               "</a> 觉得你的分享 <a href=\"" + blogUrl + "\" target=\"_blank\">" + BlogTitle + "</a> 很赞";
                    case NotificationType.ReportFeedbackRight:
                        return "我们已确认您对于分享：" + BlogTitle + " 的举报，并作出处理，感谢您的互联网环境的优化作出的贡献";
                    case NotificationType.ReportFeedbackWrong:
                        return "我们已经核实了您对于分享：<a href=\"" + blogUrl + "\" target=\"_blank\">" + BlogTitle +
                               "</a> 的举报，并认为并无违规，感谢您的互联网环境的优化作出的贡献";
                    case NotificationType.广告等垃圾信息:
                    case NotificationType.不友善内容:
                    case NotificationType.违反法律法规的内容:
                    case NotificationType.不宜公开讨论的政治内容:
                    case NotificationType.其他内容:
                        return "你的分享 <a href=\"" + blogEditUrl + "\" target=\"_blank\">" + BlogTitle + "</a> 涉嫌" +
                               NotificationType + "，请及时更正";
                    default:
                        return "";
                }
            }
        }

        [StringLength(50)]
        public string BlogTitle { get; set; }
        public int? BlogId { get; set; }
        public int? LinkId { get; set; }
        [StringLength(10)]
        public string SourceName { get; set; }
        public int? SourceId { get; set; }
        public bool Read { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    
}