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
    public class RecommentController : ApiController
    {
        readonly RecommentService _recommentService = new RecommentService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _recommentService.Dispose();
            }
            base.Dispose(disposing);
        }

        public IHttpActionResult Post(Recomment recomment)
        {
            recomment.TimeStamp=DateTime.Now;
            _recommentService.Add(recomment);
            return Ok(recomment.Id);
        }
    }
}
