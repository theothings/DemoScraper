using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Scraper for the FM Directory implements SeleniumScraper
    /// </summary>
    public class FMScraper : SeleniumScraper
    {
        public FMScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://www.fm-directory.com/results.aspx?keywords=&location=United+Kingdom&cn=true&cd=true";
       
        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "FMDIRECTORY";

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
                "OfficeAddress",
                "PhoneNumber",
                "FaxNumber",
                "EmailAddress",
                "WebsiteAddress",
                "Category"
            };

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "OfficeAddress",
                "PhoneNumber",
                "FaxNumber",
                "EmailAddress",
                "WebsiteAddress",
                "Category"
            };

            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            BrowserBackButton();
            // Call EscapeKeyBack();
            // Call BackButtonClick();
            // Or do nothing
        }

        protected override void ExtractItemFromPage()
        {
            Trace.TraceInformation("Extracting infomation from " + driver.Url);
            using (var ctx = new ScrapedItem())
            {
                var profile = new BuildUppProfile();

                profile.NameOfCompany = this.GetTextByXPath("//*[@id='mainContainer']/h2");
                profile.OfficeAddress = this.GetTextByXPath("//*[@id='companyDetails']//span[text()='Address:']/../following-sibling::td");
                profile.PhoneNumber = this.GetTextByXPath("//*[@id='companyDetails']//span[text()='Tel:']/../following-sibling::td");
                profile.FaxNumber = this.GetTextByXPath("//*[@id='companyDetails']//span[text()='Fax:']/../following-sibling::td");
                profile.EmailAddress = this.GetTextByXPath("//*[@id='companyDetails']//span[text()='Email:']/../following-sibling::td");
                profile.WebsiteAddress = this.GetTextByXPath("//*[@id='companyDetails']//span[text()='Web:']/../following-sibling::td");
                profile.Category = this.GroupAllTextByXPath("//*[@id='companyDetails']//span[text()='See also:']/../following-sibling::td/a", ", ");

                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            string buttonXPath = "//td[@id='cellNextItem']/a";
            return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//table[@class='AZListings']/tbody/tr[{0}]//a";
            int param = this.CurrentResultOnPage + 1;
            return NextElementByXPath(xPathFormat, param);
        }
    }
}
