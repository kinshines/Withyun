using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Data;

namespace Withyun.Infrastructure.Services
{
    public class ImageUrlService
    {
        readonly BlogContext _context;
        public ImageUrlService(BlogContext context)
        {
            _context = context;
        }

        public ImageUrl Add(ImageUrl image)
        {
            var blog = _context.Blog.First(b => b.Id == image.BlogId);
            blog.HtmlContent = "<html><body><p><img src='" + image.YunUrl + "' /></p>" + blog.HtmlContent.Remove(0, 12);
            _context.ImageUrl.Add(image);
            _context.SaveChanges();
            return image;
        }

        public bool Update(ImageUrl image)
        {
            var blog = _context.Blog.First(b => b.Id == image.BlogId);
            string orignalUrl = "http://www.withyun.com" + ConstValues.BlogImageDirectory + image.Url.Replace('\\', '/');
            blog.HtmlContent = blog.HtmlContent.Replace(orignalUrl, image.YunUrl);
            _context.ImageUrl.Attach(image);
            return _context.SaveChanges() > 0;
        }

        public List<ImageUrl> GetLocalList()
        {
            return _context.ImageUrl.Where(i => i.ImageStatus == ImageStatus.Local).ToList();
        } 

    }
}
