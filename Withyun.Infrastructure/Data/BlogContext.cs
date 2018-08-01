using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Withyun.Infrastructure.Data
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Collection> Collection { get; set; }
        public DbSet<Follow> Follow { get; set; }
        public DbSet<Link> Link { get; set; }
        public DbSet<LinkInvalid> LinkInvalid { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<VoteDown> VoteDown { get; set; }
        public DbSet<VoteUp> VoteUp { get; set; }
        public DbSet<ImageUrl> ImageUrl { get; set; }
        public DbSet<Recomment> Recomment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
