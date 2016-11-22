using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// RTPI consultants website scraper
    /// </summary>
    public class RTPIConsultants : SeleniumScraper
    {
        public RTPIConsultants(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "https://www.rtpiconsultants.co.uk/search?&start=";

        /// <summary>
        /// The name of the website to be used as a reference in the database
        /// </summary>
        public new string WebsiteName = "RTPICONSULTANTS";

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
                "MobileNumber",
                "EmailAddress",
                "WebsiteAddress",
                "Category",
                "AboutUs",
                "ContactName"
            };

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "OfficeAddress",
                "PhoneNumber",
                "FaxNumber",
                "MobileNumber",
                "EmailAddress",
                "WebsiteAddress",
                "Category",
                "AboutUs",
                "ContactName"
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

                profile.NameOfCompany = this.GetTextByXPath("//*[@id='consultant-header']/h1");
                profile.OfficeAddress = this.GetTextByXPath("//*[@id='consultant-tabs']/div[2]/table/tbody/tr[1]/td[1]/p");
                profile.EmailAddress = this.GetTextByXPath("//*[@id='consultant-tabs']//table//tr//p[starts-with(text(), 'Email:')]//a");
                profile.WebsiteAddress = this.GetTextByXPath("//*[@id='consultant-tabs']//table//tr//p[starts-with(text(), 'Website:')]//a");
                profile.AboutUs = this.GroupAllTextByXPath("//*[@id='consultant-tabs']/following-sibling::p", "\r\n\r\n");
                string otherDetails = this.GetTextByXPath("//*[@id='consultant-tabs']//table/tbody/tr[1]/td[2]/p");

                // Take line from paragraph by word, then replace the text
                profile.PhoneNumber = this.GetLineContaining(otherDetails, "Telephone: ").Replace("Telephone: ", "");
                profile.FaxNumber = this.GetLineContaining(otherDetails, "Fax: ").Replace("Fax: ", "");
                profile.MobileNumber = this.GetLineContaining(otherDetails, "Mobile: ").Replace("Mobile: ", "");
                profile.ContactName = this.GetLineContaining(otherDetails, "Contact: ").Replace("Contact: ", "");


                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            string buttonXPath = "//*[@id='entrylist']/p/a[text()='>']";
            return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//*[@id='entrylist']/div[@class='listitem'][{0}]//a";
            int param = this.CurrentResultOnPage + 1;
            return NextElementByXPath(xPathFormat, param);
        }
    }
}
