using System;
using System.Collections.Generic;
using System.Linq;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;

namespace Withyun.Infrastructure.Services
{
    public class ReviewService
    {
        readonly BlogContext _context;
        public ReviewService(BlogContext context)
        {
            _context = context;
        }

        public void Add(Review review)
        {
            if (review.Content.Length > 200)
            {
                review.Content = review.Content.Substring(0, 200);
            }
            review.TimeStamp = DateTime.Now;

            _context.Review.Add(review);

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
            _context.Notification.Add(notice);
            _context.SaveChanges();
        }

        public int CountByTime(Review review, DateTime lastTime)
        {
            return
                _context.Review.Count(
                    r => r.BlogId == review.BlogId && r.UserId == review.UserId && r.TimeStamp > lastTime);
        }
    }
}