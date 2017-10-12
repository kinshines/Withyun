using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Services;
using Domain.Models;
using Microsoft.AspNet.Identity;

namespace Withyun.Controllers
{
    [Authorize]
    public class LinkInvalidController : Controller
    {
        readonly LinkInvalidService _invalidService=new LinkInvalidService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _invalidService.Dispose();
            }
            base.Dispose(disposing);
        }

        //
        [HttpPost]
        public ActionResult Create(int linkId)
        {
            var userId = User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(_invalidService.Add(linkId, userId));
        }

        public ActionResult Exist(int linkId)
        {
            var userId = User.Identity.GetUserId<int>();
            int count = _invalidService.CountByLinkIdAndUserId(linkId, userId);
            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Index(int? page)
        {
            int pageNumber = (page ?? 1);
            return View(_invalidService.GetPagedList(pageNumber));
        }
        [Authorize(Roles = "admin")]
        public ActionResult Confirm(int id)
        {
            var linkInvalide = _invalidService.GetLinkInvalid(id);
            return View(linkInvalide);
        }

        [HttpPost, ActionName("Confirm")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Confirmed(int id)
        {
            _invalidService.ConfirmInvalide(id);
            return RedirectToAction("Index");
        }
	}
}