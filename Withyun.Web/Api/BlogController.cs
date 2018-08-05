using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Services;

namespace Withyun.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        readonly BlogService _blogService;
        public BlogController(BlogService blogService)
        {
            _blogService = blogService;
        }

        public IActionResult Post(Blog blog)
        {
            blog.TimeStamp = DateTime.Now;
            blog.Status = BlogStatus.Publish;
            _blogService.Add(blog);
            return Ok(blog.Id);
        }
    }
}