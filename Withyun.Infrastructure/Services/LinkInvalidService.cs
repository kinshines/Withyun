using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Withyun.Core.Dtos;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Withyun.Infrastructure.Services
{
    public class LinkInvalidService
    {
        readonly BlogContext _context;
        public LinkInvalidService(BlogContext context)
        {
            _context = context;
        }
        public void Dispose()
        {
            _context.Dispose();
        }
        public LinkInvalid FindByBlogIdAndUserId(int linkId, int userId)
        {
            return _context.LinkInvalid.FirstOrDefault(x => x.LinkId == linkId && x.UserId == userId);
        }

        public string AddOrDelete(int linkId, int userId)
        {
            var link = FindByBlogIdAndUserId(linkId, userId);
            if (link == null)
            {
                link = new LinkInvalid()
                {
                    LinkId = linkId,
                    UserId = userId,
                    TimeStamp = DateTime.Now
                };
                _context.LinkInvalid.Add(link);
                _context.SaveChanges();
                return "add";
            }
            else
            {
                _context.LinkInvalid.Remove(link);
                _context.SaveChanges();
                return "cancel";
            }
        }

        public OperationResult<string> Add(int linkId, int userId)
        {
            int count = CountByLinkIdAndUserId(linkId, userId);
            if (count == 0)
            {
                var link = new LinkInvalid()
                {
                    LinkId = linkId,
                    UserId = userId,
                    TimeStamp = DateTime.Now
                };
                _context.LinkInvalid.Add(link);
                _context.SaveChanges();
                return new OperationResult<string>(true,"add");
            }
            return new OperationResult<string>(true,"exist");
        }

        public int CountByLinkIdAndUserId(int linkId, int userId)
        {
            return _context.LinkInvalid.Count(x => x.LinkId == linkId && x.UserId == userId);
        }

        public IPagedList<LinkInvalid> GetPagedList(int pageNumber, int pageSize = 20)
        {
            var query = _context.LinkInvalid.Include(l => l.Link).OrderByDescending(l => l.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public LinkInvalid GetLinkInvalid(int id)
        {
            return _context.LinkInvalid.Include(l => l.Link).SingleOrDefault(l => l.Id == id);
        }

        public int ConfirmInvalide(int id)
        {
            var invalide = GetLinkInvalid(id);
            int linkId = invalide.LinkId;
            int blogId = invalide.Link.BlogId;
            var blog = invalide.Link.Blog;
            _context.LinkInvalid.Where(l => l.LinkId == linkId).Delete();
            _context.Link.Where(l => l.Id == linkId).Update(l => new Link {Invalide = true});
            _context.Blog.Where(b => b.Id == blogId).Update(b => new Blog {Status = BlogStatus.LinkInvalid});
            var notice = new Notification()
            {
                BlogId = blogId,
                BlogTitle = blog.Title,
                LinkId = linkId,
                NotificationType = NotificationType.LinkInvalid,
                TimeStamp = DateTime.Now,
                UserId = blog.UserId
            };
            _context.Notification.Add(notice);
            SearchService.DeleteById(blogId);
            return _context.SaveChanges();
        }
    }
}