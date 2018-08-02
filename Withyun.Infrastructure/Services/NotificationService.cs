using System;
using System.Collections.Generic;
using System.Linq;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Data;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Withyun.Infrastructure.Services
{
    public class NotificationService
    {
        readonly BlogContext _context;
        public NotificationService(BlogContext context)
        {
            _context = context;
        }

        public IPagedList<Notification> GetPagedList(int userId, int pageNumber, int pageSize = 20)
        {
            UpdateRead(userId);
            var query = _context.Notification.Where(n => n.UserId == userId);
            query = query.OrderByDescending(n => n.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public List<Notification> GetTopList(int userId, int top)
        {
            UpdateRead(userId);
            var query = _context.Notification.Where(n => n.UserId == userId);
            query = query.OrderByDescending(n => n.TimeStamp);
            return query.Take(top).ToList();
        } 
        public int UnReadCount(int userId)
        {
            return _context.Notification.Count(n => n.UserId == userId && n.Read == false);
        }

        private void UpdateRead(int userId)
        {
            _context.Notification.Where(n => n.UserId == userId && n.Read == false)
                .Update(n => new Notification {Read = true});
        }
    }
}