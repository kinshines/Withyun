using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;
using Withyun.Infrastructure.Utility;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Withyun.Infrastructure.Services
{
    public class RecommentService:IDisposable
    {
        readonly BlogContext _context;
        readonly IHostingEnvironment _env;
        public RecommentService(BlogContext context,IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IPagedList<Recomment> GetPagedList(RecommentCategory category,string recommentTitle, int pageNumber, int pageSize = 20)
        {
            var query = _context.Recomment.Where(r => r.Category == category);
            if (!string.IsNullOrWhiteSpace(recommentTitle))
            {
                query = query.Where(c => c.Title.Contains(recommentTitle));
            }
            query = query.OrderByDescending(r => r.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }
        public IPagedList<Recomment> GetPagedList(RecommentCategory category, int pageNumber, int pageSize = 36)
        {
            var query = _context.Recomment.Where(r => r.Category == category);
            query = query.OrderByDescending(r => r.Top).ThenByDescending(r => r.TimeStamp);
            return query.ToPagedList(pageNumber, pageSize);
        }

        public void SaveRecomment(IFormFile file, string title,int blogId, int[] selectedCategory)
        {
            string fileName="";
            string yunUrl = "";
            if (file.Length > 0)
            {
                fileName = DateTime.Now.ToString("yy/MM/dd/") + Guid.NewGuid().ToString("N") +
                           Path.GetExtension(file.FileName);
                string diskPath = _env.ContentRootFileProvider.GetFileInfo(ConstValues.CoverImageDirectory + fileName).PhysicalPath;
                var image = ImgHandler.ZoomPicture(Image.FromStream(file.OpenReadStream()), 200, 110);
                string diskDir = diskPath.Substring(0, diskPath.LastIndexOf("\\", StringComparison.Ordinal));
                if (!Directory.Exists(diskDir))
                {
                    Directory.CreateDirectory(diskDir);
                }
                image.Save(diskPath);
                image.Dispose();
                yunUrl = UploadUtility.UploadLocalFile(diskPath);
            }
            foreach (int category in selectedCategory)
            {
                var recomment = new Recomment()
                {
                    BlogId = blogId,
                    CoverName = fileName.Replace('/', '\\'),
                    Title = title,
                    TimeStamp = DateTime.Now,
                    Category = (RecommentCategory) category,
                    YunUrl = yunUrl,
                    ImageStatus = string.IsNullOrEmpty(yunUrl) ? ImageStatus.Local : ImageStatus.Yun
                };

                _context.Recomment.Add(recomment);
            }
            _context.SaveChanges();
        }

        public Recomment Add(Recomment recomment)
        {
            _context.Recomment.Add(recomment);
            _context.SaveChanges();
            return recomment;
        }

        public List<Recomment> GetRecommentsByCategory(RecommentCategory category,int top=18)
        {
            return
                _context.Recomment.Where(r => r.Category == category)
                    .OrderByDescending(r=>r.Top)
                    .ThenByDescending(r => r.TimeStamp)
                    .Take(top)
                    .ToList();
        }

        public List<Recomment> GetLocalList()
        {
            return _context.Recomment.Where(i => i.ImageStatus == ImageStatus.Local).ToList();
        }

        public bool Update(Recomment recomment)
        {
            _context.Recomment.Attach(recomment);
            return _context.SaveChanges() > 0;
        }

        public bool Top(int id)
        {
            return _context.Recomment.Where(r => r.Id == id).Update(r => new Recomment() {Top = true,TimeStamp = DateTime.Now}) > 0;
        }

        public bool UnTop(int id)
        {
            return _context.Recomment.Where(r => r.Id == id).Update(r => new Recomment() { Top = false }) > 0;
        }

        public bool Delete(int id)
        {
            var recomment = _context.Recomment.Find(id);
            if (recomment == null)
                return true;
            string diskPath = _env.ContentRootFileProvider.GetFileInfo(ConstValues.CoverImageDirectory + recomment.CoverName).PhysicalPath;
            try
            {
                File.Delete(diskPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"file delete error,filepath:"+diskPath);
            }
            _context.Recomment.Remove(recomment);
            return _context.SaveChanges() > 0;
        }

        public string GetIconByCategory(RecommentCategory category)
        {
            switch (category)
            {
                case RecommentCategory.剧集:
                    return "fa-tv";
                case RecommentCategory.动漫:
                    return "fa-modx";
                case RecommentCategory.图书:
                    return "fa-book";
                case RecommentCategory.教育:
                    return "fa-graduation-cap";
                case RecommentCategory.日剧:
                    return "fa-ge";
                case RecommentCategory.游戏:
                    return "fa-gamepad";
                case RecommentCategory.电影:
                    return "fa-film";
                case RecommentCategory.综艺:
                    return "fa-birthday-cake";
                case RecommentCategory.美剧:
                    return "fa-medium";
                case RecommentCategory.资料:
                    return "fa-file-text-o";
                case RecommentCategory.软件:
                    return "fa-windows";
                case RecommentCategory.韩剧:
                    return "fa-codiepie";
                case RecommentCategory.音乐:
                    return "fa-music";
                default:
                    return "fa-chrome";
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}