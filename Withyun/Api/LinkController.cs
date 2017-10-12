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
    public class LinkController : ApiController
    {
        readonly LinkService _linkService=new LinkService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _linkService.Dispose();
            }
            base.Dispose(disposing);
        }

        public IHttpActionResult Post(int blogId, List<Link> linkList)
        {
            _linkService.AddLinkList(blogId, linkList);
            return Ok();
        }
    }
}
