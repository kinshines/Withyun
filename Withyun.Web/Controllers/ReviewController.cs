using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        readonly ReviewService _reviewService;
        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review review)
        {
            if (ModelState.IsValid)
            {
                review.UserId = GetUserId();
                if (review.UserId == 0)
                {
                    return Json(new OperationResult<string>(false, "尚未登录"));
                }
                int count = _reviewService.CountByTime(review, DateTime.Now.AddHours(-1));
                if (count >= 3)
                {
                    return Json(new OperationResult<string>(false, "1小时内最多评论3次"));
                }
                review.UserName = User.Identity.Name;
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

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}