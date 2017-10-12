﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Domain.Models
{
    public class ImageUrl
    {
        public int Id { get; set; }
        [StringLength(200)]
        public string Url { get; set; }
        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
        public DateTime TimeStamp { get; set; }
        [StringLength(200)]
        public string YunUrl { get; set; }
        public ImageStatus ImageStatus { get; set; }
    }

    public enum ImageStatus : byte
    {
        Local=0,
        Pending=1,
        Uploading=2,
        Yun=3
    }
}