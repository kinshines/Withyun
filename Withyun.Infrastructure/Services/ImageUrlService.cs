using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DAL;
using Domain.Models;

namespace Domain.Services
{
    public class ImageUrlService:IDisposable
    {
        readonly BlogContext _context = new BlogContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        public ImageUrl Add(ImageUrl image)
        {
            var blog = _context.Blogs.First(b => b.Id == image.BlogId);
            blog.HtmlContent = "<html><body><p><img src='" + image.YunUrl + "' /></p>" + blog.HtmlContent.Remove(0, 12);
            _context.ImageUrls.Add(image);
            _context.SaveChanges();
            return image;
        }

        public bool Update(ImageUrl image)
        {
            var blog = _context.Blogs.First(b => b.Id == image.BlogId);
            string orignalUrl = "http://www.withyun.com" + ConstValues.BlogImageDirectory + image.Url.Replace('\\', '/');
            blog.HtmlContent = blog.HtmlContent.Replace(orignalUrl, image.YunUrl);
            _context.ImageUrls.Attach(image);
            return _context.SaveChanges() > 0;
        }

        public List<ImageUrl> GetLocalList()
        {
            return _context.ImageUrls.Where(i => i.ImageStatus == ImageStatus.Local).ToList();
        } 

    }
}
