using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Reflection.Metadata;

namespace Sanction
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://www.sahibinden.com/";
            var links = GetIlanLinks(url);
            List<Ilan> ilanlar = GetIlanlar(links);
            Export(ilanlar);
        }

        private static void Export(List<Ilan> ilanlar)
        {
            using (var writer = new StreamWriter("./ilanlar.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ilanlar);
            }
        }

        private static List<Ilan> GetIlanlar(List<string> links)
        {
            var ilanlar = new List<Ilan>();
            foreach (var link in links)
            {
                var doc = GetDocument(link);
                var ilan = new Ilan();
                ilan.Title = doc.DocumentNode.SelectSingleNode("//div[@class=\"classified-detail-wrapper \"]/div[@id=\"classifiedDetail \"]/div[@class=\"classifiedDetail \"]/div[@class=\"classifiedDetailTitle \"]/h1").InnerText;
                var xpath = "//*[@class=\"classifiedInfo\"]/h3";
                var price_raw = doc.DocumentNode.SelectSingleNode(xpath).InnerText;
                ilan.Price = ExtractPrice(price_raw);
                ilanlar.Add(ilan);
            }
            return ilanlar;
        }


        static double ExtractPrice(string raw)
        {
            var reg = new Regex(@"[\d\.,]+", RegexOptions.Compiled);
            var m = reg.Match(raw);
            if (!m.Success)
            {
                return 0;
            }
            return Convert.ToDouble(m.Value);
        }

        static List<string> GetIlanLinks(string url)
        {
            var doc = GetDocument(url); //kullanacagımız sectionda en genel anlamdaki linki vericez

            var linkNodes = doc.DocumentNode.SelectNodes("//div[@class='uiBox showcase'][1]//ul/li");  //xpath ekliyoruz

            var baseUri = new Uri(url);

            var links = new List<string>();

            foreach (var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;
                link = new Uri(baseUri, link).AbsoluteUri;
                links.Add(link);
            }
            return links;
        }



        static HtmlDocument GetDocument(string url)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }
    }
}
