using System;
using System.Configuration;

namespace Seo.Crawler.Selenium
{
    public class CrawlerOptions : ConfigurationSection
    {

        [ConfigurationProperty("FolderPath")]
        public string FolderPath
        {
            get { return (string)this["FolderPath"]; }
        }
        [ConfigurationProperty("WaitMilliSeconds")]
        public int WaitMilliSeconds
        {
            get { return (int)this["WaitMilliSeconds"]; }
        }
        [ConfigurationProperty("UserAgent")]
        public string UserAgent
        {
            get { return (string)this["UserAgent"]; }
        }

        [ConfigurationProperty("MaxPageToVisit")]
        public int MaxPageToVisit
        {
            get { return (int)this["MaxPageToVisit"]; }
        }

        [ConfigurationProperty("StartUrl")]
        public Uri StartUrl
        {
            get { return (Uri)this["StartUrl"]; }
        }

        public override string ToString()
        {
            return string.Format(
                "FolderPath:{0}, WaitMilliSeconds:{1}, UserAgent:{2}, MaxPageToVisit:{3}, StartUrl:{4}",
                FolderPath, WaitMilliSeconds, UserAgent, MaxPageToVisit, StartUrl);
        }
    }
}