using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class Picture
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("linkurl")]
        public string LinkUrl { get; set; }
        [JsonProperty("showurl")]
        public string ShowUrl { get; set; }
        [JsonProperty("ext")]
        public string Ext { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("findurl")]
        public string FindUrl { get; set; }
        [JsonProperty("recommend")]
        public int Recommend { get; set; }
    }
}
