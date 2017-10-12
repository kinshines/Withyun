using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class PictureDetail:Picture
    {
        [JsonProperty("addtime")]
        public int AddTime { get; set; }
        [JsonProperty("uid")]
        public int UserId { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("aid")]
        public int AlbumId { get; set; }
        [JsonProperty("albumname")]
        public string AlbumName { get; set; }
        [JsonProperty("catalog")]
        public List<Catalog> Catalogs { get; set; } 
    }
}
