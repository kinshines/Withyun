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
    public class RecommentController : ControllerBase
    {
        readonly RecommentService _recommentService;
        public RecommentController(RecommentService recommentService)
        {
            _recommentService = recommentService;
        }
        public IActionResult Post(Recomment recomment)
        {
            recomment.TimeStamp = DateTime.Now;
            _recommentService.Add(recomment);
            return Ok(recomment.Id);
        }
    }
}