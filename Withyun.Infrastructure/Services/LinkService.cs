using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Data;

namespace Withyun.Infrastructure.Services
{
    public class LinkService
    {
        readonly BlogContext _context;

        public LinkService(BlogContext context)
        {
            _context = context;
        }

        public void AddLinkList(int blogId, List<Link> linkList)
        {
            List<Link> databaseLinks = _context.Link.Where(l => l.BlogId == blogId).ToList();
            foreach (var link in linkList)
            {
                if (databaseLinks.Count(l => l.Url == link.Url) == 0)
                {
                    link.BlogId = blogId;
                    link.TimeStamp = DateTime.Now;
                    _context.Link.Add(link);
                }
            }
            _context.SaveChanges();
        }
    }
}
