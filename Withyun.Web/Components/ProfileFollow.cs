using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Withyun.Infrastructure.Services;

namespace Withyun.Web.Components
{
    public class ProfileFollow : ViewComponent
    {
        readonly FollowService _followService;
        public ProfileFollow(FollowService followService)
        {
            _followService = followService;
        }
        public IViewComponentResult Invoke(int id)
        {
            ViewBag.userId = id;
            ViewBag.userName = _followService.GetDistributorName(id);
            var userId = GetUserId();
            ViewBag.followed = userId != 0 && _followService.Exist(id, userId);
            return View();
        }
        private int GetUserId()
        {
            return Convert.ToInt32(((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
