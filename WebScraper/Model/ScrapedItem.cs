namespace WebScraper.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class ScrapedItem : DbContext
    {
        // Your context has been configured to use a 'ScrapedItem' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'WebScraper.Model.ScrapedItem' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'ScrapedItem' 
        // connection string in the application configuration file.
        public ScrapedItem()
            : base("name=ScrapedItem")
        {
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 360;
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<BuildUppProfile> BuildUppProfiles { get; set; }
    }
    
}