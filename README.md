# Matching Engine UI Automation

Selenium WebDriver automation tests written in C# with NUnit.

## Prerequisites

- .NET 10 SDK
- Google Chrome
- Internet access so Selenium Manager can resolve the matching ChromeDriver

## Run the test

From the repository root:

```powershell
dotnet restore
dotnet test .\MatchingEngine.UI.Tests\MatchingEngine.UI.Tests.csproj
```

The test starts Chrome in normal, visible mode and:

1. Opens `https://www.matchingengine.com/`.
2. Clicks the Cookiebot `Allow all` consent button.
3. Hovers over the `Solutions` navigation item, expands its menu, and verifies `aria-expanded="true"`.
4. Hovers over `Music and copyright solutions`, clicks `Repertoire management`, and verifies the destination page.
5. Sets the page to 60% zoom, scrolls the complete `Software features` section into view, and verifies its boundaries are visible.

No headless option is configured.

## Test Evidence

Each completed test step saves a PNG screenshot to `MatchingEngine.UI.Tests/TestResults/Evidence/<run-timestamp>/`.
Screenshots are also captured when a test fails. The evidence folder is excluded from Git commits.
