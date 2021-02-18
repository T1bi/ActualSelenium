using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using AventStack.ExtentReports;
using CheckSitemap;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NUnit.Framework;

namespace CheckSitemap
{
    [TestFixture]
    public class CheckSitemap : BaseTest
    {
        [Test]
        [TestCategory("SitemapChecking for the page what was" +
            " passed in parameter and send the result to the email " +
            "which was also passed in parameter.")]
        public void CheckSitemaps()
        {
            CheckSitemapLinks checkSitemapLinks = new CheckSitemapLinks(Driver);
            List<string> whichPagesToTest = checkSitemapLinks.GetTheSiteFromParameter(SiteFromConsole);
            checkSitemapLinks.RunTheTest(whichPagesToTest);
        }

       
        
    }
}
