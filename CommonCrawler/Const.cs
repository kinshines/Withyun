using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonCrawler
{
    public class Const
    {
        public const string BlogFileDirectory = @"E:\Withyun\images\blog\";
        public const string CoverFileDirectory = @"E:\Withyun\images\cover\";
        public const string LogConnection = "LogConnection";
    }

    public struct TableName
    {
        public const string ImageUrl = "ImageUrl";
        public const string Recomment = "Recomment";
        public const string ResourceRecord = "ResourceRecord";
        public const string FetchRecord = "FetchRecord";
    }
}
