using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Services;
using Withyun.Infrastructure.Utility;

namespace Withyun.Controllers
{
    [Authorize(Roles = "admin")]
    public class RecommentController : Controller
    {
        readonly RecommentService _recommentService;

        public RecommentController(RecommentService recommentService)
        {
            _recommentService = recommentService;
        }
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
        public ActionResult Create(string title, int blogId, int[] selectedCategory, IFormFile fileField)
        {
            ViewBag.Message = "invalide";
            if (ModelState.IsValid)
            {
                try
                {
                    _recommentService.SaveRecomment(fileField, title, blogId, selectedCategory);
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

        [AllowAnonymous]
        //[OutputCache(Duration = 1*60*60,VaryByParam = "id")]
        public ActionResult Recomment(RecommentCategory id)
        {
            var list = _recommentService.GetRecommentsByCategory(id);
            ViewBag.Title = id.ToString();
            ViewBag.Icon = _recommentService.GetIconByCategory(id);
            return PartialView(list);
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