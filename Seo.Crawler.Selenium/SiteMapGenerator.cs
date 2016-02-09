using System;
using System.Collections.Generic;
using System.Xml;

namespace Seo.Crawler.Selenium
{
    internal class SiteMapGenerator
    {
        XmlTextWriter writer;

        public SiteMapGenerator(System.IO.Stream stream, System.Text.Encoding encoding)
        {
            writer = new XmlTextWriter(stream, encoding);
            writer.Formatting = Formatting.Indented;
        }

        public SiteMapGenerator(System.IO.TextWriter w)
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

        /// <summary>
        /// Writes the beginning of the SiteMap document
        /// </summary>
        private void WriteStartDocument()
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset");

            writer.WriteAttributeString("xmlns", "http://www.google.com/schemas/sitemap/0.84");
        }

        /// <summary>
        /// Writes the end of the SiteMap document
        /// </summary>
        private void WriteEndDocument()
        {
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Closes this stream and the underlying stream
        /// </summary>
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