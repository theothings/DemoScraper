namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDeuplicateField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BuildUppProfiles", "ProCategory");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BuildUppProfiles", "ProCategory", c => c.String());
        }
    }
}
