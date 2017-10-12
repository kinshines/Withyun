using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Withyun.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        readonly ReviewService _reviewService=new ReviewService();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reviewService.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review review)
        {
            if (ModelState.IsValid)
            {
                review.UserId = User.Identity.GetUserId<int>();
                if (review.UserId == 0)
                {
                    return Json(new OperationResult<string>(false, "尚未登录"));
                }
                int count = _reviewService.CountByTime(review, DateTime.Now.AddHours(-1));
                if (count >= 3)
                {
                    return Json(new OperationResult<string>(false, "1小时内最多评论3次"));
                }
                review.UserName = User.Identity.GetUserName();
                _reviewService.Add(review);
                var formattedData = new
                {
                    ImgUrl = Url.Action("GetAvatar", "Manage", new { id = review.UserId }),
                    Time = review.TimeStamp.ToString("yyyy-MM-dd HH:mm"),
                    UserName=review.UserName,
                    UserId=review.UserId,
                    Content = review.Content
                };
                return Json(new OperationResult<object>(true, formattedData));
            }
            return Json(new OperationResult<string>(false, "评论失败"));
        }
	}
}