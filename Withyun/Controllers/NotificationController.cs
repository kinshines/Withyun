using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Domain.Models;
using Domain.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        readonly NotificationService _notificationService=new NotificationService();
        //
        // GET: /Notification/
        public ActionResult Index(int? page)
        {
            int userId = User.Identity.GetUserId<int>();
            int pageNumber = page ?? 1;
            return View(_notificationService.GetPagedList(userId, pageNumber));
        }

        public ActionResult GetTopList()
        {
            int userId = User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false), JsonRequestBehavior.AllowGet);
            }
            return Json(new OperationResult<List<Notification>>(true, _notificationService.GetTopList(userId, 10)),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnReadCount()
        {
            int userId = User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false), JsonRequestBehavior.AllowGet);
            }
            return Json(new OperationResult<int>(true, _notificationService.UnReadCount(userId)),
                JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notificationService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}