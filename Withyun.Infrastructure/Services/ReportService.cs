using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Domain.DAL;
using Domain.Models;
using EntityFramework.Extensions;
using PagedList;

namespace Domain.Services
{
    public class ReportService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        public void Add(Report report)
        {
            _context.Reports.Add(report);
            _context.SaveChanges();
        }

        public IPagedList<Report> GetPagedList(int pageNumber, int pageSize = 20)
        {
            var query = _context.Reports.Include(r => r.Blog).OrderByDescending(r => r.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public Report GetReport(int id)
        {
            return _context.Reports.Include(r => r.Blog).SingleOrDefault(r => r.Id == id);
        }

        public int ConfirmReport(int id)
        {
            var report = GetReport(id);
            int blogId = report.BlogId;
            var blog = report.Blog;
            _context.Reports.Where(r => r.BlogId == blogId).Delete();
            _context.Blogs.Where(b => b.Id == blogId).Update(b => new Blog { Status = BlogStatus.Report });
            var notice = new Notification()
            {
                BlogId = blogId,
                BlogTitle = blog.Title,
                TimeStamp = DateTime.Now,
                UserId = blog.UserId
            };
            switch (report.ReportType)
            {
                case ReportType.不友善内容:
                    notice.NotificationType=NotificationType.不友善内容;
                    break;
                case ReportType.不宜公开讨论的政治内容:
                    notice.NotificationType=NotificationType.不宜公开讨论的政治内容;
                    break;
                case ReportType.其他内容:
                    notice.NotificationType=NotificationType.其他内容;
                    break;
                case ReportType.广告等垃圾信息:
                    notice.NotificationType=NotificationType.广告等垃圾信息;
                    break;
                case ReportType.违反法律法规的内容:
                    notice.NotificationType=NotificationType.违反法律法规的内容;
                    break;
            }
            _context.Notifications.Add(notice);
            SearchService.DeleteById(blogId);
            return _context.SaveChanges();
        }
    }
}