using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Selenium based website scraper, implements pagination scraper
    /// </summary>
    public abstract class SeleniumScraper : PaginationScraper
    {
        protected IWebDriver driver = new FirefoxDriver();

        protected int CurrentResultOnPage = 0;
        protected int CurrentPage = 1;
        private string StartUrl;
        private List<string> PagesToStart;
        private int SearchesToSkip;
        protected string parentWindow;

        public string WebsiteName;
        public string DefaultStartURL;

        public SeleniumScraper(int pagesToSkip = 0, int pageLimitToStop = 0, List<string> pagesToStart = null, int searchesToSkip = 0) : base(pagesToSkip, pageLimitToStop)
        {
            this.PagesToStart = pagesToStart;
            this.SearchesToSkip = searchesToSkip;
            this.PagesToSkip = pagesToSkip;
            this.PageLimitToStop = pageLimitToStop;
        }

        override public void Start()
        {
            int index = 0;
            if(this.PagesToStart == null)
            {
                this.StartUrl = this.DefaultStartURL;
                Trace.TraceInformation("Scraper started");
                this.GoToFirstResultsPage();
                ScrapePaginatedResults();
            }
            else
            {
                foreach (string pageToStart in this.PagesToStart)
                {
                    if (index >= this.SearchesToSkip)
                    {
                        Trace.TraceInformation("Scraper started, {0}", pageToStart);
                        this.StartUrl = pageToStart;
                        this.CurrentPage = 1;
                        this.CurrentResultOnPage = 0;
                        this.GoToFirstResultsPage();
                        ScrapePaginatedResults();
                    }
                    index++;
                }
            }
           
        }
        
        protected void StandardClean(bool RemoveExactDuplicates)
        {

            Trace.TraceInformation("Cleaning results data");
            Trace.TraceInformation("Remove duplicate profiles");
            using (var ctx = new ScrapedItem())
            {
                // Loop over each row and delete duplicates by adding isDeleted flag
                var duplicateProfiles = from r in ctx.BuildUppProfiles
                                        group r by new
                                        {
                                            NameOfCompany = r.NameOfCompany,
                                            OfficeAddress = r.OfficeAddress,
                                            PhoneNumber = r.PhoneNumber,
                                            EmailAddress = r.EmailAddress,
                                            WebsiteScraped = this.WebsiteName
                                        }
                                        into g
                                        where g.Count() > 1
                                        select g;

                foreach (var g in duplicateProfiles)
                {
                    var removeProfiles = g.Skip(1);
                    foreach (var record in removeProfiles)
                    {
                        record.IsDeletedFlag = true;
                    }
                }

                // Add do not expoert for profiles that do not meet the basic requirment
                var invalidProfiles = ctx.BuildUppProfiles.Where(p =>
                        String.IsNullOrEmpty(p.NameOfCompany) ||
                        String.IsNullOrEmpty(p.EmailAddress) ||
                        String.IsNullOrEmpty(p.OfficeAddress)
                    );

                foreach (var profile in invalidProfiles)
                {
                    profile.DoNotExportFlag = true;
                }



                Trace.TraceInformation("do not export flag set for multiple company name listings");
                // Add the do not export flag for duplicate names
                var multipleAddressProfiles = from r in ctx.BuildUppProfiles
                                              where r.IsDeletedFlag != true
                                              group r by new
                                              {
                                                  NameOfCompany = r.NameOfCompany,
                                              }
                                        into g
                                              where g.Count() > 1
                                              select g;

                foreach (var g in multipleAddressProfiles)
                {
                    var removeProfiles = g.Skip(1);
                    foreach (var record in removeProfiles)
                    {
                        if (record.IsDeletedFlag != true)
                        {
                            record.DoNotExportFlag = true;
                        }
                    }
                }


                ctx.SaveChanges();
            }
        }

        protected void ExportTable(List<string> columns, string fileName, bool updateExportedStatus = true, List<string> headings = null)
        {
            using (var ctx = new ScrapedItem())
            {
                Trace.TraceInformation("Exporting results to CSV file " + fileName);
                // Loop through each row in database which isn't deleted and hasn't got a don't export flag
                var exportResults = ctx.BuildUppProfiles.Where(p => p.IsDeletedFlag != true && p.DoNotExportFlag != true && p.WebsiteScraped == WebsiteName);

                // Write it to the CSV file specified for only specific columns
                using (StreamWriter outfile = new StreamWriter(fileName))
                {
                    headings = headings ?? columns;
                    outfile.WriteLine(
                        string.Join(",", headings)
                        );
                    
                    foreach (var profile in exportResults)
                    {
                        List<string> rowValues = new List<string>();
                        // Loop over each colum name and get a list of the values
                        foreach(var column in columns)
                        {
                            rowValues.Add(
                                "\"" + this.ReplaceSafe( Convert.ToString(this.GetPropertyValue(profile, column))) + "\""
                                );
                        }

                        // Print the row of values
                        outfile.WriteLine( string.Join(",", rowValues) );
                        
                        if (updateExportedStatus)
                        {
                            profile.HasBeenExportedFlag = true;
                        }

                    }

                }

                // Update field to say they've exported
                if (updateExportedStatus)
                {
                    ctx.SaveChanges();
                }
                // Log the number of results exported
                Trace.TraceInformation("CSV file exported with " + exportResults.Count() + " results to " + fileName);
            }
        }
        private string ReplaceSafe(string source)
        {
            try
            {
                return source.Replace("\"", "");
            }
            catch (NullReferenceException e)
            {
                return source;
            }
        }

        protected string GetLineContaining(string text, string keyword)
        {
            text = text ?? "";
            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Do something with the line
                    if (line.Contains(keyword))
                    {
                        return line;
                    }
                }

                // otherwise return null
                return "";
            }
        }

        protected void BrowserBackButton()
        {
            string initialUrl = driver.Url;
            string secondUrl = driver.Url;
            Trace.TraceInformation("Browser back button from " + initialUrl + " to " + secondUrl);
            driver.Navigate().Back();
        }


        protected void BackToPreviousWindow()
        {
            // Switch to original window
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(3));
            driver.Close();
            driver.SwitchTo().Window(this.parentWindow);

        }

        protected void EscapeKeyBack(string targetElementXPath = "//body", string keys = null)
        {
            Trace.TraceInformation("Pressing escape key to go back");
            if(keys == null)
            {
                keys = Keys.Escape;
            }
            Actions actions = new Actions(driver);
            driver.FindElement(By.XPath(targetElementXPath)).SendKeys(Keys.Escape);
        }

        protected void BackButtonClick(string xpath)
        {
            Trace.TraceInformation("Clicking button to go back {0}", xpath);
            driver.FindElement(By.XPath(xpath));
        }
        

        protected string GetTextByXPath(string xPath)
        {
            try
            {
                return driver.FindElement(By.XPath(xPath)).Text;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        protected string GetAttributeValueByXPath(string xPath, string attributeName)
        {
            try
            {
                return driver.FindElement(By.XPath(xPath)).GetAttribute(attributeName);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected string GroupAllTextByXPath(string xPath, string joinText = "")
        {
            try
            {
                List<string> items = new List<string>();
                foreach (var item in driver.FindElements(By.XPath(xPath)))
                {
                    items.Add(item.Text);
                }
                return string.Join(joinText, items);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        protected override void GoToFirstResultsPage()
        {
            // configure driver
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));

            // Go to website, and click search for United Kingdom
            driver.Navigate().GoToUrl(this.StartUrl);

            BeforeIteratingResults();

            // Skip the first pages
            for (int i = 0; i < this.PagesToSkip; i++)
            {
                this.GoToNextPage();
            }
        }

        protected virtual void BeforeIteratingResults()
        {

        }

        protected bool ClickNextPageButton(string nextButtonXPath)
        {
            Trace.TraceInformation("Moving to next page from page {0}", this.CurrentPage);
            // If page limit is reached than stop
            int limit = this.PageLimitToStop;

            if (this.CurrentPage > limit && limit != 0)
            {
                return false;
            }
            
            // Check if there is a next button
            if (driver.FindElement(By.XPath(nextButtonXPath)).Displayed)
            {
                // If there is not a next button
                driver.FindElement(By.XPath(nextButtonXPath)).Click();
                // Increment the current page
                this.CurrentPage++;
                return true;
            }
            else
            {
                // return false if no next button
                return false;
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            var item =  obj.GetType().GetProperty(propertyName).GetValue(obj, null);
            return item;
        }

        public override void Recover()
        {
            if (CurrentPage <= PagesToSkip)
            {
                // Skip the first 42 pages
                for (int i = CurrentPage; i < this.PagesToSkip; i++)
                {
                    this.GoToNextPage();
                }
            }
            else
            {
                this.ScrapePaginatedResults();
            }
        }

        protected bool NextElementByXPath(string xPathFormat, int param)
        {
            // Track the result which are currently on

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
                // If there is no next result, then return false and reset the count
                this.CurrentResultOnPage = 0;
                return false;
            }
        }


        protected bool NextElementNewWindowByXPath(string xPathFormat, int param)
        {

            this.parentWindow = driver.CurrentWindowHandle;

            // Track the result which are currently on

            string pathOfNextPageLink = string.Format(xPathFormat, param);

            // If the next result exists on the current page then click the link
            if (driver.FindElements(By.XPath(pathOfNextPageLink)).Count() > 0)
            {
                // Click on the result for the next item
                driver.FindElement(By.XPath(pathOfNextPageLink)).Click();

                // Increment the result for next time
                this.CurrentResultOnPage++;

                // Switch to new window
                string nextWindow = driver.WindowHandles.Last();
                driver.SwitchTo().Window(nextWindow);

                // Return true as succesful clicked next page
                return true;
            }
            else
            {
                // If there is no next result, then return false and reset the count
                this.CurrentResultOnPage = 0;
                return false;
            }
        }
    }
}
