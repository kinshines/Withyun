using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Withyun.Infrastructure.Services;
using Withyun.Core.Dtos;
using System.Security.Claims;

namespace Withyun.Controllers
{
    [Authorize]
    public class LinkInvalidController : Controller
    {
        readonly LinkInvalidService _invalidService;
        public LinkInvalidController(LinkInvalidService invalidService)
        {
            _invalidService = invalidService;
        }

        //
        [HttpPost]
        public ActionResult Create(int linkId)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            return Json(_invalidService.Add(linkId, userId));
        }

        public ActionResult Exist(int linkId)
        {
            var userId = GetUserId();
            int count = _invalidService.CountByLinkIdAndUserId(linkId, userId);
            return Json(count);
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

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}