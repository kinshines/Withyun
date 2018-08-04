using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class AllPictureList
    {
        [JsonProperty("pages")]
        public int Pages { get; set; }
        [JsonProperty("pic")]
        public List<Picture> Pictures { get; set; } 
    }
}
