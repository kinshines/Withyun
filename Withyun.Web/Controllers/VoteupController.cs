using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNet.Identity;

namespace Withyun.Controllers
{
    [Authorize]
    public class VoteupController : Controller
    {

        readonly VoteUpService _voteupService=new VoteUpService();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _voteupService.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult Create(int blogId,string blogTitle,int distributor)
        {
            var userId=User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            var userName = User.Identity.GetUserName();
            return Json(_voteupService.AddOrDelete(blogId, blogTitle,userId,userName,distributor));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = User.Identity.GetUserId<int>();
            int count = _voteupService.CountByBlogIdAndUserId(blogId, userId);
            return Json(count, JsonRequestBehavior.AllowGet);
        }
	}
}