using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Services;

namespace Withyun.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        readonly BlogService _blogService;
        readonly SearchService _searchService;
        readonly HtmlSanitizer _sanitizer;
        public BlogController(BlogService blogService,SearchService searchService,HtmlSanitizer sanitizer)
        {
            _blogService = blogService;
            _searchService = searchService;
            _sanitizer = sanitizer;
        }

        // GET: /Blog/
        [AllowAnonymous]
        public ActionResult Index(string wd,int? page)
        {
            if (string.IsNullOrWhiteSpace(wd))
            {
                Redirect(Request.Path.ToString());
            }
            int pageCount;
            var result = _searchService.Query(wd, page, out pageCount);
            ViewBag.pageCount = pageCount;
            ViewBag.pageNumber = (page ?? 1);
            ViewBag.wd = wd;
            return View(result);
        }

        // GET: /Blog/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            Blog blog = _blogService.FindWithLinkReview(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: /Blog/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Blog blogView, string[] link, int[] linkId, string[] linkDescription)
        {
            if (ModelState.IsValid)
            {
                var userId = GetUserId();
                var blog = _blogService.Find(blogView.Id, userId);
                blog.Title = blogView.Title;
                blog.TimeStamp = DateTime.Now;
                blog.HtmlContent = _sanitizer.Sanitize(blogView.HtmlContent);
                blog.Content = blogView.Content;
                if (blogView.Status == BlogStatus.Report||blogView.Status==BlogStatus.Verify)
                {
                    blog.Status = BlogStatus.Verify;
                }
                else
                {
                    blog.Status = BlogStatus.Publish;
                }
                blog = _blogService.Update(blog, link, linkId, linkDescription);
                return RedirectToAction("PublishSuccess", new { id = blog.Id });
            }
            blogView.Links = new Collection<Link>();
            return View(blogView);
        }

        // GET: /Blog/Edit/5
        public ActionResult Edit(int? id)
        {
            var userId = GetUserId();
            var userName = User.Identity.Name;
            var blog=new Blog();
            if (id.HasValue)
            {
                blog = _blogService.Find(id.Value, userId);
            }
            if (id==null||blog == null)
            {
                blog = _blogService.FindDraftBlog(userId);
                if (blog == null)
                {
                    blog = new Blog()
                    {
                        UserId = userId,
                        UserName = userName,
                        Title = "T",
                        Content = string.Empty,
                        HtmlContent = string.Empty,
                        TimeStamp = DateTime.Now,
                        Status = BlogStatus.Draft,
                        Links = new Collection<Link>()
                    };
                    _blogService.Add(blog);
                }
                blog.Title = string.Empty;
            }
            return View(blog);
        }

        [Authorize(Roles = "admin")]
        public ActionResult EditRecomment(int? id)
        {
            var userId = GetUserId();
            var userName = User.Identity.Name;
            var blog = new Blog();
            if (id.HasValue)
            {
                blog = _blogService.Find(id.Value, userId);
            }
            if (id == null || blog == null)
            {
                blog = _blogService.FindDraftBlog(userId);
                if (blog == null)
                {
                    blog = new Blog()
                    {
                        UserId = userId,
                        UserName = userName,
                        Title = "T",
                        Content = string.Empty,
                        HtmlContent = string.Empty,
                        TimeStamp = DateTime.Now,
                        Status = BlogStatus.Draft,
                        Links = new Collection<Link>()
                    };
                    _blogService.Add(blog);
                }
                blog.Title = string.Empty;
            }
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult EditRecomment(Blog blogView, string[] link, int[] linkId, string recommentTitle, int[] selectedCategory, string[] linkDescription, IFormFile fileField)
        {
            if (ModelState.IsValid)
            {
                var userId = GetUserId();
                var blog = _blogService.Find(blogView.Id, userId);
                blog.Title = blogView.Title;
                blog.TimeStamp = DateTime.Now;
                blog.HtmlContent = _sanitizer.Sanitize(blogView.HtmlContent);
                blog.Content = blogView.Content;
                if (blogView.Status == BlogStatus.Report || blogView.Status == BlogStatus.Verify)
                {
                    blog.Status = BlogStatus.Verify;
                }
                else
                {
                    blog.Status = BlogStatus.Publish;
                }
                blog = _blogService.Update(blog, link, linkId, linkDescription);
                _blogService.SaveRecomment(fileField, recommentTitle, blog.Id, selectedCategory);
                return RedirectToAction("PublishSuccess", new { id = blog.Id });
            }
            blogView.Links = new Collection<Link>();
            return View(blogView);
        }

        // GET: /Blog/Delete/5
        public ActionResult Delete(int id)
        {
            var userId = GetUserId();
            Blog blog = _blogService.Find(id, userId);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: /Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            _blogService.Delete(id, userId);
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public ActionResult FileUpload(IFormFile file,int blogId)
        {
            return Json(_blogService.UploadImage(file,blogId));
        }
        public ActionResult PublishSuccess(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult CorrectSuccess(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        //我的分享
        public ActionResult Manage(string blogTitle, int? page)
        {
            int userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;
            int pageNumber = (page ?? 1);
            return View(_blogService.GetPagedList(BlogStatus.Publish, userId, blogTitle, pageNumber));
        }

        //内容违规
        public ActionResult Report(string blogTitle, int? page)
        {
            int userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;

            int pageNumber = (page ?? 1);
            return View(_blogService.GetPagedList(BlogStatus.Report, userId, blogTitle, pageNumber));
        }

        //待审核
        public ActionResult Verify(string blogTitle, int? page)
        {
            int userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;

            int pageNumber = (page ?? 1);
            return View(_blogService.GetPagedList(BlogStatus.Verify, userId, blogTitle, pageNumber));
        }

        //链接无效
        public ActionResult Invalide(string blogTitle, int? page)
        {
            int userId = GetUserId();
            ViewBag.BlogTitle = blogTitle;

            int pageNumber = (page ?? 1);
            return View(_blogService.GetPagedList(BlogStatus.LinkInvalid, userId, blogTitle, pageNumber));
        }

        public ActionResult GetPanelStatus(int blogId)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new OperationResult(false));
            }
            else
            {
                return Json(_blogService.GetPanelStatus(userId, blogId));
            }
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
