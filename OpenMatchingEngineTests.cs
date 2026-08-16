using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace MatchingEngine.UI.Tests.Tests;

[TestFixture]
public sealed class OpenMatchingEngineTests
{
    private IWebDriver? driver;
    private string? evidenceDirectory;

    [SetUp]
    public void SetUp()
    {
        var options = new ChromeOptions();
        driver = new ChromeDriver(options);
        driver.Manage().Window.Maximize();
        evidenceDirectory = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "TestResults",
            "Evidence",
            DateTime.Now.ToString("yyyyMMdd-HHmmss")));
        Directory.CreateDirectory(evidenceDirectory);
    }

    [Test]
    public void OpenMatchingEngineWebsite()
    {
        const string expectedUrl = "https://www.matchingengine.com/";

        driver!.Navigate().GoToUrl(expectedUrl);

        Assert.That(driver.Url, Does.StartWith("https://www.matchingengine.com"));
        Assert.That(driver.Title, Is.Not.Empty);
        CaptureEvidence("01-homepage-opened");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var allowAllButton = wait.Until(currentDriver =>
        {
            var element = currentDriver.FindElement(By.Id("CybotCookiebotDialogBodyLevelButtonLevelOptinAllowAll"));
            return element.Displayed && element.Enabled ? element : null;
        });

        allowAllButton.Click();
        CaptureEvidence("02-Cookie-Consent-Accepted");

        var solutionsMenuToggle = wait.Until(currentDriver =>
            currentDriver.FindElement(By.CssSelector("#nav-toggle-solutions .MainNavLink_text__7N2uU")));

        new Actions(driver)
            .MoveToElement(solutionsMenuToggle)
            .Perform();

        solutionsMenuToggle.Click();

        wait.Until(currentDriver =>
            currentDriver.FindElement(By.Id("nav-toggle-solutions"))
                .GetAttribute("aria-expanded") == "true");
        CaptureEvidence("03-solutions-menu-expanded");

        var musicSolutionsOption = wait.Until(currentDriver =>
            currentDriver.FindElements(By.CssSelector("span.SubNavLink_text___WBOF"))
                .FirstOrDefault(element =>
                    element.Text.Trim() == "Music and copyright solutions" && element.Displayed));

        new Actions(driver)
            .MoveToElement(musicSolutionsOption)
            .Perform();

        var repertoireManagementOption = wait.Until(currentDriver =>
        {
            return currentDriver.FindElements(By.CssSelector("span.SubNavLink_text___WBOF"))
                .FirstOrDefault(element =>
                    element.Text.Trim() == "Repertoire management" && element.Displayed && element.Enabled);
        });

        repertoireManagementOption.Click();

        wait.Until(currentDriver =>
            currentDriver.Url.Contains("/Music-and-copyright-solutions/Repertoire-management", StringComparison.OrdinalIgnoreCase));
        CaptureEvidence("04-repertoire-management-opened");

        ((IJavaScriptExecutor)driver).ExecuteScript("document.body.style.zoom = '60%';");

        var softwareFeaturesSection = wait.Until(currentDriver =>
            currentDriver.FindElements(By.XPath(
                "//h2[normalize-space()='Software features']/ancestor::div[contains(@class, 'Section_inner__wh2mp')][1]"))
                .FirstOrDefault(element => element.Displayed));

        ((IJavaScriptExecutor)driver).ExecuteScript("""
            arguments[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
            """, softwareFeaturesSection);

        wait.Until(currentDriver =>
        {
            var viewportState = (IReadOnlyDictionary<string, object>)((IJavaScriptExecutor)currentDriver)
                .ExecuteScript("""
                    const bounds = arguments[0].getBoundingClientRect();
                    return {
                        top: bounds.top,
                        bottom: bounds.bottom,
                        viewportHeight: window.innerHeight
                    };
                    """, softwareFeaturesSection)!;

            var top = Convert.ToDouble(viewportState["top"]);
            var bottom = Convert.ToDouble(viewportState["bottom"]);
            var viewportHeight = Convert.ToDouble(viewportState["viewportHeight"]);
            return top >= 0 && bottom <= viewportHeight;
        });

        Thread.Sleep(TimeSpan.FromSeconds(2));
        CaptureEvidence("05-software-features-section-visible");
    }

    [TearDown]
    public void TearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            CaptureEvidence("failure");
        }

        driver?.Quit();
        driver?.Dispose();
    }

    private void CaptureEvidence(string stepName)
    {
        if (driver is not ITakesScreenshot screenshotDriver || string.IsNullOrWhiteSpace(evidenceDirectory))
        {
            return;
        }

        var fileName = $"{stepName}-{DateTime.Now:HHmmssfff}.png";
        screenshotDriver.GetScreenshot().SaveAsFile(Path.Combine(evidenceDirectory, fileName));
    }
}
