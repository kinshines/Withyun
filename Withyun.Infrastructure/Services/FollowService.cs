using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;
using X.PagedList;

namespace Withyun.Infrastructure.Services
{
    public class FollowService
    {
        readonly BlogContext _context;

        public IPagedList<Blog> GetBlogPagedList(int userId, string blogTitle, int pageNumber, int pageSize = 20)
        {
            var distributors = _context.Follow.Where(f => f.UserId == userId).Select(f => f.DistributorId);
            var query = from b in _context.Blog
                where distributors.Contains(b.UserId)
                select b;
            if (!string.IsNullOrWhiteSpace(blogTitle))
            {
                query = query.Where(b => b.Title.Contains(blogTitle));
            }
            query = query.OrderByDescending(b => b.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public OperationResult<string> Add(int distributorId,int userId,string userName)
        {
            int count = Count(distributorId, userId);
            if (count > 0)
            {
                return new OperationResult<string>(true, "已关注");
            }
            var distributor = _context.User.Find(distributorId);
            var follow = new Follow()
            {
                DistributorId = distributorId,
                DistributorName = distributor.UserName,
                UserId = userId,
                TimeStamp = DateTime.Now
            };
            _context.Follow.Add(follow);
            var notice = new Notification()
            {
                NotificationType = NotificationType.Follow,
                TimeStamp = DateTime.Now,
                UserId = distributorId,
                SourceId = userId,
                SourceName = userName
            };
            _context.Notification.Add(notice);
            _context.SaveChanges();
            return new OperationResult<string>(true);
        }

        public OperationResult<string> Delete(int distributorId, int userId)
        {
            var follow = _context.Follow.SingleOrDefault(f => f.DistributorId == distributorId && f.UserId == userId);
            if (follow == null)
            {
                return new OperationResult<string>(false,"未关注");
            }
            _context.Follow.Remove(follow);
            _context.SaveChanges();
            return new OperationResult<string>(true);
        } 

        public bool Exist(int distributorId,int userId)
        {
            return Count(distributorId, userId) > 0;
        }

        public string GetDistributorName(int distributorId)
        {
            var distributor = _context.User.Find(distributorId);
            return distributor.UserName;
        }

        private int Count(int distributorId, int userId)
        {
            return _context.Follow.Count(f => f.DistributorId == distributorId && f.UserId == userId);
        }

    }
}