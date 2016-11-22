namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFieldsForCIBSE : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "ConstructionCategory", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "LightingConsultants", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "OtherOfficeAddresses", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "AdditionalInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "AdditionalInfo");
            DropColumn("dbo.BuildUppProfiles", "OtherOfficeAddresses");
            DropColumn("dbo.BuildUppProfiles", "LightingConsultants");
            DropColumn("dbo.BuildUppProfiles", "ConstructionCategory");
        }
    }
}
