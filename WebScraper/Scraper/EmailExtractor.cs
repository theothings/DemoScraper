using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using WebScraper.Model;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Email extractor to crawl website for URLs
    /// </summary>
    public class EmailExtractor
    {
        // Which database row to start from
        private int idToStart;

        // What page depth should be crawled
        private int limitOfPagesToCrawl;

        public EmailExtractor(int idToStart = 0, int limitOfPagesToCrawl = 10)
        {
            this.idToStart = idToStart;
            this.limitOfPagesToCrawl = limitOfPagesToCrawl; 
        }

        // Method to find an email on a single page
        string GetEmailFromSinglePage(string url)
        {
            Trace.TraceInformation("Attempting extracting email addresses from the url: " + url);
            
            string pageSource = "";


            try
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    pageSource = reader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                // It is not important if a page is unable to be scraped for any reason, so exception should not be handled, just logged
                Trace.TraceError("Exception thrown when requesting url {0}" , url);
                Trace.TraceError(e.ToString());
            }

            Trace.TraceInformation("About to perform regex for email extraction");

            // Get the email from the page content if it exists
            try
            {
                Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
                
                //find items that matches with our pattern
                MatchCollection emailMatches = emailRegex.Matches(pageSource);
                
                Trace.TraceInformation("Found {0} email addresses on page", emailMatches.Count);

                // Take the first email if an email exists
                if(emailMatches.Count > 0)
                {
                    Trace.TraceInformation("Found the email address {0}", emailMatches[0].Value);
                    return emailMatches[0].Value;
                }
                else
                {
                    Trace.TraceInformation("No email address found on page for {0}", url);
                    throw new NotFoundException();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error thrown when performing regex " + e.ToString());
                throw new NotFoundException();
            }
        }
        
        // Crawl the links on a webpage "url" until an email address if found
        string GetEmailFromWebsite(string url)
        {
            // Visit the page and check if there is an email address on the page
            try
            {
                return GetEmailFromSinglePage(url);
            }
            catch(NotFoundException e)
            {
                // Email address not found so continue to branch
                // Failure to find an email for any reason results in an error
                Trace.TraceError("Unable to find email from website: {0} due to error {1}", url, e.ToString());
            }

            try
            {
                // Loop over each of the links on the page
                var listOfLinks = GetListOfLinks(url);
                foreach(var link in listOfLinks)
                {
                    try
                    {
                        // If an email is found on the page return the url
                        return GetEmailFromSinglePage(link);
                    }
                    catch(NotFoundException e)
                    {
                        // Email address not found so continue to branch
                        // Failure to find an email for any reason results in an error
                        Trace.TraceError("Unable to find email from website: {0} due to error {1}", url, e.ToString());
                    }
                }
            }
            catch(NotFoundException e)
            {
                // Email address not found so continue to branch
                // Failure to find an email for any reason results in an error
                Trace.TraceError("Unable to find email from website: {0} due to error {1}", url, e.ToString());
            }

            // No emails were found so throw a not found exception
            throw new NotFoundException();
        }

        /// <summary>
        /// Get the list of links on on a page
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<string> GetListOfLinks(string url)
        {
            List<string> links = new List<string>();

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(url);

            // loop over each link with href attribute
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string linkValue = link.Attributes["href"].Value;
                linkValue = this.ConsolidateURL(url, linkValue);

                // Should check the link is from the same domain as the current site using GetDomain(foundUrl) == GetDomain(url)
                if (this.GetDomain(linkValue) == this.GetDomain(url))
                {
                    links.Add(linkValue);
                }
            }

            // Get a list of URLs from all the links on a page (return the top this.limitofpagestocrawl)
            return links.Take(this.limitOfPagesToCrawl).ToList();
        }

        /// <summary>
        /// Takes a url from the page and returns the full url with host name if none exists
        /// </summary>
        /// <param name="url"></param>
        /// <param name="hrefValue"></param>
        /// <returns></returns>
        private string ConsolidateURL(string url, string hrefValue)
        {
            // Turns the hrefValue url into a full url, if the hostname isn't provided, using the hostname in url
            Uri uri = new Uri(url);
            string siteHost = uri.Host;
            Uri hostUri = uri;

            // does the hrefValue have a hostname
            if (this.IsAbsoluteUrl(hrefValue))
            {
                return hrefValue;
            }
            else
            {
                return new Uri(hostUri, hrefValue).AbsoluteUri;
            }
        }

        /// <summary>
        /// Determines if host name exists in the URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        /// <summary>
        /// Get the host name from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetDomain(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.ToLower();
        }

        /// <summary>
        /// Loop over all profiles in the database and crawl for email addresses
        /// </summary>
        public void LoopOverResults()
        {
            IQueryable<BuildUppProfile> profiles;
            List<BuildUppProfile> profilesList;

            using (var ctx = new ScrapedItem())
            {
                profiles = ctx.BuildUppProfiles.Where(m => m.Id >= this.idToStart && m.WebsiteAddress != null && m.EmailAddress == null && m.HasEmailBeenChecked != true).Take(100);
                profilesList = profiles.ToList();
            }

            foreach (var profile in profilesList)
            {
                Trace.TraceInformation("About to attempt to get URL for {0}", profile.NameOfCompany);
                string email = "";
                try
                {
                    email = this.GetEmailFromWebsite(profile.WebsiteAddress);
                }
                catch (Exception e) {
                    Trace.TraceInformation("Unexpected Exception thrown: {0}", e.ToString());
                }
                string nameOfCompany = profile.NameOfCompany;

                using(var ctx = new ScrapedItem())
                {
                    //update all entries with that name of company to email address
                    foreach (var profileToUpdate in ctx.BuildUppProfiles.Where(m => m.NameOfCompany == nameOfCompany))
                    {
                        profileToUpdate.EmailAddress = email;
                        // if not found then update the EmailCheckedFlag
                        profileToUpdate.HasEmailBeenChecked = true;
                    }
                    ctx.SaveChanges();
                }
            }
        }
    }
}
