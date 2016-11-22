namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEmailScraperFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "HasEmailBeenChecked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "HasEmailBeenChecked");
        }
    }
}
