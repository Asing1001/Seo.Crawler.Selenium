using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Seo.Crawler.Selenium
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ConfigurationManager.GetSection("CrawlerOptions") as CrawlerOptions;
            Console.WriteLine("Config is {0}",options);
            var crawler = new Crawler(options);
            crawler.Start();
            Console.ReadLine();
        }
    }
}