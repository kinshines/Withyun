using System;
using System.Collections.Generic;
using System.Linq;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Withyun.Infrastructure.Services
{
    public class CollectionService
    {
        readonly BlogContext _context;
        public CollectionService(BlogContext context)
        {
            _context = context;
        }

        public Collection FindByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.Collection.FirstOrDefault(x => x.BlogId == blogId && x.UserId == userId);
        }

        public OperationResult<string> AddOrDelete(int blogId, int userId,string userName,int distributor,string blogTitle)
        {
            var collection = FindByBlogIdAndUserId(blogId, userId);
            if (collection == null)
            {
                collection = new Collection()
                {
                    BlogId = blogId,
                    Title = blogTitle,
                    UserId = userId,
                    TimeStamp = DateTime.Now
                };
                _context.Collection.Add(collection);
                var notice = new Notification()
                {
                    BlogId = blogId,
                    BlogTitle = blogTitle,
                    NotificationType = NotificationType.Collect,
                    SourceId = userId,
                    SourceName = userName,
                    UserId = distributor,
                    TimeStamp = DateTime.Now
                };
                _context.Notification.Add(notice);
                _context.SaveChanges();
                return new OperationResult<string>(true, "add");
            }
            else
            {
                _context.Collection.Remove(collection);
                _context.SaveChanges();
                return new OperationResult<string>(true, "cancel");
            }
        }

        public IPagedList<Collection> GetPagedList(int userId, string blogTitle, int pageNumber, int pageSize = 20)
        {
            var query = _context.Collection.Where(c => c.UserId == userId);
            if (!string.IsNullOrWhiteSpace(blogTitle))
            {
                query = query.Where(c => c.Title.Contains(blogTitle));
            }
            query = query.OrderByDescending(c => c.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        } 

        public int CountByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.Collection.Count(x => x.BlogId == blogId && x.UserId == userId);
        }

        public OperationResult Delete(int id, int userId)
        {
            return new OperationResult(_context.Collection.Where(c => c.Id == id && c.UserId == userId).Delete() > 0);
        }
    }
}