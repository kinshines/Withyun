using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Withyun.Infrastructure.Services
{
    public class ReportService
    {
        readonly BlogContext _context;
        readonly SearchService _searchService;
        public ReportService(BlogContext context,SearchService searchService)
        {
            _context = context;
            _searchService = searchService;
        }

        public void Add(Report report)
        {
            _context.Report.Add(report);
            _context.SaveChanges();
        }

        public IPagedList<Report> GetPagedList(int pageNumber, int pageSize = 20)
        {
            var query = _context.Report.Include(r => r.Blog).OrderByDescending(r => r.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public Report GetReport(int id)
        {
            return _context.Report.Include(r => r.Blog).SingleOrDefault(r => r.Id == id);
        }

        public int ConfirmReport(int id)
        {
            var report = GetReport(id);
            int blogId = report.BlogId;
            var blog = report.Blog;
            _context.Report.Where(r => r.BlogId == blogId).Delete();
            _context.Blog.Where(b => b.Id == blogId).Update(b => new Blog { Status = BlogStatus.Report });
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
            _context.Notification.Add(notice);
            _searchService.DeleteById(blogId);
            return _context.SaveChanges();
        }
    }
}