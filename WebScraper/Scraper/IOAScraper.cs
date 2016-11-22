using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Scraper for the IOA website
    /// </summary>
    public class IOAScraper : SeleniumScraper
    {
        private List<string> profileUrls = new List<string>();
        public IOAScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop, pagesToStart, searchesToSkip)
        {

        }

        /// <summary>
        /// Set the default start URL for this scraper
        /// </summary>
        public new string DefaultStartURL = "http://www.ioa.org.uk/find-acoustics-specialist-or-supplier";
        
        /// <summary>
        /// The name attribute to be stored in the database
        /// </summary>
        public new string WebsiteName = "IOA";

        protected override void BeforeIteratingResults()
        {
            base.BeforeIteratingResults();
            driver.FindElement(By.XPath("//*[@id='edit-submit-consultancy-services']")).Click();
            // Loop over all items and get a list of item URL's
            string linksXpath = "//*[@id='quicktabs-tabpage-buyer_s_guide-0']/div/div[2]/table/tbody/tr/td/h1/a";
            foreach (var item in driver.FindElements(By.XPath(linksXpath)))
            {
                string url = item.GetAttribute("href");
                this.profileUrls.Add(url);
            }
        }


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
                "ContactName",
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs",
                "ServicesProvided"
            };
            

            List<string> headings = new List<string>()
            {
                "Id",
                "NameOfCompany",
                "ContactName",
                "PhoneNumber",
                "EmailAddress",
                "WebsiteAddress",
                "OfficeAddress",
                "AboutUs",
                "ServicesProvided"
            };

            this.ExportTable(columns, fileName, updateExportedStatus, headings);
        }

        protected override void BackToSearchResults()
        {
            // BackToPreviousWindow();
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

                profile.NameOfCompany = this.GetTextByXPath("//*[@id='block-system-main']/div/div/table/tbody/tr/td/div[1]/h1");
                profile.ContactName = this.GetTextByXPath("//div[contains(@class,'views-field')]/strong[text()='Contact Name: ']/following-sibling::span");
                profile.PhoneNumber = this.GetTextByXPath("//div[contains(@class,'views-field')]/strong[text()='Telephone: ']/following-sibling::span");
                profile.EmailAddress = this.GetTextByXPath("//div[contains(@class,'views-field')]/strong[text()='Email Address: ']/following-sibling::span");
                profile.WebsiteAddress = this.GetTextByXPath("//div[contains(@class,'views-field')]/strong[text()='Website: ']/following-sibling::span");
                profile.OfficeAddress = this.GetTextByXPath("//span[contains(@class,'views-field')]/strong[text()='Contact Name: ']/following-sibling::span");
                profile.AboutUs = this.GetTextByXPath("//span[contains(@class, 'field-content')]/strong[text()='Company Information:']/../..");
                profile.AboutUs = string.IsNullOrEmpty(profile.AboutUs) ? null : profile.AboutUs.Replace("Company Information:", "").Trim();
                profile.ServicesProvided = this.GroupAllTextByXPath("//*[@id='buyersGuideCompany_productsAndServices']/li");

                List<string> address = new List<string>();

                string streetAddress = this.GetTextByXPath("//span[contains(@class, 'views-field-street-address')]/span");
                string addressLine1 = this.GetTextByXPath("//span[contains(@class, 'views-field-supplemental-address-1')]/span");
                string city = this.GetTextByXPath("//span[contains(@class, 'views-field-city')]/span");
                string province = this.GetTextByXPath("//span[contains(@class, 'views-field-state-province')]/span");
                string postcode = this.GetTextByXPath("//span[contains(@class, 'views-field-postal-code')]/span");
                string country = this.GetTextByXPath("//span[contains(@class, 'views-field-country')]/span");

                if (!string.IsNullOrEmpty(streetAddress))
                {
                    address.Add(streetAddress.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(addressLine1))
                {
                    address.Add(addressLine1.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(city))
                {
                    address.Add(city.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(province))
                {
                    address.Add(province.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(postcode))
                {
                    address.Add(postcode.Trim().Trim(','));
                }
                if (!string.IsNullOrEmpty(country))
                {
                    address.Add(country.Trim().Trim(','));
                }

                profile.OfficeAddress = string.Join(", ", address);


                profile.PageURL = driver.Url;
                profile.WebsiteScraped = WebsiteName;


                ctx.BuildUppProfiles.Add(profile);
                ctx.SaveChanges();

                Trace.TraceInformation("Completed scraping infomation from " + driver.Url);

            };
        }

        protected override bool GoToNextPage()
        {
            driver.FindElement(By.XPath("//*[@id='edit-submit-consultancy-services']")).Click();
            return true;
            //string buttonXPath = "";
            //return ClickNextPageButton(buttonXPath);
        }

        protected override bool VisitNextResultOnPage()
        {
            try
            {
                driver.Navigate().GoToUrl(this.profileUrls[CurrentResultOnPage]);
                this.CurrentResultOnPage++;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
            //return NextElementNewWindowByXPath(xPathFormat, param);
        }
    }
}
