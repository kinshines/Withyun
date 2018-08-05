using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNet.Identity;

namespace Withyun.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly CollectionService _collectionService=new CollectionService();

        //
        [HttpPost]
        public ActionResult Create(int blogId,int distributor,string blogTitle )
        {
            var userId = User.Identity.GetUserId<int>();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            var userName = User.Identity.Name;
            return Json(_collectionService.AddOrDelete(blogId, userId,userName,distributor,blogTitle));
        }

        public ActionResult Exist(int blogId)
        {
            var userId = User.Identity.GetUserId<int>();
            int count = _collectionService.CountByBlogIdAndUserId(blogId,userId);
            return Json(count, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(string blogTitle,int? page)
        {
            var userId = User.Identity.GetUserId<int>();
            ViewBag.BlogTitle = blogTitle;
            int pageNumber = (page ?? 1);
            return View(_collectionService.GetPagedList(userId, blogTitle, pageNumber));
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId<int>();
            return Json(_collectionService.Delete(id, userId));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _collectionService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}