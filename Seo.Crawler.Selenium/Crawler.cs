using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenQA.Selenium.Chrome;

namespace Seo.Crawler.Selenium
{
    public class Crawler
    {
        private ChromeDriver _driver;
        private CrawlerOptions _options;
        private HashSet<Uri> pagesVisited;
        private List<Uri> pagesToVisit;
        private Uri _nextUri;


        public Crawler(CrawlerOptions options)
        {
            this._options = options;

            pagesVisited = new HashSet<Uri>();
            pagesToVisit = new List<Uri>();
        }


        public void Start()
        {
            Crawl(_options.StartUrl);
        }

        private void Crawl(Uri uri)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(_options.UserAgent);
            // Initialize the Chrome Driver
            using (_driver = new ChromeDriver(chromeOptions))
            {
                Console.WriteLine("[{0}] Open page :{1}", pagesVisited.Count, uri);
                _driver.Navigate().GoToUrl(_options.StartUrl);
                pagesVisited.Add(uri);

                var filePath = _options.FolderPath + uri.PathAndQuery;
                SaveHtmlAndScreenShot(filePath);

                pagesToVisit.AddRange(GetUnvisitedLinks());
                _nextUri = RemoveAndGetUrlFromPagesToVisit();
                if (_nextUri != null)
                {
                    Crawl(_nextUri);
                }
            }
        }

        private Uri RemoveAndGetUrlFromPagesToVisit()
        {
            var result = pagesToVisit.FirstOrDefault();
            pagesToVisit.RemoveAt(0);
            return result;
        }

        private IEnumerable<Uri> GetUnvisitedLinks()
        {
            var originHost = _options.StartUrl.Host;
            var links = _driver.FindElementsByTagName("a").Select(a => new Uri(a.GetAttribute("href")));
            var sameDomainUnvisitedLinks = links.Where(l => l.Host.Contains(originHost) && !pagesVisited.Contains(l));
            return sameDomainUnvisitedLinks;
        }

        private void SaveHtmlAndScreenShot(string path)
        {
            var result = _driver.PageSource;
            File.WriteAllText(path + ".html", result);
            _driver.GetScreenshot().SaveAsFile(path+ ".png", ImageFormat.Png);
        }

    }

    public class CrawlerOptions
    {
        public string FolderPath { get; set; }
        public int WaitMilliSeconds { get; set; }
        public string UserAgent { get; set; }
        public bool AcceptQueryString { get; set; }
        public int MaxPageToVisit { get; set; }
        public Uri StartUrl { get; set; }
    }
}
