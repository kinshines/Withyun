using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Models;
using Domain.Services;

namespace Withyun.Controllers
{
    public class ProfileController : Controller
    {
        readonly BlogService _blogService=new BlogService();
        //
        // GET: /Profile/
        public ActionResult Index(int id,string blogTitle,int? page)
        {
            int pageNumber = page ?? 1;
            ViewBag.blogTitle = blogTitle;
            ViewBag.userId = id;
            return View(_blogService.GetPagedList(BlogStatus.Publish, id, blogTitle, pageNumber));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blogService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}