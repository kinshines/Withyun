using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.DAL;
using Domain.Models;
using PagedList;
using EntityFramework.Extensions;

namespace Domain.Services
{
    public class NotificationService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();

        public IPagedList<Notification> GetPagedList(int userId, int pageNumber, int pageSize = 20)
        {
            UpdateRead(userId);
            var query = _context.Notifications.Where(n => n.UserId == userId);
            query = query.OrderByDescending(n => n.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public List<Notification> GetTopList(int userId, int top)
        {
            UpdateRead(userId);
            var query = _context.Notifications.Where(n => n.UserId == userId);
            query = query.OrderByDescending(n => n.TimeStamp);
            return query.Take(top).ToList();
        } 
        public int UnReadCount(int userId)
        {
            return _context.Notifications.Count(n => n.UserId == userId && n.Read == false);
        }

        private void UpdateRead(int userId)
        {
            _context.Notifications.Where(n => n.UserId == userId && n.Read == false)
                .Update(n => new Notification {Read = true});
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}