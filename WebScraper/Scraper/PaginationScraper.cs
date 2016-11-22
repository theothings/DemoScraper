using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WebScraper.Scraper
{
    /// <summary>
    /// Base pagination class for scraping websites with pagination
    /// </summary>
    public abstract class PaginationScraper
    {
        protected int PagesToSkip;
        protected int PageLimitToStop;

        public PaginationScraper(int pagesToSkip = 0, int pageLimitToStop = 0)
        {
            this.PagesToSkip = pagesToSkip;
            this.PageLimitToStop = pageLimitToStop;
        }

        virtual public void Start()
        {
            Trace.TraceInformation("Scraper started");
            this.GoToFirstResultsPage();
            ScrapePaginatedResults();
        }

        /// <summary>
        /// Loop over paginated results
        /// </summary>
        protected void ScrapePaginatedResults()
        {
            do
            {
                // Loop over each result item on the page and extract information
                while(VisitNextResultOnPage())
                {
                    ExtractItemFromPage();
                    BackToSearchResults();
                }
            }
            while (GoToNextPage());
        }

        /// <summary>
        /// Go to the first page of pagination
        /// </summary>
        abstract protected void GoToFirstResultsPage();

        /// <summary>
        /// Open all the page links and call ExtractItemFromPage() for each result
        /// </summary>
        /// <returns>false if there is no next result on page</returns>
        abstract protected bool VisitNextResultOnPage();

        /// <summary>
        /// When on a paginated results page, visit the next page
        /// </summary>
        /// <returns>return false if no page exists</returns>
        abstract protected bool GoToNextPage();

        /// <summary>
        /// Extract the infomation from a page and save to the database
        /// </summary>
        abstract protected void ExtractItemFromPage();

        /// <summary>
        /// Go back to the page results from the Item page
        /// </summary>
        abstract protected void BackToSearchResults();

        /// <summary>
        /// Export database to CSV file
        /// </summary>
        /// <param name="fileName">filename to save CSV file to</param>
        /// <param name="updateExportedStatus">If true, the database is updated with a flag to say the profile is exported</param>
        abstract public void ExportToCSV(string fileName, bool updateExportedStatus = true);

        /// <summary>
        /// Recover from scrape exception
        /// </summary>
        public virtual void Recover()
        {
            this.ScrapePaginatedResults();
        }

        abstract public void CleanData(bool RemoveExactDuplicates = true);
    }
}
