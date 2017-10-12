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
    public class BlogController : ApiController
    {
        readonly BlogService _blogService = new BlogService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blogService.Dispose();
            }
            base.Dispose(disposing);
        }

        public IHttpActionResult Post(Blog blog)
        {
            blog.TimeStamp = DateTime.Now;
            blog.Status = BlogStatus.Publish;
            _blogService.Add(blog);
            return Ok(blog.Id);
        }
    }
}
