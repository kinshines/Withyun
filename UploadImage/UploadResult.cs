using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class UploadResult
    {
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("ubburl")]
        public string UbbUrl { get; set; }
        [JsonProperty("htmlurl")]
        public string HtmlUrl { get; set; }
        [JsonProperty("linkurl")]
        public string LinkUrl { get; set; }

    }
}
