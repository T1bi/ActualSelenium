using NUnit.Framework;

namespace CheckSitemap
{
    [SetUpFixture]
    public static class NamespaceSetup
    {
        public static void ExecuteForCreatingReportsNamespace(TestContext testContext)
        {
            Reporter.StartReporter();
        }
    }
}
