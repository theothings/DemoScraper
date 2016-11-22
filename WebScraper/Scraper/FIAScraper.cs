using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Scraper implementation for FIA website
    /// </summary>
    public class FIAScraper : SeleniumScraper
    {
        public FIAScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://www.fia.uk.com/membership/member-directory.html";

        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "FIA";

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
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs",
            };

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs",
            };

            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            // BrowserBackButton();
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

                string nameOfCompanyXpath = string.Format("//*[@id='member-listing']/li[{0}]/div[1]/h3", this.CurrentResultOnPage + 1);
                string emailAddressXpath = string.Format("//*[@id='member-listing']/li[{0}]/div[3]/ul/li/a[contains(.,'Email us')]", this.CurrentResultOnPage + 1);
                string phoneNumberXpath = string.Format("//*[@id='member-listing']/li[{0}]/div[3]/p/span/strong", this.CurrentResultOnPage + 1);
                string officeAddressXpath = string.Format("//*[@id='member-listing']/li[{0}]/div[2]/p[@class='location']", this.CurrentResultOnPage + 1);
                //string aboutUsAddressXpath = string.Format("", this.CurrentResultOnPage + 1);
                string servicesProvidedXPath = string.Format("//*[@id='member-listing']/li[{0}]/div[1]/p[2]/strong", this.CurrentResultOnPage + 1);
                string websiteAddressXpath = string.Format("//*[@id='member-listing']/li[{0}]/div[3]/ul/li/a[contains(.,'Visit Website')]", this.CurrentResultOnPage + 1);



                profile.NameOfCompany = this.GetTextByXPath(nameOfCompanyXpath);
                profile.EmailAddress = this.GetAttributeValueByXPath(emailAddressXpath, "href")?.Replace("mailto:", "");
                profile.PhoneNumber = this.GetTextByXPath(phoneNumberXpath);
                profile.OfficeAddress = this.GetTextByXPath(officeAddressXpath);
                profile.ServicesProvided = this.GetTextByXPath(servicesProvidedXPath);
                profile.WebsiteAddress = this.GetAttributeValueByXPath(websiteAddressXpath, "href");

                
                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;

                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            string buttonXPath = "//*[@id='content']//a[text()='Show more']";
            return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//*[@id='member-listing']/li[{0}]";
            int param = this.CurrentResultOnPage + 1;

            string pathOfNextPageLink = string.Format(xPathFormat, param);

            // If the next result exists on the current page then click the link
            if (driver.FindElements(By.XPath(pathOfNextPageLink)).Count() > 0)
            {
                // Click on the result for the next item
                driver.FindElement(By.XPath(pathOfNextPageLink)).Click();

                // Increment the result for next time
                this.CurrentResultOnPage++;

                // Return true as succesful clicked next page
                return true;
            }
            else
            {
                // Ignore the reseting of currentResult on page, ass we stay on the page
                return false;
            }
        }
    }
}
