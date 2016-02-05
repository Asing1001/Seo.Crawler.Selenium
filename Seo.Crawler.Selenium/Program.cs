using System;
using System.Drawing.Imaging;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Seo.Crawler.Selenium
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CrawlerOptions()
            {
                FolderPath = "C:/Agilebet/Seo/Seo0205/",
                WaitMilliSeconds = 100,
                UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X) seo.crawler",
                AcceptQueryString = true,
                MaxPageToVisit = 10,
                StartUrl = new Uri("https://preview.188bet.com/")
            };
            
            var crawler = new Crawler(options);
            crawler.Start();
        }
    }
}