using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Seo.Crawler.Selenium
{
    internal class SiteMapGenerator
    {
        XmlTextWriter writer;

        public SiteMapGenerator(Stream stream, Encoding encoding)
        {
            writer = new XmlTextWriter(stream, encoding);
            writer.Formatting = Formatting.Indented;
        }

        public SiteMapGenerator(TextWriter w)
        {
            writer = new XmlTextWriter(w);
            writer.Formatting = Formatting.Indented;
        }

        public void Generate(HashSet<Uri> links)
        {
            WriteStartDocument();
            foreach (var link in links)
            {
                WriteItem(link.OriginalString, DateTime.Now, "1.0");
            }
            WriteEndDocument();
            Close();
        }

        private void WriteStartDocument()
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset");

            writer.WriteAttributeString("xmlns", "http://www.google.com/schemas/sitemap/0.84");
        }

        private void WriteEndDocument()
        {
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private void Close()
        {
            writer.Flush();
            writer.Close();
        }

        private void WriteItem(string link, DateTime publishedDate, string priority)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", link);
            writer.WriteElementString("lastmod", publishedDate.ToString("yyyy-MM-dd"));
            writer.WriteElementString("changefreq", "always");
            writer.WriteElementString("priority", priority);
            writer.WriteEndElement();
        }

    }
}