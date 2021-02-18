using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Xml;
using AutomationResources;
using HtmlAgilityPack;
using NLog;
using NUnit.Framework;
using OpenQA.Selenium;


namespace CheckSitemap
{
    [Category("CreatingReports")]
    [TestFixture]
    public class BaseTest
    {
        //private static TestContext _testContext;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected IWebDriver Driver { get; private set; }
        public TestContext TestContext { get; set; }
        private ScreenshotTaker ScreenshotTaker { get; set; }
        public string HtmlReportFullPath => Reporter.HtmlReportFullPath;
        public string  EmailFromConsole => TestContext.Parameters.Get("Email");
        public string SiteFromConsole => TestContext.Parameters.Get("Site");
        public int ErrorsCountOnTheSite => CheckSitemapLinks.TotalNumberOfSitesContainingErrors;
        public int WarningsCountOnTheSite => CheckSitemapLinks.TotalNumberOfSitesContainingWarnings;
        public int OkCountOnTheSite => CheckSitemapLinks.TotalNumberOfSitesContainingOk;
        public int CountAllOpenedPageOnTheSite => CheckSitemapLinks.TotalNumberOfSites;

        [SetUp]
        public void SetupForEverySingleTestMethod()
        {
            Reporter.StartReporter();
            Logger.Debug("*************************************** TEST STARTED");
            Logger.Debug("*************************************** TEST STARTED");
            Reporter.AddTestCaseMetadataToHtmlReport(TestContext.CurrentContext);
            var factory = new WebDriverFactory();
            Driver = factory.Create(BrowserType.Chrome);
            Driver.Manage().Window.Maximize();
            ScreenshotTaker = new ScreenshotTaker(Driver, TestContext);
        }

        [TearDown]
        public void TearDownForEverySingleTestMethod()
        {
            Logger.Debug(GetType().FullName + " started a method tear down");
            try
            {
                TakeScreenshotForTestFailure();
            }
            catch (Exception e)
            {
                Logger.Error(e.Source);
                Logger.Error(e.StackTrace);
                Logger.Error(e.InnerException);
                Logger.Error(e.Message);
            }
            finally
            {
                SendEmailWhenNeeded();
                
                StopBrowser();
                Logger.Debug(TestContext.CurrentContext.Test.Name);
                Logger.Debug("*************************************** TEST STOPPED");
                Logger.Debug("*************************************** TEST STOPPED");
            }
        }

        private void SendEmailWhenNeeded()
        {
            bool EmailKeyFromConfig = Convert.ToBoolean(ConfigurationManager.AppSettings["SendReportWhenOK"]);
            if (EmailKeyFromConfig)
            {
                SendEmailReport(EmailFromConsole, HtmlReportFullPath);
            }
            else
            {
                if (ErrorsCountOnTheSite > 0 | WarningsCountOnTheSite > 0)
                {
                    SendEmailReport(EmailFromConsole, HtmlReportFullPath);
                }
            }
        }

        private void TakeScreenshotForTestFailure()
        {
            if (ScreenshotTaker != null)
            {
                ScreenshotTaker.CreateScreenshotIfTestFailed();
                Reporter.ReportTestOutcome(ScreenshotTaker.ScreenshotFilePath);
            }
            else
            {
                Reporter.ReportTestOutcome("");
            }
        }

        private void StopBrowser()
        {
            if (Driver == null)
                return;
            Driver.Quit();
            Driver = null;
            Logger.Trace("Browser stopped successfully.");
        }

        internal void SendEmailReport(string whomToSend, string whatToSend)
        {
            string attachmentToSend = whatToSend;
            string username02 = ConfigurationManager.AppSettings["ReportSenderEmailUsername"];
            string password02 = ConfigurationManager.AppSettings["ReportSenderEmailPasssord"];

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(ConfigurationManager.AppSettings["ReportSenderEmailAdress"]);
            mail.To.Add(whomToSend);
            mail.IsBodyHtml = true;

            var doc = new HtmlDocument();
            doc.Load(whatToSend);
            SetTheSubjectOfTheEmailReport(mail, doc);
            SetTheBodyOfTheEmailReport(mail, doc);

            Attachment attachment = new Attachment(attachmentToSend);
            mail.Attachments.Add(attachment);

            SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["ReportSenderEmailServer"], 
                Convert.ToInt32(ConfigurationManager.AppSettings["ReportSenderEmailPort"]));
            smtp.Credentials = new NetworkCredential(username02, password02);
            smtp.EnableSsl = false;
            smtp.Send(mail);
        }

        private void SetTheBodyOfTheEmailReport(MailMessage mail, HtmlDocument doc)
        {
            var node = doc.DocumentNode.SelectSingleNode("//*[@class='test-content hide']");
            var testName = doc.DocumentNode.SelectSingleNode("//*[@class='test-name']");
            string whatWasTestedToMail = "<h4>The test included " + SiteFromConsole + " sitemap pages.</h4>";
            string testNametoMail = "<h3>" + testName.InnerText + "</h3>";
            string htmlContent = node.InnerHtml;

            mail.Body = string.Concat(testNametoMail + whatWasTestedToMail + htmlContent);
        }

        private void SetTheSubjectOfTheEmailReport(MailMessage mail, HtmlDocument doc)
        {
            
            string Subject = $"CheckSitemaps {DateTime.Now:yyyy.MM.dd HH:mm} - {SetTheCountedNumbersToEmailSubject()} - {SiteFromConsole} - ";

            mail.Subject = string.Concat(Subject + FindOutTheTestStatusToTheSubject(doc));
        }

        private string SetTheCountedNumbersToEmailSubject()
        {
            return $"{CountAllOpenedPageOnTheSite.ToString()}/{OkCountOnTheSite.ToString()}/{ErrorsCountOnTheSite.ToString()}/{WarningsCountOnTheSite.ToString()}";
        }

        private static string FindOutTheTestStatusToTheSubject(HtmlDocument doc)
        {
            var testResultTable = doc.DocumentNode.SelectSingleNode("//*[@class='bordered table-results']//tbody");
            string testStatus = "";
                if (testResultTable.InnerHtml.Contains("status fail"))
                {
                    testStatus = "Fail";
                }
                if (testResultTable.InnerHtml.Contains("status pass"))
                {
                    testStatus = "OK";
                    if (testResultTable.InnerHtml.Contains("status warning"))
                    {
                        testStatus = "Warning";
                        if (testResultTable.InnerHtml.Contains("status error"))
                        {
                            testStatus = "Error";
                        }
                    }
                    if (testResultTable.InnerHtml.Contains("status error"))
                    {
                        testStatus = "Error";
                    }
                }
            return testStatus;
        }

    }
}