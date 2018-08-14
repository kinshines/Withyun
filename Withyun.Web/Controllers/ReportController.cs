using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;
        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Report report)
        {
            if (ModelState.IsValid)
            {
                var userId = GetUserId();
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

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}