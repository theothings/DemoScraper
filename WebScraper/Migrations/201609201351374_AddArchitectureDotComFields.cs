namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddArchitectureDotComFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "HousingExperience", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "CommercialExperience", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "OverseasExperience", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "OverseasExperience");
            DropColumn("dbo.BuildUppProfiles", "CommercialExperience");
            DropColumn("dbo.BuildUppProfiles", "HousingExperience");
        }
    }
}
