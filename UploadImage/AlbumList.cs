using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class AlbumList
    {
        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("album")]
        public List<Album> Albums { get; set; } 
    }
}
