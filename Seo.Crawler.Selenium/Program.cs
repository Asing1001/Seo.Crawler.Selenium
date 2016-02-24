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
            Crawler crawler;
            try
            {
                var options = ConfigurationManager.GetSection("CrawlerOptions") as CrawlerOptions;
                logger.Info("Config is {0}", options);
                crawler = new Crawler(options);
                crawler.Start();
            }
            catch(Exception ex)
            {

                logger.Fatal(ex);
            }
        }
    }
}