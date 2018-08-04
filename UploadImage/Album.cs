using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class Album
    {
        [JsonProperty("aid")]
        public int AlbumId { get; set; }
        [JsonProperty("albumname")]
        public string AlbumName { get; set; }
        [JsonProperty("num")]
        public int Num { get; set; }
        [JsonProperty("pic")]
        public List<ShortPicture> Pictures { get; set; } 
    }
}
