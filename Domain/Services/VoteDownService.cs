using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.DAL;
using Domain.Models;

namespace Domain.Services
{
    public class VoteDownService:IDisposable
    {
        readonly BlogContext _context=new BlogContext();
        public void Dispose()
        {
            _context.Dispose();
        }
        public VoteDown FindByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteDowns.FirstOrDefault(x => x.BlogId == blogId && x.UserId == userId);
        }

        public OperationResult<string> AddOrDelete(int blogId, int userId)
        {
            var vote = FindByBlogIdAndUserId(blogId, userId);
            if (vote == null)
            {
                vote = new VoteDown()
                {
                    BlogId = blogId,
                    UserId = userId,
                    TimeStamp = DateTime.Now
                };
                _context.VoteDowns.Add(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "add");
            }
            else
            {
                _context.VoteDowns.Remove(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "cancel");
            }
        }

        public int CountByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteDowns.Count(x => x.BlogId == blogId && x.UserId == userId);
        }
    }
}