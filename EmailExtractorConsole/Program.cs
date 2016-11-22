using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Scraper;

namespace EmailExtractorConsole
{
    /// <summary>
    /// Example implementations of the Email Crawler
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain domain = AppDomain.CurrentDomain;

            // Set a timeout interval of 10 seconds for infinite web response times
            domain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(10));

            // Sample useage only
            var extractor = new EmailExtractor();
            while (true)
            {
                extractor.LoopOverResults();
                Trace.TraceInformation("100 entries checked checkpoint");
            }
        }
    }
}
