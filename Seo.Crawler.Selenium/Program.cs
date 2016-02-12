using System;
using System.Configuration;
using NLog;

namespace Seo.Crawler.Selenium
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
                
                var options = ConfigurationManager.GetSection("CrawlerOptions") as CrawlerOptions;
                logger.Info("Config is {0}", options);
                var crawler = new Crawler(options);
                crawler.Start();
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                logger.Fatal(ex);
                Console.ReadLine();
            }
        }
    }
}