using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Services;
using Microsoft.AspNet.Identity;

namespace Withyun.Controllers
{
    [Authorize]
    public class VotedownController : Controller
    {
        readonly VoteDownService _votedownService=new VoteDownService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _votedownService.Dispose();
            }
            base.Dispose(disposing);
        }

        //
        [HttpPost]
        public ActionResult Create(int blogId)
        {
            var userId = User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(_votedownService.AddOrDelete(blogId, userId));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = User.Identity.GetUserId<int>();
            int count = _votedownService.CountByBlogIdAndUserId(blogId, userId);
            return Json(count, JsonRequestBehavior.AllowGet);
        }
	}
}