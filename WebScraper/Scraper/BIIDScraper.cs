using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Implements the scraper for the BIID website
    /// </summary>
    public class BIIDScraper : SeleniumScraper
    {
        public BIIDScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://biid.org.uk/find-a-designer/results?utf8=%E2%9C%93&designer_name=";
        

        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "BIID";

        public override void CleanData(bool RemoveExactDuplicates = true)
        {
            StandardClean(RemoveExactDuplicates);
        }

        public override void ExportToCSV(string fileName, bool updateExportedStatus = true)
        {
            List<string> columns = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AreasServed",
                "AboutUs",
                "ContactName",
                "PhoneNumber"
            };

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AreasServed",
                "AboutUs",
                "ContactName",
                "PhoneNumber"
            };

            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            BrowserBackButton();
        }

        protected override void ExtractItemFromPage()
        {
            Trace.TraceInformation("Extracting infomation from " + driver.Url);
            using (var ctx = new ScrapedItem())
            {
                var profile = new BuildUppProfile();

                profile.NameOfCompany = this.GetTextByXPath("//*[@id='page_container']/header/div[3]/div/h1/span");
                profile.WebsiteAddress = this.GetAttributeValueByXPath("//*[@id='page_container']/section/div/div/div[2]/div[2]/div/div/p[1]/a", "href");
                profile.OfficeAddress = this.GetTextByXPath("//*[@id='page_container']/section/div/div/div[2]/div[2]/div/div/p[2]");
                profile.AreasServed = this.GroupAllTextByXPath("//*[@id='page_container']/section/div/div/div[3]/div[2]/ul/li", ", ");
                profile.AboutUs = this.GroupAllTextByXPath("//*[@id='page_container']/section/div/div/div[3]/div[1]/p");
                profile.ContactName = this.GetTextByXPath("//*[@id='page_container']/header/div[3]/div/h1");

                if (!string.IsNullOrEmpty(profile.NameOfCompany))
                {
                    profile.ContactName = profile.ContactName.Replace(profile.NameOfCompany, "").Trim();
                }
                string infoBox = this.GetTextByXPath("//*[@id='page_container']/section/div/div/div[2]/div[2]/div/div/p[1]");
                profile.PhoneNumber = this.GetLineContaining(infoBox, "Tel:").Replace("Tel: ", "");

                try
                {
                    profile.PageURL = driver.Url;
                }
                catch (Exception e) { };

                try
                {
                    profile.WebsiteScraped = WebsiteName;
                }
                catch (Exception e) { };


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            // No new pages in the pagination for this website
            return false;
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//*[@id='page_container']/section/div/div/section[1]/article[{0}]//a";
            int param = this.CurrentResultOnPage + 1;
            return NextElementByXPath(xPathFormat, param);
        }
    }
}
