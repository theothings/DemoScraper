using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Profile scraper for the NADC website
    /// </summary>
    public class NADCScraper : SeleniumScraper
    {
        public NADCScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://www.nadc.org.uk/s.do?searchCategory=";
        

        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "NADC";

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
                "BusinessType",
                "AreasServed",
                "EmailAddress",
                "WebsiteAddress",
                "NameOfCompany",
                "AboutUs"
            };
            
            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "OfficeAddress",
                "BusinessType",
                "AreasServed",
                "EmailAddress",
                "WebsiteAddress",
                "NameOfCompany",
                "AboutUs"
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

                profile.NameOfCompany = this.GetTextByXPath("//*[@id='njh_container']/div[3]/div/div/div[1]/h2");

                List<string> address = new List<string>();
                string addressLine = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Address: ']/following-sibling::td");
                string county = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='County: ']/following-sibling::td");
                string postcode = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Postcode: ']/following-sibling::td");
                string country = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Country: ']/following-sibling::td");

                if (!string.IsNullOrEmpty(addressLine))
                {
                    address.Add(addressLine.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(county))
                {
                    address.Add(county.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(postcode))
                {
                    address.Add(postcode.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(country))
                {
                    address.Add(addressLine.Trim().Trim(','));
                }

                profile.OfficeAddress = string.Join(", ", address);


                profile.PhoneNumber = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Telephone: ']/following-sibling::td");
                profile.BusinessType = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Type: ']/following-sibling::td");
                profile.AreasServed = this.GetTextByXPath("//*[@id='OrderingForm']/table//tr//td//table//td[text()='Search District: ']/following-sibling::td");


                profile.EmailAddress = this.GetTextByXPath("//*[@id='OrderingForm']/table//span[@class='email_l']");
                profile.WebsiteAddress = this.GetTextByXPath("//*[@id='OrderingForm']/table//span[@class='website_l']");

                profile.AboutUs = this.GroupAllTextByXPath("//*[@id='OrderingForm']/table/tbody//div[@class='description']/following-sibling::*");

                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            string buttonXPath = "//*[@id='searchResultsDiv']/table/tbody/tr/td[2]/a[text()='Next']";
            return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            string xPathFormat = "//*[@id='searchResultsDiv']/form/table[{0}]//a";
            int param = this.CurrentResultOnPage + 1;
            return NextElementByXPath(xPathFormat, param);
        }
    }
}
