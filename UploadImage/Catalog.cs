using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class Catalog
    {
        [JsonProperty("cid")]
        public int CategoryId { get; set; }
        [JsonProperty("catalogname")]
        public string CatalogName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
