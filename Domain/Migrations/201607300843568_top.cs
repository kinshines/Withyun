namespace Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class top : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recomment", "Top", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Recomment", "Top");
        }
    }
}
