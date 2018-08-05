using System;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;
using NLogUtility;

namespace Withyun.Controllers
{
    [Authorize(Roles = "admin")]
    public class RecommentController : Controller
    {
        readonly RecommentService _recommentService=new RecommentService();
        //
        // GET: /Recomment/

        public ActionResult Manage(RecommentCategory category, string recommentTitle, int? page)
        {
            recommentTitle = recommentTitle.Trim();
            int pageNumber = page ?? 1;
            ViewBag.RecommentTitle = recommentTitle;
            ViewBag.RecommentCategory = category;
            return View(_recommentService.GetPagedList(category, recommentTitle, pageNumber));
        }

        [AllowAnonymous]
        public ActionResult Index(RecommentCategory id, int? page)
        {
            int pageNumber = page ?? 1;
            ViewBag.Category = (int) id;
            ViewBag.Title = id.ToString();
            return View(_recommentService.GetPagedList(id, pageNumber));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string title, int blogId, int[] selectedCategory)
        {
            ViewBag.Message = "invalide";
            if (ModelState.IsValid)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["fileField"];
                    _recommentService.SaveRecomment(file, title, blogId, selectedCategory);
                    ViewBag.Message = "success";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "fail";
                    Logger.Error(ex, "首页推荐保存错误：");
                }
            }
            return View();
        }

        [ChildActionOnly]
        [AllowAnonymous]
        //[OutputCache(Duration = 1*60*60,VaryByParam = "id")]
        public ActionResult Recomment(RecommentCategory id)
        {
            var list = _recommentService.GetRecommentsByCategory(id);
            ViewBag.Title = id.ToString();
            ViewBag.Icon = _recommentService.GetIconByCategory(id);
            return PartialView(list);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _recommentService.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult Top(int id)
        {
            return _recommentService.Top(id) ? Json("ok") : Json("error");
        }

        [HttpPost]
        public ActionResult UnTop(int id)
        {
            return _recommentService.UnTop(id) ? Json("ok") : Json("error");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            return _recommentService.Delete(id) ? Json("ok") : Json("error");
        }
    }
}