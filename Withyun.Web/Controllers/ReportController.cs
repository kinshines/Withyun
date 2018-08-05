using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNet.Identity;

namespace Withyun.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService=new ReportService();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reportService.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Report report)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId == 0)
                {
                    return Json(new OperationResult(false));
                }
                report.TimeStamp=DateTime.Now;
                report.UserId = userId;
                _reportService.Add(report);
                return Json(new OperationResult(true));
            }
            return Json(new OperationResult(false));
        }

        [Authorize(Roles = "admin")]
        public ActionResult Index(int? page)
        {
            int pageNumber = (page ?? 1);
            return View(_reportService.GetPagedList(pageNumber));
        }

        [Authorize(Roles = "admin")]
        public ActionResult Confirm(int id)
        {
            var report = _reportService.GetReport(id);
            return View(report);
        }

        [HttpPost, ActionName("Confirm")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult Confirmed(int id)
        {
            _reportService.ConfirmReport(id);
            return RedirectToAction("Index");
        }
	}
}