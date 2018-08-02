using System;
using System.Collections.Generic;
using System.Linq;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;

namespace Withyun.Infrastructure.Services
{
    public class VoteUpService
    {
        readonly BlogContext _context;
        public VoteUpService(BlogContext context)
        {
            _context = context;
        }

        public VoteUp FindByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteUp.FirstOrDefault(x => x.BlogId == blogId && x.UserId == userId);
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
                _context.VoteUp.Add(vote);
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
                _context.Notification.Add(notice);
                _context.SaveChanges();
                return new OperationResult<string>(true,"add");
            }
            else
            {
                _context.VoteUp.Remove(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "cancel");
            }
        }

        public int CountByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteUp.Count(x => x.BlogId == blogId && x.UserId == userId);
        }
    }
}