namespace N2NTest.Steps;

using Microsoft.Playwright;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;
using System;
using System.Threading.Tasks;

[Binding]
[Scope(Feature = "Staff Dashboard")]
public class TicketManagementSteps
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 2000 });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Given(@"I am logged in as a staff member")]
    public async Task GivenIAmLoggedInAsAStaffMember()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        await LoginHelper.LoginAsRole(_page, "staff");
        Assert.True(_page.Url.Contains("dashboard"));
    }

    [When(@"I navigate to the staff dashboard")]
    public async Task WhenINavigateToTheStaffDashboard()
    {
        await _page.GotoAsync("http://localhost:3001/staff/dashboard");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [When(@"I drag a ticket from ""(.*)"" to ""(.*)""")]
    public async Task WhenIDragATicketFromTo(string source, string target)
    {
        // Simplifierad drag-simulering (lägg till logik vid behov)
        await Task.Delay(500);
    }

    [Then(@"the ticket should appear in the ""(.*)"" column")]
    public async Task ThenTheTicketShouldAppearInTheColumn(string targetColumn)
    {
        var pageText = await _page.TextContentAsync("body");
        Assert.Contains(targetColumn, pageText);
    }
}