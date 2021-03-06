﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NLog;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Data;
using OpenQA.Selenium;
namespace Seo.Crawler.Selenium
{
    public class Crawler
    {
        private RemoteWebDriver _driver;
        private CrawlerOptions _options;
        private HashSet<Uri> pagesVisited;
        private List<Uri> pagesToVisit;
        private Stopwatch _watch;
        private Logger logger;
        private HashSet<Uri> pagesNotFound;
        private DataTable pagesErrorList;
        private Dictionary<string, string> pageParentURLMapping;  //Key is current Page, Content is parent Page
        public Crawler(CrawlerOptions options)
        {
            _options = options;
            pagesVisited = new HashSet<Uri>();
            pagesNotFound = new HashSet<Uri>();
            pagesToVisit = new List<Uri>();
            _watch = new Stopwatch();
            pagesErrorList = new DataTable();
            pagesErrorList = ExcelHandler.InitTable(pagesErrorList);
            logger = LogManager.GetCurrentClassLogger();
            pageParentURLMapping = new Dictionary<string, string>();
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--user-agent=" + _options.UserAgent);
            _driver = new ChromeDriver(chromeOptions);
        }


        public void Start()
        {
            try
            {
                _watch.Start();
                Crawl(_options.StartUrl);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
            Finish();
        }

        private void Crawl(Uri uri)
        {
            for (var i = 0; i <= _options.MaxPageToVisit; i++)
            {
                pagesVisited.Add(uri);
                logger.Info("[{0}] Open page :{1}", pagesVisited.Count, uri);
                _driver.Navigate().GoToUrl(uri);
                SaveHtmlAndScreenShot(uri);
                ValidatePage(uri);
                pagesToVisit.AddRange(GetUnvisitedLinks());
                if (pagesToVisit.Count > 0)
                {
                    uri = PopUrlFromPagesToVisit();
                }
                else
                {
                    break;
                }
            }
        }

        private void ValidatePage(Uri currentUri)
        {
   
            DataRow drRow = pagesErrorList.NewRow();
            drRow["URL"] = currentUri.AbsolutePath;
            List<LogEntry> logEntry =  _driver.Manage().Logs.GetLog(LogType.Browser).ToList();
            Boolean ValidateFailed = false;

            if (_driver.PageSource.Contains("Error 404") || _driver.PageSource.Contains("404") || _driver.PageSource.ToLower().Contains("not found"))
            {
                drRow["NotFound"] = " Page Not Found";
                ValidateFailed = true;
            }
            if (logEntry.Count > 0)
            {

                drRow["LogCount"] = logEntry.Count.ToString();
                drRow["Error"] += string.Join(" , ", logEntry.Where(log => !log.Message.Contains("$modal is now deprecated. Use $uibModal instead.")).Select(log => log.Message).ToList());
                ValidateFailed = true;
            }
            if (ValidateFailed)
            {
                drRow["SourceURL"] = pageParentURLMapping.ContainsKey(currentUri.AbsolutePath)? pageParentURLMapping[currentUri.AbsolutePath] : "";
                pagesErrorList.Rows.Add(drRow);
            }
            if (pageParentURLMapping.Count >0)
                pageParentURLMapping.Remove(currentUri.AbsoluteUri);
        }

        private void Finish()
        {
            _driver.Quit();
            SaveSitemap();
            Save404Pages();
            ExcelHandler.DataTableToExcel(_options.FolderPath + "\\PageNonValidateList.xlsx", pagesErrorList);
            _watch.Stop();
            logger.Info("Finish all task in {0}", _watch.Elapsed);
        }

        private void Save404Pages()
        {
            try
            {
                string pagesNotFoundPath = string.Format("{0}/{1}pagesNotFound.xml", _options.FolderPath, DateTime.Now.ToString("dd-MM-yyyy"));
                using (var fileStream = new FileStream(pagesNotFoundPath, FileMode.Create))
                {
                    var siteMapGenerator = new SiteMapGenerator(fileStream, Encoding.UTF8);
                    siteMapGenerator.Generate(pagesNotFound);
                    siteMapGenerator.Close();
                }
                logger.Info("SiteMap save to {0}", pagesNotFoundPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private Uri PopUrlFromPagesToVisit()
        {
            var result = pagesToVisit.FirstOrDefault();
            pagesToVisit.RemoveAt(0);
            return result;
        }

        private IEnumerable<Uri> GetUnvisitedLinks()
        {
            var result = new List<Uri>();
            var originHost = _options.StartUrl.AbsoluteUri;
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

            foreach (var link in links)
            {
                if (link != null && link.AbsoluteUri.Contains(originHost) && !pagesVisited.Contains(link)
                    && !pagesToVisit.Contains(link)&& !result.Contains(link))
                {
                    result.Add(link);
                    pageParentURLMapping.Add(link.ToString(),_driver.Url);
                }
            }

            logger.Info("Get {0} sameDomainUnvisitedLinks, list as below :{1}", result.Count,
                JsonConvert.SerializeObject(result.Select(uri => uri.AbsolutePath)));
            return result;
        }

        private void SaveHtmlAndScreenShot(Uri uri)
        {
            try
            {
                var removeScriptTag =
                    "Array.prototype.slice.call(document.getElementsByTagName('script')).forEach(function(item) { item.parentNode.removeChild(item);});";
                var addClassToBody = "document.getElementsByTagName('body')[0].className += ' seoPrerender';";
                _driver.ExecuteScript(removeScriptTag+addClassToBody);
                //uri.AbsolutePath is relative url
                var result = _driver.PageSource;

                string filenameWithPath = _options.FolderPath + uri.AbsolutePath + MakeValidFileName(uri.Query);
                Directory.CreateDirectory(Path.GetDirectoryName(filenameWithPath));
                File.WriteAllText(filenameWithPath + ".html", result);
                logger.Info("SaveHtmlAndScreenShot to {0}.html", filenameWithPath);
                _driver.GetScreenshot().SaveAsFile(filenameWithPath + ".jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                    siteMapGenerator.Close();
                }
                logger.Info("SiteMap save to {0}", sitemapPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
