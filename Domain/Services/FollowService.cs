using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Domain.DAL;
using Domain.Models;
using PagedList;

namespace Domain.Services
{
    public class FollowService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();

        public IPagedList<Blog> GetBlogPagedList(int userId, string blogTitle, int pageNumber, int pageSize = 20)
        {
            var distributors = _context.Follows.Where(f => f.UserId == userId).Select(f => f.DistributorId);
            var query = from b in _context.Blogs
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
            var distributor = _context.Users.Find(distributorId);
            var follow = new Follow()
            {
                DistributorId = distributorId,
                DistributorName = distributor.UserName,
                UserId = userId,
                TimeStamp = DateTime.Now
            };
            _context.Follows.Add(follow);
            var notice = new Notification()
            {
                NotificationType = NotificationType.Follow,
                TimeStamp = DateTime.Now,
                UserId = distributorId,
                SourceId = userId,
                SourceName = userName
            };
            _context.Notifications.Add(notice);
            _context.SaveChanges();
            return new OperationResult<string>(true);
        }

        public OperationResult<string> Delete(int distributorId, int userId)
        {
            var follow = _context.Follows.SingleOrDefault(f => f.DistributorId == distributorId && f.UserId == userId);
            if (follow == null)
            {
                return new OperationResult<string>(false,"未关注");
            }
            _context.Follows.Remove(follow);
            _context.SaveChanges();
            return new OperationResult<string>(true);
        } 

        public bool Exist(int distributorId,int userId)
        {
            return Count(distributorId, userId) > 0;
        }

        public string GetDistributorName(int distributorId)
        {
            var distributor = _context.Users.Find(distributorId);
            return distributor.UserName;
        }

        private int Count(int distributorId, int userId)
        {
            return _context.Follows.Count(f => f.DistributorId == distributorId && f.UserId == userId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}