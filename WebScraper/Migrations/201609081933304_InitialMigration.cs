namespace WebScraper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BuildUppProfiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NameOfCompany = c.String(),
                        OfficeAddress = c.String(),
                        EmailAddress = c.String(),
                        WebsiteAddress = c.String(),
                        PhoneNumber = c.String(),
                        FaxNumber = c.String(),
                        ServicesProvided = c.String(),
                        TypeOfSurveyor = c.String(),
                        BusinessType = c.String(),
                        PartnersAndDirectors = c.String(),
                        Languages = c.String(),
                        AboutUs = c.String(),
                        DateEstablished = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BuildUppProfiles");
        }
    }
}
