using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Services;

namespace Withyun.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUrlController : ControllerBase
    {
        readonly ImageUrlService _imageService;
        public ImageUrlController(ImageUrlService imageService)
        {
            _imageService = imageService;
        }
        public IActionResult Post(ImageUrl image)
        {
            image.TimeStamp = DateTime.Now;
            _imageService.Add(image);
            return Ok(image.Id);
        }
    }
}