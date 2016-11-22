namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEndsDirectoryFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "SectorsServed", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "CompanyType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "CompanyType");
            DropColumn("dbo.BuildUppProfiles", "SectorsServed");
        }
    }
}
