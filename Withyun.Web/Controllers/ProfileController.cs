using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Services;

namespace Withyun.Controllers
{
    public class ProfileController : Controller
    {
        readonly BlogService _blogService;

        public ProfileController(BlogService blogService)
        {
            _blogService = blogService;
        }
        //
        // GET: /Profile/
        public ActionResult Index(int id,string blogTitle,int? page)
        {
            int pageNumber = page ?? 1;
            ViewBag.blogTitle = blogTitle;
            ViewBag.userId = id;
            return View(_blogService.GetPagedList(BlogStatus.Publish, id, blogTitle, pageNumber));
        }
    }
}