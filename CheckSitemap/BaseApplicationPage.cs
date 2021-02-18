using OpenQA.Selenium;

namespace CheckSitemap
{
    public class BaseApplicationPage
    {
        public BaseApplicationPage(IWebDriver driver)
        {
            Driver = driver;
        }

        protected IWebDriver Driver { get; set; }
        }
    }
