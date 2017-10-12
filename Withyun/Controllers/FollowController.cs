using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Domain.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        readonly FollowService _followService=new FollowService();
        //
        // GET: /Follow/
        public ActionResult Index(string blogTitle,int? page)
        {
            var userId = User.Identity.GetUserId<int>();
            ViewBag.BlogTitle = blogTitle;
            int pageNumber = page ?? 1;
            return View(_followService.GetBlogPagedList(userId, blogTitle, pageNumber));
        }

        public ActionResult Add(int distributor)
        {
            var userId = User.Identity.GetUserId<int>();
            var userName = User.Identity.GetUserName();
            return Json(_followService.Add(distributor, userId,userName));
        }

        public ActionResult Delete(int distributor)
        {
            var userId = User.Identity.GetUserId<int>();
            return Json(_followService.Delete(distributor, userId));
        }

        [AllowAnonymous]
        public new ActionResult Profile(int id)
        {
            ViewBag.userId = id;
            ViewBag.userName = _followService.GetDistributorName(id);
            var userId = User.Identity.GetUserId<int>();
            ViewBag.followed = userId != 0 && _followService.Exist(id, userId);
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _followService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}