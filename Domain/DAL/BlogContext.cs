using Domain.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Domain.DAL
{
    public class BlogContext : DbContext
    {
        public BlogContext() : base("DefaultConnection")
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<LinkInvalid> LinkInvalids { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<VoteDown> VoteDowns { get; set; }
        public DbSet<VoteUp> VoteUps { get; set; }
        public DbSet<ImageUrl> ImageUrls { get; set; }
        public DbSet<Recomment> Recomments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        }
    }
}