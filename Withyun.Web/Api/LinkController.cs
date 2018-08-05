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
    public class LinkController : ControllerBase
    {
        readonly LinkService _linkService;
        public LinkController(LinkService linkService)
        {
            _linkService = linkService;
        }

        public IActionResult Post(int blogId, List<Link> linkList)
        {
            _linkService.AddLinkList(blogId, linkList);
            return Ok();
        }
    }
}