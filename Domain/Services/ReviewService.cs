using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.DAL;
using Domain.Models;

namespace Domain.Services
{
    public class ReviewService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        public void Add(Review review)
        {
            if (review.Content.Length > 200)
            {
                review.Content = review.Content.Substring(0, 200);
            }
            review.TimeStamp = DateTime.Now;

            _context.Reviews.Add(review);

            var notice = new Notification()
            {
                BlogId = review.BlogId,
                BlogTitle = review.BlogTitle,
                NotificationType = NotificationType.Review,
                SourceId = review.UserId,
                SourceName = review.UserName,
                UserId = review.Distributor,
                TimeStamp = DateTime.Now
            };
            _context.Notifications.Add(notice);
            _context.SaveChanges();
        }

        public int CountByTime(Review review, DateTime lastTime)
        {
            return
                _context.Reviews.Count(
                    r => r.BlogId == review.BlogId && r.UserId == review.UserId && r.TimeStamp > lastTime);
        }
    }
}