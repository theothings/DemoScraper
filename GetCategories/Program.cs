using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Model;

namespace GetCategories
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> allCategories = new List<string>();
            int profiles = 0;

            using(var db = new ScrapedItem())
            {
                // Loop over each profile
                foreach(var profile in db.BuildUppProfiles.Where(p => p.Category != null && p.Category != ""))
                {
                    profiles++;
                    Trace.TraceInformation("There is {0} profiles checked for categories", profiles);

                    // If profile is null or empty
                    if (!string.IsNullOrEmpty(profile.Category))
                    {
                        // Split the categories into a categories list
                        var categories = profile.Category.Split(',');
                        string category;
                        foreach(string categoryRaw in categories)
                        {
                            category  = categoryRaw.Trim();

                            if (!allCategories.Contains(category))
                            {
                                allCategories.Add(category);
                                Trace.TraceInformation("Added the category, {0} to the list", category);
                            }
                        }
                    }
                }
            }

            using (StreamWriter outfile = new StreamWriter("categories.csv"))
            {
                // Loop over list and output to a csv file
                foreach (var category in allCategories)
                {
                    outfile.WriteLine(category);
                }
            }
        }
    }
}
