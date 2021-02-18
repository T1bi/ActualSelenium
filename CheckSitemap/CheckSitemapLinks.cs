using AventStack.ExtentReports;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml.Linq;

namespace CheckSitemap
{
    public class CheckSitemapLinks : BaseApplicationPage
    {
        public CheckSitemapLinks(IWebDriver driver) : base(driver)
        {
        }

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static int TotalNumberOfSitesContainingWarnings { get; set; }
        public static int TotalNumberOfSitesContainingErrors { get; private set; }
        public static int TotalNumberOfSitesContainingOk { get; private set; }
        public static int TotalNumberOfSites { get; private set; }

        internal List<string> SelectWhichPageToTest(List<string> links)
        {
            List<string> AlllinksOnSiteMap = new List<string>();

            foreach (var sitemapfromlinks in links)
            {
                var actualSiteMap = sitemapfromlinks;

                try
                {
                    XElement sitemap = XElement.Load(sitemapfromlinks);
                    // ... XNames.
                    XName url = XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    XName loc = XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    // ... Loop over url elements.
                    // ... Then access each loc element.
                    foreach (var urlElement in sitemap.Elements(url))
                    {
                        var locElement = urlElement.Element(loc);
                        AlllinksOnSiteMap.Add(locElement.Value);
                    }
                }
                catch (Exception exceptonFromLoadingXml)
                {

                    Reporter.LogTestStepForBugLogger(Status.Error, $"This sitemap: {actualSiteMap} is invalid! Error: {exceptonFromLoadingXml} ");
                }




            }
            return AlllinksOnSiteMap;
        }
        public void RunTheTest(List<string> links)
        {

            List<string> linksOnSiteMap = SelectWhichPageToTest(links);

            foreach (var url in linksOnSiteMap)
            {
                try
                {
                    TryToOpenTheUrl(url);
                    
                    HttpWebRequest httpReq2 = (HttpWebRequest)WebRequest.Create(url);
                    httpReq2.AllowAutoRedirect = false;

                    HttpWebResponse httpRes = (HttpWebResponse)httpReq2.GetResponse();
                    
                    if (httpRes.StatusCode == HttpStatusCode.OK)
                    {
                        CountOpenedSites("SiteIsOk");
                    }
                    else 
                    {
                        Reporter.LogTestStepForBugLogger(Status.Warning, $"This url have been opened: {url}  ,status code: {(int)httpRes.StatusCode}-{httpRes.StatusCode}");
                        CountOpenedSites("siteContainsWarning");
                    }
                    httpRes.Close();
                }
                catch (WebException ex)
                {
                    Reporter.LogTestStepForBugLogger(Status.Error, $"This url have been opened: {url}  ,status: {ex.Message}");
                    CountOpenedSites("siteContainsError");
                }
                finally
                {
                    CountOpenedSites("allSiteCase");
                }
            }
        }

        private void CountOpenedSites(string openedSite)
        {
            switch (openedSite)
                {
                    case "siteContainsWarning":
                    TotalNumberOfSitesContainingWarnings += 1;
                    break;
                case "siteContainsError":
                    TotalNumberOfSitesContainingErrors += 1;
                    break;
                case "SiteIsOk":
                    TotalNumberOfSitesContainingOk += 1;
                    break;
                case "allSiteCase":
                    TotalNumberOfSites += 1;
                    break;

            }
        }

        private void TryToOpenTheUrl(string url)
        {
            try
            {
                _logger.Info($"Attempting to open this url=>{url}");
                Driver.Navigate().GoToUrl(url);
            }
            catch (Exception expectionFromOpeningTheUrl)
            {
                _logger.Info($"We coould not open the url! The reason is: {expectionFromOpeningTheUrl}");
            }
        }

        internal List<string> GetTheSiteFromParameter(string SitePropFromConsole)
        {
            List<string> linksOnSiteMap = new List<string>();

            string workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.FullName, @"Sites\SiteMaps.xml");

            try
            {
                XElement sitemapsXml = XElement.Load(path);

                XName url = XName.Get("URL");
                if (SitePropFromConsole == "All")
                {
                    foreach (var urlElement in sitemapsXml.Elements())
                    {
                        linksOnSiteMap.Add(urlElement.Element(url).Value);
                    }
                    return linksOnSiteMap;
                }
                else
                {
                    XName projectName = XName.Get(SitePropFromConsole);

                    foreach (var urlElement in sitemapsXml.Elements(projectName))
                    {
                        linksOnSiteMap.Add(urlElement.Element(url).Value);
                    }
                    return linksOnSiteMap;
                }
            }
            catch (Exception exceptionFromLoadingXMLfile)
            {
                Reporter.LogTestStepForBugLogger(Status.Error, $"The Sitemaps.xml file is not valid! {exceptionFromLoadingXMLfile}");
                return null;
            }


        }

    }
}