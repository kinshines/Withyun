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
    public class VotedownController : Controller
    {
        readonly VoteDownService _votedownService;
        public VotedownController(VoteDownService voteDownService)
        {
            _votedownService = voteDownService;
        }

        //
        [HttpPost]
        public ActionResult Create(int blogId)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(_votedownService.AddOrDelete(blogId, userId));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = GetUserId();
            int count = _votedownService.CountByBlogIdAndUserId(blogId, userId);
            return Json(count);
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}