using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using OpenQA.Selenium.Chrome;

namespace Seo.Crawler.Selenium
{
    public class Crawler
    {
        private ChromeDriver _driver;
        private CrawlerOptions _options;
        private HashSet<Uri> pagesVisited;
        private List<Uri> pagesToVisit;
        private Stopwatch _watch;


        public Crawler(CrawlerOptions options)
        {
            _options = options;
            pagesVisited = new HashSet<Uri>();
            pagesToVisit = new List<Uri>();
            _watch = new Stopwatch();
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(_options.UserAgent);
            _driver = new ChromeDriver(chromeOptions);
            //To init screenshot
            _driver.GetScreenshot();
        }


        public void Start()
        {
            _watch.Start();
            Crawl(_options.StartUrl);
        }

        private void Crawl(Uri uri)
        {
            pagesVisited.Add(uri);
            Console.WriteLine("[{0}] Open page :{1}", pagesVisited.Count, uri);
            _driver.Navigate().GoToUrl(uri);
            SaveHtmlAndScreenShot(uri);
            pagesToVisit.AddRange(GetUnvisitedLinks());
            if (pagesToVisit.Count>=1 && pagesVisited.Count < _options.MaxPageToVisit)
            {
                Crawl(PopUrlFromPagesToVisit());
            }
            else
            {
                Finish();
            }

        }

        private void Finish()
        {
            _driver.Dispose();
            SaveSitemap();
            _watch.Stop();
            Console.WriteLine("Finish all task in {0} ms", _watch.ElapsedMilliseconds);
        }

        private Uri PopUrlFromPagesToVisit()
        {
            var result = pagesToVisit.FirstOrDefault();
            pagesToVisit.RemoveAt(0);
            return result;
        }

        private IEnumerable<Uri> GetUnvisitedLinks()
        {
            var originHost = _options.StartUrl.Host;
            var links = _driver.FindElementsByCssSelector("a[href]")
                .Select(a =>
                {
                    try
                    {
                        return new Uri(a.GetAttribute("href"));
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                );
            var sameDomainUnvisitedLinks = links.Where(link => link != null && link.Host.Contains(originHost) && !pagesVisited.Contains(link) 
                && !pagesToVisit.Contains(link));
            return sameDomainUnvisitedLinks;
        }

        private void SaveHtmlAndScreenShot(Uri uri)
        {
            try
            {
                //uri.AbsolutePath is relative url
                var result = _driver.PageSource;
                string filenameWithPath = _options.FolderPath + uri.AbsolutePath + MakeValidFileName(uri.Query);
                Directory.CreateDirectory(Path.GetDirectoryName(filenameWithPath));
                File.WriteAllText(filenameWithPath + ".html", result);
                Console.WriteLine("SaveHtmlAndScreenShot to {0}", filenameWithPath);
                _driver.GetScreenshot().SaveAsFile(filenameWithPath + ".jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveHtmlAndScreenShot throw exception {0}", ex);
            }
        }

        private string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private void SaveSitemap()
        {
            try
            {
                string sitemapPath = string.Format("{0}/{1}sitemap.xml", _options.FolderPath, DateTime.Now.ToString("dd-MM-yyyy"));
                using (var fileStream = new FileStream(sitemapPath, FileMode.Create))
                {
                    var siteMapGenerator = new SiteMapGenerator(fileStream, Encoding.UTF8);
                    siteMapGenerator.Generate(pagesVisited);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveSitemap throw error {0}", ex);
            }
        }
    }
}
