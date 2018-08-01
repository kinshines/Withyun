using System;
using System.Collections.Generic;
using System.Text;

namespace Withyun.Core.Enums
{
    public enum ImageStatus : byte
    {
        Local = 0,
        Pending = 1,
        Uploading = 2,
        Yun = 3
    }
}
