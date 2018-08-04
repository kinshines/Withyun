using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UploadImage
{
    public class ErrorResult
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("info")]
        public string Info { get; set; }
    }
}
