namespace Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Content = c.String(),
                        HtmlContent = c.String(),
                        UserId = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 10),
                        TimeStamp = c.DateTime(nullable: false),
                        Score = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Collection",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(maxLength: 50),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.ImageUrl",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(maxLength: 200),
                        BlogId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                        YunUrl = c.String(maxLength: 200),
                        ImageStatus = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.Link",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 500),
                        Url = c.String(maxLength: 500),
                        BlogId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                        Invalide = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.LinkInvalid",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        LinkId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Link", t => t.LinkId, cascadeDelete: true)
                .Index(t => t.LinkId);
            
            CreateTable(
                "dbo.Report",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        ReportType = c.Byte(nullable: false),
                        Content = c.String(maxLength: 200),
                        UserId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.Review",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        Content = c.String(nullable: false, maxLength: 200),
                        UserId = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 10),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.VoteDown",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.VoteUp",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.Follow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DistributorId = c.Int(nullable: false),
                        DistributorName = c.String(nullable: false, maxLength: 10),
                        UserId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notification",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        NotificationType = c.Byte(nullable: false),
                        BlogTitle = c.String(maxLength: 50),
                        BlogId = c.Int(),
                        LinkId = c.Int(),
                        SourceName = c.String(maxLength: 10),
                        SourceId = c.Int(),
                        Read = c.Boolean(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Recomment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        BlogId = c.Int(nullable: false),
                        Category = c.Byte(nullable: false),
                        CoverName = c.String(maxLength: 200),
                        TimeStamp = c.DateTime(nullable: false),
                        YunUrl = c.String(maxLength: 200),
                        ImageStatus = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId, cascadeDelete: true)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 10),
                        PasswordHash = c.String(nullable: false, maxLength: 128),
                        SecurityStamp = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 50),
                        EmailConfirmed = c.Boolean(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        LoginTime = c.DateTime(),
                        HasAlipay = c.Boolean(nullable: false),
                        HasWechat = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Recomment", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.VoteUp", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.VoteDown", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.Review", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.Report", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.LinkInvalid", "LinkId", "dbo.Link");
            DropForeignKey("dbo.Link", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.ImageUrl", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.Collection", "BlogId", "dbo.Blog");
            DropIndex("dbo.Recomment", new[] { "BlogId" });
            DropIndex("dbo.VoteUp", new[] { "BlogId" });
            DropIndex("dbo.VoteDown", new[] { "BlogId" });
            DropIndex("dbo.Review", new[] { "BlogId" });
            DropIndex("dbo.Report", new[] { "BlogId" });
            DropIndex("dbo.LinkInvalid", new[] { "LinkId" });
            DropIndex("dbo.Link", new[] { "BlogId" });
            DropIndex("dbo.ImageUrl", new[] { "BlogId" });
            DropIndex("dbo.Collection", new[] { "BlogId" });
            DropTable("dbo.User");
            DropTable("dbo.Recomment");
            DropTable("dbo.Notification");
            DropTable("dbo.Follow");
            DropTable("dbo.VoteUp");
            DropTable("dbo.VoteDown");
            DropTable("dbo.Review");
            DropTable("dbo.Report");
            DropTable("dbo.LinkInvalid");
            DropTable("dbo.Link");
            DropTable("dbo.ImageUrl");
            DropTable("dbo.Collection");
            DropTable("dbo.Blog");
        }
    }
}
