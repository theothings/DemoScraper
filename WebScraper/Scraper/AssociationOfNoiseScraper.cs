using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Scraper to scrape associationOfNoise website, using Selenium
    /// </summary>
    public class AssociationOfNoise : SeleniumScraper
    {
        /// <summary>
        /// A list of the profiles URLs must be collected beforehand to avoid navigating between pages which is slow
        /// </summary>
        private List<string> profileUrls = new List<string>();

        public AssociationOfNoise(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://www.association-of-noise-consultants.co.uk/members-search/";

        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "ASSOCIATIONOFNOISE";

        /// <summary>
        /// Collect all the URLs of individual profiles
        /// </summary>
        protected override void BeforeIteratingResults()
        {
            base.BeforeIteratingResults();
            
            // The Xpath of all the individual profile links
            string linksXpath = "//div[@class='member-search-results']/div/a[1]";

            // Store each profile link in this.profileUrls
            foreach(var item in driver.FindElements(By.XPath(linksXpath)))
            {
                // Get the link from the URL
                string url = item.GetAttribute("href");
                this.profileUrls.Add(url);
            }
        }

        public override void CleanData(bool RemoveExactDuplicates = true)
        {
            // Use the base Clean data method
            StandardClean(RemoveExactDuplicates);
        }

        /// <summary>
        /// Export the data needed for this particular site
        /// </summary>
        /// <param name="fileName">The filtname to save the output CSV too</param>
        /// <param name="updateExportedStatus">Flag for whether to change the exported satatus</param>
        public override void ExportToCSV(string fileName, bool updateExportedStatus = true)
        {
            // Columns which should be exported
            List<string> columns = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs"
            };

            // The columns to display in the CSV
            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs"
            };

            // Export the data with appropriate headings
            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            // No back button is needed for this implementation
        }

        protected override void ExtractItemFromPage()
        {
            Trace.TraceInformation("Extracting infomation from " + driver.Url);

            using (var ctx = new ScrapedItem())
            {
                var profile = new BuildUppProfile();

                profile.NameOfCompany = this.GetTextByXPath("/html/body/div/section[1]/div/div/div/div/h2");
                profile.EmailAddress = this.GetTextByXPath("//*[@id='member-info']/tbody/tr/th[text()='Email']/following-sibling::td/a");
                profile.PhoneNumber = this.GetTextByXPath("//*[@id='member-info']/tbody/tr/th[text()='Telephone']/following-sibling::td");
                profile.WebsiteAddress = this.GetTextByXPath("//*[@id='member-info']/tbody/tr/th[text()='Website']/following-sibling::td/a");

                var addressLines = new List<string>();
                var addressLinesCorrected = new List<string>();

                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[5]/td"));
                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[7]/td"));
                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[9]/td"));
                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[11]/td"));
                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[13]/td"));
                addressLines.Add(this.GroupAllTextByXPath("//*[@id='member-info']/tbody/tr[15]/td"));

                // Loop over each addressLine only, copying non blanks into corrected version
                foreach(string addressLine in addressLines)
                {
                    if (!string.IsNullOrEmpty(addressLine?.Trim()))
                    {
                        addressLinesCorrected.Add(addressLine);
                    }
                }


                profile.OfficeAddress = string.Join(", ", addressLinesCorrected);

                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            // No next page is required for this implementation
            return true;
        }

        protected override bool VisitNextResultOnPage()
        {
            // Go to the next profile and get the URLs
            try
            {
                driver.Navigate().GoToUrl(this.profileUrls[CurrentResultOnPage]);
                this.CurrentResultOnPage++;
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
