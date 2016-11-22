using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Use this as a template reference for building TemplateScraper
    /// </summary>
    public class TemplateScraper : SeleniumScraper
    {
        public TemplateScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "INSERT THE DEFAULT URL HERE";

        /// <summary>
        /// Website name to use internally in the database
        /// </summary>
        public new string WebsiteName = "UNIQUE WEBSITE NAME, KEEP FIXED";

        public override void CleanData(bool RemoveExactDuplicates = true)
        {
            StandardClean(RemoveExactDuplicates);
        }

        public override void ExportToCSV(string fileName, bool updateExportedStatus = true)
        {
            List<string> columns = new List<string>()
            {
                "Id",
                "NameOfCompany"
            };

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany"
            };

            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            BrowserBackButton();
            // Call EscapeKeyBack();
            // Call BackButtonClick();
            // BackToPreviousWindow();
            // Or do nothing
        }

        protected override void ExtractItemFromPage()
        {
            Trace.TraceInformation("Extracting infomation from " + driver.Url);
            using (var ctx = new ScrapedItem())
            {
                var profile = new BuildUppProfile();

                profile.NameOfCompany = this.GetTextByXPath("//h1");

                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            string buttonXPath = "";
            return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//div[{0}]";
            int param = this.CurrentResultOnPage + 1;
            return NextElementByXPath(xPathFormat, param);
            //return NextElementNewWindowByXPath(xPathFormat, param);
        }
    }
}
