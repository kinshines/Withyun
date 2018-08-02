using System;
using System.Collections.Generic;
using System.Linq;
using Withyun.Core.Dtos;
using Withyun.Core.Entities;
using Withyun.Infrastructure.Data;

namespace Withyun.Infrastructure.Services
{
    public class VoteDownService
    {
        readonly BlogContext _context;
        public VoteDownService(BlogContext context)
        {
            _context = context;
        }

        public VoteDown FindByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteDown.FirstOrDefault(x => x.BlogId == blogId && x.UserId == userId);
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
                _context.VoteDown.Add(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "add");
            }
            else
            {
                _context.VoteDown.Remove(vote);
                _context.SaveChanges();
                return new OperationResult<string>(true, "cancel");
            }
        }

        public int CountByBlogIdAndUserId(int blogId, int userId)
        {
            return _context.VoteDown.Count(x => x.BlogId == blogId && x.UserId == userId);
        }
    }
}