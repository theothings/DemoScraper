namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalBuildUppColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "IsDeletedFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.BuildUppProfiles", "HasBeenExportedFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.BuildUppProfiles", "DoNotExportFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.BuildUppProfiles", "WebsiteScraped", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "PageURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "RICSRegulatedFirm", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "RICSRegulatedFirm");
            DropColumn("dbo.BuildUppProfiles", "PageURL");
            DropColumn("dbo.BuildUppProfiles", "WebsiteScraped");
            DropColumn("dbo.BuildUppProfiles", "DoNotExportFlag");
            DropColumn("dbo.BuildUppProfiles", "HasBeenExportedFlag");
            DropColumn("dbo.BuildUppProfiles", "IsDeletedFlag");
        }
    }
}
