using WebScraper.Scraper;
using System.Diagnostics;

namespace WebScraperConsole
{
    /// <summary>
    /// Example program to run the scraper for IOAScraper, and collect data
    /// </summary>
    class Program
    {
        const string ScrapeMode = "scrape";
        const string CleanMode = "clean";
        const string ExportMode = "export";
        const string OutputFileName = "output.txt";

        static void Main(string[] args)
        {
            // Example useage for IOAScraper
            IOAScraper scraper = new IOAScraper(Properties.Settings.Default.PagesToSkip, Properties.Settings.Default.PageLimitToStop);
            

            // Determin the mode of the application (whether to scrape the website, clean duplicates, or export data to csv)
            switch (Properties.Settings.Default.Mode)
            {
                case ScrapeMode:
                    StartScrape(scraper);
                    break;
                case CleanMode:
                    CleanDB(scraper);
                    break;
                case ExportMode:
                    Export(scraper, OutputFileName);
                    break;
                default:
                    StartScrape(scraper);
                    break;
            }

            // End tracing
            Trace.Flush();
        }

        /// <summary>
        /// Start the scrape
        /// </summary>
        /// <param name="scraper">The individual website scraper</param>
        static void StartScrape(PaginationScraper scraper)
        {
            scraper.Start();
        }

        /// <summary>
        /// Clean the database using the individual scrapers implementation for the website
        /// </summary>
        /// <param name="scraper">Each profile is valid for the individual scraper</param>
        static void CleanDB(PaginationScraper scraper)
        {
            Trace.TraceInformation("Cleaning the DB for duplicate values");
            scraper.CleanData();
        }

        /// <summary>
        /// Export the scrapers data into a csv
        /// </summary>
        /// <param name="scraper">The scraper which implements its own rules for what data to export</param>
        /// <param name="outputFileName">The file path to save the csv</param>
        static void Export(PaginationScraper scraper, string outputFileName)
        {
            Trace.TraceInformation("Starting exporting the data to CSV file");
            scraper.ExportToCSV(outputFileName, false);
        }

    }
}
