using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.DAL;
using Domain.Models;

namespace Domain.Services
{
    public class VoteUpService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();

        public VoteUp FindByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteUps.FirstOrDefault(x => x.BlogId == blogId && x.UserId == userId);
        }

        public OperationResult<string> AddOrDelete(int blogId, string blogTitle, int userId, string userName, int distributor)
        {
            var vote = FindByBlogIdAndUserId(blogId, userId);
            if (vote == null)
            {
                vote = new VoteUp()
                {
                    BlogId = blogId,
                    UserId = userId,
                    TimeStamp = DateTime.Now
                };
                _context.VoteUps.Add(vote);
                var notice = new Notification()
                {
                    UserId = distributor,
                    BlogId = blogId,
                    BlogTitle = blogTitle,
                    NotificationType = NotificationType.VoteUp,
                    SourceId = userId,
                    SourceName = userName,
                    TimeStamp = DateTime.Now
                };
                _context.Notifications.Add(notice);
                _context.SaveChanges();
                return new OperationResult<string>(true,"add");
            }
            else
            {
                _context.VoteUps.Remove(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "cancel");
            }
        }

        public int CountByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteUps.Count(x => x.BlogId == blogId && x.UserId == userId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}