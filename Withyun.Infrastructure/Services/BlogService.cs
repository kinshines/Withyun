using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web;
using PagedList;
using System.IO;
using Domain.DAL;
using Domain.Helper;
using Domain.Models;
using Domain.ViewModels;
using UploadImage;
using Withyun.Infrastructure.Data;
using X.PagedList;
using Withyun.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Withyun.Core.Dtos;

namespace Domain.Services
{
    public class BlogService
    {
        readonly BlogContext _context;

        public BlogService(BlogContext context)
        {
            _context = context;
        }

        public IPagedList<Blog> GetPagedList(int pageNumber,int pageSize)
        {
            return _context.Blog.OrderBy(b => b.Id).ToPagedList(pageNumber, pageSize);
        }

        public IPagedList<Blog> GetPagedList(BlogStatus status, int userId, string blogTitle, int pageNumber, int pageSize=20)
        {
            var query = _context.Blog.Where(b => b.Status == status);
            query = query.Where(b => b.UserId == userId);
            if (!string.IsNullOrWhiteSpace(blogTitle))
            {
                query = query.Where(b => b.Title.Contains(blogTitle));
            }
            query = query.OrderByDescending(b => b.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public Blog Find(int id)
        {
            return _context.Blog.FirstOrDefault(x => x.Id == id);
        }

        public Blog FindDraftBlog(int userId)
        {
            return _context.Blog.FirstOrDefault(x => x.UserId == userId && x.Status == BlogStatus.Draft);
        }

        public Blog FindWithLinkReview(int id)
        {
            return
                _context.Blog.Where(b => b.Status == BlogStatus.Publish && b.Id == id)
                    .Include(b => b.Links)
                    .Include(b => b.Reviews)
                    .FirstOrDefault();
        }
        public Blog Find(int id,int userId)
        {
            return _context.Blog.Include(b=>b.Links).FirstOrDefault(x => x.Id == id && x.UserId == userId);
        }

        public Blog Add(Blog blog)
        {
            _context.Blog.Add(blog);
            _context.SaveChanges();
            SearchService.AddBlog(blog);
            return blog;
        }

        public Blog Update(Blog blog, string[] links, int[] linkIds, string[] linkDescriptions)
        {
            FixBlogLink(blog, links, linkIds, linkDescriptions);
            _context.SaveChanges();
            SearchService.AddBlog(blog);
            return blog;
        }

        private void FixBlogLink(Blog blog, string[] links, int[] linkIds, string[] linkDescriptions)
        {
            //数据库存储的链接
            List<Link> dblinks = blog.Links.ToList();
            int[] dblinkIds = dblinks.Select(x => x.Id).ToArray();
            if (linkIds == null)
            {
                linkIds = new int[0];
            }
            //待删除的链接
            var removeIds = dblinkIds.Except(linkIds).ToList();
            for (int i = 0; i < linkIds.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(links[i]))
                {
                    removeIds.Add(linkIds[i]);
                    continue;
                }
                var id = linkIds[i];
                var link = dblinks.Single(l => l.Id == id);
                if (!link.Url.Equals(links[i].Trim(), StringComparison.OrdinalIgnoreCase)||!link.Description.Equals(linkDescriptions[i].Trim(),StringComparison.OrdinalIgnoreCase))
                {
                    link.Url = links[i].Trim();
                    link.Description = linkDescriptions[i].Trim();
                    link.TimeStamp=DateTime.Now;
                    link.Invalide = false;
                    _context.LinkInvalid.RemoveRange(link.LinkInvalids.ToList());
                    link.LinkInvalids.Clear();
                }
            }

            //增加新链接
            for (int i = linkIds.Length; i < links.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(links[i]))
                {
                    continue;
                }
                var addLink = new Link()
                {
                    Blog = blog,
                    BlogId = blog.Id,
                    TimeStamp = DateTime.Now,
                    Url = links[i].Trim(),
                    Description = linkDescriptions[i].Trim()
                };
                blog.Links.Add(addLink);
            }

            var removeLinks = from l in blog.Links
                              where removeIds.Contains(l.Id)
                              select l;
            foreach (var link in removeLinks.ToList())
            {
                _context.LinkInvalid.RemoveRange(link.LinkInvalids.ToList());
                link.LinkInvalids.Clear();
                blog.Links.Remove(link);
                _context.Link.Remove(link);
            }
        }

        public void Delete(int id,int userId)
        {
            var blog = Find(id, userId);
            if(blog==null)
                return;
            blog.Status=BlogStatus.Delete;
            SearchService.DeleteById(blog.Id);
            _context.SaveChanges();
        }

        public OperationResult<ImageUrl> UploadImage(HttpPostedFileBase file,int blogId)
        {
            if (file.ContentLength > 0)
            {
                string fileName = DateTime.Now.ToString("yy/MM/dd/") + Guid.NewGuid().ToString("N") +
                                  Path.GetExtension(file.FileName);
                string diskPath = HttpContext.Current.Server.MapPath("~" + ConstValues.BlogImageDirectory + fileName);
                string diskDir = diskPath.Substring(0, diskPath.LastIndexOf("\\", StringComparison.Ordinal));
                if (!Directory.Exists(diskDir))
                {
                    Directory.CreateDirectory(diskDir);
                }
                file.SaveAs(diskPath);
                string yunUrl = UploadUtility.UploadLocalFile(diskPath);
                var imageUrl = new ImageUrl()
                {
                    BlogId = blogId,
                    Url = fileName.Replace('/','\\'),
                    TimeStamp = DateTime.Now,
                    YunUrl = yunUrl,
                    ImageStatus = string.IsNullOrEmpty(yunUrl) ? ImageStatus.Local : ImageStatus.Yun
                };

                _context.ImageUrl.Add(imageUrl);
                _context.SaveChanges();
                if (string.IsNullOrEmpty(yunUrl))
                {
                    imageUrl.Url = "http://www.withyun.com" + ConstValues.BlogImageDirectory + fileName;
                }

                return new OperationResult<ImageUrl>(true, imageUrl);
            }
            return new OperationResult<ImageUrl>(false, null);
        }

        public OperationResult<PanelStatus> GetPanelStatus(int userId, int blogId)
        {
            var status = new PanelStatus()
            {
                HasVoteUp = _context.VoteUp.Count(x => x.BlogId == blogId && x.UserId == userId) > 0,
                HasVoteDown = _context.VoteDown.Count(x => x.BlogId == blogId && x.UserId == userId) > 0,
                HasCollection = _context.Collection.Count(x => x.BlogId == blogId && x.UserId == userId) > 0
            };
            return new OperationResult<PanelStatus>(true, status);
        }

        public void SaveRecomment(HttpPostedFileBase file, string title, int blogId, int[] selectedCategory)
        {
            string fileName = "";
            string yunUrl = "";
            if (file.ContentLength > 0)
            {
                fileName = DateTime.Now.ToString("yy/MM/dd/") + Guid.NewGuid().ToString("N") +
                           Path.GetExtension(file.FileName);
                string diskPath = HttpContext.Current.Server.MapPath("~" + ConstValues.CoverImageDirectory + fileName);
                var image = ImgHandler.ZoomPicture(Image.FromStream(file.InputStream), 200, 110);
                string diskDir = diskPath.Substring(0, diskPath.LastIndexOf("\\", StringComparison.Ordinal));
                if (!Directory.Exists(diskDir))
                {
                    Directory.CreateDirectory(diskDir);
                }
                image.Save(diskPath);
                image.Dispose();
                yunUrl = UploadUtility.UploadLocalFile(diskPath);
            }
            foreach (int category in selectedCategory)
            {
                var recomment = new Recomment()
                {
                    BlogId = blogId,
                    CoverName = fileName.Replace('/', '\\'),
                    Title = title,
                    TimeStamp = DateTime.Now,
                    Category = (RecommentCategory)category,
                    YunUrl = yunUrl,
                    ImageStatus = string.IsNullOrEmpty(yunUrl) ? ImageStatus.Local : ImageStatus.Yun
                };

                _context.Recomment.Add(recomment);
            }
            _context.SaveChanges();
        }
    }
}