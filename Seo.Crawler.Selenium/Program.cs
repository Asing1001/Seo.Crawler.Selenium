using System;
using System.Configuration;

namespace Seo.Crawler.Selenium
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = ConfigurationManager.GetSection("CrawlerOptions") as CrawlerOptions;
                Console.WriteLine("Config is {0}", options);
                var crawler = new Crawler(options);
                crawler.Start();
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unexpected error occur:{0}",ex);
                Console.ReadLine();
            }
        }
    }
}