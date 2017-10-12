using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Domain.Models;
using Domain.Services;

namespace Withyun.Api
{
    public class ImageUrlController : ApiController
    {
        readonly ImageUrlService _imageService = new ImageUrlService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imageService.Dispose();
            }
            base.Dispose(disposing);
        }

        public IHttpActionResult Post(ImageUrl image)
        {
            image.TimeStamp=DateTime.Now;
            _imageService.Add(image);
            return Ok(image.Id);
        }
    }
}
