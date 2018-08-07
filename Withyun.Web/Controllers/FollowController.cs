using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Withyun.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Withyun.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        readonly FollowService _followService;
        public FollowController(FollowService followService)
        {
            _followService = followService;
        }
        //
        // GET: /Follow/
        public ActionResult Index(string blogTitle,int? page)
        {
            var userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;
            int pageNumber = page ?? 1;
            return View(_followService.GetBlogPagedList(userId, blogTitle, pageNumber));
        }

        public ActionResult Add(int distributor)
        {
            var userId = GetUserId();
            var userName = User.Identity.Name;
            return Json(_followService.Add(distributor, userId,userName));
        }

        public ActionResult Delete(int distributor)
        {
            var userId = GetUserId();
            return Json(_followService.Delete(distributor, userId));
        }

        [AllowAnonymous]
        public new ActionResult Profile(int id)
        {
            ViewBag.userId = id;
            ViewBag.userName = _followService.GetDistributorName(id);
            var userId = GetUserId();
            ViewBag.followed = userId != 0 && _followService.Exist(id, userId);
            return PartialView();
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}