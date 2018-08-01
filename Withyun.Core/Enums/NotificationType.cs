using System;
using System.Collections.Generic;
using System.Text;

namespace Withyun.Core.Enums
{
    public enum NotificationType : byte
    {
        Follow,
        LinkInvalid,
        Review,
        VoteUp,
        Collect,
        ReportAdvertise,
        ReportFeedbackRight,
        ReportFeedbackWrong,
        广告等垃圾信息,
        不友善内容,
        违反法律法规的内容,
        不宜公开讨论的政治内容,
        其他内容
    }

    public enum ReportType : byte
    {
        广告等垃圾信息,
        不友善内容,
        违反法律法规的内容,
        不宜公开讨论的政治内容,
        其他内容
    }
}
