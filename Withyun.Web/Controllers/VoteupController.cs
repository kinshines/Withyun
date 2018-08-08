using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Dtos;
using Withyun.Infrastructure.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class VoteupController : Controller
    {

        readonly VoteUpService _voteupService;
        public VoteupController(VoteUpService voteUpService)
        {
            _voteupService = voteUpService;
        }

        [HttpPost]
        public ActionResult Create(int blogId,string blogTitle,int distributor)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            var userName = User.Identity.Name;
            return Json(_voteupService.AddOrDelete(blogId, blogTitle,userId,userName,distributor));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = GetUserId();
            int count = _voteupService.CountByBlogIdAndUserId(blogId, userId);
            return Json(count);
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}