namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCiatFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "Staff", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "ContactName", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "Specialisms", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "Specialisms");
            DropColumn("dbo.BuildUppProfiles", "ContactName");
            DropColumn("dbo.BuildUppProfiles", "Staff");
        }
    }
}
