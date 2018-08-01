using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DAL;
using Domain.Models;

namespace Domain.Services
{
    public class LinkService:IDisposable
    {
        readonly BlogContext _context = new BlogContext();

        public void AddLinkList(int blogId, List<Link> linkList)
        {
            List<Link> databaseLinks = _context.Links.Where(l => l.BlogId == blogId).ToList();
            foreach (var link in linkList)
            {
                if (databaseLinks.Count(l => l.Url == link.Url) == 0)
                {
                    link.BlogId = blogId;
                    link.TimeStamp = DateTime.Now;
                    _context.Links.Add(link);
                }
            }
            _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
