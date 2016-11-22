namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSocialMediaAndMobileNumberFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BuildUppProfiles", "FacebookURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "TwitterURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "LinkedInURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "YoutubeURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "GooglePlusURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "PinterestURL", c => c.String());
            AddColumn("dbo.BuildUppProfiles", "MobileNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BuildUppProfiles", "MobileNumber");
            DropColumn("dbo.BuildUppProfiles", "PinterestURL");
            DropColumn("dbo.BuildUppProfiles", "GooglePlusURL");
            DropColumn("dbo.BuildUppProfiles", "YoutubeURL");
            DropColumn("dbo.BuildUppProfiles", "LinkedInURL");
            DropColumn("dbo.BuildUppProfiles", "TwitterURL");
            DropColumn("dbo.BuildUppProfiles", "FacebookURL");
        }
    }
}
