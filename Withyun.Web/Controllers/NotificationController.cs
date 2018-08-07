using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Withyun.Infrastructure.Services;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Withyun.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        readonly NotificationService _notificationService;
        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        //
        // GET: /Notification/
        public ActionResult Index(int? page)
        {
            int userId = GetUserId();
            int pageNumber = page ?? 1;
            return View(_notificationService.GetPagedList(userId, pageNumber));
        }

        public ActionResult GetTopList()
        {
            int userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(new OperationResult<List<Notification>>(true, _notificationService.GetTopList(userId, 10)));
        }

        public ActionResult GetUnReadCount()
        {
            int userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(new OperationResult<int>(true, _notificationService.UnReadCount(userId)));
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}