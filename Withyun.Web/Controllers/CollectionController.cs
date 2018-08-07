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
    public class CollectionController : Controller
    {
        private readonly CollectionService _collectionService;
        public CollectionController(CollectionService collectionService)
        {
            _collectionService = collectionService;
        }
        //
        [HttpPost]
        public ActionResult Create(int blogId,int distributor,string blogTitle )
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            var userName = User.Identity.Name;
            return Json(_collectionService.AddOrDelete(blogId, userId,userName,distributor,blogTitle));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = GetUserId();
            int count = _collectionService.CountByBlogIdAndUserId(blogId,userId);
            return Json(count);
        }

        public ActionResult Index(string blogTitle,int? page)
        {
            var userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;
            int pageNumber = (page ?? 1);
            return View(_collectionService.GetPagedList(userId, blogTitle, pageNumber));
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var userId = GetUserId();
            return Json(_collectionService.Delete(id, userId));
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}