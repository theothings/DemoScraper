namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHouzzFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "Category", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "TypicalJobCost", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "ProfessionalInfomation", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "AreasServed", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "Awards", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "IsSponsored", c => c.Boolean(nullable: false));
            AddColumn("dbo.BuildUppProfiles", "ProCategory", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "Rating", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "Rating");
            DropColumn("dbo.BuildUppProfiles", "ProCategory");
            DropColumn("dbo.BuildUppProfiles", "IsSponsored");
            DropColumn("dbo.BuildUppProfiles", "Awards");
            DropColumn("dbo.BuildUppProfiles", "AreasServed");
            DropColumn("dbo.BuildUppProfiles", "ProfessionalInfomation");
            DropColumn("dbo.BuildUppProfiles", "TypicalJobCost");
            DropColumn("dbo.BuildUppProfiles", "Category");
        }
    }
}
